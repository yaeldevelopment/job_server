using Microsoft.AspNetCore.Mvc;
using WebApplication14.Models;
using WebApplication14;
using MongoDB.Driver;
using server.Models;

namespace server.Controllers
{
 

        [ApiController]
        [Route("api/[controller]")]
        public class employerseController : Controller
        {

            private readonly MongoDBManager<employers> _managQuery;
            public employerseController(IConfiguration configuration)
            {
                _managQuery = new MongoDBManager<employers>(configuration, "employers");

            }
              [HttpGet]
            public ActionResult<List<employers>> Get()
            {
                var collection = _managQuery.GetCollectionByName<employers>();
                return collection.Find(_ => true).ToList();
            }
        }
}
