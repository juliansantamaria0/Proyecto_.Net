# AutoTallerManager

Sistema de gestión integral para un taller automotriz: **API REST** en **ASP.NET Core** (arquitectura hexagonal) y **SPA** en JavaScript modular.

## Despliegue en producción

| Componente | Plataforma | URL |
|------------|------------|-----|
| **Aplicación web** (frontend) | Netlify | **https://proyectoneet.netlify.app** |
| **API REST** (backend) | Railway | **https://alert-motivation-production.up.railway.app** |
| **Health check** | Railway | https://alert-motivation-production.up.railway.app/health |

Arquitectura **híbrida**: el frontend estático se sirve desde Netlify; la API y PostgreSQL viven en Railway. En local, la API puede servir también la carpeta `frontend/` en un solo puerto.

### Credenciales de demostración (producción y local)

| Rol | Correo | Contraseña |
|-----|--------|------------|
| Admin | admin@autotaller.com | Admin123! |
| Mecánico | mecanico@autotaller.com | Mecanico123! |
| Recepcionista | recepcion@autotaller.com | Recepcion123! |

Los clientes se registran desde la pantalla de registro (`POST /api/auth/register`).

### Variables de entorno (referencia)

No subir secretos al repositorio. Configurar en los paneles de cada plataforma:

**Railway (API)**

| Variable | Descripción |
|----------|-------------|
| `DATABASE_URL` | Referencia al servicio PostgreSQL (Railway la inyecta al enlazar la BD) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `FRONTEND_URL` | `https://proyectoneet.netlify.app` (CORS) |
| `JWT__Key` | Clave JWT segura (distinta a la de desarrollo) |

**Netlify (frontend)**

| Variable | Descripción |
|----------|-------------|
| `API_BASE_URL` | `https://alert-motivation-production.up.railway.app` (sin `/api`; el build la añade) |

El archivo `netlify.toml` en la raíz define base `frontend`, comando `node scripts/generate-env.js` y publicación de la SPA.

---

## Arquitectura

```
src/
├── Domain/          # Entidades, enums, reglas de negocio, interfaces
├── Application/     # DTOs, casos de uso, AutoMapper
├── Infrastructure/  # EF Core, repositorios, Unit of Work
└── API/             # Controladores, JWT, Rate Limiting, Swagger
```

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 14+ (Docker en puerto **5433** o instalación local en **5432**)

- **Producción / Railway:** solo **PostgreSQL** (`DATABASE_URL` o `DatabaseProvider: PostgreSQL`).
- **Desarrollo local:** **SQLite** por defecto (`appsettings.Development.json`) o **PostgreSQL** con Docker (`docker compose up -d`).

> **Nota (entorno Development):** al ejecutar con el perfil por defecto (`ASPNETCORE_ENVIRONMENT=Development`), `appsettings.Development.json` usa **SQLite** (`autotaller.db`) para poder probar sin Docker. Si sigue los pasos de PostgreSQL más abajo, use `docker compose up -d` y, opcionalmente, alinee `appsettings.Development.json` con la misma cadena de conexión de `appsettings.json`, o ejecute con `ASPNETCORE_ENVIRONMENT=Production`.

## Frontend (SPA)

El frontend está en la carpeta `frontend/` y se sirve automáticamente al ejecutar la API.

```bash
docker compose up -d
dotnet run --project src/API/AutoTallerManager.API.csproj
```

Abra **http://localhost:5192** (o el puerto HTTP configurado) en el navegador.

### Estructura del frontend

```
frontend/
├── index.html
├── css/styles.css
└── js/
    ├── config.js      # URL API, roles, constantes
    ├── auth.js        # JWT (localStorage o sessionStorage según "recordarme")
    ├── api.js         # Cliente fetch centralizado
    ├── router.js      # Enrutador por hash
    ├── ui.js          # Toasts, modales, paginación
    ├── utils.js
    ├── app.js         # Punto de entrada
    └── views/         # Módulos por pantalla
```

### Credenciales demo

Ver tabla en [Despliegue en producción](#despliegue-en-producción) o **Usuarios de prueba (seed)** más abajo.

---

## Configuración de base de datos (PostgreSQL)

### Paso 1 — Iniciar PostgreSQL con Docker

Asegúrese de que **Docker Desktop** esté en ejecución, luego:

```bash
docker compose up -d
```

Esto crea la base `AutoTallerDB` en **localhost:5433** con usuario/contraseña `postgres`/`postgres`.

### Paso 2 — Ejecutar la API

```bash
dotnet run --project src/API/AutoTallerManager.API.csproj
```

Las migraciones EF Core y el seed de datos se aplican automáticamente al iniciar.

### PostgreSQL local (sin Docker)

Si usa una instalación local en el puerto **5432**, copie el ejemplo y ajuste su contraseña:

```bash
copy src\API\appsettings.Local.json.example src\API\appsettings.Local.json
```

Edite `appsettings.Local.json` con su contraseña real. Ese archivo no se sube a git.

### Migraciones manuales

Se aplican automáticamente al iniciar la API. También puede ejecutarlas manualmente:

```bash
dotnet ef database update --project src/Infrastructure/AutoTallerManager.Infrastructure.csproj --startup-project src/API/AutoTallerManager.API.csproj
```

## Ejecución

```bash
dotnet run --project src/API/AutoTallerManager.API.csproj
```

- Swagger UI: `https://localhost:7xxx/swagger`
- API base: `https://localhost:7xxx/api`

## Usuarios de prueba (seed)

| Rol            | Correo                   | Contraseña      |
|----------------|--------------------------|-----------------|
| Admin          | admin@autotaller.com     | Admin123!       |
| Mecánico       | mecanico@autotaller.com  | Mecanico123!    |
| Recepcionista  | recepcion@autotaller.com | Recepcion123!   |

Los usuarios con rol **Cliente** se crean por autoregistro (ver abajo), no vienen en el seed.

## Autenticación JWT

1. `POST /api/auth/login` con `{ "correo": "...", "password": "..." }`
2. Copie el `token` de la respuesta
3. En Swagger, pulse **Authorize** e ingrese: `Bearer {token}`

### Registro público (rol Cliente)

| Método | Ruta | Descripción |
|--------|------|-------------|
| `POST` | `/api/auth/register` | Alta de cliente + usuario (sin autenticación) |
| `POST` | `/api/usuarios/registrar` | Mismo flujo; controlador público dedicado |

Cuerpo: `{ "nombre", "telefono", "correo", "password" }`. Devuelve JWT y datos de usuario.

## Endpoints principales

| Recurso           | Ruta base                  | Roles principales              |
|-------------------|----------------------------|--------------------------------|
| Clientes          | `/api/clientes`            | Admin, Recepcionista           |
| Vehículos         | `/api/vehiculos`           | Admin, Recepcionista           |
| Órdenes servicio  | `/api/ordenesservicio`     | Todos (según acción)           |
| Repuestos         | `/api/repuestos`           | Admin (CRUD), otros (lectura)  |
| Facturas          | `/api/facturas`            | Admin, Mecánico                |
| Usuarios          | `/api/usuarios`            | Admin (CRUD staff)             |
| Registro cliente  | `/api/usuarios/registrar`  | Anónimo                        |
| Mi perfil         | `/api/clientes/mi-perfil`  | Cliente                        |
| Auditorías        | `/api/auditorias`          | Admin                          |

### Paginación

Todos los listados aceptan `pageNumber` y `pageSize`. La respuesta incluye el encabezado `X-Total-Count`.

### Rate Limiting

- `/api/ordenesservicio/*`: 60 solicitudes/minuto
- `/api/repuestos/*`: 30 solicitudes/minuto
- Respuesta al exceder: HTTP **429**

## Casos de uso implementados

- **RegistrarClienteConVehiculo**: `POST /api/clientes/registrar-con-vehiculos`
- **CrearOrdenServicio**: `POST /api/ordenesservicio`
- **ActualizarOrdenConTrabajoRealizado**: `PUT /api/ordenesservicio/{id}/trabajo`
- **GenerarFactura**: `POST /api/facturas/generar`

## Reglas de negocio

- Un vehículo no puede tener dos órdenes activas simultáneamente
- Validación de stock antes de asignar repuestos
- Descuento de inventario al completar una orden
- No se eliminan clientes/vehículos con órdenes activas
- Fecha estimada de entrega según tipo de servicio

## Migraciones

- `InitialCreate` — esquema inicial con todas las entidades del dominio

---

## Checklist de entrega

| Ítem | Estado |
|------|--------|
| URLs públicas documentadas (Netlify + Railway) | ✅ |
| Login y API operativos en producción | ✅ |
| Desarrollo local sin romper producción (`Development` → SQLite) | ✅ |
| Secretos fuera de git (`.gitignore`: `appsettings.Local.json`, `*.db`) | ✅ |
| CORS configurado (`FRONTEND_URL` en Railway) | ✅ |
| `netlify.toml` + variable `API_BASE_URL` en Netlify | ✅ |

**Antes de entregar el repo:** haga `git push` con el `README.md` y `netlify.toml` actualizados. Si el enunciado pide video o memoria, adjunte capturas de ambas URLs y un login de prueba con el usuario Admin.
