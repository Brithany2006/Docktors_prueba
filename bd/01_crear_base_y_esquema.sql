-- =====================================================================
-- 01_crear_base_y_esquema.sql
-- Crea la base de datos de PRACTICA y todas las tablas de la aplicacion.
-- Es una base nueva y vacia: no contiene datos reales de pacientes.
-- Ejecutar desde la carpeta bd con:
--   sqlcmd -S localhost\SQLEXPRESS -E -C -i 01_crear_base_y_esquema.sql
-- Las tablas de Hangfire (tareas en segundo plano) las crea la API sola al arrancar.
-- =====================================================================
IF DB_ID('Doktors_Practica') IS NULL
    CREATE DATABASE Doktors_Practica;
GO
USE Doktors_Practica;
GO

CREATE TABLE [AuditoriaLog] (
    [Id] bigint NOT NULL IDENTITY,
    [Fecha] datetime2 NOT NULL,
    [Usuario] nvarchar(max) NOT NULL,
    [NombreUsuario] nvarchar(max) NULL,
    [Rol] nvarchar(max) NULL,
    [Sucursal] nvarchar(max) NULL,
    [Origen] nvarchar(max) NOT NULL,
    [Accion] nvarchar(max) NULL,
    [Entidad] nvarchar(max) NOT NULL,
    [IdEntidad] int NULL,
    [Operacion] nvarchar(max) NOT NULL,
    [Resumen] nvarchar(max) NULL,
    [Cambios] nvarchar(max) NULL,
    [Paciente] nvarchar(max) NULL,
    [Cedula] nvarchar(max) NULL,
    [Ip] nvarchar(max) NULL,
    CONSTRAINT [PK_AuditoriaLog] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Empresa] (
    [IdEmpresa] int NOT NULL IDENTITY,
    [RUC] nvarchar(13) NOT NULL,
    [Nombre] nvarchar(200) NOT NULL,
    [FechaCreacion] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_Empresa] PRIMARY KEY ([IdEmpresa])
);
GO


CREATE TABLE [ExcepcionesVigencia] (
    [Id] int NOT NULL IDENTITY,
    [Cedula] nvarchar(max) NOT NULL,
    [Paciente] nvarchar(max) NULL,
    [Motivo] nvarchar(max) NOT NULL,
    [CreadoPor] nvarchar(max) NOT NULL,
    [CreadoPorNombre] nvarchar(max) NULL,
    [Fecha] datetime2 NOT NULL,
    [Vence] datetime2 NOT NULL,
    [Activo] bit NOT NULL,
    [RevocadaPor] nvarchar(max) NULL,
    [FechaRevocacion] datetime2 NULL,
    CONSTRAINT [PK_ExcepcionesVigencia] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [PacienteParticular] (
    [Id] int NOT NULL IDENTITY,
    [NombreCompleto] nvarchar(200) NOT NULL,
    [Cedula] nvarchar(13) NOT NULL,
    [Telefono] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [PasswordPlain] nvarchar(max) NULL,
    [FechaCreacion] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_PacienteParticular] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [PacientesDerivacion] (
    [Id] int NOT NULL IDENTITY,
    [Cedula] nvarchar(20) NOT NULL,
    [NombreCompleto] nvarchar(200) NOT NULL,
    [Telefono] nvarchar(max) NOT NULL,
    [FechaRegistro] datetime2 NOT NULL,
    CONSTRAINT [PK_PacientesDerivacion] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [PacientesPdf] (
    [Id] int NOT NULL IDENTITY,
    [NombreCompleto] nvarchar(max) NOT NULL,
    [RUC_Empresa] nvarchar(max) NOT NULL,
    [NombreArchivo] nvarchar(max) NULL,
    [NombreArchivoPDF] nvarchar(max) NULL,
    CONSTRAINT [PK_PacientesPdf] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [SincronizacionLog] (
    [Id] int NOT NULL IDENTITY,
    [RUC_Empresa] nvarchar(max) NULL,
    [IniciadaUtc] datetime2 NOT NULL,
    [FinalizadaUtc] datetime2 NULL,
    [TotalLeidos] int NOT NULL,
    [TotalCopiados] int NOT NULL,
    [TotalOmitidos] int NOT NULL,
    [Estado] nvarchar(20) NOT NULL,
    [Mensaje] nvarchar(max) NULL,
    CONSTRAINT [PK_SincronizacionLog] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [UsuarioPaciente] (
    [IdUsuarioPaciente] int NOT NULL IDENTITY,
    [Cedula] nvarchar(20) NOT NULL,
    [Telefono] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(255) NOT NULL,
    [PasswordPlain] nvarchar(max) NULL,
    [IdPaciente] int NOT NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [FechaCreacion] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_UsuarioPaciente] PRIMARY KEY ([IdUsuarioPaciente])
);
GO


CREATE TABLE [WebhookLogs] (
    [Id] int NOT NULL IDENTITY,
    [Timestamp] datetime2 NOT NULL,
    [Tipo] nvarchar(50) NOT NULL,
    [Telefono] nvarchar(20) NULL,
    [Estado] nvarchar(50) NULL,
    [RawPayload] nvarchar(max) NULL,
    CONSTRAINT [PK_WebhookLogs] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [WhatsAppLogs] (
    [Id] int NOT NULL IDENTITY,
    [Fecha] datetime2 NOT NULL,
    [Plantilla] nvarchar(max) NOT NULL,
    [Parametros] nvarchar(max) NOT NULL,
    [Resultado] nvarchar(max) NOT NULL,
    [Respuesta] nvarchar(max) NULL,
    [IdPaciente] int NULL,
    CONSTRAINT [PK_WhatsAppLogs] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [PacienteMaestro] (
    [Id] int NOT NULL IDENTITY,
    [IdEmpresa] int NULL,
    [RUC_Empresa] nvarchar(13) NOT NULL,
    [NombreCompleto] nvarchar(200) NOT NULL,
    [Normalizado] nvarchar(200) NOT NULL,
    [CreadoUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_PacienteMaestro] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PacienteMaestro_Empresa_IdEmpresa] FOREIGN KEY ([IdEmpresa]) REFERENCES [Empresa] ([IdEmpresa]) ON DELETE NO ACTION
);
GO


CREATE TABLE [UsuarioEmpresa] (
    [IdUsuario] int NOT NULL IDENTITY,
    [IdEmpresa] int NULL,
    [Nombre] nvarchar(max) NOT NULL,
    [RUC] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [PasswordPlain] nvarchar(max) NULL,
    [FechaCreacion] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Rol] nvarchar(max) NOT NULL DEFAULT N'UsuarioEmpresa',
    [Sucursal] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_UsuarioEmpresa] PRIMARY KEY ([IdUsuario]),
    CONSTRAINT [FK_UsuarioEmpresa_Empresa_IdEmpresa] FOREIGN KEY ([IdEmpresa]) REFERENCES [Empresa] ([IdEmpresa]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Derivaciones] (
    [Id] int NOT NULL IDENTITY,
    [IdPaciente] int NOT NULL,
    [FechaEmision] datetime2 NOT NULL,
    [FechaExpiracion] datetime2 NOT NULL,
    [EspecialidadRequerida] nvarchar(max) NULL,
    [Observaciones] nvarchar(max) NULL,
    [FechaRegistro] datetime2 NOT NULL,
    [Activo] bit NOT NULL,
    [Sucursal] nvarchar(450) NULL,
    [CodigoValidacion] nvarchar(max) NULL,
    CONSTRAINT [PK_Derivaciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Derivaciones_PacientesDerivacion_IdPaciente] FOREIGN KEY ([IdPaciente]) REFERENCES [PacientesDerivacion] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [PacienteDocumento] (
    [Id] int NOT NULL IDENTITY,
    [PacienteMaestroId] int NOT NULL,
    [NombreArchivo] nvarchar(260) NOT NULL,
    [RutaOrigen] nvarchar(500) NOT NULL,
    [RutaLocal] nvarchar(500) NOT NULL,
    [HashSha256] nvarchar(64) NULL,
    [FechaArchivoUtc] datetime2 NULL,
    [CopiadoUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_PacienteDocumento] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PacienteDocumento_PacienteMaestro_PacienteMaestroId] FOREIGN KEY ([PacienteMaestroId]) REFERENCES [PacienteMaestro] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [PacienteEstudio] (
    [Id] int NOT NULL IDENTITY,
    [IdPaciente] int NOT NULL,
    [Tipo] nvarchar(30) NOT NULL,
    [NombreArchivo] nvarchar(255) NOT NULL,
    [RutaRelativa] nvarchar(500) NOT NULL,
    [FechaDocumento] datetime2 NULL,
    [Fuente] nvarchar(50) NOT NULL,
    [CreadoUtc] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_PacienteEstudio] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PacienteEstudio_PacienteMaestro_IdPaciente] FOREIGN KEY ([IdPaciente]) REFERENCES [PacienteMaestro] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Doctores] (
    [Id] int NOT NULL IDENTITY,
    [IdUsuario] int NOT NULL,
    [Nombre] nvarchar(200) NOT NULL,
    [Especialidad] nvarchar(150) NOT NULL,
    [Consultorio] nvarchar(50) NOT NULL,
    [Sucursal] nvarchar(max) NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Doctores] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Doctores_UsuarioEmpresa_IdUsuario] FOREIGN KEY ([IdUsuario]) REFERENCES [UsuarioEmpresa] ([IdUsuario]) ON DELETE NO ACTION
);
GO


CREATE TABLE [CitasDerivacion] (
    [Id] int NOT NULL IDENTITY,
    [IdDerivacion] int NULL,
    [IdDoctor] int NOT NULL,
    [FechaHora] datetime2 NOT NULL,
    [Estado] nvarchar(450) NULL,
    [EsSubsecuente] bit NOT NULL,
    [EsTurnoExtra] bit NOT NULL,
    [FechaRegistro] datetime2 NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_CitasDerivacion] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CitasDerivacion_Derivaciones_IdDerivacion] FOREIGN KEY ([IdDerivacion]) REFERENCES [Derivaciones] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CitasDerivacion_Doctores_IdDoctor] FOREIGN KEY ([IdDoctor]) REFERENCES [Doctores] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ExcepcionesDoctor] (
    [Id] int NOT NULL IDENTITY,
    [IdDoctor] int NOT NULL,
    [FechaExcepcion] datetime2 NOT NULL,
    [Motivo] nvarchar(max) NULL,
    CONSTRAINT [PK_ExcepcionesDoctor] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExcepcionesDoctor_Doctores_IdDoctor] FOREIGN KEY ([IdDoctor]) REFERENCES [Doctores] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [HorariosDoctores] (
    [Id] int NOT NULL IDENTITY,
    [IdDoctor] int NOT NULL,
    [DiaSemana] int NOT NULL,
    [HoraInicio] time NOT NULL,
    [HoraFin] time NOT NULL,
    CONSTRAINT [PK_HorariosDoctores] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HorariosDoctores_Doctores_IdDoctor] FOREIGN KEY ([IdDoctor]) REFERENCES [Doctores] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [DerivacionesBot] (
    [Id] int NOT NULL IDENTITY,
    [CodigoValidacion] nvarchar(100) NOT NULL,
    [Cedula] nvarchar(max) NOT NULL,
    [PacienteNombre] nvarchar(max) NOT NULL,
    [Especialidad] nvarchar(max) NOT NULL,
    [UnidadMedica] nvarchar(max) NOT NULL,
    [Telefono] nvarchar(max) NOT NULL,
    [EstadoBot] nvarchar(max) NOT NULL,
    [FechaRecepcion] datetime2 NOT NULL,
    [IntentosReagendamiento] int NOT NULL,
    [IdCitaDerivacion] int NULL,
    CONSTRAINT [PK_DerivacionesBot] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DerivacionesBot_CitasDerivacion_IdCitaDerivacion] FOREIGN KEY ([IdCitaDerivacion]) REFERENCES [CitasDerivacion] ([Id])
);
GO


CREATE TABLE [RequisitosCita] (
    [Id] int NOT NULL IDENTITY,
    [IdCita] int NOT NULL,
    [Descripcion] nvarchar(200) NOT NULL,
    [Realizado] bit NOT NULL,
    [FechaRealizado] datetime2 NULL,
    [UsuarioRealizo] nvarchar(100) NULL,
    CONSTRAINT [PK_RequisitosCita] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RequisitosCita_CitasDerivacion_IdCita] FOREIGN KEY ([IdCita]) REFERENCES [CitasDerivacion] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_CitasDerivacion_Activo] ON [CitasDerivacion] ([Activo]);
GO


CREATE INDEX [IX_CitasDerivacion_Estado] ON [CitasDerivacion] ([Estado]);
GO


CREATE INDEX [IX_CitasDerivacion_FechaHora] ON [CitasDerivacion] ([FechaHora]);
GO


CREATE INDEX [IX_CitasDerivacion_IdDerivacion] ON [CitasDerivacion] ([IdDerivacion]);
GO


CREATE INDEX [IX_CitasDerivacion_IdDoctor] ON [CitasDerivacion] ([IdDoctor]);
GO


CREATE INDEX [IX_Derivaciones_Activo] ON [Derivaciones] ([Activo]);
GO


CREATE INDEX [IX_Derivaciones_FechaEmision] ON [Derivaciones] ([FechaEmision]);
GO


CREATE INDEX [IX_Derivaciones_IdPaciente] ON [Derivaciones] ([IdPaciente]);
GO


CREATE INDEX [IX_Derivaciones_Sucursal] ON [Derivaciones] ([Sucursal]);
GO


CREATE UNIQUE INDEX [IX_DerivacionesBot_CodigoValidacion] ON [DerivacionesBot] ([CodigoValidacion]);
GO


CREATE INDEX [IX_DerivacionesBot_IdCitaDerivacion] ON [DerivacionesBot] ([IdCitaDerivacion]);
GO


CREATE INDEX [IX_Doctores_IdUsuario] ON [Doctores] ([IdUsuario]);
GO


CREATE UNIQUE INDEX [IX_Empresa_RUC] ON [Empresa] ([RUC]);
GO


CREATE INDEX [IX_ExcepcionesDoctor_IdDoctor] ON [ExcepcionesDoctor] ([IdDoctor]);
GO


CREATE INDEX [IX_HorariosDoctores_IdDoctor] ON [HorariosDoctores] ([IdDoctor]);
GO


CREATE INDEX [IX_PacienteDocumento_PacienteMaestroId_NombreArchivo_HashSha256] ON [PacienteDocumento] ([PacienteMaestroId], [NombreArchivo], [HashSha256]);
GO


CREATE INDEX [IX_PacienteEstudio_IdPaciente_Tipo_FechaDocumento] ON [PacienteEstudio] ([IdPaciente], [Tipo], [FechaDocumento]);
GO


CREATE INDEX [IX_PacienteMaestro_IdEmpresa] ON [PacienteMaestro] ([IdEmpresa]);
GO


CREATE UNIQUE INDEX [IX_PacienteMaestro_RUC_Empresa_Normalizado] ON [PacienteMaestro] ([RUC_Empresa], [Normalizado]);
GO


CREATE UNIQUE INDEX [IX_PacienteParticular_Cedula] ON [PacienteParticular] ([Cedula]);
GO


CREATE UNIQUE INDEX [IX_PacientesDerivacion_Cedula] ON [PacientesDerivacion] ([Cedula]);
GO


CREATE INDEX [IX_RequisitosCita_IdCita] ON [RequisitosCita] ([IdCita]);
GO


CREATE INDEX [IX_UsuarioEmpresa_IdEmpresa] ON [UsuarioEmpresa] ([IdEmpresa]);
GO


