using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockTestLab.Shared;
using Razor.Components.Data;
using Razor.Components.Services;

namespace MockTestLab.Hybrid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<HttpContextAccessor>();

            builder.Services.AddScoped<AppState>();
            builder.Services.AddScoped<IAppService, AppService>();
            builder.Services.AddScoped<LoginViewModel>();

            return builder.Build();
        }
    }
}