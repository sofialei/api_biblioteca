using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Biblioteca_API.Tests;

public class IntegrationTests : IClassFixture<BibliotecaWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IntegrationTests(BibliotecaWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(int libroId, int socioId)> SembrarAsync()
    {
        var libro = await _client.PostAsJsonAsync("/api/libros", new
        {
            titulo = "Rayuela",
            autor = "Cortázar",
            isbn = Random.Shared.NextInt64(1_000_000_000_000, 9_999_999_999_999).ToString(),
            disponible = true
        });
        var socio = await _client.PostAsJsonAsync("/api/socios", new
        {
            nombre = "Ana",
            email = $"{Guid.NewGuid():N}@mail.com",
            dni = Random.Shared.Next(10_000_000, 99_999_999).ToString()
        });

        libro.EnsureSuccessStatusCode();
        socio.EnsureSuccessStatusCode();

        var libroId = (await libro.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        var socioId = (await socio.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
        return (libroId, socioId);
    }

    private async Task<JsonDocument> GraphQLAsync(string query)
    {
        var response = await _client.PostAsJsonAsync("/graphql", new { query });
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }

    private static bool TieneErrores(JsonDocument doc) => doc.RootElement.TryGetProperty("errors", out _);

    [Theory]
    [InlineData("/api/libros")]
    [InlineData("/api/socios")]
    [InlineData("/api/prestamos/activos")]
    public async Task Rest_Get_Listados_Responden200(string url)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Rest_Prestamo_DeLibroYaPrestado_EsRechazado()
    {
        var (libroId, socioId) = await SembrarAsync();
        var body = new { libroId, socioId };

        var primero = await _client.PostAsJsonAsync("/api/prestamos", body);
        var segundo = await _client.PostAsJsonAsync("/api/prestamos", body);

        Assert.True(primero.IsSuccessStatusCode);
        Assert.False(segundo.IsSuccessStatusCode);
    }

    [Fact]
    public async Task GraphQL_Libros_DevuelveLibrosSinErrores()
    {
        await SembrarAsync();

        using var doc = await GraphQLAsync("{ libros { id titulo autor isbn disponible } }");

        Assert.False(TieneErrores(doc));
        Assert.True(doc.RootElement.GetProperty("data").GetProperty("libros").GetArrayLength() > 0);
    }

    [Fact]
    public async Task GraphQL_RegistrarPrestamo_Funciona_YApareceEnActivos()
    {
        var (libroId, socioId) = await SembrarAsync();

        using var mutacion = await GraphQLAsync(
            $"mutation {{ registrarPrestamo(input: {{ libroId: {libroId}, socioId: {socioId} }}) {{ id libroId socioId devuelto }} }}");
        Assert.False(TieneErrores(mutacion));

        using var activos = await GraphQLAsync("{ prestamosActivos { id libroId socioId devuelto } }");
        Assert.False(TieneErrores(activos));
        Assert.True(activos.RootElement.GetProperty("data").GetProperty("prestamosActivos").GetArrayLength() > 0);
    }

    [Fact]
    public async Task GraphQL_RegistrarPrestamo_LibroOcupado_DevuelveError()
    {
        var (libroId, socioId) = await SembrarAsync();
        var mutacion =
            $"mutation {{ registrarPrestamo(input: {{ libroId: {libroId}, socioId: {socioId} }}) {{ id }} }}";

        using var primero = await GraphQLAsync(mutacion);
        using var segundo = await GraphQLAsync(mutacion);

        Assert.False(TieneErrores(primero));
        Assert.True(TieneErrores(segundo));
    }

    [Fact]
    public async Task GraphQL_HistorialSocio_IncluyeLosPrestamosDevueltos()
    {
        var (libroId, socioId) = await SembrarAsync();

        using var creado = await GraphQLAsync(
            $"mutation {{ registrarPrestamo(input: {{ libroId: {libroId}, socioId: {socioId} }}) {{ id }} }}");
        var prestamoId = creado.RootElement.GetProperty("data").GetProperty("registrarPrestamo").GetProperty("id").GetInt32();

        using var devuelto = await GraphQLAsync($"mutation {{ devolverPrestamo(id: {prestamoId}) {{ id devuelto }} }}");
        Assert.False(TieneErrores(devuelto));

        using var historial = await GraphQLAsync($"{{ historialSocio(socioId: {socioId}) {{ id devuelto }} }}");
        Assert.False(TieneErrores(historial));
        Assert.Equal(1, historial.RootElement.GetProperty("data").GetProperty("historialSocio").GetArrayLength());
    }
}
