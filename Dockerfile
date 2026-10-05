# Etapa 1: Compilar la aplicación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LaboratorioFront.csproj", "./"]
RUN dotnet restore "LaboratorioFront.csproj"
COPY . .
RUN dotnet publish "LaboratorioFront.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Servir con NGINX
FROM nginx:alpine
WORKDIR /usr/share/nginx/html
# Limpiar el directorio de nginx por defecto
RUN rm -rf ./*
# Copiar los archivos compilados de blazor
COPY --from=build /app/publish/wwwroot .
# Copiar configuración personalizada de nginx para manejar el enrutamiento SPA
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
