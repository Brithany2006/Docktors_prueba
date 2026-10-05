using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace LaboratorioFront.Services;

public class DerivacionesService
{
    public class RequisitoCita { public string Descripcion {get;set;} = ""; public bool Realizado {get;set;} }
    public class CitaDerivacion
	{
		public int IdDoctor { get; set; }

		public int? IdDerivacion { get; set; }

		public DateTime FechaHora { get; set; }

		public string Estado { get; set; } = "Pendiente";

		public bool EsSubsecuente { get; set; }
		public bool EsTurnoExtra { get; set; }
		public System.Collections.Generic.List<RequisitoCita>? Requisitos { get; set; }
	}

	public class CapacidadDia
	{
		public string Fecha { get; set; } = string.Empty;

		public int Citas { get; set; }

		public int TotalSlots { get; set; }

		public double Ocupacion { get; set; }

		public string Color { get; set; } = string.Empty;
	}

	public class CitasDoctorDto
	{
		public int DoctorId { get; set; }

		public List<CitaDerivacionResponse> Citas { get; set; } = new List<CitaDerivacionResponse>();
	}
	public class CitaDerivacionResponse
	{
		public int Id { get; set; }

		public int? IdDerivacion { get; set; }

		public DateTime FechaHora { get; set; }

		public string Estado { get; set; } = string.Empty;
		public bool Activo { get; set; } = true;
		/// <summary>Cuando se registro la cita en el sistema (para saber desde cuando esta agendada).</summary>
		public DateTime? FechaRegistro { get; set; }

		public bool EsSubsecuente { get; set; }
		public bool EsTurnoExtra { get; set; }

		public Derivacion? Derivacion { get; set; }

		public Doctor? Doctor { get; set; }
		public System.Collections.Generic.List<RequisitoCita>? Requisitos { get; set; }
	}

	private readonly HttpClient _http;

	public DerivacionesService(HttpClient http)
	{
		_http = http;
	}

	        public async Task<PacienteDerivacion?> GetPacienteByCedulaAsync(string cedula)
        {
            var res = await _http.GetAsync($"api/derivaciones/pacientes/{cedula}");
            if (res.IsSuccessStatusCode) return await res.Content.ReadFromJsonAsync<PacienteDerivacion>();
            return null;
        }

        public async Task<List<PacienteDerivacion>> GetPacientesAsync()
	{
		return (await _http.GetFromJsonAsync<List<PacienteDerivacion>>("api/derivaciones/pacientes")) ?? new List<PacienteDerivacion>();
	}

	public async Task<List<Doctor>> GetDoctoresAsync(string sucursal = "")
	{
		return (await _http.GetFromJsonAsync<List<Doctor>>($"api/derivaciones/doctores?sucursal={sucursal}")) ?? new List<Doctor>();
	}

	public async Task<List<string>> GetEspecialidadesAsync()
	{
		return (await _http.GetFromJsonAsync<List<string>>("api/derivaciones/doctores/especialidades")) ?? new List<string>();
	}

	
    public async Task<bool> BorrarDerivacionAsync(int id)
    {
        var result = await _http.PostAsync($"api/derivaciones/derivaciones/{id}/borrar", null);
        return result.IsSuccessStatusCode;
    }

    public async Task<bool> BorrarCitaAsync(int id)
    {
        var result = await _http.PostAsync($"api/derivaciones/citas/{id}/borrar", null);
        return result.IsSuccessStatusCode;
    }

    public async Task<List<Derivacion>> GetDerivacionesBorradasAsync(string sucursal = "")
    {
        return await _http.GetFromJsonAsync<List<Derivacion>>($"api/derivaciones/derivaciones-borradas?sucursal={sucursal}") ?? new List<Derivacion>();
    }

        public async Task<List<CitaDerivacionResponse>> GetCitasSinSubsecuenteAsync(string sucursal = "")
    {
        return await _http.GetFromJsonAsync<List<CitaDerivacionResponse>>($"api/derivaciones/citas-sin-subsecuente?sucursal={sucursal}") ?? new List<CitaDerivacionResponse>();
    }

    public async Task<List<CitaDerivacionResponse>> GetCitasBorradasAsync(string sucursal = "")
    {
        return await _http.GetFromJsonAsync<List<CitaDerivacionResponse>>($"api/derivaciones/citas-borradas?sucursal={sucursal}") ?? new List<CitaDerivacionResponse>();
    }

	
    public class PaginacionResponse<T>
    {
        public int TotalRegistros { get; set; }
        public List<T> Data { get; set; } = new List<T>();
    }

        public async Task<PaginacionResponse<Derivacion>> GetDerivacionesPaginadasAsync(int page, int pageSize, string search, string especialidad = "", DateTime? fecha = null)
    {
        try {
            string url = $"api/derivaciones/derivaciones/paginadas?page={page}&pageSize={pageSize}&search={Uri.EscapeDataString(search ?? "")}&especialidad={Uri.EscapeDataString(especialidad ?? "")}";
            if (fecha.HasValue) url += $"&fecha={fecha.Value.ToString("yyyy-MM-dd")}";
            return await _http.GetFromJsonAsync<PaginacionResponse<Derivacion>>(url) ?? new PaginacionResponse<Derivacion>();
        } catch {
            return new PaginacionResponse<Derivacion>();
        }
    }
        
    public async Task<List<Derivacion>> GetDerivacionesAsync()
	{
		return (await _http.GetFromJsonAsync<List<Derivacion>>("api/derivaciones/derivaciones")) ?? new List<Derivacion>();
	}

	public async Task<PacienteDerivacion?> CreatePacienteAsync(PacienteDerivacion p)
	{
		HttpResponseMessage httpResponseMessage = await _http.PostAsJsonAsync("api/derivaciones/pacientes", p);
		if (httpResponseMessage.IsSuccessStatusCode)
		{
			return await httpResponseMessage.Content.ReadFromJsonAsync<PacienteDerivacion>();
		}
		return null;
	}

	public async Task<Derivacion?> CreateDerivacionAsync(Derivacion d)
	{
		HttpResponseMessage httpResponseMessage = await _http.PostAsJsonAsync("api/derivaciones/derivaciones", d);
		if (httpResponseMessage.IsSuccessStatusCode)
		{
			return await httpResponseMessage.Content.ReadFromJsonAsync<Derivacion>();
		}
		return null;
	}

	/// <summary>Igual que CreateDerivacionAsync, pero devuelve el mensaje del API (por ejemplo "codigo ya en uso") cuando falla.</summary>
	public async Task<(Derivacion? derivacion, string error)> CreateDerivacionConErrorAsync(Derivacion d)
	{
		HttpResponseMessage httpResponseMessage = await _http.PostAsJsonAsync("api/derivaciones/derivaciones", d);
		if (httpResponseMessage.IsSuccessStatusCode)
		{
			return (await httpResponseMessage.Content.ReadFromJsonAsync<Derivacion>(), string.Empty);
		}
		string cuerpo = (await httpResponseMessage.Content.ReadAsStringAsync()).Trim('"');
		return (null, string.IsNullOrWhiteSpace(cuerpo) ? "Error al crear la derivación." : cuerpo);
	}

	public async Task<List<CitaDerivacionResponse>> GetAllCitasAsync()
	{
		try
		{
			return (await _http.GetFromJsonAsync<List<CitaDerivacionResponse>>("api/derivaciones/citas/calendario")) ?? new List<CitaDerivacionResponse>();
		}
		catch
		{
			return new List<CitaDerivacionResponse>();
		}
	}

	public async Task<bool> DeleteCitaAsync(int id)
	{
		try
		{
			return (await _http.PostAsync($"api/derivaciones/citas/{id}/eliminar", null)).IsSuccessStatusCode;
		}
		catch
		{
			return false;
		}
	}

	public async Task<bool> MarcarAtendidoAsync(int id)
	{
		return (await _http.PostAsync($"api/derivaciones/citas/{id}/atendido", null)).IsSuccessStatusCode;
	}

	public async Task<bool> MarcarNoAsistioAsync(int id)
	{
		return (await _http.PostAsync($"api/derivaciones/citas/{id}/no-asistio", null)).IsSuccessStatusCode;
	}

	public async Task<List<CapacidadDia>> GetCalendarioDoctorAsync(int idDoctor, int year, int month)
	{
		return (await _http.GetFromJsonAsync<List<CapacidadDia>>($"api/derivaciones/doctores/{idDoctor}/calendario?year={year}&month={month}")) ?? new List<CapacidadDia>();
	}

	public async Task<CitasDoctorDto?> GetCitasDeDoctorAsync(int idUsuario)
	{
		return await _http.GetFromJsonAsync<CitasDoctorDto>($"api/derivaciones/citas/doctor/{idUsuario}");
	}

	
        public async Task<PaginacionResponse<CitaDerivacionResponse>> GetCitasPaginadasAsync(int page, int pageSize, string search, string estado = "", DateTime? fecha = null, int? doctorId = null, bool soloSubsecuentes = false)
    {
        try {
            string url = $"api/derivaciones/citas/paginadas?page={page}&pageSize={pageSize}&search={Uri.EscapeDataString(search ?? "")}&estado={Uri.EscapeDataString(estado ?? "")}";
            if (fecha.HasValue) url += $"&fecha={fecha.Value.ToString("yyyy-MM-dd")}";
            if (doctorId.HasValue) url += $"&doctorId={doctorId.Value}";
            if (soloSubsecuentes) url += "&soloSubsecuentes=true";
            return await _http.GetFromJsonAsync<PaginacionResponse<CitaDerivacionResponse>>(url) ?? new PaginacionResponse<CitaDerivacionResponse>();
        } catch {
            return new PaginacionResponse<CitaDerivacionResponse>();
        }
    }
        
    public async Task<List<CitaDerivacionResponse>> BuscarCitasAsync(string q)
	{
		if (string.IsNullOrWhiteSpace(q))
		{
			return new List<CitaDerivacionResponse>();
		}
		return (await _http.GetFromJsonAsync<List<CitaDerivacionResponse>>("api/derivaciones/citas/buscar?q=" + Uri.EscapeDataString(q))) ?? new List<CitaDerivacionResponse>();
	}

	public async Task<List<CitaDerivacionResponse>> GetAllCitasCalendarioAsync()
	{
		return (await _http.GetFromJsonAsync<List<CitaDerivacionResponse>>("api/derivaciones/citas/calendario")) ?? new List<CitaDerivacionResponse>();
	}

	public async Task<(bool Exito, string Mensaje)> AgendarCitaAsync(CitaDerivacion c)
	{
		HttpResponseMessage httpResponseMessage = await _http.PostAsJsonAsync("api/derivaciones/citas", c);
		if (httpResponseMessage.IsSuccessStatusCode)
		{
			return (Exito: true, Mensaje: "");
		}
		string text = await httpResponseMessage.Content.ReadAsStringAsync();
		return (Exito: false, Mensaje: string.IsNullOrWhiteSpace(text) ? "Error al agendar." : text);
	}

	public async Task<List<string>> GetAvailableSlotsAsync(int idDoctor, DateTime date)
	{
		return (await _http.GetFromJsonAsync<List<string>>($"api/derivaciones/doctores/{idDoctor}/slots?date={date:yyyy-MM-dd}")) ?? new List<string>();
	}

	public async Task<bool> UpdateHorariosDoctorAsync(int idDoctor, List<HorarioDoctor> horarios)
	{
		return (await _http.PostAsJsonAsync($"api/derivaciones/doctores/{idDoctor}/horarios", horarios)).IsSuccessStatusCode;
	}

	public async Task<bool> EditarDerivacionAsync(int id, Derivacion d)
	{
		return (await _http.PostAsJsonAsync($"api/derivaciones/derivaciones/{id}/editar", d)).IsSuccessStatusCode;
	}


		public async Task<(bool Exito, string Mensaje)> EditarCitaAsync(int id, CitaDerivacion c)
	{
		var response = await _http.PostAsJsonAsync($"api/derivaciones/citas/{id}/editar", c);
		if (response.IsSuccessStatusCode) return (true, "Cita actualizada correctamente.");
		var error = await response.Content.ReadAsStringAsync();
		return (false, string.IsNullOrEmpty(error) ? "Error desconocido." : error);
	}
        public class ActividadDoctor
        {
            public string Doctor { get; set; } = "";
            public string Especialidad { get; set; } = "";
            public int Total { get; set; }
            public int CitasNormales { get; set; }
            public int TurnosExtra { get; set; }
            public int TotalSlots { get; set; }
            public double Ocupacion { get; set; }
            public int Capacidad { get; set; }
            public int Agendadas { get; set; }
            /// <summary>Supera el 100% de la capacidad, o tiene citas agendadas sin capacidad configurada.</summary>
            public bool Sobrecupo => Ocupacion > 100 || (Capacidad == 0 && Agendadas > 0);
            /// <summary>Tiene horario laborable en algun dia del rango seleccionado (y no es un dia libre suyo).</summary>
            public bool TrabajaEnRango { get; set; } = true;
            public int Atendidos { get; set; }
            public int NoAsistio { get; set; }
            public int Pendientes { get; set; }
            /// <summary>Citas "En curso": el paciente aun no confirma por WhatsApp; no cuentan como cita del doctor.</summary>
            public int NoConfirmaron { get; set; }
            /// <summary>Capacidad - Agendadas (minimo 0).</summary>
            public int CuposLibres { get; set; }
            /// <summary>Atendidos / (Atendidos + No asistio) en %; nulo si todavia no hay citas cerradas.</summary>
            public double? PorcAsistencia { get; set; }
            /// <summary>"No asistio" de pacientes que habian confirmado por WhatsApp (posible asistencia sin registrar).</summary>
            public int NoAsistioConfirmadas { get; set; }
        }

        public class MetricasResponse
        {
            public string FechaInicio { get; set; } = "";
            public string FechaFin { get; set; } = "";
            public int DerivacionesCreadas { get; set; }
            public int CitasAgendadas { get; set; }
            public int CitasCreadas { get; set; }
            public List<ActividadDoctor> ActividadDoctores { get; set; } = new();
        }

                public async Task<MetricasResponse?> GetMetricasRangoAsync(DateTime fechaInicio, DateTime fechaFin, string sucursal = "")
        {
            return await _http.GetFromJsonAsync<MetricasResponse>($"api/derivaciones/metricas?fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}&sucursal={sucursal}");
        }
        public async Task<MetricasResponse?> GetMetricasAsync(DateTime fecha, string sucursal = "")
        {
            return await _http.GetFromJsonAsync<MetricasResponse>($"api/derivaciones/metricas?fechaInicio={fecha:yyyy-MM-dd}&fechaFin={fecha:yyyy-MM-dd}&sucursal={sucursal}");
        }


    public class ExcepcionDoctor
    {
        public int Id { get; set; }
        public int IdDoctor { get; set; }
        public DateTime FechaExcepcion { get; set; } = DateTime.Today;
        public string Motivo { get; set; } = string.Empty;
    }

    public async Task<List<ExcepcionDoctor>> GetExcepcionesAsync(int idDoctor)
    {
        var response = await _http.GetAsync("api/DoctoresExcepciones/" + idDoctor);
        if (response.IsSuccessStatusCode) {
            return await response.Content.ReadFromJsonAsync<List<ExcepcionDoctor>>() ?? new List<ExcepcionDoctor>();
        }
        return new List<ExcepcionDoctor>();
    }

    public async Task<bool> AddExcepcionAsync(ExcepcionDoctor excepcion)
    {
        var response = await _http.PostAsJsonAsync("api/DoctoresExcepciones", excepcion);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteExcepcionAsync(int id)
    {
        var response = await _http.DeleteAsync("api/DoctoresExcepciones/" + id);
        return response.IsSuccessStatusCode;
    }

    public async Task<PacienteDerivacion?> BuscarPacientePorCedulaAsync(string cedula)
    {
        try {
            var pacs = await GetPacientesAsync();
            return pacs.FirstOrDefault(p => p.Cedula == cedula);
        } catch { return null; }
    }
}








