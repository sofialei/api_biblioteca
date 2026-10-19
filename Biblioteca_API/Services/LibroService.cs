ususing Biblioteca_API.Data;
using Biblioteca_API.DTOs;
using Biblioteca_API.Interfaces;
using Biblioteca_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca_API.Services;

public class LibroService : ILibroService
{
    private readonly BibliotecaDbContext _context;

    public LibroService(BibliotecaDbContext context)
    {
        _context = context;
    }

    public async Task<List<LibroResponseDTO>> ObtenerTodosAsync()
    {
        var libros = await _context.Libros.AsNoTracking().ToListAsync();
        return libros.Select(ToDto).ToList();
    }

    public async Task<LibroResponseDTO?> ObtenerPorIdAsync(int id)
    {
        var libro = await _context.Libros.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);

        return libro is null ? null : ToDto(libro);
    }

    public async Task<LibroResponseDTO> CrearAsync(LibroDTO dto)
    {
        var isbn = dto.ISBN.Trim();

        if (await _context.Libros.AnyAsync(l => l.ISBN == isbn))
            throw new InvalidOperationException($"Ya existe un libro con el ISBN {isbn}.");

        var libro = new Libro
        {
            Titulo = dto.Titulo.Trim(),
            Autor = dto.Autor.Trim(),
            ISBN = isbn,
            Genero = dto.Genero.Trim(),
            Disponible = true
        };

        _context.Libros.Add(libro);
        await _context.SaveChangesAsync();

        return ToDto(libro);
    }

    public async Task<bool> ActualizarAsync(int id, LibroDTO dto)
    {
        var existente = await _context.Libros.FindAsync(id);
        if (existente is null)
            return false;

        var isbn = dto.ISBN.Trim();

        if (await _context.Libros.AnyAsync(l => l.ISBN == isbn && l.Id != id))
            throw new InvalidOperationException($"Ya existe otro libro con el ISBN {isbn}.");

        existente.Titulo = dto.Titulo.Trim();
        existente.Autor = dto.Autor.Trim();
        existente.ISBN = isbn;
        existente.Genero = dto.Genero.Trim();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var libro = await _context.Libros.FindAsync(id);
        if (libro is null)
            return false;

        if (await _context.Prestamos.AnyAsync(p => p.LibroId == id))
            throw new InvalidOperationException(
                "No se puede eliminar el libro porque tiene préstamos registrados.");

        _context.Libros.Remove(libro);
        await _context.SaveChangesAsync();
        return true;
    }

    private static LibroResponseDTO ToDto(Libro l) => new()
    {
        Id = l.Id,
        Titulo = l.Titulo,
        Autor = l.Autor,
        ISBN = l.ISBN,
        Genero = l.Genero,
        Disponible = l.Disponible
    };
}