# Doktors - Version de practica

Copia de trabajo del sistema de Centro Medico Klinic (front Blazor + API ASP.NET Core + SQL Server)
para practicar sin riesgo. Funciona solo en tu equipo, con una base de datos propia y datos inventados.

**No esta conectada a produccion.** No tiene claves, tokens ni datos reales de pacientes, y no envia
mensajes de WhatsApp. Nada de lo que hagas aqui afecta al sistema real.

## Requisitos

- Windows con **.NET SDK 8** o mas nuevo (`dotnet --version`).
- **SQL Server Express** con la instancia `localhost\SQLEXPRESS` y la herramienta `sqlcmd`.
  Tu usuario de Windows debe poder crear bases de datos (por defecto los administradores locales pueden).
- Conexion a internet la primera vez (para descargar los paquetes NuGet del front).

## Puesta en marcha

Abre PowerShell dentro de esta carpeta y ejecuta, en orden:

```powershell
# 1. Crear la base de datos de practica y las tablas (una sola vez)
sqlcmd -S localhost\SQLEXPRESS -E -C -i bd\01_crear_base_y_esquema.sql

# 2. Cargar los datos ficticios (se puede repetir cuando quieras volver a empezar)
sqlcmd -S localhost\SQLEXPRESS -E -C -d Doktors_Practica -i bd\02_datos_de_prueba.sql

# 3. Arrancar la API (puerto 5000) y el front (puerto 5089)
.\iniciar.ps1
```

La primera vez el front tarda en compilar. Cuando `front.log` diga `Now listening`, abre
http://localhost:5089 e ingresa con uno de los usuarios de abajo. Para apagar todo: `.\detener.ps1`.

Si tu instancia de SQL Server no se llama `SQLEXPRESS`, cambia `Server=` en `api\appsettings.json`
y usa ese nombre en los comandos `sqlcmd`.

## Usuarios de prueba

La clave de todos es **Practica2026**.

| Usuario       | Rol                | Que ve                                     |
|---------------|--------------------|--------------------------------------------|
| admin         | Admin              | Panel Admin (usuarios, carrusel, horarios) |
| admincentro   | AdminCentro        | Panel Admin Centro (metricas, auditoria)   |
| adminderiv    | AdminDerivaciones  | Panel de Derivaciones                      |
| caja1         | UsuarioCaja Centro | Caja                                       |
| cajamilan     | UsuarioCaja Milan  | Caja de la sede Milan                      |
| doctor1       | Doctor Centro      | Agenda de cardiologia                      |
| doctor2       | Doctor Centro      | Agenda de medicina interna                 |
| doctormilan   | Doctor Milan       | Agenda de cardiologia en Milan             |
| empresa1      | UsuarioEmpresa     | Portal de empresa                          |

El login permite 5 intentos por minuto desde el mismo equipo; si ves el error 429, espera un minuto.

Los datos ficticios incluyen 6 pacientes (cedulas `09999999xx`) con citas en distintos estados para
recorrer cada pestana: pendiente, en curso, no asistio, atendido sin subsecuente y subsecuente agendada.

## Contenido de la carpeta

| Carpeta o archivo | Que es                                                              |
|-------------------|---------------------------------------------------------------------|
| `front\`          | Codigo fuente del front (Blazor WebAssembly). Aqui se practica.     |
| `api\`            | API ya compilada, con configuracion de practica (`appsettings.json`). |
| `bd\`             | Scripts SQL: crear la base y cargar datos ficticios.                |
| `iniciar.ps1`, `detener.ps1` | Encender y apagar la API y el front.                     |

En `front\`: `Pages\` (pantallas), `Shared\` (componentes y menus), `Services\` (llamadas a la API),
`wwwroot\` (estilos, scripts, imagenes). El front en modo desarrollo llama a la API en
`http://localhost:5000/` (esta fijado en `front\Program.cs`).

## Como esta organizado el menu

- **Panel Admin:** Nuevo Registro (Usuario Empresa y Doctor), Gestion Administrativa (Usuarios, Doctores y
  Derivaciones, Citas Medicas, Importar Excel, Sincronizar PDFs), Carrusel y Horarios Medicos.
- **Derivaciones:** Lista, Nueva Derivacion, Agenda Diaria, Bot WhatsApp (carga, estado y panel),
  Citas Generadas, No Atendidos, Procedimientos, Subsecuentes (pendientes y agendados) y Excepciones de vigencia.
- **Caja:** el buscador de citas muestra el codigo de validacion de cada paciente.

## Que no funciona en practica (es normal)

- **Resultados de laboratorio** (Odoo) y **WhatsApp** no estan conectados: esas pantallas salen vacias
  o con error y no se envia ningun mensaje.
- Sincronizar PDFs usa carpetas locales vacias (`api\datos\`).

## Reglas

- Trabaja solo aqui. No copies a esta carpeta claves, tokens, archivos `appsettings` reales ni datos de pacientes.
- No cambies la configuracion para apuntar a la base de datos o al servidor reales.
- Si tu cambio sirve, entregalo como archivos modificados o un resumen de lo que cambiaste; no se publica desde aqui.

## Problemas frecuentes

| Sintoma                                     | Que hacer                                                                 |
|---------------------------------------------|---------------------------------------------------------------------------|
| `iniciar.ps1` dice que la API no arranco    | Mira `api.err` y `api.log`. Casi siempre es que la base no existe o la instancia de SQL tiene otro nombre. |
| Puerto 5000 o 5089 ocupado                  | Ejecuta `.\detener.ps1`; si sigue, cierra el programa que usa ese puerto. |
| El login no responde / `ERR_CONNECTION_REFUSED` | La API no esta encendida: revisa que `api.log` diga que esta escuchando. |
| Quieres empezar de cero                     | Repite el paso 2 (borra y vuelve a crear los datos ficticios).            |
