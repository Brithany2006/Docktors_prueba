namespace LaboratorioFront.Services.Models
{
   
        public class CrearPacienteParticularDto
        {
            public string NombreCompleto { get; set; }
            public string Cedula { get; set; }
        public string Telefono { get; set; } = string.Empty;
            public string Password { get; set; }
        }
    }