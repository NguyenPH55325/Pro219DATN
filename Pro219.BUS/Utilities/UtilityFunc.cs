using System.Security.Cryptography;
using System.Text;
using Mailjet.Client;
using Mailjet.Client.Resources;
using System;
using Newtonsoft.Json.Linq;

namespace Pro219.API.Utilities
{
    public class UtilityFunc
    {
        public string HashPassword(string password)
        {
            //4297F44B13955235245B2497399D7A93 
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(password);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            md5.Clear();
            return sb.ToString();

        }

        public async Task<bool> SendEmailToAddress(string emailAddress,string nameRecivecer, string subject, string body, string bodyHTML)
        {
            MailjetClient client = new MailjetClient("d6c5e816cacd0645137110f9f3401997", "6a39d9d6578839fd30be7993fc049d4b");
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
               .Property(Send.FromEmail, "hlk9@proton.me")
               .Property(Send.FromName, "Adam Store")
               .Property(Send.Subject, subject)
               .Property(Send.TextPart, body)
               .Property(Send.HtmlPart, bodyHTML)
               .Property(Send.Recipients, new JArray {
                new JObject {
                 {"Email",emailAddress}
                 }
                   });
            MailjetResponse response = await client.PostAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(string.Format("Total: {0}, Count: {1}\n", response.GetTotal(), response.GetCount()));
                Console.WriteLine(response.GetData());
                return true;
            }
            else
            {
                Console.WriteLine(string.Format("StatusCode: {0}\n", response.StatusCode));
                Console.WriteLine(string.Format("ErrorInfo: {0}\n", response.GetErrorInfo()));
                Console.WriteLine(string.Format("ErrorMessage: {0}\n", response.GetErrorMessage()));
                return false;
            }
        }

        public string GenerateRandomString(int count)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, count)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
