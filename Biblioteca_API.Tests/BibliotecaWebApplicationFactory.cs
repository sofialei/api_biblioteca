using Biblioteca_API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Biblioteca_API.Tests;

public class BibliotecaWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "BibliotecaTests-" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<BibliotecaDbContext>));
            services.AddDbContext<BibliotecaDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }
}
