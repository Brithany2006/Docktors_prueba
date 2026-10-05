using System;

namespace LaboratorioFront.Services;

public class HorarioDoctor
{
	public int Id { get; set; }

	public int IdDoctor { get; set; }

	public int DiaSemana { get; set; }

	public TimeSpan HoraInicio { get; set; }

	public TimeSpan HoraFin { get; set; }
}
