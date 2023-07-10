using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using System.Net;
namespace MockTestLab.Api.Services
{
    public class OneToManyMessageViewModel
    {
        public string From { get; set; }
        public List<string> To { get; set; }
        public string Body { get; set; }
    }
    public class OneToOneMessageViewModel
    {
        public string SenderId { get; set; }
        public string SessionId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Body { get; set; }
    }
    public interface IMessage
    {
        Task<bool> SendOneToOneSMS(OneToOneMessageViewModel oneToOneMessageViewModel);
        Task<bool> SendOneToManySMS(OneToManyMessageViewModel oneToManyMessageViewModel);
        Task<bool> VerifyOTP(string sessionId, int otp);
    }
    public class GridSMS : IMessage
    {
        public Task<bool> SendOneToOneSMS(OneToOneMessageViewModel oneToOneMessageViewModel)
        {
            var accountSid = "AC31065e9882fd970d6d0a0991a711d49b";
            var authToken = "ceab5afb3160599fcdcab5070c45e0a3";
            //TwilioClient.Init(accountSid, authToken);
            //var messageOptions = new CreateMessageOptions(
            //    new PhoneNumber("+91" + oneToOneMessageViewModel.To));
            //messageOptions.MessagingServiceSid = "MG00d10076af63739d51ad3938a66a25b0";
            //messageOptions.Body = oneToOneMessageViewModel.Body;

            //var message = MessageResource.Create(messageOptions);
            ////Console.WriteLine(message.Body);
            //if (message.Status == MessageResource.StatusEnum.Accepted)
            //{
            //    return new Task<bool>(() => true);
            //}
            //return new Task<bool>(() => false);


            try
            {
                // Find your Account SID and Auth Token at twilio.com/console
                // and set the environment variables. See http://twil.io/secure
                //string accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
                //string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

                TwilioClient.Init(accountSid, authToken);

                var message = MessageResource.Create(
                    body: "Join Earth's mightiest heroes. Like Kevin Bacon.",
                    from: new Twilio.Types.PhoneNumber("+15017122661"),
                    to: new Twilio.Types.PhoneNumber("+91" + oneToOneMessageViewModel.To)
                );

                Console.WriteLine(message.Sid);
                return new Task<bool>(() => true);
            }
            catch (Exception)
            {
                return new Task<bool>(() => false);
            }
        }
        public Task<bool> SendOneToManySMS(OneToManyMessageViewModel oneToManyMessageViewModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> VerifyOTP(string sessionId, int otp)
        {
            throw new NotImplementedException();
        }
    }
    public class MtalkzSMS : IMessage
    {
        static HttpClient client = new HttpClient();
        public async Task<bool> SendOneToOneSMS(OneToOneMessageViewModel oneToOneMessage)
        {
            string SMSBaseUrl = System.Configuration.ConfigurationManager.AppSettings["SMSBaseUrl"];
            string SMSApiKey = System.Configuration.ConfigurationManager.AppSettings["SMSApiKey"];
            string apiUrl = SMSBaseUrl + "?apikey=" + SMSApiKey + "&senderid=" + oneToOneMessage.SenderId
                    + "&number=" + oneToOneMessage.To
                    + "&message=" + oneToOneMessage.Body
                    + "&format = json";
            try
            {
                // Update port # in the following line.
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                // Get request the SMS
                var mobileNo = await GetSendOTPAsync(apiUrl);
                if (mobileNo != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public Task<bool> SendOneToManySMS(OneToManyMessageViewModel oneToManyMessageViewModel)
        {
            throw new NotImplementedException();
        }
        static async Task<Uri> PostMobileNoAsync(string mobileNo)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(
                "api/products", mobileNo);
            response.EnsureSuccessStatusCode();

            // return URI of the created resource.
            return response.Headers.Location;
        }
        static async Task<string> GetSendOTPAsync(string path)
        {
            string mobileNo = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                mobileNo = await response.Content.ReadAsStringAsync();
            }
            return mobileNo;
        }
        static async Task<string> GetVerifyOTPAsync(string path)
        {
            string mobileNo = null;
            HttpResponseMessage response = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                mobileNo = await response.Content.ReadAsStringAsync();
            }
            return mobileNo;
        }
        public async Task<bool> VerifyOTP(string sessionId, int otp)
        {
            string SMSBaseUrl = System.Configuration.ConfigurationManager.AppSettings["SMSBaseUrl"];
            string SMSApiKey = System.Configuration.ConfigurationManager.AppSettings["SMSApiKey"];

            string SMSSessionId = sessionId;

            try
            {
                string apiUrl = SMSBaseUrl + "?apikey=" + SMSApiKey + "&sessionId=" + SMSSessionId + "&otp=" + otp;
                // Update port # in the following line.
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                //var url = await PostMobileNoAsync(oneToOneMessageViewModel.To);

                // Get request the SMS
                var mobileNo = await GetVerifyOTPAsync(apiUrl);
                if (mobileNo != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
