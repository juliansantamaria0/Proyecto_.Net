using AutoTallerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoTallerManager.Infrastructure.Persistence;

public class AutoTallerDbContext(DbContextOptions<AutoTallerDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<OrdenServicio> OrdenesServicio => Set<OrdenServicio>();
    public DbSet<Repuesto> Repuestos => Set<Repuesto>();
    public DbSet<DetalleOrden> DetalleOrdenes => Set<DetalleOrden>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Telefono).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Correo).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Correo).IsUnique();
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.ToTable("Vehiculos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Marca).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Modelo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Vin).HasMaxLength(17).IsRequired();
            entity.HasIndex(e => e.Vin).IsUnique();
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Vehiculos)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrdenServicio>(entity =>
        {
            entity.ToTable("OrdenesServicio");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TrabajoRealizado).HasMaxLength(2000);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.CostoManoObra).HasPrecision(18, 2);
            entity.HasOne(e => e.Vehiculo)
                .WithMany(v => v.OrdenesServicio)
                .HasForeignKey(e => e.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Mecanico)
                .WithMany(u => u.OrdenesAsignadas)
                .HasForeignKey(e => e.MecanicoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Repuesto>(entity =>
        {
            entity.ToTable("Repuestos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Categoria).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 2);
            entity.HasIndex(e => e.Codigo).IsUnique();
        });

        modelBuilder.Entity<DetalleOrden>(entity =>
        {
            entity.ToTable("DetalleOrdenes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CostoUnitario).HasPrecision(18, 2);
            entity.HasOne(e => e.OrdenServicio)
                .WithMany(o => o.Detalles)
                .HasForeignKey(e => e.OrdenServicioId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Repuesto)
                .WithMany(r => r.DetallesOrden)
                .HasForeignKey(e => e.RepuestoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.ToTable("Facturas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroFactura).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MontoManoObra).HasPrecision(18, 2);
            entity.Property(e => e.MontoRepuestos).HasPrecision(18, 2);
            entity.Property(e => e.MontoTotal).HasPrecision(18, 2);
            entity.HasIndex(e => e.NumeroFactura).IsUnique();
            entity.HasOne(e => e.OrdenServicio)
                .WithOne(o => o.Factura)
                .HasForeignKey<Factura>(e => e.OrdenServicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Correo).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
            entity.HasIndex(e => e.Correo).IsUnique();
            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.ToTable("Auditorias");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Entidad).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Detalle).HasMaxLength(1000);
            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
