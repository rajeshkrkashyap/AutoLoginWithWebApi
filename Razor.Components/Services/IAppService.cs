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
    public interface IAppService
    {
        Task<bool> RefreshToken();
        public Task<AuthUserResponse> AuthenticateUser(LoginViewModel loginModel);
        Task<(bool IsSuccess, string ErrorMessage)> RegisterUser(RegisterViewModel registerUser);
        //Task<List<StudentModel>> GetAllStudents();
    }
}
