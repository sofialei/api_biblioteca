using Biblioteca_API.DTOs;

namespace Biblioteca_API.Interfaces;

public interface ILibroService
{
    Task<List<LibroResponseDTO>> ObtenerTodosAsync();
    Task<LibroResponseDTO?> ObtenerPorIdAsync(int id);
    Task<LibroResponseDTO> CrearAsync(LibroDTO dto);
    Task<bool> ActualizarAsync(int id, LibroDTO dto);
    Task<bool> EliminarAsync(int id);
}