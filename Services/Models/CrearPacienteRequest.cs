using System.ComponentModel.DataAnnotations;

public class CrearPacienteRequest
{
    [Required]
    public string NombreCompleto { get; set; } = "";

    [Required]
    [StringLength(10, MinimumLength = 10)]
    public string Cedula { get; set; } = "";
}
