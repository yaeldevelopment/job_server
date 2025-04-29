using System.Net.Mail;
using System.Net;
using SharpCompress.Common;
using MongoDB.Driver;
using SharpCompress.Crypto;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Engines;

namespace server.Models
{
    public  class Helper
    {
        public static string Decrypt(string encryptedBase64, string secretKey)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedBase64);
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);

            // דואגים שהמפתח יהיה באורך תקני - 16/24/32 בייטים
            keyBytes = ResizeKey(keyBytes, 32); // AES-256

            // הגדרת מנוע ההצפנה עם Padding
            BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new AesEngine());
            cipher.Init(false, new Org.BouncyCastle.Crypto.Parameters.KeyParameter(keyBytes));


            byte[] output = cipher.DoFinal(encryptedBytes);

            return Encoding.UTF8.GetString(output);
        }

        private static byte[] ResizeKey(byte[] key, int size)
        {
            byte[] resized = new byte[size];
            Array.Copy(key, resized, Math.Min(key.Length, size));
            return resized;
        }

        public static async Task SendEmailAsync(string toEmail, string subject, string body, string? url = null)
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




                    // הוספת קובץ מצורף אם קיים
                    if (url != null)
                    {
                        // מוסיפים fl_attachment כדי להוריד את הקובץ
                        string downloadUrl = $"{url}?fl_attachment&version={DateTime.UtcNow.Ticks}";

                        using (WebClient client = new WebClient())
                        {
                            byte[] fileBytes = client.DownloadData(downloadUrl);
                            var fileName = Path.GetFileName(new Uri(url).AbsolutePath); // מקבל את שם הקובץ

                            Attachment attachment = new Attachment(new MemoryStream(fileBytes), fileName);
                            message.Attachments.Add(attachment);
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

