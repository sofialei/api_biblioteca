using Biblioteca_API.Data;
using Biblioteca_API.DTOs;
using Biblioteca_API.Models;
using Biblioteca_API.Services;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca_API.Tests;

public class PrestamoServiceTests
{
    private static async Task<(PrestamoService service, BibliotecaDbContext context, Libro libro, Socio socio)> CrearAsync()
    {
        var options = new DbContextOptionsBuilder<BibliotecaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new BibliotecaDbContext(options);

        var libro = new Libro
        {
            Titulo = "Rayuela",
            Autor = "Julio Cortázar",
            ISBN = "9781234567890",
            Genero = "Novela",
            Disponible = true
        };
        var socio = new Socio { Nombre = "Ana Pérez", Email = "ana@mail.com", DNI = "12345678" };

        context.Libros.Add(libro);
        context.Socios.Add(socio);
        await context.SaveChangesAsync();

        return (new PrestamoService(context), context, libro, socio);
    }

    [Fact]
    public async Task Registrar_LibroDisponible_CreaPrestamoYMarcaLibroNoDisponible()
    {
        var (service, context, libro, socio) = await CrearAsync();

        var prestamo = await service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = socio.Id });

        Assert.True(prestamo.Id > 0);
        Assert.False(prestamo.Devuelto);
        Assert.Null(prestamo.FechaDevolucion);
        Assert.Equal("Rayuela", prestamo.TituloLibro);
        Assert.Equal("Ana Pérez", prestamo.NombreSocio);
        Assert.False((await context.Libros.FindAsync(libro.Id))!.Disponible);
    }

    [Fact]
    public async Task Registrar_LibroYaPrestado_LanzaExcepcion()
    {
        var (service, _, libro, socio) = await CrearAsync();
        await service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = socio.Id });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = socio.Id }));
    }

    [Fact]
    public async Task Registrar_LibroInexistente_LanzaKeyNotFound()
    {
        var (service, _, _, socio) = await CrearAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.RegistrarAsync(new PrestamoDTO { LibroId = 999, SocioId = socio.Id }));
    }

    [Fact]
    public async Task Registrar_SocioInexistente_LanzaKeyNotFound()
    {
        var (service, _, libro, _) = await CrearAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = 999 }));
    }

    [Fact]
    public async Task Devolver_PrestamoActivo_MarcaDevueltoYLiberaLibro()
    {
        var (service, context, libro, socio) = await CrearAsync();
        var prestamo = await service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = socio.Id });

        var devuelto = await service.DevolverAsync(prestamo.Id);

        Assert.True(devuelto.Devuelto);
        Assert.NotNull(devuelto.FechaDevolucion);
        Assert.True((await context.Libros.FindAsync(libro.Id))!.Disponible);
    }

    [Fact]
    public async Task Devolver_DosVeces_LanzaExcepcion()
    {
        var (service, _, libro, socio) = await CrearAsync();
        var prestamo = await service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = socio.Id });
        await service.DevolverAsync(prestamo.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DevolverAsync(prestamo.Id));
    }

    [Fact]
    public async Task Devolver_PrestamoInexistente_LanzaKeyNotFound()
    {
        var (service, _, _, _) = await CrearAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.DevolverAsync(999));
    }

    [Fact]
    public async Task Registrar_LibroDevuelto_PuedePrestarseDeNuevo()
    {
        var (service, _, libro, socio) = await CrearAsync();
        var primero = await service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = socio.Id });
        await service.DevolverAsync(primero.Id);

        var segundo = await service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = socio.Id });

        Assert.NotEqual(primero.Id, segundo.Id);
        Assert.False(segundo.Devuelto);
    }

    [Fact]
    public async Task ObtenerActivos_SoloDevuelveLosNoDevueltos()
    {
        var (service, context, libro, socio) = await CrearAsync();

        var otroLibro = new Libro { Titulo = "Ficciones", Autor = "Borges", ISBN = "9780987654321", Genero = "Cuento" };
        context.Libros.Add(otroLibro);
        await context.SaveChangesAsync();

        var devuelto = await service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = socio.Id });
        await service.DevolverAsync(devuelto.Id);
        var activo = await service.RegistrarAsync(new PrestamoDTO { LibroId = otroLibro.Id, SocioId = socio.Id });

        var activos = await service.ObtenerActivosAsync();

        Assert.Single(activos);
        Assert.Equal(activo.Id, activos[0].Id);
    }

    [Fact]
    public async Task ObtenerHistorial_IncluyeActivosYDevueltosSoloDelSocio()
    {
        var (service, context, libro, socio) = await CrearAsync();

        var otroLibro = new Libro { Titulo = "Ficciones", Autor = "Borges", ISBN = "9780987654321", Genero = "Cuento" };
        var otroSocio = new Socio { Nombre = "Beto Gómez", Email = "beto@mail.com", DNI = "11111111" };
        context.Libros.Add(otroLibro);
        context.Socios.Add(otroSocio);
        await context.SaveChangesAsync();

        var primero = await service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = socio.Id });
        await service.DevolverAsync(primero.Id);
        await service.RegistrarAsync(new PrestamoDTO { LibroId = libro.Id, SocioId = socio.Id });
        await service.RegistrarAsync(new PrestamoDTO { LibroId = otroLibro.Id, SocioId = otroSocio.Id });

        var historial = await service.ObtenerHistorialPorSocioAsync(socio.Id);

        Assert.Equal(2, historial.Count);
        Assert.All(historial, p => Assert.Equal(socio.Id, p.SocioId));
    }

    [Fact]
    public async Task ObtenerHistorial_SocioInexistente_LanzaKeyNotFound()
    {
        var (service, _, _, _) = await CrearAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ObtenerHistorialPorSocioAsync(999));
    }
}
