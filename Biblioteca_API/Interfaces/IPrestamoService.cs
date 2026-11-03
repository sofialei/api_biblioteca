using Biblioteca_API.DTOs;

namespace Biblioteca_API.Interfaces;

public interface IPrestamoService
{
    /// <exception cref="KeyNotFoundException">El libro o el socio no existe.</exception>
    /// <exception cref="InvalidOperationException">El libro ya está prestado.</exception>
    Task<PrestamoResponseDTO> RegistrarAsync(PrestamoDTO dto);

    /// <exception cref="KeyNotFoundException">El préstamo no existe.</exception>
    /// <exception cref="InvalidOperationException">El préstamo ya fue devuelto.</exception>
    Task<PrestamoResponseDTO> DevolverAsync(int prestamoId);

    Task<List<PrestamoResponseDTO>> ObtenerActivosAsync();

    /// <exception cref="KeyNotFoundException">El socio no existe.</exception>
    Task<List<PrestamoResponseDTO>> ObtenerHistorialPorSocioAsync(int socioId);
}
