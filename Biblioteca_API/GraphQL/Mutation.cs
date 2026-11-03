using Biblioteca_API.DTOs;
using Biblioteca_API.Interfaces;
using HotChocolate;

namespace Biblioteca_API.GraphQL;

public class Mutation
{
    public async Task<PrestamoResponseDTO> RegistrarPrestamo(
        PrestamoDTO input,
        [Service] IPrestamoService prestamoService)
    {
        try
        {
            return await prestamoService.RegistrarAsync(input);
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
        {
            throw new GraphQLException(ex.Message);
        }
    }

    public async Task<PrestamoResponseDTO> DevolverPrestamo(
        int id,
        [Service] IPrestamoService prestamoService)
    {
        try
        {
            return await prestamoService.DevolverAsync(id);
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
        {
            throw new GraphQLException(ex.Message);
        }
    }
}
