using Biblioteca_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca_API.Data;

public class BibliotecaDbContext : DbContext
{
    public BibliotecaDbContext(DbContextOptions<BibliotecaDbContext> options)
        : base(options)
    {
    }

    public DbSet<Libro> Libros => Set<Libro>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
}