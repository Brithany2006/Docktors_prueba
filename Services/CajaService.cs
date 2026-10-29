using System.Net.Http.Json;

public class CajaService
{
    private readonly HttpClient _http;

    public CajaService(HttpClient http)
    {
        _http = http;
    }

    public async Task<CrearPacienteResponse?> CrearPaciente(
        CrearPacienteRequest request)
    {
        var response = await _http.PostAsJsonAsync(
            "api/caja/paciente-particular", request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content
            .ReadFromJsonAsync<CrearPacienteResponse>();
    }
}
