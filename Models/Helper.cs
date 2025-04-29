using System.Net.Mail;
using System.Net;
using SharpCompress.Common;
using MongoDB.Driver;
using SharpCompress.Crypto;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Engines;
using System.Security.Cryptography;

namespace server.Models
{
    public  class Helper
    {
        public static string Decrypt(string encryptedBase64, string key)
        {
            // המרת ה־Base64 בחזרה לבייטים
            byte[] cipherBytes = Convert.FromBase64String(encryptedBase64);
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);  // המפתח
            byte[] ivBytes = new byte[16]; // יצירת IV ריק בגודל 16 (לפי AES)

            // חשוב מאוד לשים לב לאורך המפתח ולהתאים אותו
            if (keyBytes.Length != 32) // AES-256 מצריך מפתח באורך 32 בתים
            {
                throw new Exception("The key must be 32 bytes long for AES-256 encryption.");
            }

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.IV = ivBytes;  // השתמש ב-IV ריק
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                {
                    // קרא את הטקסט המפוענח
                    return srDecrypt.ReadToEnd();
                }
            }
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

