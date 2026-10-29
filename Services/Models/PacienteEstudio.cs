namespace LaboratorioFront.Services;
public class PacienteEstudio
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;          // RX_INFORME / RX_IMAGEN
    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaRelativa { get; set; } = string.Empty;
    public DateTime? FechaDocumento { get; set; }
    public string Fuente { get; set; } = string.Empty;
}
