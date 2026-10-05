using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using LaboratorioFront.Services.Models;

namespace LaboratorioFront.Services;

public class ApiService
{
	public class PacientePdf
	{
		public int Id { get; set; }

		public string NombreCompleto { get; set; } = string.Empty;

		public string RUC_Empresa { get; set; } = string.Empty;

		public string NombreArchivoPDF { get; set; } = string.Empty;

		public string NombreArchivo { get; set; } = string.Empty;
	}

	public class EmpresaPacientesResponse
	{
		public string Ruc { get; set; } = string.Empty;

		public string? NombreEmpresa { get; set; }

		public int TotalPacientes { get; set; }

		public List<PacienteItem> Pacientes { get; set; } = new List<PacienteItem>();
	}

	public class PacienteItem
	{
		public int Id { get; set; }

		public string NombreCompleto { get; set; } = string.Empty;

		public string Normalizado { get; set; } = string.Empty;
	}

	public class OdooCitaDto
	{
		public int Id { get; set; }

		public string NumeroCita { get; set; } = string.Empty;

		public string Paciente { get; set; } = string.Empty;

		public string Cedula { get; set; } = string.Empty;

		public DateTime? FechaAtencion { get; set; }

		public List<OdooExamenDto> Examenes { get; set; } = new List<OdooExamenDto>();
	}

	public class OdooExamenDto
	{
		public int IdSubstudy { get; set; }

		public string TipoResultado { get; set; } = string.Empty;

		public string Nombre { get; set; } = string.Empty;

		public DateTime? FechaDocumento { get; set; }

		public bool TieneAdjunto1 { get; set; }

		public bool TieneAdjunto2 { get; set; }

		public string Adjunto1Nombre { get; set; } = string.Empty;

		public string Adjunto2Nombre { get; set; } = string.Empty;

		public bool TieneInformePdf { get; set; }

		public string Apreciacion { get; set; } = string.Empty;

		public string Comentario { get; set; } = string.Empty;

		public List<OdooExamenValorDto>? Valores { get; set; } = new List<OdooExamenValorDto>();
	}

	public class OdooExamenValorDto
	{
		public string Nombre { get; set; } = string.Empty;

		public string Valor { get; set; } = string.Empty;

		public string RangoNormal { get; set; } = string.Empty;

		public string Unidad { get; set; } = string.Empty;
	}

	private readonly HttpClient _http;

	private readonly TokenStore _tokens;

	private string? _token;

	public HttpClient Http => _http;

	public ApiService(HttpClient http, TokenStore tokens)
	{
		_http = http;
		_tokens = tokens;
		_http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		_http.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
		{
			NoCache = true,
			NoStore = true
		};
		_http.DefaultRequestHeaders.Pragma.ParseAdd("no-cache");
	}

	public async Task SetTokenAsync(string? token)
	{
		SetToken(token);
		await _tokens.SetAsync(token);
	}

	public void SetToken(string? token)
	{
		_token = token;
		if (!string.IsNullOrWhiteSpace(token))
		{
			_http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}
		else
		{
			_http.DefaultRequestHeaders.Authorization = null;
		}
	}

	public async Task LogoutAsync()
	{
		await _tokens.ClearAsync();
		SetToken(null);
	}

	public async Task<string?> LoginAsync(string usuario, string password)
	{
		_ = 2;
		try
		{
			HttpResponseMessage httpResponseMessage = await _http.PostAsJsonAsync("auth/login", new { usuario, password });
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return null;
			}
			if ((await httpResponseMessage.Content.ReadFromJsonAsync<JsonElement>()).TryGetProperty("token", out var value))
			{
				string token = value.GetString();
				await SetTokenAsync(token);
				return token;
			}
			return null;
		}
		catch
		{
			return null;
		}
	}

	public async Task<bool> ImportarPacientesAsync(MultipartFormDataContent content)
	{
		try
		{
			return (await _http.PostAsync("admin/pacientes/importar-excel", content)).IsSuccessStatusCode;
		}
		catch
		{
			return false;
		}
	}

	public async Task<List<PacienteEstudio>> GetRxInformesPorPacienteAsync(int idPaciente)
	{
		_ = 1;
		try
		{
			string requestUri = $"admin/pacientes/{idPaciente}/rx/informes";
			HttpResponseMessage httpResponseMessage = await _http.GetAsync(requestUri);
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return new List<PacienteEstudio>();
			}
			return JsonSerializer.Deserialize<List<PacienteEstudio>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}) ?? new List<PacienteEstudio>();
		}
		catch
		{
			return new List<PacienteEstudio>();
		}
	}

	public async Task<List<PacienteEstudio>> GetRxImagenesPorPacienteAsync(int idPaciente)
	{
		_ = 1;
		try
		{
			string requestUri = $"admin/pacientes/{idPaciente}/rx/imagenes";
			HttpResponseMessage httpResponseMessage = await _http.GetAsync(requestUri);
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return new List<PacienteEstudio>();
			}
			return JsonSerializer.Deserialize<List<PacienteEstudio>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}) ?? new List<PacienteEstudio>();
		}
		catch
		{
			return new List<PacienteEstudio>();
		}
	}

	public string BuildPacienteEstudioFileUrl(int estudioId)
	{
		Uri? baseUri = _http.BaseAddress ?? new Uri("https://doktors.med.ec/");
		string relativeUri = $"documentos/paciente-estudio/{estudioId}/file";
		UriBuilder uriBuilder = new UriBuilder(new Uri(baseUri, relativeUri));
		if (!string.IsNullOrWhiteSpace(_token))
		{
			string text = "access_token=" + Uri.EscapeDataString(_token);
			if (string.IsNullOrEmpty(uriBuilder.Query))
			{
				uriBuilder.Query = text;
			}
			else
			{
				uriBuilder.Query = uriBuilder.Query.TrimStart('?') + "&" + text;
			}
		}
		return uriBuilder.Uri.ToString();
	}

	public string BuildPacienteFileUrl(string rutaRelativa)
	{
		if (string.IsNullOrWhiteSpace(rutaRelativa))
		{
			return "#";
		}
		rutaRelativa = rutaRelativa.TrimStart('\\', '/');
		return new Uri(new Uri((_http.BaseAddress ?? new Uri("https://doktors.med.ec/api/")).GetLeftPart(UriPartial.Authority)), "pacientes-files/" + rutaRelativa).ToString();
	}

	public async Task<List<PacienteEstudio>> GetRxInformesPorNombreAsync(string nombreCompleto)
	{
		string requestUri = "documentos/paciente/" + Uri.EscapeDataString(nombreCompleto) + "/rx/informes";
		HttpResponseMessage httpResponseMessage = await Http.GetAsync(requestUri);
		if (!httpResponseMessage.IsSuccessStatusCode)
		{
			return new List<PacienteEstudio>();
		}
		return JsonSerializer.Deserialize<List<PacienteEstudio>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		}) ?? new List<PacienteEstudio>();
	}

	public async Task<List<PacienteEstudio>> GetRxImagenesPorNombreAsync(string nombreCompleto)
	{
		string requestUri = "documentos/paciente/" + Uri.EscapeDataString(nombreCompleto) + "/rx/imagenes";
		HttpResponseMessage httpResponseMessage = await Http.GetAsync(requestUri);
		if (!httpResponseMessage.IsSuccessStatusCode)
		{
			return new List<PacienteEstudio>();
		}
		return JsonSerializer.Deserialize<List<PacienteEstudio>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		}) ?? new List<PacienteEstudio>();
	}

	public async Task<string> GetPdfBase64PorNombreAsync(string nombreArchivo)
	{
		_ = 1;
		try
		{
			HttpResponseMessage httpResponseMessage = await _http.GetAsync("pacientes/ver-por-nombre/" + Uri.EscapeDataString(nombreArchivo));
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return string.Empty;
			}
			return Convert.ToBase64String(await httpResponseMessage.Content.ReadAsByteArrayAsync());
		}
		catch
		{
			return string.Empty;
		}
	}

	public async Task<string> GetPdfBase64PorApellidosAsync(string apellidosNombres)
	{
		_ = 1;
		try
		{
			HttpResponseMessage httpResponseMessage = await _http.GetAsync("pacientes/ver-por-paciente/" + Uri.EscapeDataString(apellidosNombres));
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return string.Empty;
			}
			return Convert.ToBase64String(await httpResponseMessage.Content.ReadAsByteArrayAsync());
		}
		catch
		{
			return string.Empty;
		}
	}

	public async Task<List<PacientePdf>> GetPacientesPorCarpetaAsync()
	{
		_ = 1;
		try
		{
			HttpResponseMessage httpResponseMessage = await _http.GetAsync("pacientes/listar");
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return new List<PacientePdf>();
			}
			return JsonSerializer.Deserialize<List<PacientePdf>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}) ?? new List<PacientePdf>();
		}
		catch
		{
			return new List<PacientePdf>();
		}
	}

	public async Task<List<OdooCitaDto>> GetMisResultadosExternosPacienteAsync(string ruc, string cedula)
	{
		_ = 1;
		try
		{
			string requestUri = "documentos/paciente/" + Uri.EscapeDataString(ruc) + "/resultados-externos?cedula=" + Uri.EscapeDataString(cedula);
			HttpResponseMessage httpResponseMessage = await _http.GetAsync(requestUri);
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return new List<OdooCitaDto>();
			}
			return JsonSerializer.Deserialize<List<OdooCitaDto>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}) ?? new List<OdooCitaDto>();
		}
		catch
		{
			return new List<OdooCitaDto>();
		}
	}

	public string BuildOdooPacienteEspecificoZipUrl(string ruc, string cedula)
	{
		Uri? baseUri = _http.BaseAddress ?? new Uri("https://doktors.med.ec/api/");
		string relativeUri = "documentos/paciente/" + Uri.EscapeDataString(ruc) + "/resultados-externos/zip?cedula=" + Uri.EscapeDataString(cedula);
		UriBuilder uriBuilder = new UriBuilder(new Uri(baseUri, relativeUri));
		if (!string.IsNullOrWhiteSpace(_token))
		{
			NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(uriBuilder.Query);
			nameValueCollection["access_token"] = _token;
			uriBuilder.Query = nameValueCollection.ToString();
		}
		return uriBuilder.ToString();
	}

	public async Task<List<OdooCitaDto>> GetMisResultadosExternosAsync()
	{
		_ = 1;
		try
		{
			HttpResponseMessage httpResponseMessage = await _http.GetAsync("documentos/empresa/mis-resultados-externos");
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return new List<OdooCitaDto>();
			}
			return (await httpResponseMessage.Content.ReadFromJsonAsync<List<OdooCitaDto>>()) ?? new List<OdooCitaDto>();
		}
		catch
		{
			return new List<OdooCitaDto>();
		}
	}

	public string BuildOdooArchivo2Url(int idSubstudy)
	{
		Uri? baseUri = _http.BaseAddress ?? new Uri("https://doktors.med.ec/api/");
		string relativeUri = $"documentos/paciente/resultados-externos/archivo2/{idSubstudy}";
		UriBuilder uriBuilder = new UriBuilder(new Uri(baseUri, relativeUri));
		if (!string.IsNullOrWhiteSpace(_token))
		{
			string text = "access_token=" + Uri.EscapeDataString(_token);
			uriBuilder.Query = (string.IsNullOrEmpty(uriBuilder.Query) ? text : (uriBuilder.Query.TrimStart('?') + "&" + text));
		}
		return uriBuilder.Uri.ToString();
	}

	public string BuildOdooInformePdfUrl(int idSubstudy)
	{
		Uri? baseUri = _http.BaseAddress ?? new Uri("https://doktors.med.ec/api/");
		string relativeUri = $"documentos/empresa/mis-resultados-externos/odoo-report/{idSubstudy}";
		UriBuilder uriBuilder = new UriBuilder(new Uri(baseUri, relativeUri));
		if (!string.IsNullOrWhiteSpace(_token))
		{
			string text = "access_token=" + Uri.EscapeDataString(_token);
			uriBuilder.Query = (string.IsNullOrEmpty(uriBuilder.Query) ? text : (uriBuilder.Query.TrimStart('?') + "&" + text));
		}
		return uriBuilder.Uri.ToString();
	}

	public string BuildOdooGenerarPdfUrl(int citaId, string tipoResultado)
	{
		return $"{_http.BaseAddress}documentos/empresa/mis-resultados-externos/cita/{citaId}/pdf/{Uri.EscapeDataString(tipoResultado)}?access_token={_token}";
	}

	public string BuildOdooArchivoUrl(int idSubstudy)
	{
		Uri? baseUri = _http.BaseAddress ?? new Uri("https://doktors.med.ec/api/");
		string relativeUri = $"documentos/paciente/resultados-externos/archivo/{idSubstudy}";
		UriBuilder uriBuilder = new UriBuilder(new Uri(baseUri, relativeUri));
		if (!string.IsNullOrWhiteSpace(_token))
		{
			string text = "access_token=" + Uri.EscapeDataString(_token);
			uriBuilder.Query = (string.IsNullOrEmpty(uriBuilder.Query) ? text : (uriBuilder.Query.TrimStart('?') + "&" + text));
		}
		return uriBuilder.Uri.ToString();
	}

	public string BuildOdooZipUrl()
	{
		Uri? baseUri = _http.BaseAddress ?? new Uri("https://doktors.med.ec/api/");
		string relativeUri = "documentos/empresa/mis-resultados-externos/zip";
		UriBuilder uriBuilder = new UriBuilder(new Uri(baseUri, relativeUri));
		if (!string.IsNullOrWhiteSpace(_token))
		{
			string text = "access_token=" + Uri.EscapeDataString(_token);
			uriBuilder.Query = (string.IsNullOrEmpty(uriBuilder.Query) ? text : (uriBuilder.Query.TrimStart('?') + "&" + text));
		}
		return uriBuilder.Uri.ToString();
	}

	public string BuildOdooPacienteZipUrl(string cedula)
	{
		Uri? baseUri = _http.BaseAddress ?? new Uri("https://doktors.med.ec/api/");
		string relativeUri = "documentos/empresa/mis-resultados-externos/zip/" + cedula;
		UriBuilder uriBuilder = new UriBuilder(new Uri(baseUri, relativeUri));
		if (!string.IsNullOrWhiteSpace(_token))
		{
			string text = "access_token=" + Uri.EscapeDataString(_token);
			uriBuilder.Query = (string.IsNullOrEmpty(uriBuilder.Query) ? text : (uriBuilder.Query.TrimStart('?') + "&" + text));
		}
		return uriBuilder.Uri.ToString();
	}

	public string BuildLegacyZipUrl(string nombreCompleto, string nombreArchivo)
	{
		Uri? baseUri = _http.BaseAddress ?? new Uri("https://doktors.med.ec/api/");
		string relativeUri = "documentos/legacy-zip";
		UriBuilder uriBuilder = new UriBuilder(new Uri(baseUri, relativeUri));
		List<string> list = new List<string>();
		if (!string.IsNullOrWhiteSpace(_token))
		{
			list.Add("access_token=" + Uri.EscapeDataString(_token));
		}
		if (!string.IsNullOrWhiteSpace(nombreCompleto))
		{
			list.Add("nombreCompleto=" + Uri.EscapeDataString(nombreCompleto));
		}
		if (!string.IsNullOrWhiteSpace(nombreArchivo))
		{
			list.Add("nombreArchivo=" + Uri.EscapeDataString(nombreArchivo));
		}
		string text = string.Join("&", list);
		uriBuilder.Query = (string.IsNullOrEmpty(uriBuilder.Query) ? text : (uriBuilder.Query.TrimStart('?') + "&" + text));
		return uriBuilder.Uri.ToString();
	}

	public async Task<List<PacientePdf>> GetPacientesPdfAsync()
	{
		_ = 1;
		try
		{
			HttpResponseMessage httpResponseMessage = await _http.GetAsync("documentos/pacientes");
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return new List<PacientePdf>();
			}
			return (await httpResponseMessage.Content.ReadFromJsonAsync<List<PacientePdf>>()) ?? new List<PacientePdf>();
		}
		catch
		{
			return new List<PacientePdf>();
		}
	}

	public async Task<List<EmpresaPacientesResponse>> GetEmpresasConPacientesAsync()
	{
		_ = 1;
		try
		{
			HttpResponseMessage httpResponseMessage = await _http.GetAsync("admin/empresas/pacientes");
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				return new List<EmpresaPacientesResponse>();
			}
			return JsonSerializer.Deserialize<List<EmpresaPacientesResponse>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}) ?? new List<EmpresaPacientesResponse>();
		}
		catch
		{
			return new List<EmpresaPacientesResponse>();
		}
	}

	public async Task<List<PacienteEstudio>> GetEcoInformesPorNombreAsync(string nombreCompleto)
	{
		string requestUri = "documentos/paciente/" + Uri.EscapeDataString(nombreCompleto) + "/eco/informes";
		HttpResponseMessage httpResponseMessage = await Http.GetAsync(requestUri);
		if (!httpResponseMessage.IsSuccessStatusCode)
		{
			return new List<PacienteEstudio>();
		}
		return JsonSerializer.Deserialize<List<PacienteEstudio>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		}) ?? new List<PacienteEstudio>();
	}

	public async Task<CrearPacienteResponse> CrearPacienteParticularAsync(CrearPacienteRequest request)
	{
		HttpResponseMessage httpResponseMessage = await _http.PostAsJsonAsync("api/caja/paciente-particular", request);
		if (!httpResponseMessage.IsSuccessStatusCode)
		{
			throw new Exception(await httpResponseMessage.Content.ReadAsStringAsync());
		}
		return (await httpResponseMessage.Content.ReadFromJsonAsync<CrearPacienteResponse>()) ?? throw new Exception("Respuesta invalida del servidor.");
	}

	public async Task<List<PacienteEstudio>> GetEcoImagenesPorNombreAsync(string nombreCompleto)
	{
		string requestUri = "documentos/paciente/" + Uri.EscapeDataString(nombreCompleto) + "/eco/imagenes";
		HttpResponseMessage httpResponseMessage = await Http.GetAsync(requestUri);
		if (!httpResponseMessage.IsSuccessStatusCode)
		{
			return new List<PacienteEstudio>();
		}
		return JsonSerializer.Deserialize<List<PacienteEstudio>>(await httpResponseMessage.Content.ReadAsStringAsync(), new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		}) ?? new List<PacienteEstudio>();
	}

	public async Task<string?> LoginPacienteAsync(string cedula, string password)
	{
		HttpResponseMessage httpResponseMessage = await _http.PostAsJsonAsync("Auth/login-paciente", new
		{
			usuario = cedula,
			password = password
		});
		if (!httpResponseMessage.IsSuccessStatusCode)
		{
			return null;
		}
		LoginResponse loginResponse = await httpResponseMessage.Content.ReadFromJsonAsync<LoginResponse>();
		if (loginResponse != null && loginResponse.token != null)
		{
			SetToken(loginResponse.token);
		}
		return loginResponse?.token;
	}

	public async Task<List<string>> GetCarouselImagesAsync()
	{
		HttpResponseMessage httpResponseMessage = await _http.GetAsync("admin/carousel");
		if (httpResponseMessage.IsSuccessStatusCode)
		{
			return (await httpResponseMessage.Content.ReadFromJsonAsync<List<string>>()) ?? new List<string>();
		}
		return new List<string>();
	}

	public async Task<string> UploadCarouselImageAsync(MultipartFormDataContent content)
	{
		HttpResponseMessage obj = await _http.PostAsync("admin/carousel", content);
		obj.EnsureSuccessStatusCode();
		return (await obj.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("url").GetString();
	}

	public async Task DeleteCarouselImageAsync(string filename)
	{
		await _http.DeleteAsync("admin/carousel?filename=" + Uri.EscapeDataString(filename));
	}

	public string GetBaseUrl()
	{
		return new Uri((_http.BaseAddress ?? new Uri("https://doktors.med.ec/api/")).GetLeftPart(UriPartial.Authority)).ToString().TrimEnd('/');
	}
}
