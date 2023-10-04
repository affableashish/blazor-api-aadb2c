using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;

namespace BlazorServerClient.Data;

public class WeatherForecastService
{
    private readonly IAuthorizationHeaderProvider _authorizationHeaderProvider;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IDownstreamApi _downstreamApi;
    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
    
    private const string ServiceName = "MyProtectedWebAPI"; // Name coming from: .AddDownstreamApi("MyProtectedWebAPI", builder.Configuration.GetSection("ProtectedWebAPI"))

    public WeatherForecastService(HttpClient httpClient,
        IAuthorizationHeaderProvider authorizationHeaderProvider,
        IDownstreamApi downstreamApi,
        IConfiguration configuration,
        MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
    {
        _httpClient = httpClient;
        _authorizationHeaderProvider = authorizationHeaderProvider;
        _downstreamApi = downstreamApi;
        _consentHandler = consentHandler;
        _configuration = configuration;
    }
    
    public async Task<IEnumerable<WeatherForecast>?> GetForecastAsync()
    {
        try
        {
            // The base url of the downstream API is already setup in Program.cs in this line: .AddDownstreamApi("MyProtectedWebAPI", builder.Configuration.GetSection("ProtectedWebAPI"))
            // So relative path just shows the path after that base url
            var result = await _downstreamApi.GetForUserAsync<IEnumerable<WeatherForecast>>(ServiceName,
                options => options.RelativePath = "/weather");
            
            return result;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            _consentHandler.HandleException(ex);
        }

        // If you end up here, you get nothing.
        return null;
    }
    
    // Alternate way, but I like _downstreamApi way better, so not using it.
    // When our application tries to acquire a token to call the web API and it fails (for example, if the user hasn’t
    // granted consent for these scopes), this attribute will handle the exception and challenge the user to provide consent.
    // Just needs to be used once, especially on the first GET so that you'll be good to go for calls that follow.
    [AuthorizeForScopes(ScopeKeySection = "ProtectedWebAPI:Scopes")]
    public async Task<IEnumerable<WeatherForecast>?> GetForecastAnotherWayAsync()
    {
        try
        {
            await PrepareAuthenticatedClient();
            var response = await _httpClient.GetAsync("/weather");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}.");
            }
            
            var forecasts = await response.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>();
            return forecasts;
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            _consentHandler.HandleException(ex);
        }

        // If you end up here, you get nothing.
        return Enumerable.Empty<WeatherForecast>();
    }
    
    private async Task PrepareAuthenticatedClient()
    {
        // This app requests the scopes needed to do a specific thing with the Authorization server.
        var scopes = _configuration.GetSection("ProtectedWebAPI:Scopes").Get<string[]>()!;
        var accessToken = await _authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(scopes);
        _httpClient.DefaultRequestHeaders.Add("Authorization", accessToken);
        
        //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        //_httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
}