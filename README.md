# Portal de Sugestões do ERP

Ver `PRD.md` para visão de produto, escopo e fases de construção (seção 14).

## Estrutura do repositório

```
backend/    API .NET 8 (Domain, Application, Infrastructure, Api, Tests)
frontend/   Aplicação Angular + DevExtreme
docs/       Documentação de apoio (contrato de API, etc.)
```

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/) e npm
- [Docker](https://www.docker.com/) (para rodar o SQL Server localmente) — opcional para desenvolvimento sem banco

## Backend

```bash
# subir SQL Server + MailHog (SMTP de teste) local, requer Docker
docker compose up -d

cd backend
dotnet restore
dotnet ef database update --project src/PortalSugestao.Infrastructure --startup-project src/PortalSugestao.Api
dotnet run --project src/PortalSugestao.Api
```

A API sobe com Swagger em `https://localhost:<porta>/swagger`.

A connection string e a chave de assinatura do JWT mock ficam em `appsettings.Development.json` (não versionado — veja `.gitignore`). Se o arquivo não existir na sua máquina, crie-o com base no exemplo abaixo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=PortalSugestao;User Id=sa;Password=Local_dev_only_123!;TrustServerCertificate=True;"
  },
  "MockAuth": {
    "SigningKey": "dev-only-mock-sso-signing-key-nao-usar-em-producao-32chars",
    "Issuer": "PortalSugestao.MockSso",
    "Audience": "PortalSugestao.Api",
    "TokenExpirationMinutes": 480
  },
  "Smtp": {
    "Host": "127.0.0.1",
    "Port": 1025,
    "From": "portal-sugestao@local.test"
  }
}
```

> A senha do `MSSQL_SA_PASSWORD` no `docker-compose.yml` deve ser a mesma usada na connection string acima.

> Use `127.0.0.1` (não `localhost`) no `Smtp:Host` — em alguns ambientes Windows a resolução de `localhost` tenta IPv6 primeiro e o MailHog (via port-forward do Docker Desktop) só responde em IPv4, causando timeouts intermitentes no envio.

### Notificações por e-mail (MailHog)

E-mails de aprovação/rejeição/comentário do Admin (regra 7.5 do PRD) são enviados para um SMTP de teste local — nada sai para a internet. Veja os e-mails recebidos em `http://localhost:8025` (sobe junto com `docker compose up -d`).

### Autenticação (mock)

Não há SSO real com o ERP ainda (ponto em aberto do PRD). Para obter um token de teste:

```bash
curl -X POST https://localhost:<porta>/api/auth/mock-login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@empresa.com","nome":"Admin Teste","empresa":"Empresa Exemplo","role":"AdminInterno"}'
```

Use o `token` retornado no header `Authorization: Bearer <token>` para chamar os demais endpoints (ver `docs/api-contract.md`).

## Frontend

```bash
cd frontend/portal-sugestao
npm install
npm start
```

A aplicação sobe em `http://localhost:4200` e espera a API em `environment.ts` (`apiUrl`).

## Fases do projeto

O progresso de construção segue as fases descritas no `PRD.md` (seção 14). Concluídas até aqui: **Fase 0** (definições e preparação), **Fase 1** (autenticação mock + cadastro base), **Fase 2** (moderação), **Fase 3** (votação e ranking), **Fase 4** (comentários) e **Fase 5** (notificações por e-mail).
