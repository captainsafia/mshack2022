using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Tags;
using MSHack2022.Analyzers;
using System.Collections.Immutable;
using System.Composition;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MSHack2022.Codefixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class MoveMiddlewareToClassFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.MoveMiddlewareToClass.Id);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            // TODO: Support multiple suggested class locations, e.g. nested, same file, new file
            context.RegisterCodeFix(
                CodeAction.Create("Convert to middleware class",
                    cancellationToken => MoveToMiddlewareClass(diagnostic, context.Document, cancellationToken),
                    equivalenceKey: DiagnosticDescriptors.MoveMiddlewareToClass.Id),
                diagnostic);
        }

        return Task.CompletedTask;
    }

    private static async Task<Document> MoveToMiddlewareClass(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return document;
        }

        var diagnosticTarget = root.FindNode(diagnostic.Location.SourceSpan);
        
        if (diagnosticTarget is InvocationExpressionSyntax invocationExpression)
        {
            var compilationUnit = diagnosticTarget.FirstAncestorOrSelf<CompilationUnitSyntax>();

            if (compilationUnit is null)
            {
                return document;
            }

            // TODO: Check for name collisions
            var middlewareClassName = "Middleware1";

            // app.UseMiddleware1();
            var registerMiddlewareInvocation = ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("app"),
                        IdentifierName($"Use{middlewareClassName}"))));

            // TODO: Support hoisting closures to middleware constructor arguments

            var middlewareClassDeclarations =  List(new MemberDeclarationSyntax[] {
                // internal class Middleware1
                ClassDeclaration(middlewareClassName)
                    .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword)))
                    .WithMembers(List(new MemberDeclarationSyntax[]{
                        // private readonly RequestDelegate _next;
                        FieldDeclaration(
                            VariableDeclaration(IdentifierName("RequestDelegate"))
                                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier("_next")))))
                            .WithModifiers(TokenList(new [] { Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword)})),
                        // public Middleware1(RequestDelegate next)
                        ConstructorDeclaration(Identifier(middlewareClassName))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithParameterList(ParameterList(SingletonSeparatedList(
                                    Parameter(Identifier("next")).WithType(IdentifierName("RequestDelegate")))))
                            .WithBody(Block(SingletonList<StatementSyntax>(
                                ExpressionStatement(
                                    AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName("_next"),
                                        IdentifierName("next")))))),
                        // public Task InvokeAsync(HttpContext context)
                        MethodDeclaration(IdentifierName("Task"), Identifier("InvokeAsync"))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithParameterList(ParameterList(SingletonSeparatedList(
                                Parameter(Identifier("context")).WithType(IdentifierName("HttpContext")))))
                            .WithBody(Block(SingletonList<StatementSyntax>(
                                ReturnStatement(
                                    InvocationExpression(IdentifierName("_next"))
                                        .WithArgumentList(ArgumentList(SingletonSeparatedList(
                                            Argument(IdentifierName("context")))))))))})),
                // internal static class Middleware1Extensions
                ClassDeclaration($"{middlewareClassName}Extensions")
                    .WithModifiers(TokenList(new [] {
                        Token(SyntaxKind.InternalKeyword),
                        Token(SyntaxKind.StaticKeyword)}))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                        // public static IApplicationBuilder UseMiddleware2(this IApplicationBuilder builder)
                        MethodDeclaration(IdentifierName("IApplicationBuilder"), Identifier($"Use{middlewareClassName}"))
                            .WithModifiers(TokenList(new [] {
                                Token(SyntaxKind.PublicKeyword),
                                Token(SyntaxKind.StaticKeyword)}))
                            .WithParameterList(
                                ParameterList(SingletonSeparatedList(
                                    Parameter(Identifier("builder"))
                                        .WithModifiers(TokenList(Token(SyntaxKind.ThisKeyword)))
                                        .WithType(IdentifierName("IApplicationBuilder")))))
                            .WithBody(Block(SingletonList<StatementSyntax>(
                                // return builder.UseMiddleware<Middleware1>();
                                ReturnStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("builder"),
                                            GenericName(Identifier("UseMiddleware"))
                                                .WithTypeArgumentList(TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(IdentifierName(middlewareClassName))))))))))))
            });

            var newRoot = root.ReplaceNode(compilationUnit, compilationUnit.WithMembers(middlewareClassDeclarations));
            newRoot = newRoot.ReplaceNode(invocationExpression, registerMiddlewareInvocation);

            return document.WithSyntaxRoot(newRoot);
        }

        return document;
    }
}