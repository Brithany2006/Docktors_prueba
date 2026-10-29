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

		/// <summary>Sede de la ficha de doctor que se esta mostrando.</summary>
		public string Sucursal { get; set; } = string.Empty;

		/// <summary>Sedes en las que el usuario tiene ficha de doctor (si hay mas de una se muestra el selector).</summary>
		public List<string> Sucursales { get; set; } = new List<string>();

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

	
    public class NoConfirmoItem
    {
        public int IdBot { get; set; }
        public string Paciente { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
        public string EstadoBot { get; set; } = string.Empty;
        public DateTime? FechaCitaAnterior { get; set; }
        public int? IdDoctorAnterior { get; set; }
        public bool CanceladaPorNoConfirmar { get; set; }
    }

    public class NoConfirmaronResponse
    {
        public int TotalRegistros { get; set; }
        public List<NoConfirmoItem> Data { get; set; } = new List<NoConfirmoItem>();
    }

    /// <summary>Pacientes que no confirmaron su cita por WhatsApp (de la sede del usuario), con busqueda por nombre o cedula.</summary>
    public async Task<NoConfirmaronResponse> GetNoConfirmaronAsync(string search = "", int page = 1, int pageSize = 50)
    {
        var url = $"api/derivaciones/no-confirmaron?page={page}&pageSize={pageSize}&search={Uri.EscapeDataString(search ?? string.Empty)}";
        return (await _http.GetFromJsonAsync<NoConfirmaronResponse>(url)) ?? new NoConfirmaronResponse();
    }

    /// <summary>Activa al paciente: crea su cita manual (turno extra) y lo saca del proceso del bot.</summary>
    public async Task<(bool Exito, string Mensaje)> ActivarNoConfirmoAsync(int idBot, int idDoctor, DateTime fechaHora, bool esTurnoExtra)
    {
        var res = await _http.PostAsJsonAsync($"api/derivaciones/no-confirmaron/{idBot}/activar", new { idDoctor, fechaHora, esTurnoExtra });
        if (res.IsSuccessStatusCode) return (true, string.Empty);
        var texto = await res.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(texto) ? "No se pudo activar al paciente." : texto.Trim('"'));
    }

    /// <summary>Prepara la derivacion de ese paciente para darle cita (no crea la cita).</summary>
    public async Task<(Derivacion? Derivacion, string Mensaje)> PrepararNoConfirmoAsync(int idBot)
    {
        var res = await _http.PostAsync($"api/derivaciones/no-confirmaron/{idBot}/preparar", null);
        if (res.IsSuccessStatusCode) return (await res.Content.ReadFromJsonAsync<Derivacion>(), string.Empty);
        var texto = await res.Content.ReadAsStringAsync();
        return (null, string.IsNullOrWhiteSpace(texto) ? "No se pudo preparar el paciente." : texto);
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

	public async Task<(bool Ok, string Estado, string Mensaje)> RevertirNoAsistioAsync(int id)
	{
		var res = await _http.PostAsync($"api/derivaciones/citas/{id}/revertir-no-asistio", null);
		var texto = await res.Content.ReadAsStringAsync();
		if (res.IsSuccessStatusCode)
		{
			using var doc = System.Text.Json.JsonDocument.Parse(texto);
			return (true, doc.RootElement.GetProperty("estado").GetString() ?? "Pendiente", string.Empty);
		}
		return (false, string.Empty, string.IsNullOrWhiteSpace(texto) ? "No se pudo revertir la cita." : texto.Trim('"'));
	}

	public async Task<List<CapacidadDia>> GetCalendarioDoctorAsync(int idDoctor, int year, int month)
	{
		return (await _http.GetFromJsonAsync<List<CapacidadDia>>($"api/derivaciones/doctores/{idDoctor}/calendario?year={year}&month={month}")) ?? new List<CapacidadDia>();
	}

	public class ExcepcionVigenciaDto
	{
		public int Id { get; set; }
		public string Cedula { get; set; } = "";
		public string? Paciente { get; set; }
		public string Motivo { get; set; } = "";
		public string? CreadoPorNombre { get; set; }
		public string? CreadoPor { get; set; }
		public DateTime Fecha { get; set; }
		public DateTime Vence { get; set; }
		public string? RevocadaPor { get; set; }
		public DateTime? FechaRevocacion { get; set; }
		public bool Vigente { get; set; }
	}

	public async Task<List<ExcepcionVigenciaDto>> GetExcepcionesVigenciaAsync() =>
		await _http.GetFromJsonAsync<List<ExcepcionVigenciaDto>>("api/excepciones-vigencia") ?? new();

	/// <summary>Devuelve null si se concedio; si no, el mensaje de la API.</summary>
	public async Task<string?> CrearExcepcionVigenciaAsync(string cedula, string motivo, int dias)
	{
		var resp = await _http.PostAsJsonAsync("api/excepciones-vigencia", new { cedula, motivo, dias });
		return resp.IsSuccessStatusCode ? null : (await resp.Content.ReadAsStringAsync()).Trim('"');
	}

	public async Task<bool> RevocarExcepcionVigenciaAsync(int id) =>
		(await _http.PostAsync($"api/excepciones-vigencia/{id}/revocar", null)).IsSuccessStatusCode;

	public async Task<CitasDoctorDto?> GetCitasDeDoctorAsync(int idUsuario, string? sucursal = null)
	{
		var url = $"api/derivaciones/citas/doctor/{idUsuario}";
		if (!string.IsNullOrWhiteSpace(sucursal)) url += "?sucursal=" + Uri.EscapeDataString(sucursal);
		return await _http.GetFromJsonAsync<CitasDoctorDto>(url);
	}

	
        public async Task<PaginacionResponse<CitaDerivacionResponse>> GetCitasPaginadasAsync(int page, int pageSize, string search, string estado = "", DateTime? fecha = null, int? doctorId = null, bool soloSubsecuentes = false, DateTime? fechaHasta = null, string? especialidad = null, bool? reagendado = null, string? tipo = null)
    {
        try {
            string url = $"api/derivaciones/citas/paginadas?page={page}&pageSize={pageSize}&search={Uri.EscapeDataString(search ?? "")}&estado={Uri.EscapeDataString(estado ?? "")}";
            if (fecha.HasValue) url += $"&fecha={fecha.Value.ToString("yyyy-MM-dd")}";
            if (doctorId.HasValue) url += $"&doctorId={doctorId.Value}";
            if (soloSubsecuentes) url += "&soloSubsecuentes=true";
            if (fechaHasta.HasValue) url += $"&fechaHasta={fechaHasta.Value.ToString("yyyy-MM-dd")}";
            if (!string.IsNullOrWhiteSpace(especialidad)) url += $"&especialidad={Uri.EscapeDataString(especialidad)}";
            if (reagendado.HasValue) url += $"&reagendado={(reagendado.Value ? "true" : "false")}";
            if (!string.IsNullOrWhiteSpace(tipo)) url += $"&tipo={Uri.EscapeDataString(tipo)}";
            return await _http.GetFromJsonAsync<PaginacionResponse<CitaDerivacionResponse>>(url) ?? new PaginacionResponse<CitaDerivacionResponse>();
        } catch {
            return new PaginacionResponse<CitaDerivacionResponse>();
        }
    }
        
    public async Task<List<CitaDerivacionResponse>> BuscarCitasAsync(string q, string? sucursal = null)
	{
		if (string.IsNullOrWhiteSpace(q))
		{
			return new List<CitaDerivacionResponse>();
		}
		return (await _http.GetFromJsonAsync<List<CitaDerivacionResponse>>("api/derivaciones/citas/buscar?q=" + Uri.EscapeDataString(q) + (string.IsNullOrWhiteSpace(sucursal) ? "" : "&sucursal=" + Uri.EscapeDataString(sucursal)))) ?? new List<CitaDerivacionResponse>();
	}

	public async Task<List<CitaDerivacionResponse>> GetAllCitasCalendarioAsync()
	{
		return (await _http.GetFromJsonAsync<List<CitaDerivacionResponse>>("api/derivaciones/citas/calendario")) ?? new List<CitaDerivacionResponse>();
	}

	public class ConflictoHorarioDto
	{
		public bool HayConflicto { get; set; }
		public string Mensaje { get; set; } = string.Empty;
	}

	public async Task<ConflictoHorarioDto> VerificarConflictoHorarioAsync(int idDoctor, DateTime fechaHora, int? excluirCitaId = null, int? excluirDerivacionId = null, string? excluirCedula = null)
	{
		try
		{
			string url = $"api/derivaciones/doctores/{idDoctor}/conflicto-horario?fechaHora={Uri.EscapeDataString(fechaHora.ToString("yyyy-MM-ddTHH:mm:ss"))}";
			if (excluirCitaId.HasValue && excluirCitaId.Value > 0) url += $"&excluirCitaId={excluirCitaId.Value}";
			if (excluirDerivacionId.HasValue && excluirDerivacionId.Value > 0) url += $"&excluirDerivacionId={excluirDerivacionId.Value}";
			if (!string.IsNullOrWhiteSpace(excluirCedula)) url += $"&excluirCedula={Uri.EscapeDataString(excluirCedula)}";
			return (await _http.GetFromJsonAsync<ConflictoHorarioDto>(url)) ?? new ConflictoHorarioDto();
		}
		catch
		{
			return new ConflictoHorarioDto();
		}
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

	public async Task<List<string>> GetAvailableSlotsAsync(int idDoctor, DateTime date, bool eco = false)
	{
		return (await _http.GetFromJsonAsync<List<string>>($"api/derivaciones/doctores/{idDoctor}/slots?date={date:yyyy-MM-dd}{(eco ? "&eco=true" : "")}")) ?? new List<string>();
	}

	/// <summary>Turno del horario del doctor en un dia: Libre = sin cita; Pasado = la hora ya paso.</summary>
	public class SlotAgenda
	{
		public string Hora { get; set; } = "";
		public bool Libre { get; set; }
		public bool Pasado { get; set; }
	}

	public async Task<List<SlotAgenda>> GetAgendaSlotsAsync(int idDoctor, DateTime date)
	{
		return (await _http.GetFromJsonAsync<List<SlotAgenda>>($"api/derivaciones/doctores/{idDoctor}/agenda-slots?date={date:yyyy-MM-dd}")) ?? new List<SlotAgenda>();
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
            public int AgendadasIess { get; set; }
            public int AgendadasParticular { get; set; }
            public int AgendadasSinClasificar { get; set; }
            public int AgendadasAntiguo { get; set; }
            public int AtendidosIess { get; set; }
            public int AtendidosParticular { get; set; }
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








