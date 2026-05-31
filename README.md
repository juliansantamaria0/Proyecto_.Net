# AutoTallerManager

Backend RESTful para la gestión integral de un taller automotriz, implementado con **ASP.NET Core** y arquitectura hexagonal (Ports & Adapters).

## Arquitectura

```
src/
├── AutoTallerManager.Domain/          # Entidades, enums, reglas de negocio, interfaces
├── AutoTallerManager.Application/     # DTOs, casos de uso, AutoMapper
├── AutoTallerManager.Infrastructure/  # EF Core, repositorios, Unit of Work
└── AutoTallerManager.API/             # Controladores, JWT, Rate Limiting, Swagger
```

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 14+ (Docker en puerto **5433** o instalación local en **5432**)

El proveedor activo es **PostgreSQL** (`DatabaseProvider: PostgreSQL` en `appsettings.json`). La rama MySQL no está habilitada en esta versión.

## Frontend (SPA)

El frontend está en la carpeta `frontend/` y se sirve automáticamente al ejecutar la API.

```bash
docker compose up -d
dotnet run --project src/AutoTallerManager.API
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

Ver tabla de usuarios de prueba arriba.

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
dotnet run --project src/AutoTallerManager.API
```

Las migraciones EF Core y el seed de datos se aplican automáticamente al iniciar.

### PostgreSQL local (sin Docker)

Si usa una instalación local en el puerto **5432**, copie el ejemplo y ajuste su contraseña:

```bash
copy src\AutoTallerManager.API\appsettings.Local.json.example src\AutoTallerManager.API\appsettings.Local.json
```

Edite `appsettings.Local.json` con su contraseña real. Ese archivo no se sube a git.

### Migraciones manuales

Se aplican automáticamente al iniciar la API. También puede ejecutarlas manualmente:

```bash
dotnet ef database update --project src/AutoTallerManager.Infrastructure --startup-project src/AutoTallerManager.API
```

## Ejecución

```bash
dotnet run --project src/AutoTallerManager.API
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
