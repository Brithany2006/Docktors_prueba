namespace LaboratorioFront.Services.Models
{
    public class LoginResponse
    {
        public string token { get; set; } = string.Empty;
        public UsuarioInfo usuario { get; set; } = new();
    }

    public class UsuarioInfo
    {
        public int IdPaciente { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}