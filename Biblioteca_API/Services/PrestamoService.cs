using Biblioteca_API.Data;
using Biblioteca_API.DTOs;
using Biblioteca_API.Interfaces;
using Biblioteca_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca_API.Services;

/// <summary>
/// Reglas de préstamos y devoluciones. REST y GraphQL usan este mismo servicio,
/// así las reglas se aplican igual desde los dos accesos.
/// </summary>
public class PrestamoService : IPrestamoService
{
    private readonly BibliotecaDbContext _context;

    public PrestamoService(BibliotecaDbContext context)
    {
        _context = context;
    }

    public async Task<PrestamoResponseDTO> RegistrarAsync(PrestamoDTO dto)
    {
        var libro = await _context.Libros.FindAsync(dto.LibroId)
            ?? throw new KeyNotFoundException($"No existe el libro con id {dto.LibroId}.");

        var socio = await _context.Socios.FindAsync(dto.SocioId)
            ?? throw new KeyNotFoundException($"No existe el socio con id {dto.SocioId}.");

        // Doble control: la bandera Disponible y la existencia de un préstamo activo.
        var tienePrestamoActivo = await _context.Prestamos
            .AnyAsync(p => p.LibroId == libro.Id && !p.Devuelto);

        if (!libro.Disponible || tienePrestamoActivo)
            throw new InvalidOperationException($"El libro '{libro.Titulo}' ya está prestado.");

        var prestamo = new Prestamo
        {
            LibroId = libro.Id,
            SocioId = socio.Id,
            Libro = libro,
            Socio = socio,
            FechaPrestamo = DateTime.UtcNow,
            Devuelto = false
        };

        libro.Disponible = false;
        _context.Prestamos.Add(prestamo);

        // Un solo SaveChanges: el préstamo y la disponibilidad se guardan juntos.
        await _context.SaveChangesAsync();

        return ToDto(prestamo);
    }

    public async Task<PrestamoResponseDTO> DevolverAsync(int prestamoId)
    {
        var prestamo = await _context.Prestamos
            .Include(p => p.Libro)
            .Include(p => p.Socio)
            .FirstOrDefaultAsync(p => p.Id == prestamoId)
            ?? throw new KeyNotFoundException($"No existe el préstamo con id {prestamoId}.");

        if (prestamo.Devuelto)
            throw new InvalidOperationException("Este préstamo ya fue devuelto.");

        prestamo.Devuelto = true;
        prestamo.FechaDevolucion = DateTime.UtcNow;

        if (prestamo.Libro is not null)
            prestamo.Libro.Disponible = true;

        await _context.SaveChangesAsync();

        return ToDto(prestamo);
    }

    public async Task<List<PrestamoResponseDTO>> ObtenerActivosAsync()
    {
        var prestamos = await _context.Prestamos.AsNoTracking()
            .Include(p => p.Libro)
            .Include(p => p.Socio)
            .Where(p => !p.Devuelto)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToListAsync();

        return prestamos.Select(ToDto).ToList();
    }

    public async Task<List<PrestamoResponseDTO>> ObtenerHistorialPorSocioAsync(int socioId)
    {
        if (!await _context.Socios.AnyAsync(s => s.Id == socioId))
            throw new KeyNotFoundException($"No existe el socio con id {socioId}.");

        // El historial incluye préstamos activos y devueltos; nunca se borran.
        var prestamos = await _context.Prestamos.AsNoTracking()
            .Include(p => p.Libro)
            .Include(p => p.Socio)
            .Where(p => p.SocioId == socioId)
            .OrderByDescending(p => p.FechaPrestamo)
            .ToListAsync();

        return prestamos.Select(ToDto).ToList();
    }

    private static PrestamoResponseDTO ToDto(Prestamo p) => new()
    {
        Id = p.Id,
        LibroId = p.LibroId,
        TituloLibro = p.Libro?.Titulo ?? string.Empty,
        SocioId = p.SocioId,
        NombreSocio = p.Socio?.Nombre ?? string.Empty,
        FechaPrestamo = p.FechaPrestamo,
        FechaDevolucion = p.FechaDevolucion,
        Devuelto = p.Devuelto
    };
}
