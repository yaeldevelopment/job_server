using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using WebApplication14.Models;

using NewtonsoftJson = Newtonsoft.Json.JsonConvert;  // Alias ל-Newtonsoft.Json
using MongoDBJson = MongoDB.Bson.IO.JsonConvert;
using Microsoft.AspNetCore.Identity.Data;
using Hanssens.Net;
using System.Net.Mail;
using System.Net;
using server.Models;
namespace WebApplication14.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class employeesController : Controller
    {
       
        private readonly MongoDBManager<employees> _managQuery;
        public employeesController(IConfiguration configuration)
        {
            _managQuery = new MongoDBManager<employees>(configuration, "employees");
           
        }
        
       
         [HttpGet]
        public ActionResult<List<employees>> Get()
        {
            var collection = _managQuery.GetCollectionByName<employees>();
            return collection.Find(_ => true).ToList();
        }

       public class employee_login
        {
            public string mail { get; set; }
            public string password { get; set; }
        }
        [HttpPost("getByEmployee")]
        public async Task<IActionResult> Get(employee_login employee_Login)
        {
            try
            {
                var collection = await _managQuery.QueryBymailAsync(employee_Login.mail, employee_Login.password);

                if (collection != null)
                {
                    return Ok(collection); // התחברות מוצלחת
                }

                var employee = await _managQuery.QueryBymailOnlyAsync(employee_Login.mail);
                if (employee != null)
                {
                    return Unauthorized("Incorrect password."); // במקום להחזיר Ok עם המייל, מחזירים 401
                }

                return NotFound("Employee not found."); // אם המשתמש לא נמצא כלל
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("getByEmployee_id")]
        public async Task<IActionResult> Get(string mail)
        {
            try
            {
                var collection = await _managQuery.QueryBymailOnlyAsync(mail);

                if (collection != null)
                {
                    return Ok(collection);
                }
                else
                {
                    return Ok(null);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
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
        [HttpPost("insertById")]
        public async Task<IActionResult> Insert_row([FromBody] employees employees)
        {
            try
            {
                employees.Id = null; // MongoDB will generate the Id automatically
                await _managQuery.InsertAsync(employees);
                var collection = await _managQuery.QueryBymailOnlyAsync(employees.mail);
                return Ok(collection); // התחברות מוצלחת

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        public class ForgotPasswordRequest
        {
            public string Mail { get; set; }
            public string Password { get; set; }
        }
        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {

                if (string.IsNullOrEmpty(request.Mail) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest("Mail and Password are required.");
                }


               
                string body = $@"
<html>
<head>
    <meta charset='UTF-8'>
</head>
<body style='direction: rtl; text-align: right; font-family: Arial, sans-serif;'>
    <h2>שלום,</h2>
    <p>הקוד שלך לאימות שינוי הסיסמה הוא: <b>{request.Password}</b></p>
</body>
</html>";


                await Helper.SendEmailAsync(request.Mail, "אימות שינוי סיסמה",
                   body);

              
                return Ok(new { message = "קוד אימות נשלח למייל." }); // ✔️ מחזירים JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאה בשרת: {ex.Message}");
            }
        }


        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] Reset_Pass request)
        {
            try
            {
                var user = await _managQuery.QueryBymailOnlyAsync(request.Email);
                if (user == null)
                {
                    return NotFound("משתמש לא נמצא.");
                }
                user.password = request.NewPassword;
                // עדכון סיסמה במסד הנתונים
                await _managQuery.UpdateAsync(request.Email, user);

                return Ok(new { user = user }); // ✔️ מחזירים JSON
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"שגיאה בשרת: {ex.Message}");
            }
        }

        [HttpPut("updateById")]
        public async Task<IActionResult> Update_row([FromBody] employees location)
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
    }
}
