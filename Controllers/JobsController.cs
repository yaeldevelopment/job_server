using Microsoft.AspNetCore.Mvc;
using WebApplication14.Models;
using WebApplication14;
using MongoDB.Bson;
using server.Models;
using MongoDB.Driver;
using NewtonsoftJson = Newtonsoft.Json.JsonConvert;  // Alias ל-Newtonsoft.Json
namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : Controller
    {
   
        private readonly MongoDBManager<Jobs> _managQuery;
        public JobsController(IConfiguration configuration)
        {
            _managQuery = new MongoDBManager<Jobs>(configuration, "jobs");
       
        }


        [HttpGet]
        public ActionResult<List<Jobs>> Get()
        {
            var collection = _managQuery.GetCollectionByName<Jobs>();
            return collection.Find(_ => true).ToList();
        }


        //[HttpGet("getByMail/{mail}")]
        //public async Task<IActionResult> getByMail(string mail)
        //{
        //    try
        //    {
        //        var result = await _managQuery.QueryBymailAsync(mail);
        //        if (result == null)
        //        {
        //            return NotFound("Document not found");
        //        }

        //        // Return serializable object (e.g., a DTO or BsonDocument)
        //        return Ok(result.ToJson());
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}


        [HttpDelete("deleteById/{id}")]
        public async Task<IActionResult> DeleteById(string id)
        {
            try
            {
                var result = await _managQuery.DeleteByIdAsync(id);
                if (result == null)
                {
                    return NotFound("Document not found");
                }

                // המרת התוצאה לפורמט JSON תקני
                var jsonResult = NewtonsoftJson.SerializeObject(result);

                return Ok(jsonResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //[HttpPost("insertById")]
        //public async Task<IActionResult> Insert_row([FromBody] employees location)
        //{
        //    try
        //    {
        //        location.Id = null; // MongoDB will generate the Id automatically
        //        await _managQuery.InsertAsync(location);
        //        return CreatedAtAction(nameof(GetById), new { id = location.Id }, location);


        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
        [HttpPut("updateById")]
        public async Task<IActionResult> Update_row([FromBody] Jobs location)
        {
            try
            {


                await _managQuery.UpdateAsync(location.Id, location);
                // המרת התוצאה לפורמט JSON תקני


                return NoContent(); // Return 204 No Content to indicate success
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        public class FormData
        {
            public Jobs job { get; set; }
            public string mail { get; set; }
            public employees employee { get; set; }
        }
        [HttpPost("Send_Resum")]
        public async Task<IActionResult> Send_Resum([FromBody] FormData formData)
        {
            try
            {
                string body = $@"
                <html>
                <head>
                    <meta charset='UTF-8'>
                </head>
                <body style='direction: rtl; text-align: right; font-family: Arial, sans-serif;'>
                    <h2>שלום רב,</h2>
                    <p>פניית מועמד למשרת {formData.job.title}</b></p>
                    <p>תודה והמשך יום נעים,</b></p>
                </body>
                </html>";

                await Helper.SendEmailAsync(formData.mail, "פניית מועמד", body, formData.employee.resume);
                formData.job.employees_send.Add(formData.employee.Id);
                await _managQuery.UpdateFieldAsync_ById(formData.job.Id, "employees_send", formData.job.employees_send);
                body = $@"
                <html>
                <head>
                <meta charset='UTF-8'>
                </head>
                <body style='direction: rtl; text-align: right; font-family: Arial, sans-serif;'>
                <h2>שלום רב,</h2>
                <p>פנייתך למשרת {formData.job.title} התקבלה בהצלחה</b></p>
                <p>תודה והמשך יום נעים,</b></p>
                </body>
                </html>";
                await Helper.SendEmailAsync(formData.employee.mail, "פנייתך", body);

                return Ok();

             
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
     
    }
}
