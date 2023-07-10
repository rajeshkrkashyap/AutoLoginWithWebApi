using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Razor.Components.Data;
using Razor.Components.Services;
using Razor.Components;
using MockTestLab.Shared;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<HttpContextAccessor>();

builder.Services.AddMvc().AddNewtonsoftJson();

builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<IAppService, AppService>();
builder.Services.AddScoped<LoginViewModel>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();
