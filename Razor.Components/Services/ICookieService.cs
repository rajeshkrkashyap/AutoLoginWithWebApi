using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
namespace Razor.Components.Services
{
    public interface ICookieService
    {
        string AuthToken { get; }
    }

    public class CookieService : ICookieService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CookieService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string AuthToken
        {
            get
            {
                HttpContext httpContext = _httpContextAccessor.HttpContext;
                return httpContext.Request.Cookies["AuthToken"];
            }
        }
    }
}
