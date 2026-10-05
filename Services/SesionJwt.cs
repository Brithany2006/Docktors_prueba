using System;
using System.Linq;
using System.Text.Json;

namespace LaboratorioFront.Services;

/// <summary>
/// Lectura del JWT guardado en el navegador para decidir si una pagina puede mostrarse.
/// Es solo una ayuda de interfaz: la autorizacion real la hace la API en cada endpoint.
/// </summary>
public static class SesionJwt
{
    public record Datos(bool Valida, string Rol, string Ruc);

    public static Datos Leer(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return new Datos(false, "", "");

        try
        {
            var partes = token.Split('.');
            if (partes.Length < 2) return new Datos(false, "", "");

            var base64 = partes[1].Replace('-', '+').Replace('_', '/');
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            using var doc = JsonDocument.Parse(Convert.FromBase64String(base64));
            var raiz = doc.RootElement;

            if (raiz.TryGetProperty("exp", out var exp) && exp.TryGetInt64(out var segundos)
                && DateTimeOffset.FromUnixTimeSeconds(segundos) <= DateTimeOffset.UtcNow)
            {
                return new Datos(false, "", "");
            }

            return new Datos(true, Texto(raiz, "role"), Texto(raiz, "RUC").ToLowerInvariant());
        }
        catch
        {
            return new Datos(false, "", "");
        }
    }

    private static string Texto(JsonElement raiz, string nombre)
    {
        var prop = raiz.EnumerateObject()
            .FirstOrDefault(p => p.Name.Equals(nombre, StringComparison.OrdinalIgnoreCase));
        if (prop.Value.ValueKind == JsonValueKind.String) return prop.Value.GetString() ?? "";
        if (prop.Value.ValueKind == JsonValueKind.Array)
            return prop.Value.EnumerateArray().Select(e => e.GetString()).FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? "";
        return "";
    }
}
