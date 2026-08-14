# Simulador local da api_authentication

O `AuthService` do frontend (`frontend/portal-sugestao/src/app/core/auth/auth.service.ts`) faz
login em duas etapas: primeiro contra a `api_authentication` real do ERP
(`environment.authApiUrl`), depois contra a sessão local do Portal (`POST /api/auth/sessao`).

A `api_authentication` real ainda não existe num ambiente acessível a partir desta máquina de
desenvolvimento. `backend/tools/ErpAuthSimulado` é um projeto ASP.NET Core minimal API que simula
só o endpoint `POST /api/authentication/logar` (mesmo contrato de request/response já implementado
no `AuthService`), pra permitir testar o fluxo completo de login localmente.

**Isto não é a especificação da `api_authentication` real** — é uma simulação escrita a partir do
contrato já assumido no código (`LoginErpResponse` em `auth.service.ts`). Se/quando o time do ERP
fornecer a documentação oficial, ela deve virar a fonte de verdade (e este simulador, ajustado ou
substituído para bater com ela).

## Rodando

```
cd backend/tools/ErpAuthSimulado
dotnet run --urls http://localhost:5112
```

`environment.ts` (dev) já aponta `authApiUrl` para essa porta.

## Usuários demo

| EmpresaID | Login   | Senha       | Perfil               |
|-----------|---------|-------------|-----------------------|
| EMP1      | admin   | admin123    | Admin (`AdminPortalSugestoes: true`) |
| EMP1      | cliente | cliente123  | Cliente               |

Qualquer outra combinação devolve `{ Erro: true, Mensagem: "Usuário ou senha inválidos." }`
(sempre HTTP 200, igual ao contrato real esperado).
