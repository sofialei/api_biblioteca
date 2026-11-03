using Biblioteca_API.Data;
using Biblioteca_API.DTOs;
using Biblioteca_API.Interfaces;
using Biblioteca_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca_API.Services;

public class SocioService : ISocioService
{
    private readonly BibliotecaDbContext _context;

    public SocioService(BibliotecaDbContext context)
    {
        _context = context;
    }

    public async Task<List<SocioResponseDTO>> ObtenerTodosAsync()
    {
        var socios = await _context.Socios.AsNoTracking().ToListAsync();
        return socios.Select(ToDto).ToList();
    }

    public async Task<SocioResponseDTO?> ObtenerPorIdAsync(int id)
    {
        var socio = await _context.Socios.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
        return socio is null ? null : ToDto(socio);
    }

    public async Task<SocioResponseDTO> CrearAsync(SocioDTO dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var dni = dto.DNI.Trim();

        if (await _context.Socios.AnyAsync(s => s.Email == email))
            throw new InvalidOperationException($"Ya existe un socio con el email {email}.");

        if (await _context.Socios.AnyAsync(s => s.DNI == dni))
            throw new InvalidOperationException($"Ya existe un socio con el DNI {dni}.");

        var socio = new Socio
        {
            Nombre = dto.Nombre.Trim(),
            Email = email,
            DNI = dni
        };

        _context.Socios.Add(socio);
        await _context.SaveChangesAsync();
        return ToDto(socio);
    }

    public async Task<bool> ActualizarAsync(int id, SocioDTO dto)
    {
        var existente = await _context.Socios.FindAsync(id);
        if (existente is null)
            return false;

        var email = dto.Email.Trim().ToLowerInvariant();
        var dni = dto.DNI.Trim();

        if (await _context.Socios.AnyAsync(s => s.Email == email && s.Id != id))
            throw new InvalidOperationException($"Ya existe otro socio con el email {email}.");

        if (await _context.Socios.AnyAsync(s => s.DNI == dni && s.Id != id))
            throw new InvalidOperationException($"Ya existe otro socio con el DNI {dni}.");

        existente.Nombre = dto.Nombre.Trim();
        existente.Email = email;
        existente.DNI = dni;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var socio = await _context.Socios.FindAsync(id);
        if (socio is null)
            return false;

        if (await _context.Prestamos.AnyAsync(p => p.SocioId == id))
            throw new InvalidOperationException(
                "No se puede eliminar el socio porque tiene préstamos registrados.");

        _context.Socios.Remove(socio);
        await _context.SaveChangesAsync();
        return true;
    }

    private static SocioResponseDTO ToDto(Socio s) => new()
    {
        Id = s.Id,
        Nombre = s.Nombre,
        Email = s.Email,
        DNI = s.DNI
    };
}
