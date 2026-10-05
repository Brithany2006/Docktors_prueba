using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using LaboratorioFront;
using LaboratorioFront.Services;
using System.Net.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ==========================================
// ?? CONFIGURACIÓN BASE DE LA API
// ==========================================
string apiBase;

if (builder.HostEnvironment.IsDevelopment())
{
    // ??? ENTORNO LOCAL: Apunta estrictamente a tu API local en Visual Studio
    // (Asegúrate de que este puerto coincida con el HTTP o HTTPS de tu launchSettings.json)
    apiBase = "http://localhost:5000/";
}
else
{
    // ?? ENTORNO DE PRODUCCIÓN: Lee de la configuración o usa el dominio real
    apiBase = builder.Configuration["ApiBaseUrl"] ?? "https://doktors.med.ec/";
}

// Asegurar que termine con "/"
if (!apiBase.EndsWith("/"))
    apiBase += "/";

// ==========================================
// ?? REGISTRAR HttpClient ÚNICO con timeout extendido
// ==========================================
builder.Services.AddScoped(sp =>
{
    var http = new HttpClient
    {
        BaseAddress = new Uri(apiBase),
        Timeout = TimeSpan.FromMinutes(10) // Timeout aumentado para sincronizaciones
    };
    return http;
});

// ==========================================
// ?? SERVICIOS (inyección de dependencias)
// ==========================================
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<TokenStore>(); // Guarda/carga JWT del localStorage
builder.Services.AddScoped<CajaService>();
builder.Services.AddScoped<LaboratorioFront.Services.DerivacionesService>();

// ==========================================
// ?? CONSTRUIR Y EJECUTAR APLICACIÓN
// ==========================================
var host = builder.Build();

// Al iniciar, aplicar el token almacenado si existe
var api = host.Services.GetRequiredService<ApiService>();
var tokens = host.Services.GetRequiredService<TokenStore>();
var token = await tokens.GetAsync();
api.SetToken(token);

// Inicia la app
await host.RunAsync();
