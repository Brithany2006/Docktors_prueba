using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace LaboratorioFront.Services;

/// <summary>
/// Si la API responde 401 a una peticion que llevaba token, la sesion ya no sirve (vencio, el usuario se desactivo o cambio la clave):
/// se borra el token y se vuelve al login con un aviso, en lugar de dejar la pantalla con errores sueltos.
/// </summary>
public class SesionExpiradaHandler : DelegatingHandler
{
	private readonly NavigationManager _nav;
	private readonly IJSRuntime _js;
	private bool _redirigiendo;

	public SesionExpiradaHandler(NavigationManager nav, IJSRuntime js)
	{
		_nav = nav;
		_js = js;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var respuesta = await base.SendAsync(request, cancellationToken);

		bool llevabaToken = request.Headers.Authorization != null;
		bool esLogin = request.RequestUri?.AbsolutePath.Contains("/auth/", StringComparison.OrdinalIgnoreCase) == true;
		bool enLogin = _nav.Uri.Contains("/login", StringComparison.OrdinalIgnoreCase);

		if (respuesta.StatusCode == HttpStatusCode.Unauthorized && llevabaToken && !esLogin && !enLogin && !_redirigiendo)
		{
			_redirigiendo = true;
			try { await _js.InvokeVoidAsync("laboratorio.removeToken"); } catch { }
			_nav.NavigateTo("/login?sesion=expirada", forceLoad: true);
		}

		return respuesta;
	}
}
