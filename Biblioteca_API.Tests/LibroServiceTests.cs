using Biblioteca_API.Data;
using Biblioteca_API.DTOs;
using Biblioteca_API.Models;
using Biblioteca_API.Services;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca_API.Tests;

public class LibroServiceTests
{
    private static (LibroService service, BibliotecaDbContext context) Crear()
    {
        var options = new DbContextOptionsBuilder<BibliotecaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new BibliotecaDbContext(options);
        return (new LibroService(context), context);
    }

    private static LibroDTO LibroValido(string isbn = "9781234567890") => new()
    {
        Titulo = "Rayuela",
        Autor = "Julio Cortázar",
        ISBN = isbn,
        Genero = "Novela"
    };

    [Fact]
    public async Task Crear_LibroNuevo_QuedaDisponible()
    {
        var (service, _) = Crear();

        var creado = await service.CrearAsync(LibroValido());

        Assert.True(creado.Id > 0);
        Assert.True(creado.Disponible);
    }

    [Fact]
    public async Task Crear_IsbnDuplicado_LanzaExcepcion()
    {
        var (service, _) = Crear();
        await service.CrearAsync(LibroValido());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CrearAsync(LibroValido()));
    }

    [Fact]
    public async Task ObtenerPorId_Inexistente_DevuelveNull()
    {
        var (service, _) = Crear();

        Assert.Null(await service.ObtenerPorIdAsync(999));
    }

    [Fact]
    public async Task Actualizar_Existente_CambiaLosDatos()
    {
        var (service, _) = Crear();
        var creado = await service.CrearAsync(LibroValido());

        var dto = LibroValido();
        dto.Titulo = "Rayuela (edición nueva)";

        Assert.True(await service.ActualizarAsync(creado.Id, dto));
        Assert.Equal("Rayuela (edición nueva)",
            (await service.ObtenerPorIdAsync(creado.Id))!.Titulo);
    }

    [Fact]
    public async Task Actualizar_Inexistente_DevuelveFalse()
    {
        var (service, _) = Crear();

        Assert.False(await service.ActualizarAsync(999, LibroValido()));
    }

    [Fact]
    public async Task Eliminar_SinPrestamos_Elimina()
    {
        var (service, _) = Crear();
        var creado = await service.CrearAsync(LibroValido());

        Assert.True(await service.EliminarAsync(creado.Id));
        Assert.Null(await service.ObtenerPorIdAsync(creado.Id));
    }

    [Fact]
    public async Task Eliminar_ConPrestamos_LanzaExcepcion()
    {
        var (service, context) = Crear();
        var creado = await service.CrearAsync(LibroValido());

        context.Prestamos.Add(new Prestamo { LibroId = creado.Id, SocioId = 1 });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EliminarAsync(creado.Id));
    }
}