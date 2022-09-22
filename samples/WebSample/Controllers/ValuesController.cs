using Microsoft.AspNetCore.Mvc;
using WebSample.Shared.Processors;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IValueProcessor _processor;

        public ValuesController(IValueProcessor processor)
        {
            _processor = processor;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            return await _processor.GetValues();
        }

        // GET api/<ValuesController>/5
        //[HttpGet("{id}")]
        //public async Task<string> Get(int id)
        //{
        //    return await _processor.GetValue(id);
        //}

        //// POST api/<ValuesController>
        [HttpPost]
        public async Task Post([FromBody] string value)
        {
            await _processor.AddValue(value);
        }

        //// PUT api/<ValuesController>/5
        //[HttpPut("{id}")]
        //public async Task Put(int id, [FromBody] string value)
        //{
        //    await _processor.UpdateValue(id, value);
        //}

        //// DELETE api/<ValuesController>/5
        //[HttpDelete("{id}")]
        //public async Task Delete(int id)
        //{
        //    await _processor.RemoveValue(id);
        //}
    }
}
