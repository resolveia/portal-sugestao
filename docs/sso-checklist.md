# Checklist — Definição do SSO entre ERP e Portal de Sugestões

> Referente ao **ponto em aberto #1** do PRD (seção 12) e à **Fase 6 — Integração com o ERP** (seção 14).
> **Atualizado em 2026-08-12** com as definições da reunião do usuário com o time técnico do ERP.

## Resolvido (reunião de 2026-08-12)

| Pergunta | Definição |
|---|---|
| A API de validação do token é nossa ou de um serviço central? | **Nossa** — implementada pelo próprio time do Portal, dentro do ERP. Pode deixar de ser necessária no futuro (possível centralização), mas hoje é local. |
| O que é o token? | Um **dado criptografado** que a API usa para identificar/conectar o usuário. |
| Quais campos vêm no login? | Flexível — o time do ERP devolve os campos que a aplicação pedir. |
| Domínio | Portal e API ficam no **mesmo domínio** — elimina complicação de cookie cross-site. |
| Formato de erro | Sempre **200 OK**, com `{ Erro: true, Mensagem: "..." }` no corpo em caso de falha (exceto rota inexistente, que é 404 real). |
| Validade da sessão | **Cookie HttpOnly, 24h, sem renovação.** Em 401, redirecionar para a página de login. |
| Login manual continua existindo? | **Sim** — em paralelo ao login automático via token. Precisa de uma rota separada que recebe o token da URL e loga automaticamente. |

## Implementado nesta sessão (simulado, ver Fase 6 no PRD)

Como o algoritmo/chave de criptografia real do token ainda não foi definido, a arquitetura-alvo já foi implementada com um **token simulado** (fácil de trocar depois):

- Backend: cookie HttpOnly `portal_sugestao_session` (24h) substituindo o Bearer/localStorage; `POST /api/auth/login-token` (equivalente à rota que o ERP vai chamar); `ErpTokenSimuladoService` decodifica um token fake (base64, sem criptografia real); `POST /api/auth/logout` limpa o cookie; `GET /api/auth/tokens-demo` gera tokens de demonstração para os dois perfis.
- Frontend: rota `/login/token` (lê `?token=...` da URL, loga automaticamente) convivendo com `/login` (manual, que ganhou botões para simular a entrada via ERP nos dois perfis).
- Validado ponta a ponta via Playwright e curl — ver PRD.md, Fase 6.

## Ainda em aberto — próxima rodada com o time do ERP

1. **Algoritmo de criptografia do token** (AES simétrico? RSA?) e como a **chave/certificado** será compartilhada com segurança entre os times.
2. **O token decriptado já traz os dados do usuário** (nome, email, empresa, role) **ou só um identificador** que exige uma consulta extra (a alguma API ou ao banco do ERP) para buscar esses dados? Se for isso, como o Portal acessa essa fonte?
3. **Lista exata de campos** a pedir: confirmar que o ERP consegue mandar **id do usuário, nome, email, empresa/cliente de origem e uma role/perfil** que diferencie Cliente de Admin interno do Portal.
4. **Nome do cookie**: seguir um padrão já usado nos outros apps da empresa, ou o Portal fica livre para definir (hoje usa `portal_sugestao_session`)?
5. **Data/URL real do botão "Portal de Sugestão"** dentro do ERP e ambiente de homologação para o teste de integração fim a fim.

## Próximos passos após a definição do ponto 1-2

1. Substituir `ErpTokenSimuladoService` pela decriptação real (algoritmo/chave definidos com o ERP).
2. Implementar o botão "Portal de Sugestão" dentro do ERP, apontando para `/login/token?token=...`.
3. Testes de integração ponta a ponta em homologação.
