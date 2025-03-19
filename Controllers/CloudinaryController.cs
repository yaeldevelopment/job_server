using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using static WebApplication14.Controllers.employeesController;

[Route("api/cloudinary")]
[ApiController]
public class CloudinaryController : ControllerBase
{
    private readonly string cloudName;
    private readonly string apiKey;
    private readonly string apiSecret;

    public CloudinaryController()
    {
        cloudName = Environment.GetEnvironmentVariable("CloudName") ?? throw new ArgumentNullException("CloudName is missing");
        apiKey = Environment.GetEnvironmentVariable("ApiKey") ?? throw new ArgumentNullException("ApiKey is missing");
        apiSecret = Environment.GetEnvironmentVariable("ApiSecret") ?? throw new ArgumentNullException("ApiSecret is missing");
    }
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string email)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("קובץ לא תקין.");
        }

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("אימייל חסר.");
        }

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/resumes");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);

        }

        // יצירת שם קובץ חדש לפי המייל
        var fileExtension = Path.GetExtension(file.FileName);
        var safeEmailName = email.Split('@')[0]; // שימוש רק בשם לפני ה-@
        var fileName = $"{safeEmailName}{fileExtension}";

        var filePath = Path.Combine(uploadsFolder, fileName);

        // שמירה עם החלפת קובץ קיים
        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            await file.CopyToAsync(stream);

        }

        var fileUrl = $"assets/resumes/{fileName}";

        return Ok(new { message = "✅ קובץ נשמר בהצלחה!", path = fileUrl });
    }

    [HttpPost("generate-signature")]
    public IActionResult GenerateSignature([FromBody] SignatureRequest request)
    {
        if (string.IsNullOrEmpty(request.Folder))
        {
            return BadRequest(new { error = "Folder parameter is required" });
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string stringToSign = $"folder={request.Folder}&timestamp={timestamp}";

        // ✨ בדיקה לפני החתימה
        Console.WriteLine($"🔹 String to sign: {stringToSign}");
        Console.WriteLine($"🔹 ApiSecret: {apiSecret}");

        string signature = GenerateSHA1Signature(stringToSign, apiSecret);

        Console.WriteLine($"🔹 Signature generated: {signature}");

        return Ok(new { timestamp, signature, apiKey, cloudName, stringToSign });
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
