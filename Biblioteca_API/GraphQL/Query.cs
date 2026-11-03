using Biblioteca_API.DTOs;
using Biblioteca_API.Interfaces;
using HotChocolate;

namespace Biblioteca_API.GraphQL;

public class Query
{
    public Task<List<LibroResponseDTO>> GetLibros([Service] ILibroService libroService)
        => libroService.ObtenerTodosAsync();

    public Task<LibroResponseDTO?> GetLibro(int id, [Service] ILibroService libroService)
        => libroService.ObtenerPorIdAsync(id);

    public Task<List<SocioResponseDTO>> GetSocios([Service] ISocioService socioService)
        => socioService.ObtenerTodosAsync();

    public Task<SocioResponseDTO?> GetSocio(int id, [Service] ISocioService socioService)
        => socioService.ObtenerPorIdAsync(id);

    public Task<List<PrestamoResponseDTO>> GetPrestamosActivos([Service] IPrestamoService prestamoService)
        => prestamoService.ObtenerActivosAsync();

    public async Task<List<PrestamoResponseDTO>> GetHistorialSocio(int socioId, [Service] IPrestamoService prestamoService)
    {
        try
        {
            return await prestamoService.ObtenerHistorialPorSocioAsync(socioId);
        }
        catch (KeyNotFoundException ex)
        {
            throw new GraphQLException(ex.Message);
        }
    }
}
