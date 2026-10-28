using System;
using System.Collections.Generic;

namespace LaboratorioFront.Services;

public class Derivacion
{
	public int Id { get; set; }

	public int IdPaciente { get; set; }

	public DateTime FechaEmision { get; set; }

	public DateTime FechaExpiracion { get; set; }

	public string EspecialidadRequerida { get; set; } = string.Empty;

		public string Observaciones { get; set; } = string.Empty;

	public string CodigoValidacion { get; set; } = string.Empty;
	/// <summary>IESS / Particular / Sin clasificar (lo calcula la API segun el codigo de validacion).</summary>
	public string TipoPaciente { get; set; } = string.Empty;
	/// <summary>"Bot" (llego por el Excel del bot) o "Caja" (registrada a mano); vacio si no tiene codigo.</summary>
	public string? Origen { get; set; }
    public string InstitucionReferente { get; set; } = string.Empty;

	public PacienteDerivacion? Paciente { get; set; }

	public List<DerivacionesService.CitaDerivacionResponse> Citas { get; set; } = new List<DerivacionesService.CitaDerivacionResponse>();
}



