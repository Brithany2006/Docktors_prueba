-- =====================================================================
-- 02_datos_de_prueba.sql
-- Datos 100% FICTICIOS para practicar. Se puede ejecutar mas de una vez:
-- primero borra lo que creo antes con los mismos identificadores.
-- Ejecutar con:
--   sqlcmd -S localhost\SQLEXPRESS -E -C -d Doktors_Practica -i 02_datos_de_prueba.sql
--
-- Usuarios de prueba (clave de todos: Practica2026). Se guarda en texto y la API
-- la convierte a hash la primera vez que el usuario ingresa.
--   admin        Admin                 (panel Admin)
--   admincentro  AdminCentro           (panel Admin Centro)
--   adminderiv   AdminDerivaciones     (panel Derivaciones)
--   caja1        UsuarioCaja  Centro   (Caja)
--   cajamilan    UsuarioCaja  Milan    (Caja Milan)
--   doctor1      Doctor       Centro   (cardiologia)
--   doctor2      Doctor       Centro   (medicina interna)
--   doctormilan  Doctor       Milan    (cardiologia)
--   empresa1     UsuarioEmpresa        (portal de empresa)
-- =====================================================================
SET NOCOUNT ON;
USE Doktors_Practica;

-- ---------- limpieza de una carga anterior ----------
DELETE c FROM CitasDerivacion c JOIN Derivaciones d ON d.Id = c.IdDerivacion JOIN PacientesDerivacion p ON p.Id = d.IdPaciente WHERE p.Cedula LIKE '09999999%';
DELETE d FROM Derivaciones d JOIN PacientesDerivacion p ON p.Id = d.IdPaciente WHERE p.Cedula LIKE '09999999%';
DELETE FROM PacientesDerivacion WHERE Cedula LIKE '09999999%';
DELETE FROM DerivacionesBot WHERE CodigoValidacion LIKE 'CVAPRACT%';
DELETE h FROM HorariosDoctores h JOIN Doctores d ON d.Id = h.IdDoctor JOIN UsuarioEmpresa u ON u.IdUsuario = d.IdUsuario WHERE u.RUC LIKE 'doctor%';
DELETE d FROM Doctores d JOIN UsuarioEmpresa u ON u.IdUsuario = d.IdUsuario WHERE u.RUC LIKE 'doctor%';
DELETE FROM UsuarioEmpresa WHERE RUC IN ('admin','admincentro','adminderiv','caja1','cajamilan','doctor1','doctor2','doctormilan','empresa1');
DELETE FROM Empresa WHERE RUC = '9999999999001';

-- ---------- empresa y usuarios ----------
INSERT INTO Empresa (RUC, Nombre, FechaCreacion, Activo) VALUES ('9999999999001', 'EMPRESA DE PRACTICA S.A.', SYSDATETIME(), 1);
DECLARE @emp int = SCOPE_IDENTITY();

INSERT INTO UsuarioEmpresa (IdEmpresa, Nombre, RUC, PasswordHash, FechaCreacion, Activo, Rol, Sucursal) VALUES
 (@emp, 'Administrador Practica',        'admin',       'Practica2026', SYSDATETIME(), 1, 'Admin',             'Centro'),
 (@emp, 'Administrador Centro Practica', 'admincentro', 'Practica2026', SYSDATETIME(), 1, 'AdminCentro',       'Centro'),
 (@emp, 'Admin Derivaciones Practica',   'adminderiv',  'Practica2026', SYSDATETIME(), 1, 'AdminDerivaciones', 'Centro'),
 (@emp, 'Caja 1 Practica',               'caja1',       'Practica2026', SYSDATETIME(), 1, 'UsuarioCaja',       'Centro'),
 (@emp, 'Caja Milan Practica',           'cajamilan',   'Practica2026', SYSDATETIME(), 1, 'UsuarioCaja',       'Milan'),
 (@emp, 'Doctor Uno Practica',           'doctor1',     'Practica2026', SYSDATETIME(), 1, 'Doctor',            'Centro'),
 (@emp, 'Doctor Dos Practica',           'doctor2',     'Practica2026', SYSDATETIME(), 1, 'Doctor',            'Centro'),
 (@emp, 'Doctor Milan Practica',         'doctormilan', 'Practica2026', SYSDATETIME(), 1, 'Doctor',            'Milan'),
 (@emp, 'Empresa Practica',              'empresa1',    'Practica2026', SYSDATETIME(), 1, 'UsuarioEmpresa',    'Centro');

-- ---------- doctores y horarios (lunes a viernes, de 08:00 a 13:00) ----------
INSERT INTO Doctores (IdUsuario, Nombre, Especialidad, Consultorio, Sucursal, Activo)
SELECT IdUsuario, 'DR. PRACTICA UNO',   'CARDIOLOGIA',      '1', 'Centro', 1 FROM UsuarioEmpresa WHERE RUC = 'doctor1' UNION ALL
SELECT IdUsuario, 'DRA. PRACTICA DOS',  'MEDICINA INTERNA', '2', 'Centro', 1 FROM UsuarioEmpresa WHERE RUC = 'doctor2' UNION ALL
SELECT IdUsuario, 'DR. PRACTICA MILAN', 'CARDIOLOGIA',      '1', 'Milan',  1 FROM UsuarioEmpresa WHERE RUC = 'doctormilan';

INSERT INTO HorariosDoctores (IdDoctor, DiaSemana, HoraInicio, HoraFin)
SELECT d.Id, v.dia, '08:00', '13:00'
FROM Doctores d JOIN UsuarioEmpresa u ON u.IdUsuario = d.IdUsuario
CROSS JOIN (VALUES (1),(2),(3),(4),(5)) v(dia)
WHERE u.RUC LIKE 'doctor%';

-- ---------- pacientes, derivaciones y citas ficticias ----------
DECLARE @hoy date = CAST(SYSDATETIME() AS date);
DECLARE @d1 int = (SELECT d.Id FROM Doctores d JOIN UsuarioEmpresa u ON u.IdUsuario = d.IdUsuario WHERE u.RUC = 'doctor1');
DECLARE @d2 int = (SELECT d.Id FROM Doctores d JOIN UsuarioEmpresa u ON u.IdUsuario = d.IdUsuario WHERE u.RUC = 'doctor2');
DECLARE @dm int = (SELECT d.Id FROM Doctores d JOIN UsuarioEmpresa u ON u.IdUsuario = d.IdUsuario WHERE u.RUC = 'doctormilan');

INSERT INTO PacientesDerivacion (Cedula, NombreCompleto, Telefono, FechaRegistro) VALUES
 ('0999999901', 'PACIENTE PRACTICA UNO',    '0990000001', SYSDATETIME()),
 ('0999999902', 'PACIENTE PRACTICA DOS',    '0990000002', SYSDATETIME()),
 ('0999999903', 'PACIENTE PRACTICA TRES',   '0990000003', SYSDATETIME()),
 ('0999999904', 'PACIENTE PRACTICA CUATRO', '0990000004', SYSDATETIME()),
 ('0999999905', 'PACIENTE PRACTICA CINCO',  '0990000005', SYSDATETIME()),
 ('0999999906', 'PACIENTE PRACTICA SEIS',   '0990000006', SYSDATETIME());

INSERT INTO Derivaciones (IdPaciente, FechaEmision, FechaExpiracion, EspecialidadRequerida, Observaciones, FechaRegistro, Activo, Sucursal, CodigoValidacion)
SELECT p.Id, DATEADD(day, -30, @hoy), DATEADD(day, 60, @hoy), v.esp, '', SYSDATETIME(), 1, v.suc, v.cod
FROM PacientesDerivacion p
JOIN (VALUES
  ('0999999901', 'CARDIOLOGIA',      'Centro', 'CVAPRACT0001'),
  ('0999999902', 'CARDIOLOGIA',      'Centro', 'CVAPRACT0002'),
  ('0999999903', 'MEDICINA INTERNA', 'Centro', 'CVAPRACT0003'),
  ('0999999904', 'MEDICINA INTERNA', 'Centro', 'CVAPRACT0004'),
  ('0999999905', 'CARDIOLOGIA',      'Milan',  'CVAPRACT0005'),
  ('0999999906', 'CARDIOLOGIA',      'Centro', 'CVAPRACT0006')
) v(ced, esp, suc, cod) ON v.ced = p.Cedula;

-- Estados de cita para recorrer las pestanas: Atendido sin subsecuente, Pendiente, No Asistio, Subsecuente agendada, En curso
INSERT INTO CitasDerivacion (IdDerivacion, IdDoctor, FechaHora, Estado, EsSubsecuente, EsTurnoExtra, FechaRegistro, Activo)
SELECT d.Id, v.doc, DATEADD(minute, v.minutos, CAST(DATEADD(day, v.dias, @hoy) AS datetime2)), v.estado, v.sub, 0, SYSDATETIME(), 1
FROM Derivaciones d
JOIN PacientesDerivacion p ON p.Id = d.IdPaciente
JOIN (VALUES
  ('0999999901', @d1,  -10, 540, 'Atendido',   0),   -- sin subsecuente: sale en Subsecuentes Pendientes
  ('0999999902', @d1,    2, 580, 'Pendiente',  0),
  ('0999999903', @d2,   -3, 600, 'No Asistio', 0),   -- sale en No Atendidos
  ('0999999904', @d2,  -20, 660, 'Atendido',   0),
  ('0999999904', @d2,    5, 600, 'Pendiente',  1),   -- subsecuente agendada
  ('0999999905', @dm,    1, 500, 'Pendiente',  0),
  ('0999999906', @d1,    3, 620, 'En curso',   0)
) v(ced, doc, dias, minutos, estado, sub) ON v.ced = p.Cedula;

-- Registros del bot en estados que NO disparan mensajes (4_CONFIRMADO, 8_DESCARTADO)
INSERT INTO DerivacionesBot (CodigoValidacion, Cedula, PacienteNombre, Especialidad, UnidadMedica, Telefono, EstadoBot, FechaRecepcion, IntentosReagendamiento, IdCitaDerivacion) VALUES
 ('CVAPRACT0101', '0999999911', 'PACIENTE BOT UNO',  'CARDIOLOGIA',      'KLINIC CENTRO', '0990000011', '4_CONFIRMADO', SYSDATETIME(), 0, NULL),
 ('CVAPRACT0102', '0999999912', 'PACIENTE BOT DOS',  'MEDICINA INTERNA', 'KLINIC CENTRO', '0990000012', '4_CONFIRMADO', SYSDATETIME(), 0, NULL),
 ('CVAPRACT0103', '0999999913', 'PACIENTE BOT TRES', 'CARDIOLOGIA',      'KLINIC MILAN',  '0990000013', '8_DESCARTADO', SYSDATETIME(), 0, NULL);

SELECT 'usuarios' AS dato, COUNT(*) AS n FROM UsuarioEmpresa WHERE RUC IN ('admin','admincentro','adminderiv','caja1','cajamilan','doctor1','doctor2','doctormilan','empresa1')
UNION ALL SELECT 'doctores', COUNT(*) FROM Doctores
UNION ALL SELECT 'pacientes', COUNT(*) FROM PacientesDerivacion
UNION ALL SELECT 'derivaciones', COUNT(*) FROM Derivaciones
UNION ALL SELECT 'citas', COUNT(*) FROM CitasDerivacion
UNION ALL SELECT 'registros del bot', COUNT(*) FROM DerivacionesBot;
