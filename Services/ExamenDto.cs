using System;

namespace LaboratorioFront.Services;

public class ExamenDto
{
	public string NombreArchivo { get; set; } = string.Empty;

	public string Base64 { get; set; } = string.Empty;

	public int Id { get; set; }

	public string Nombre { get; set; } = string.Empty;

	public DateTime Fecha { get; set; }

	public string Resultado { get; set; } = string.Empty;

	public string DocumentoPdfBase64 { get; set; } = string.Empty;

	public int PacienteId { get; set; }
}
