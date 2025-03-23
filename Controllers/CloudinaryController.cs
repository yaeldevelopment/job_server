using Amazon.Runtime.Internal;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using WebApplication14.Models;
using WebApplication14;

[Route("api/cloudinary")]
[ApiController]
public class CloudinaryController : ControllerBase
{
    private readonly Cloudinary _cloudinary;
    private readonly string apiSecret;
    private readonly MongoDBManager<employees> _managQuery;


    public CloudinaryController(IConfiguration configuration)
    {    _managQuery = new MongoDBManager<employees>(configuration, "employees");

        var cloudName = Environment.GetEnvironmentVariable("CloudName") ?? throw new ArgumentNullException("CloudName is missing");
        var apiKey = Environment.GetEnvironmentVariable("ApiKey") ?? throw new ArgumentNullException("ApiKey is missing");
        apiSecret = Environment.GetEnvironmentVariable("ApiSecret") ?? throw new ArgumentNullException("ApiSecret is missing");

        Account account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        this.apiSecret = apiSecret;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadToCloudinary(IFormFile file, [FromQuery] string email)
    {
        if (file == null || file.Length == 0)
            return BadRequest("קובץ לא תקין.");

        if (string.IsNullOrEmpty(email))
            return BadRequest("אימייל חסר.");

        var safeEmailName = email.Split('@')[0];
        var publicId = $"resumes/{safeEmailName}";

        // בדיקה אם קיים קובץ קיים
        var getResourceResult = _cloudinary.GetResource(new GetResourceParams(publicId));
        bool exists = getResourceResult.StatusCode == System.Net.HttpStatusCode.OK;

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            PublicId = publicId,
            Overwrite = true
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var user = await _managQuery.QueryBymailOnlyAsync(email);
            if (user != null)
            { 
                await _managQuery.UpdateFieldAsync_ById(user.Id, "resume", uploadResult.SecureUrl.ToString()); 
              
            }
            
            uploadResult.SecureUrl.ToString();
            var message = exists ? "✅ קובץ עודכן בהצלחה!" : "✅ קובץ חדש נשמר בהצלחה!";
            return Ok(new
            {
                message,
                path = uploadResult.SecureUrl.ToString()
            });
        }
        else
        {
            return StatusCode((int)uploadResult.StatusCode, new { error = "העלאה נכשלה" });
        }
    }

    [HttpPost("generate-signature")]
    public IActionResult GenerateSignature([FromBody] SignatureRequest request)
    {
        if (string.IsNullOrEmpty(request.Folder))
            return BadRequest(new { error = "Folder parameter is required" });

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string stringToSign = $"folder={request.Folder}&timestamp={timestamp}";

        string signature = GenerateSHA1Signature(stringToSign, apiSecret);

        return Ok(new { timestamp, signature });
    }

    private static string GenerateSHA1Signature(string data, string key)
    {
        using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key)))
        {
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}

public class SignatureRequest
{
    public string Folder { get; set; }
}
