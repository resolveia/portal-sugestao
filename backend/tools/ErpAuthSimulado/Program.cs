using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "Frontend";

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    // Mesma convenção do api_portal_sugestoes local: PascalCase, igual aos nomes em C#.
    options.SerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

app.UseCors(CorsPolicy);

// Simula a api_authentication real do ERP (ver docs/erp-auth-simulador.md) só pra permitir testar,
// em ambiente local, o fluxo de login que o AuthService do frontend hoje chama de verdade
// (environment.authApiUrl). Substituir authApiUrl pela URL real assim que ela existir.
app.MapPost("/api/authentication/logar", (LogarRequest request) =>
{
    var usuario = UsuariosDemo.Todos.FirstOrDefault(u =>
        string.Equals(u.EmpresaId, request.EmpresaID, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(u.Login, request.Login, StringComparison.OrdinalIgnoreCase) &&
        u.Senha == request.Senha);

    if (usuario is null)
    {
        return Results.Ok(new LogarResponse(true, "Usuário ou senha inválidos.", null));
    }

    return Results.Ok(new LogarResponse(
        false,
        null,
        new UsuarioErpDto(usuario.Nome, usuario.Login, usuario.Id, usuario.EmpresaId, usuario.AdminPortalSugestoes)));
});

app.Run();

internal record LogarRequest(string EmpresaID, string Login, string Senha, string Modulo);

internal record LogarResponse(bool Erro, string? Mensagem, UsuarioErpDto? Usuario);

internal record UsuarioErpDto(string Nome, string Login, int Id, string EmpresaId, bool AdminPortalSugestoes);

internal record UsuarioDemo(string EmpresaId, string Login, string Senha, int Id, string Nome, bool AdminPortalSugestoes);

internal static class UsuariosDemo
{
    public static readonly UsuarioDemo[] Todos =
    [
        new UsuarioDemo("EMP1", "admin", "admin123", 1, "Admin ERP (demo)", AdminPortalSugestoes: true),
        new UsuarioDemo("EMP1", "cliente", "cliente123", 2, "Cliente ERP (demo)", AdminPortalSugestoes: false)
    ];
}
