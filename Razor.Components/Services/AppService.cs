using MockTestLab.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Razor.Components.Services
{
    public class AppService : IAppService
    {
        public async Task<AuthUserResponse> AuthenticateUser(LoginViewModel loginModel)
        {
            var returnResponse = new AuthUserResponse();
            using (var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.Login}";

                var serializedStr = JsonConvert.SerializeObject(loginModel);

                var response = await client.PostAsync(url, new StringContent(serializedStr, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string contentStr = await response.Content.ReadAsStringAsync();
                    returnResponse = JsonConvert.DeserializeObject<AuthUserResponse>(contentStr);
                }
            }
            return returnResponse;
        }

        //public async Task<List<StudentModel>> GetAllStudents()
        //{
        //    var returnResponse = new List<StudentModel>();
        //    using (var client = new HttpClient())
        //    {
        //        var url = $"{Setting.BaseUrl}{APIs.GetAllStudents}";

        //        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Setting.UserDetail?.AccessToken}");
        //        var response = await client.GetAsync(url);

        //        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        //        {
        //            bool isTokenRefreshed = await RefreshToken();
        //            if (isTokenRefreshed) return await GetAllStudents();
        //        }
        //        else
        //        {
        //            if (response.IsSuccessStatusCode)
        //            {
        //                string contentStr = await response.Content.ReadAsStringAsync();
        //                var UserManagerResponse = JsonConvert.DeserializeObject<UserManagerResponse>(contentStr);
        //                if (UserManagerResponse.IsSuccess)
        //                {
        //                    returnResponse = JsonConvert.DeserializeObject<List<StudentModel>>(UserManagerResponse.Content.ToString());
        //                }
        //            }
        //        }

        //    }
        //    return returnResponse;
        //}

        public async Task<bool> RefreshToken()
        {
            bool isTokenRefreshed = false;
            using (var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.RefreshToken}";

                var serializedStr = JsonConvert.SerializeObject(new AuthenticationResponse
                {
                    RefreshToken = Setting.UserDetail.RefreshToken,
                    AccessToken = Setting.UserDetail.AccessToken
                });

                try
                {
                    var response = await client.PostAsync(url, new StringContent(serializedStr, Encoding.UTF8, "application/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        string contentStr = await response.Content.ReadAsStringAsync();
                        var userManagerResponse = JsonConvert.DeserializeObject<AuthUserResponse>(contentStr);
                        if (userManagerResponse!=null && userManagerResponse.IsSuccess)
                        {
                            var tokenDetails = JsonConvert.DeserializeObject<AuthenticationResponse>(userManagerResponse.Message);
                            
                            Setting.UserDetail.AccessToken = tokenDetails.AccessToken;
                            Setting.UserDetail.RefreshToken = tokenDetails.RefreshToken;

                            string userDetailsStr = JsonConvert.SerializeObject(Setting.UserDetail);
                           
                            //await SecureStorage.SetAsync(nameof(Setting.UserDetail), userDetailsStr);
                            isTokenRefreshed = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }


            }
            return isTokenRefreshed;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> RegisterUser(RegisterViewModel registerUser)
        {
            string errorMessage = string.Empty;
            bool isSuccess = false;
            using (var client = new HttpClient())
            {
                var url = $"{Setting.BaseUrl}{APIs.Register}";

                var serializedStr = JsonConvert.SerializeObject(registerUser);
                var response = await client.PostAsync(url, new StringContent(serializedStr, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    isSuccess = true;
                }
                else
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }
            }
            return (isSuccess, errorMessage);
        }
    }
}
