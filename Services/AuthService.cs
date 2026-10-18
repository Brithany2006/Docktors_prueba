using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    public AuthService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    private const string TokenKey = "jwtToken";
    private const string RucKey = "userRuc";

    public async Task<bool> LoginAsync(string usuario, string clave)
    {
        var response = await _http.PostAsJsonAsync("auth/login", new { usuario, password = clave });

        if (!response.IsSuccessStatusCode)
            return false;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        if (json.TryGetProperty("token", out var tokenProp))
        {
            var token = tokenProp.GetString();
            if (!string.IsNullOrEmpty(token))
            {
                await _js.InvokeVoidAsync("sessionStorage.setItem", TokenKey, token);
                // opcional: extraer y guardar RUC si lo necesitas
                return true;
            }
        }

        return false;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _js.InvokeAsync<string>("sessionStorage.getItem", TokenKey);
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("sessionStorage.removeItem", TokenKey);
        await _js.InvokeVoidAsync("sessionStorage.removeItem", RucKey);
    }
} 
