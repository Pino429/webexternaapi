# Etapa base con runtime de ASP.NET 6
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Etapa de build con SDK de .NET 6
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copiar archivos del proyecto
COPY . .

# Publicar el proyecto
RUN dotnet publish WebClienteCore.csproj -c Release -o /app/publish

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Comando de inicio
ENTRYPOINT ["dotnet", "WebClienteCore.dll"]
