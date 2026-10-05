using System;

namespace LaboratorioFront.Services;

public class PacienteDerivacion
{
	public int Id { get; set; }

	public string Cedula { get; set; } = string.Empty;

	public string NombreCompleto { get; set; } = string.Empty;

	public string Telefono { get; set; } = string.Empty;

	public DateTime FechaRegistro { get; set; }
}
