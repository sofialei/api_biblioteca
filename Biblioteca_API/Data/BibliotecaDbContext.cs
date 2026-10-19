using Biblioteca_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca_API.Data;

public class BibliotecaDbContext : DbContext
{
    public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options)
        : base(options) { }

    public DbSet<Libro> Libros => Set<Libro>();
    public DbSet<Socio> Socios => Set<Socio>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Libro>(e =>
        {
            e.Property(l => l.Titulo).IsRequired().HasMaxLength(200);
            e.Property(l => l.Autor).IsRequired().HasMaxLength(150);
            e.Property(l => l.ISBN).IsRequired().HasMaxLength(20);
            e.HasIndex(l => l.ISBN).IsUnique();
        });

        modelBuilder.Entity<Socio>(e =>
        {
            e.Property(s => s.Nombre).IsRequired().HasMaxLength(150);
            e.Property(s => s.Email).IsRequired().HasMaxLength(150);
            e.Property(s => s.DNI).IsRequired().HasMaxLength(15);
            e.HasIndex(s => s.DNI).IsUnique();
        });

        modelBuilder.Entity<Prestamo>(e =>
        {
            e.HasOne(p => p.Libro)
             .WithMany(l => l.Prestamos)
             .HasForeignKey(p => p.LibroId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(p => p.Socio)
             .WithMany(s => s.Prestamos)
             .HasForeignKey(p => p.SocioId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}