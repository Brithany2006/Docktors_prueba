# Detiene la API y el front de practica que se iniciaron con iniciar.ps1 (usa los PID guardados en .pids).
# No toca ningun otro proceso.
$archivo = Join-Path $PSScriptRoot '.pids'
if (Test-Path $archivo) {
    foreach ($id in Get-Content $archivo) {
        if ($id -match '^\d+$') { & taskkill /PID $id /T /F 2>$null | Out-Null }   # /T cierra tambien los procesos hijos
    }
    Remove-Item $archivo -Force
}
