# syntax=docker/dockerfile:1.7

# ===========================================================================
# Stage 1: Build & publish
# Usamos la imagen completa del SDK de .NET, que pesa varios GB. Solo se usa
# para compilar y publicar el binario. Después se descarta — no acaba en la
# imagen final. Este es el patrón "multi-stage build" estándar.
# ===========================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos primero solo el csproj y restauramos los paquetes NuGet.
# Si en builds futuros el código cambia pero el csproj no, Docker reutiliza
# esta capa cacheada y nos ahorra descargar los paquetes otra vez.
COPY BlipChallengeApi.csproj ./
RUN dotnet restore BlipChallengeApi.csproj

# Ahora copiamos el resto del código y publicamos en modo Release.
COPY . ./
RUN dotnet publish BlipChallengeApi.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ===========================================================================
# Stage 2: Runtime
# Imagen mínima con solo el runtime de ASP.NET Core. Mucho más pequeña que
# la del SDK (~220MB vs ~1.5GB). Esta es la que corre en producción.
# ===========================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# El puerto en el que va a escuchar la app dentro del contenedor.
EXPOSE 8080

# Le decimos a Kestrel (el servidor HTTP de ASP.NET Core) que escuche en
# 0.0.0.0:8080. Sin esto, escucharía solo en localhost dentro del contenedor
# y no sería accesible desde fuera.
ENV ASPNETCORE_URLS=http://+:8080

# Copiamos solo los binarios publicados desde el stage de build.
# El código fuente, los obj/, los bin/, los paquetes NuGet... nada de eso
# acaba en la imagen final. Solo lo necesario para ejecutar.
COPY --from=build /app/publish ./

# Punto de entrada del contenedor.
ENTRYPOINT ["dotnet", "BlipChallengeApi.dll"]
