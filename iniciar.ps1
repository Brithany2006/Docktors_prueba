# Arranca la API de practica (puerto 5000) y el front (puerto 5089).
# Uso:  .\iniciar.ps1            (API + front)
#       .\iniciar.ps1 -SoloApi   (solo la API)
# Requisitos: .NET SDK 8 o mas nuevo y la base Doktors_Practica creada (ver LEEME.md).
param([switch]$SoloApi)

$ErrorActionPreference = 'Stop'
$aqui = $PSScriptRoot
$env:DOTNET_ROLL_FORWARD = 'LatestMajor'        # permite correr proyectos net8 con un runtime mas nuevo
$env:ASPNETCORE_ENVIRONMENT = 'Development'

# La API guarda aqui los archivos de pruebas (PDF, Rx, Eco); queda dentro de esta carpeta
foreach ($d in 'datos\pacientes', 'datos\rx', 'datos\eco') {
    New-Item -ItemType Directory -Force -Path (Join-Path "$aqui\api" $d) | Out-Null
}

& "$aqui\detener.ps1"
$pids = @()

Write-Host 'Iniciando la API en http://localhost:5000 ...'
$api = Start-Process dotnet -ArgumentList 'LaboratorioApi.dll' -WorkingDirectory "$aqui\api" -PassThru `
    -RedirectStandardOutput "$aqui\api.log" -RedirectStandardError "$aqui\api.err" -WindowStyle Hidden
$pids += $api.Id
$pids | Set-Content (Join-Path $aqui '.pids')

$listo = $false
for ($i = 0; $i -lt 40 -and -not $api.HasExited; $i++) {
    Start-Sleep -Seconds 1
    try {
        Invoke-WebRequest 'http://localhost:5000/api/BotDerivaciones/estado-envio' -UseBasicParsing -TimeoutSec 3 | Out-Null
        $listo = $true; break
    } catch {
        # sin sesion la API responde 401: significa que ya esta arriba
        if ($_.Exception.Response) { $listo = $true; break }
    }
}
if (-not $listo) {
    Write-Host "La API no arranco. Revisa api.err y api.log en $aqui" -ForegroundColor Red
    & "$aqui\detener.ps1"; exit 1
}
Write-Host 'API lista.' -ForegroundColor Green

if (-not $SoloApi) {
    Write-Host 'Iniciando el front en http://localhost:5089 (la primera vez restaura paquetes y compila; puede tardar) ...'
    $front = Start-Process dotnet -ArgumentList 'run', '--project', 'front', '--no-launch-profile', '--urls', 'http://localhost:5089' `
        -WorkingDirectory $aqui -PassThru -RedirectStandardOutput "$aqui\front.log" -RedirectStandardError "$aqui\front.err" -WindowStyle Hidden
    $pids += $front.Id
    $pids | Set-Content (Join-Path $aqui '.pids')
    Write-Host 'Cuando front.log diga "Now listening", abre http://localhost:5089 . Usuarios de prueba en LEEME.md.'
}
