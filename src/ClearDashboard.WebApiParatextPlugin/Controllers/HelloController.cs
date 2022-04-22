using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.WebApiParatextPlugin.Controllers
{

    public class HelloMessage
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Message { get; set; }
    }

    public class HelloMessageFactory
    {
        public HelloMessage Create()
        {
            return new HelloMessage { From = "ClearDashboard Web API plugin", To = "Dear calling process", Message = $"I'm alive! @ {DateTime.Now.ToString("G")}" };
        }
    }
    public class HelloController : ApiController
    {
        public HelloController(HelloMessageFactory factory, ILogger<HelloController> logger)
        {
            Factory = factory;
            Logger = logger;
        }

        public HelloMessageFactory Factory { get; }
        public ILogger<HelloController> Logger { get; }

        // GET api/hello 
        public HelloMessage Get()
        {
            Logger.LogInformation("HelloController.Get() called.");
            return Factory.Create();
        }
    }
}
