# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

# Copiar el proyecto y restaurar las dependencias
COPY . .
RUN dotnet restore

# Construir el proyecto
RUN dotnet build --no-restore

# Comando para ejecutar el servicio
CMD ["dotnet", "run"]
