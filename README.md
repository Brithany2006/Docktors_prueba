# Doktors - Frontend (Laboratorio Clínico)

Aplicación web desarrollada en Blazor para la interfaz de usuario del sistema de laboratorio médico Doktors. Permite la administración de pacientes, gestión de doctores, dashboards y seguimiento de citas.

---

## 🛠️ Tecnologías Utilizadas

* **Framework:** Blazor WebAssembly / Server (.NET)
* **Lenguaje:** C# / Razor / HTML / CSS
* **Despliegue / Servidor:** Docker / NGINX

---

## 📂 Estructura Principal del Proyecto

* `Services/`: Servicios para la comunicación con la API Backend (autenticación, pacientes, exámenes).
* `Shared/`: Componentes reutilizables de interfaz (dashboards, pestañas de gestión).
* `wwwroot/`: Archivos estáticos (estilos CSS, imágenes, scripts).

---

## 🚀 Cómo Ejecutar el Proyecto

### Requisitos Previos
* .NET SDK 8.0 o superior

### Pasos de Instalación
1. Abrir la carpeta del proyecto en Visual Studio Code.
2. Verificar que la API Backend (`LaboratorioApi`) esté en ejecución.
3. Iniciar el proyecto desde la terminal:
   ```bash
   dotnet run