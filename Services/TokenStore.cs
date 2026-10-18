using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace LaboratorioFront.Services;

public class TokenStore
{
	private readonly IJSRuntime _js;

	private const string Key = "jwt_token";

	public TokenStore(IJSRuntime js)
	{
		_js = js;
	}

	public ValueTask SetAsync(string? token)
	{
		if (string.IsNullOrWhiteSpace(token))
		{
			return _js.InvokeVoidAsync("sessionStorage.removeItem", "jwt_token");
		}
		return _js.InvokeVoidAsync("sessionStorage.setItem", "jwt_token", token);
	}

	public async Task<string?> GetAsync()
	{
		return await JSRuntimeExtensions.InvokeAsync<string>(_js,"sessionStorage.getItem", new object[1] { "jwt_token" });
	}

	public ValueTask ClearAsync()
	{
		return _js.InvokeVoidAsync("sessionStorage.removeItem", "jwt_token");
	}
}
