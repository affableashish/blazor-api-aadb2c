using BlazorServerClient.Data;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Extra stuff I added - Start
var initialScopes = builder.Configuration.GetSection("ProtectedWebAPI:Scopes").Get<string[]>()!;
// Extra stuff I added - End

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAdB2C"))
    // Extra stuff I added - Start.
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    //https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-web-app-call-api-call-api?tabs=aspnetcore#option-2-call-a-downstream-web-api-with-the-helper-class
    .AddDownstreamApi("MyProtectedWebAPI", builder.Configuration.GetSection("ProtectedWebAPI"))
    .AddInMemoryTokenCaches();
    // Extra stuff I added - End

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    // Comment out this line if you want to not go to the login page when the app launches.
    //options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();

// Extra stuff I added - Start
builder.Services.Configure<OpenIdConnectOptions>(builder.Configuration.GetSection("AzureAdB2C"));

builder.Services.AddScoped<WeatherForecastService>();

// Configure the HttpClient for the forecast service
var protectedApiUrl = builder.Configuration["ProtectedWebAPI:BaseUrl"]!;
// I went with IDownstreamApi _downstreamApi, so don't really need this to call the backend API
builder.Services.AddHttpClient<WeatherForecastService>(client =>
{
    client.BaseAddress = new Uri(protectedApiUrl);
});
// Extra stuff I added - End

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

// Extra stuff I added - Start
//app.UseAuthentication();
// Extra stuff I added - End
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();