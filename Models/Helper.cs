using System.Net.Mail;
using System.Net;
using SharpCompress.Common;
using MongoDB.Driver;

namespace server.Models
{
    public  class Helper
    {
  

        public static async Task SendEmailAsync(string toEmail, string subject, string body, IWebHostEnvironment? env=null, string? url = null)
        {
            try
            {
                var fromAddress = new MailAddress(Environment.GetEnvironmentVariable("mail"), "עבודה ביעל");
                var toAddress = new MailAddress(toEmail);
                string fromPassword = Environment.GetEnvironmentVariable("pass_mail"); // **וודאי שאת משתמשת בסיסמת אפליקציה**

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress))
                {
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    if (env!=null)
                    {
                        Uri uri = new Uri(url);
                        string relativePath = uri.AbsolutePath.TrimStart('/');


                        string filePath = Path.Combine(env.WebRootPath, relativePath);



                        // הוספת קובץ מצורף אם קיים
                        if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
                        {
                            message.Attachments.Add(new Attachment(filePath));
                        }
                    }
                   

                    await smtp.SendMailAsync(message);
                    Console.WriteLine("✅ המייל נשלח בהצלחה!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ שגיאה בשליחת המייל: {ex.Message}");
            }
        }
    }

}

