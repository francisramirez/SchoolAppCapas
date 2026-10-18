# SchoolApp — Arquitectura N Capas (3 proyectos)

Solución de ejemplo para **Visual Studio 2022** y **.NET 8**.

## Proyectos
- **SchoolApp.Web**: ASP.NET Core MVC, controladores, vistas Razor y configuración.
- **SchoolApp.BL**: servicios, validaciones y reglas de negocio.
- **SchoolApp.DAL**: modelos, EF Core, SQLite, DbContext y repositorios.

## Ejecución
1. Instala .NET SDK 8 y abre `SchoolApp.sln` en Visual Studio 2022.
2. Restaura los paquetes NuGet.
3. Establece `SchoolApp.Web` como proyecto de inicio.
4. Ejecuta con F5 o desde la terminal: `dotnet run --project SchoolApp.Web`.
5. Abre la URL indicada en consola. La base SQLite `schoolapp.db` se crea automáticamente en el directorio de trabajo.

## Referencias
`SchoolApp.Web` → `SchoolApp.BL` y `SchoolApp.DAL` (solo para registrar infraestructura en `Program.cs`).
`SchoolApp.BL` → `SchoolApp.DAL`.
`SchoolApp.DAL` → Entity Framework Core SQLite.

Este ejemplo usa `EnsureCreated` para facilitar la primera ejecución; para producción se recomiendan migraciones de EF Core, pruebas y DTOs independientes del modelo de persistencia.

**Nota:** Esta es una solución nueva de ejemplo, no una conversión del código original, ya que no se proporcionó el proyecto fuente.
