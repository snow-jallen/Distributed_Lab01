using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shared;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(string key)
        {
            if(System.IO.File.Exists(key))
                return System.IO.File.ReadAllText(key);
            return null;
        }

        // POST api/values
        [HttpPost]
        public ActionResult<MessageDto> Post([FromBody]MessageDto msg)
        {
            System.IO.File.WriteAllText(msg.Key, msg.Value);
            msg.Result = $"Saved on server at {DateTime.Now}";
            return msg;
        }
    }  
}
