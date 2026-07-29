using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortalSugestao.Infrastructure.Data;

namespace PortalSugestao.Tests;

/// <summary>
/// Sobe a API real (Program.cs) para testes de integração, trocando o SQL Server por um
/// banco EF Core InMemory isolado por instância — não depende de Docker/SQL Server rodando.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"TestDb_{Guid.NewGuid()}";

    static CustomWebApplicationFactory()
    {
        // Program.cs lê MockAuth/Smtp de builder.Configuration antes do Build(); variáveis de
        // ambiente já estão disponíveis nesse ponto, diferente de ConfigureAppConfiguration.
        // Não depende do appsettings.Development.json (gitignored, pode não existir na máquina).
        Environment.SetEnvironmentVariable("MockAuth__SigningKey", "test-signing-key-not-for-production-use-32-chars-min");
        Environment.SetEnvironmentVariable("MockAuth__Issuer", "PortalSugestao.MockSso");
        Environment.SetEnvironmentVariable("MockAuth__Audience", "PortalSugestao.Api");
        Environment.SetEnvironmentVariable("MockAuth__TokenExpirationMinutes", "480");
        Environment.SetEnvironmentVariable("Smtp__Host", string.Empty);
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins__0", "http://localhost:4200");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PortalSugestaoDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<PortalSugestaoDbContext>(options => options.UseInMemoryDatabase(_dbName));
        });
    }

    /// <summary>Novo DbContext apontando para o mesmo banco InMemory da API, para inspecionar o estado nos asserts.</summary>
    public PortalSugestaoDbContext CreateDbContext() =>
        Services.CreateScope().ServiceProvider.GetRequiredService<PortalSugestaoDbContext>();
}
