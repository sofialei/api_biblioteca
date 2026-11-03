using Biblioteca_API.Data;
using Biblioteca_API.DTOs;
using Biblioteca_API.Models;
using Biblioteca_API.Services;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca_API.Tests;

public class SocioServiceTests
{
    private static (SocioService service, BibliotecaDbContext context) Crear()
    {
        var options = new DbContextOptionsBuilder<BibliotecaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new BibliotecaDbContext(options);
        return (new SocioService(context), context);
    }

    private static SocioDTO SocioValido(string email = "ana@mail.com", string dni = "12345678") => new()
    {
        Nombre = "Ana Pérez",
        Email = email,
        DNI = dni
    };

    [Fact]
    public async Task Crear_SocioNuevo_AsignaIdYNormalizaEmail()
    {
        var (service, _) = Crear();

        var creado = await service.CrearAsync(SocioValido(email: "  Ana@Mail.COM "));

        Assert.True(creado.Id > 0);
        Assert.Equal("ana@mail.com", creado.Email);
    }

    [Fact]
    public async Task Crear_DniDuplicado_LanzaExcepcion()
    {
        var (service, _) = Crear();
        await service.CrearAsync(SocioValido());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CrearAsync(SocioValido(email: "otro@mail.com")));
    }

    [Fact]
    public async Task Crear_EmailDuplicado_LanzaExcepcion()
    {
        var (service, _) = Crear();
        await service.CrearAsync(SocioValido());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CrearAsync(SocioValido(dni: "87654321")));
    }

    [Fact]
    public async Task ObtenerTodos_DevuelveLosSociosCreados()
    {
        var (service, _) = Crear();
        await service.CrearAsync(SocioValido());
        await service.CrearAsync(SocioValido("beto@mail.com", "11111111"));

        Assert.Equal(2, (await service.ObtenerTodosAsync()).Count);
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
        var creado = await service.CrearAsync(SocioValido());

        var dto = SocioValido();
        dto.Nombre = "Ana María Pérez";

        Assert.True(await service.ActualizarAsync(creado.Id, dto));
        Assert.Equal("Ana María Pérez", (await service.ObtenerPorIdAsync(creado.Id))!.Nombre);
    }

    [Fact]
    public async Task Actualizar_Inexistente_DevuelveFalse()
    {
        var (service, _) = Crear();

        Assert.False(await service.ActualizarAsync(999, SocioValido()));
    }

    [Fact]
    public async Task Actualizar_ConDniDeOtroSocio_LanzaExcepcion()
    {
        var (service, _) = Crear();
        await service.CrearAsync(SocioValido());
        var otro = await service.CrearAsync(SocioValido("beto@mail.com", "11111111"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ActualizarAsync(otro.Id, SocioValido("beto@mail.com", "12345678")));
    }

    [Fact]
    public async Task Eliminar_SinPrestamos_Elimina()
    {
        var (service, _) = Crear();
        var creado = await service.CrearAsync(SocioValido());

        Assert.True(await service.EliminarAsync(creado.Id));
        Assert.Null(await service.ObtenerPorIdAsync(creado.Id));
    }

    [Fact]
    public async Task Eliminar_Inexistente_DevuelveFalse()
    {
        var (service, _) = Crear();

        Assert.False(await service.EliminarAsync(999));
    }

    [Fact]
    public async Task Eliminar_ConPrestamos_LanzaExcepcion()
    {
        var (service, context) = Crear();
        var creado = await service.CrearAsync(SocioValido());

        context.Prestamos.Add(new Prestamo { LibroId = 1, SocioId = creado.Id });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.EliminarAsync(creado.Id));
    }
}
