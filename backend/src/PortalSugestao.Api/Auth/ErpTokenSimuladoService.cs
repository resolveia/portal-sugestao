using System.Text;
using System.Text.Json;
using PortalSugestao.Domain.Enums;

namespace PortalSugestao.Api.Auth;

/// <summary>
/// Simula o token criptografado que o ERP vai enviar via SSO real (PRD, ponto em aberto #1).
/// O time do ERP definiu (2026-08-12) que o token é um dado criptografado usado para identificar
/// o usuário; o algoritmo/chave real ainda não foi definido. Até lá, este serviço só faz um
/// base64 de um JSON simples — sem criptografia de verdade. Substituir por decriptação real
/// (algoritmo/chave a combinar com o time do ERP) quando isso for definido.
/// </summary>
public class ErpTokenSimuladoService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static readonly ErpTokenSimuladoPayload AdminDemo = new(
        ErpUserId: "erp-demo-admin",
        Nome: "Admin ERP (demo)",
        Email: "admin.demo@erp.local",
        Empresa: "ERP Demo Ltda",
        Role: RoleUsuario.AdminInterno);

    public static readonly ErpTokenSimuladoPayload ClienteDemo = new(
        ErpUserId: "erp-demo-cliente",
        Nome: "Cliente ERP (demo)",
        Email: "cliente.demo@erp.local",
        Empresa: "Empresa Cliente Demo Ltda",
        Role: RoleUsuario.Cliente);

    public string GerarToken(ErpTokenSimuladoPayload payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public ErpTokenSimuladoPayload? Decodificar(string token)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            return JsonSerializer.Deserialize<ErpTokenSimuladoPayload>(json, JsonOptions);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }
    }
}

public record ErpTokenSimuladoPayload(string ErpUserId, string Nome, string Email, string Empresa, RoleUsuario Role);
