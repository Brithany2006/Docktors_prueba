using System;

namespace LaboratorioFront.Services;

/// <summary>
/// Textos de presentacion. Solo cambian lo que se ve en pantalla; los datos guardados
/// y los identificadores internos no se modifican.
/// </summary>
public static class Etiquetas
{
    /// <summary>Muestra "EKGMILAN" donde el dato guardado dice "electromilan".</summary>
    public static string Doctor(string? texto)
    {
        if (string.IsNullOrEmpty(texto)) return texto ?? "";
        return texto.Replace("electromilan", "EKGMILAN", StringComparison.OrdinalIgnoreCase);
    }
}
