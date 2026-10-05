using System.Collections.Generic;

namespace LaboratorioFront.Services;

public class Doctor
{
	public int Id { get; set; }

	public int IdUsuario { get; set; }

	public string Nombre { get; set; } = string.Empty;

	public string Especialidad { get; set; } = string.Empty;

	public string Consultorio { get; set; } = string.Empty;

	public string Sucursal { get; set; } = "Centro";

	public bool Activo { get; set; }

	public List<HorarioDoctor> Horarios { get; set; } = new List<HorarioDoctor>();

	public List<DerivacionesService.ExcepcionDoctor> Excepciones { get; set; } = new List<DerivacionesService.ExcepcionDoctor>();

	/// <summary>True si el doctor tiene horario laborable ese dia y no tiene un dia libre registrado.</summary>
	public bool EstaDeTurno(System.DateTime fecha)
	{
		int dia = fecha.DayOfWeek == System.DayOfWeek.Sunday ? 7 : (int)fecha.DayOfWeek;
		if (!Horarios.Any(h => h.DiaSemana == dia)) return false;
		return !Excepciones.Any(e => e.FechaExcepcion.Date == fecha.Date);
	}
}
