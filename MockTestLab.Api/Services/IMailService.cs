using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MockTestLab.Api.Services
{
    public interface IMailService
    {

        Task SendEmailAsync(string toEmail, string subject, string content);
    }

    public class SendGridMailService : IMailService
    {
        private IConfiguration _configuration;

        public SendGridMailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string content)
        {
            //var apiKey = _configuration["SendGridAPIKey"];
            //var client = new SendGridClient(apiKey);
            //var from = new EmailAddress("test@authdemo.com", "Hi");
            //var to = new EmailAddress(toEmail);
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
            //var response = await client.SendEmailAsync(msg);

            SendEmailForVarification(toEmail, subject, content);
        }

        private async Task SendEmaiVaiMailChimp(string toEmail, string subject, string content)
        {
            string apiKey = "bc14a214caad2048c38e5562bb0af073-us14";
            string ansotherApiKey = "7de533ca973c0e6ddec6dc2f26e6649232762f02"; https://app.sparkpost.com/account/api-keys

            string serverPrefix = "YOUR_SERVER_PREFIX";
            string listId = "3e464119b4";
            string recipientEmail = "rajesh.kr.kashyap@gmail.com";
            string senderEmail = "ConnectTo2023@gmail.com";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri($"https://{serverPrefix}.api.mailchimp.com/3.0/");

            // Prepare the request
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"lists/{listId}/members");
            request.Headers.Add("Authorization", $"apikey {apiKey}");

            // Build the request body
            var requestBody = new
            {
                email_address = recipientEmail,
                status = "pending",
                merge_fields = new
                {
                    FNAME = "Recipient",
                    LNAME = "Lastname"
                }
            };

            request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            // Send the request
            HttpResponseMessage response = await client.SendAsync(request);

            // Check the response status
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Verification email sent successfully!");
            }
            else
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to send verification email. Error: {errorMessage}");
            }

            client.Dispose();
        }

        private async Task SendEmailForVarification(string toEmail, string subject, string body)
        {

            string smtpHost = "smtp.sparkpostmail.com\r\n";
            int smtpPort = 587; // Update with the appropriate SMTP port
            string smtpUsername = "SMTP_Injection";
            string smtpPassword = "7de533ca973c0e6ddec6dc2f26e6649232762f02"; //ApiKey As a Password
            string senderEmail = "ConnectTo@mamtastore.com";
            string recipientEmail = toEmail;//"Rajesh.kr.kashyap@gmail.com";
            //subject = "Test Email";
            //body = "This is a test email.";

            try
            {
                // Create a new MailMessage
                MailMessage mail = new MailMessage(senderEmail, recipientEmail, subject, body);

                // Create an instance of the SmtpClient
                SmtpClient smtpClient = new SmtpClient(smtpHost, smtpPort);
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                // Send the email
                await smtpClient.SendMailAsync(mail);

                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email. Error: {ex.Message}");
            }
        }
    }
}
