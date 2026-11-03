using Biblioteca_API.DTOs;

namespace Biblioteca_API.Interfaces;

public interface ISocioService
{
    Task<List<SocioResponseDTO>> ObtenerTodosAsync();
    Task<SocioResponseDTO?> ObtenerPorIdAsync(int id);
    Task<SocioResponseDTO> CrearAsync(SocioDTO dto);
    Task<bool> ActualizarAsync(int id, SocioDTO dto);
    Task<bool> EliminarAsync(int id);
}
