using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortalSugestao.Api.Auth;
using PortalSugestao.Infrastructure.Data;
using PortalSugestao.Infrastructure.Notificacoes;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "PortalSugestaoFrontend";

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // Alinhado à convenção da plataforma (api_authentication/api_portal_sugestoes real):
        // JSON em PascalCase, igual aos nomes das propriedades em C# (ver docs/api-contract.md).
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT da sessão local (emitido por /api/auth/sessao). Informe apenas o token (sem o prefixo 'Bearer').",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<PortalSugestaoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<MockAuthOptions>(builder.Configuration.GetSection(MockAuthOptions.SectionName));
builder.Services.AddScoped<MockTokenService>();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddScoped<NotificacaoService>();

var mockAuthOptions = builder.Configuration.GetSection(MockAuthOptions.SectionName).Get<MockAuthOptions>()
    ?? throw new InvalidOperationException("Seção 'MockAuth' não configurada em appsettings.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = mockAuthOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = mockAuthOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mockAuthOptions.SigningKey)),
            ValidateLifetime = true
        };
        // O fluxo real do ERP autentica via cookie HttpOnly (não Authorization header) —
        // ver docs/sso-checklist.md. Se não vier no cookie, cai no header Bearer padrão
        // (usado hoje pelos testes de integração via TestHelpers).
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(AuthCookieDefaults.Name, out var cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Testes de integração usam o ambiente "Testing" e não têm um endpoint HTTPS real —
// UseHttpsRedirection causaria um 307 para um host que não existe no TestServer.
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// Marcador necessário para o <c>WebApplicationFactory&lt;Program&gt;</c> nos testes de integração
/// conseguir referenciar esta classe gerada implicitamente pelos top-level statements.
/// </summary>
public partial class Program { }
