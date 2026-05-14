# Blip Challenge API

Mi entrega al challenge técnico de Blip.

La API devuelve los 5 repositorios C# más antiguos de Takenet (el antiguo nombre de Blip) consultando GitHub en tiempo real. El bot de Blip consume esa API y los muestra al usuario en un carrusel.


## Stack

- .NET 10 con ASP.NET Core
- HttpClient para hablar con la API de GitHub
- Azure App Service (plan F1, gratuito) como hosting
- GitHub Actions para el deploy automático en cada push a main
- Bot configurado en la plataforma de Blip

Si necesitas correrlo en .NET 8 solo hay que cambiar el TargetFramework en el csproj.


## Correr la API localmente

    git clone https://github.com/nacho995/BlipChallengeApi.git
    cd BlipChallengeApi
    dotnet run

Después abre /swagger en el navegador para ver la documentación interactiva, o llama directamente al endpoint:

    GET /api/repositories


## URL desplegada

https://blipchallengeapi-nacho995.azurewebsites.net/api/repositories

Swagger UI: https://blipchallengeapi-nacho995.azurewebsites.net/swagger

Aviso: el tier gratuito de Azure deja la app dormida tras un rato sin tráfico. La primera petición puede tardar 30-60 segundos (cold start), las siguientes son instantáneas.


## Importar el bot en Blip

El flujo está exportado en el .json que hay en el repo. Para importarlo:

1. Entras al portal de Blip (https://portal.blip.ai)
2. Abres la sección Builder
3. Importas el archivo .json desde el menú

Docs oficiales: https://help.blip.ai/hc/en-us/articles/360051989154


## Decisiones de diseño

Separé el código en tres carpetas para que cada cosa tenga su sitio. Los controladores solo manejan la petición HTTP, los services hablan con GitHub y filtran los repos, y los models describen los datos que entran y salen. Si en el futuro hubiera que cambiar la fuente de datos solo tocaría el service.

El controlador no instancia el service directamente, lo recibe por inyección de dependencias a través de una interfaz (IGithubService). El HttpClient está registrado con AddHttpClient tipado, que es lo que recomienda Microsoft para evitar problemas de socket pooling y poder configurar la base URL en un sitio.

El carrusel de Blip no está hardcodeado. Hay un bloque de script que recibe el JSON de la API y construye los items dinámicamente iterando sobre el array, así que si la API devuelve más o menos repos el bot se adapta solo sin tocar el flujo.

Las validaciones de input del usuario en el bot se hacen con regex permisivas, para que "Hola", "HOLA", "hola!" o "holaa" pasen igual. La idea es no romper la conversación por una mayúscula o un signo de admiración.
