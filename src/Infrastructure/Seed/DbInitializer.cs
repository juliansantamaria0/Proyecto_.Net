using AutoTallerManager.Domain.Entities;
using AutoTallerManager.Domain.Enums;
using AutoTallerManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Npgsql;

namespace AutoTallerManager.Infrastructure.Seed;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AutoTallerDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AutoTallerDbContext>>();
        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "MySQL";

        try
        {
            if (provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
                await context.Database.EnsureCreatedAsync();
            else
                await context.Database.MigrateAsync();
        }
        catch (MySqlException ex) when (ex.Number is 1045 or 1049)
        {
            logger.LogCritical(
                "Error de conexión MySQL (acceso o base de datos). Verifique ConnectionStrings:DefaultConnection " +
                "y que el servidor MySQL esté en ejecución.");
            throw;
        }
        catch (MySqlException ex)
        {
            logger.LogCritical(
                ex,
                "No se pudo conectar a MySQL. Inicie el servidor MySQL o cambie DatabaseProvider a SQLite/PostgreSQL.");
            throw;
        }
        catch (PostgresException ex) when (ex.SqlState == "28P01")
        {
            logger.LogCritical(
                "Autenticación PostgreSQL fallida. Corrija la contraseña o use DatabaseProvider=MySQL/SQLite.");
            throw;
        }
        catch (NpgsqlException ex)
        {
            logger.LogCritical(
                ex,
                "No se pudo conectar a PostgreSQL. Inicie el servidor o cambie DatabaseProvider en appsettings.");
            throw;
        }

        if (await context.Usuarios.AnyAsync())
            return;

        logger.LogInformation("Seeding initial data...");

        var admin = new Usuario
        {
            Nombre = "Administrador",
            Correo = "admin@autotaller.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Rol = RolUsuario.Admin,
            Activo = true
        };

        var mecanico = new Usuario
        {
            Nombre = "Juan Mecánico",
            Correo = "mecanico@autotaller.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mecanico123!"),
            Rol = RolUsuario.Mecanico,
            Activo = true
        };

        var recepcionista = new Usuario
        {
            Nombre = "María Recepcionista",
            Correo = "recepcion@autotaller.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Recepcion123!"),
            Rol = RolUsuario.Recepcionista,
            Activo = true
        };

        context.Usuarios.AddRange(admin, mecanico, recepcionista);

        context.Repuestos.AddRange(
            new Repuesto { Codigo = "FIL-001", Descripcion = "Filtro de aceite", Categoria = "Filtros", CantidadStock = 50, StockMinimo = 10, PrecioUnitario = 15.99m },
            new Repuesto { Codigo = "PAST-001", Descripcion = "Pastillas de freno", Categoria = "Frenos", CantidadStock = 8, StockMinimo = 15, PrecioUnitario = 45.50m },
            new Repuesto { Codigo = "ACE-001", Descripcion = "Aceite motor 5W30", Categoria = "Lubricantes", CantidadStock = 100, StockMinimo = 20, PrecioUnitario = 28.00m }
        );

        await context.SaveChangesAsync();
        logger.LogInformation("Seed data created successfully.");
    }
}
