using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSample.Shared.Processors
{
    public interface IValueProcessor
    {
        Task<string[]> GetValues();
        Task<string> GetValue(int id);
        Task AddValue(string value);
        Task UpdateValue(int id, string value);
        Task RemoveValue(int id);
    }
}
