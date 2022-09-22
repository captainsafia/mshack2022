using Microsoft.JSInterop;

namespace BlazorSample;

public static class JSInvokableMethods
{
    // Make this method non-public to trigger an analyzer diagnostic and code fix.
    [JSInvokable]
    public static void HelloWorld()
    {
        Console.WriteLine("Hello, world!");
    }
}
