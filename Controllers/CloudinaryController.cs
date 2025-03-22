using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

[Route("api/cloudinary")]
[ApiController]
public class CloudinaryController : ControllerBase
{
    private readonly Cloudinary _cloudinary;
    private readonly string apiSecret;

    public CloudinaryController()
    {
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

        var safeEmailName = email.Split('@')[0]; // Use part before @
        var publicId = $"resumes/{safeEmailName}"; // Save in 'resumes' folder

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            PublicId = publicId,
            Overwrite = true
        };


        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
        {
            return Ok(new
            {
                message = "✅ קובץ נשמר ב-Cloudinary בהצלחה!",
                url = uploadResult.SecureUrl.ToString()
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
