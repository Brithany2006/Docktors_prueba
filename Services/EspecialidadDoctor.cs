using System;

namespace LaboratorioFront.Services;

/// <summary>
/// Que doctores pueden atender una derivacion segun su especialidad. El ecocardiograma lo atiende solo el doctor
/// marcado por el API (Dr. Toro de Centro); el resto se compara por familia de especialidad.
/// </summary>
public static class EspecialidadDoctor
{
	public static bool EsEco(string? especialidad) =>
		(especialidad ?? string.Empty).IndexOf("ECOCARDIO", StringComparison.OrdinalIgnoreCase) >= 0;

	public static bool Coincide(Doctor doc, string? reqEspecialidad) =>
		EsEco(reqEspecialidad) ? doc.AtiendeEco : CoincideEspecialidad(doc.Especialidad, reqEspecialidad);

	public static bool CoincideEspecialidad(string? docEspecialidad, string? reqEspecialidad)
	{
		if (string.IsNullOrEmpty(docEspecialidad) || string.IsNullOrEmpty(reqEspecialidad)) return false;
		var doc = docEspecialidad.ToUpper(); var req = reqEspecialidad.ToUpper();
		if (doc.Contains("CARDIO") && req.Contains("CARDIO") && !req.Contains("ELECTRO") && !doc.Contains("ELECTRO")) return true;
		if (doc.Contains("ELECTRO") && req.Contains("ELECTRO")) return true;
		if (doc.Contains("GINECO") && req.Contains("GINECO")) return true;
		if (doc.Contains("PEDIATRIA") && req.Contains("PEDIATRIA")) return true;
		if (doc.Contains("NEURO") && req.Contains("NEURO")) return true;
		if (doc.Contains("GASTRO") && req.Contains("GASTRO")) return true;
		if (doc.Contains("MEDICINA INTERNA") && req.Contains("MEDICINA INTERNA")) return true;
		if (doc.Contains("TRAUMATOLOGIA") && req.Contains("TRAUMATOLOGIA")) return true;
		if (doc.Contains("ENDOCRINO") && req.Contains("ENDOCRINO")) return true;
		if (doc.Contains("NEFRO") && req.Contains("NEFRO")) return true;
		if (doc.Contains("UROLOGIA") && req.Contains("UROLOGIA")) return true;
		if (doc.Contains("NEUMO") && req.Contains("NEUMO")) return true;
		if (doc.Contains("OTORRINO") && req.Contains("OTORRINO")) return true;
		if (doc.Contains("OFTALMO") && req.Contains("OFTALMO")) return true;
		if (doc.Contains("DERMATO") && req.Contains("DERMATO")) return true;
		if (doc.Contains("ONCO") && req.Contains("ONCO")) return true;
		return doc == req;
	}
}
