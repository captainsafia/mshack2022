using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSample.Shared.Processors
{
    public class ValueProcessor : IValueProcessor
    {
        public Task AddValue(string value)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetValue(int id)
        {
            return Task.FromResult("value");
        }

        public Task<string[]> GetValues()
        {
            return Task.FromResult(new string[] { "value1", "value2" });
        }

        public Task RemoveValue(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateValue(int id, string value)
        {
            throw new NotImplementedException();
        }
    }
}
