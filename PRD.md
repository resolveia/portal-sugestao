# PRD — Portal de Sugestões do ERP

## 1. Visão Geral

O **Portal de Sugestões** é um novo módulo/aplicação satélite ao ERP, que permite aos clientes do sistema sugerir novas funcionalidades, votar em sugestões já cadastradas e acompanhar quais itens são os mais votados — servindo como insumo para priorização do roadmap de evolução do produto.

O portal é acessado a partir de um botão **"Portal de Sugestão"** dentro do ERP, que abre a aplicação em uma nova aba do navegador, autenticando o cliente via SSO com a sessão já ativa no ERP.

## 2. Objetivos

- Dar aos clientes um canal formal e transparente para propor melhorias no ERP.
- Priorizar o backlog de evolução do produto com base em dados reais de demanda (votos).
- Reduzir sugestões duplicadas/dispersas hoje enviadas por e-mail, suporte ou informalmente.
- Aumentar o engajamento e a percepção de que o cliente é ouvido no roadmap do produto.

## 3. Problema / Motivação

Atualmente não existe um canal centralizado para captar e priorizar pedidos de novas funcionalidades vindos dos clientes. Sugestões se perdem em e-mails, chamados de suporte ou conversas informais, dificultando a equipe de produto a identificar o que realmente tem maior demanda entre a base de clientes.

## 4. Público-alvo

- **Clientes do ERP** (usuários finais das empresas que usam o sistema): sugerem, votam e comentam.
- **Equipe interna (Admin/Produto)**: modera, aprova, categoriza e gerencia as sugestões.

## 5. Escopo (MVP — fase única)

Todas as funcionalidades abaixo compõem a primeira e única fase planejada no momento:

1. **Cadastro de sugestões** pelo cliente (produto/ERP de origem, título, descrição, resultado esperado com a solicitação, categoria/módulo — todos obrigatórios).
2. **Moderação/aprovação** pela equipe interna antes da sugestão ficar pública.
3. **Votação** em sugestões aprovadas, com limite de **3 votos ativos por usuário**, com **realocação** (o cliente pode remover um voto de uma sugestão e aplicá-lo em outra a qualquer momento).
4. **Listagem/ranking de mais votadas** — visão pública ordenada por número de votos.
5. **Comentários** nas sugestões (thread de discussão por sugestão).
6. **Categorias/módulos** e **Produtos** (ERPs comercializados pela empresa — ex: AJORS.OOH, AJORS.SIGN) cadastrados e mantidos dentro do próprio portal pela equipe interna. Toda sugestão indica a qual produto se refere.
7. **Notificações por e-mail** ao cliente quando houver mudanças relevantes na sugestão (ex: aprovação, rejeição, nova resposta/comentário da equipe).
8. **Autenticação via SSO** com o ERP (mecanismo de token a definir com o time técnico do ERP).
9. **Multi-tenant compartilhado**: sugestões e votos são visíveis e compartilhados entre todos os clientes/empresas que usam o ERP (portal único, não isolado por empresa).
10. **Status de andamento tipo Kanban/roadmap público** (Em análise, Planejado, Em desenvolvimento, Lançado) — definido pelo Admin interno em sugestões já publicadas, visível a todos no ranking (ver seção 7.6). Sugestões marcadas como "Lançado" deixam de aceitar novos votos.

### Fora de escopo (nesta fase)

- Perfis intermediários de moderação (ex: suporte/CS); apenas **Cliente** e **Admin interno**.
- Isolamento de sugestões por empresa/tenant.
- Integração automática de categorias com módulos já existentes no ERP (categorias são geridas de forma independente no portal).

## 6. Perfis de Usuário e Permissões

| Ação | Cliente | Admin Interno |
|---|---|---|
| Criar sugestão | ✅ | ✅ |
| Editar/excluir própria sugestão (antes de aprovada) | ✅ | ✅ |
| Aprovar/rejeitar sugestão | ❌ | ✅ |
| Editar/mesclar/ocultar sugestão já publicada | ❌ | ✅ |
| Votar (até 3 ativos, com realocação) | ✅ | ❌* |
| Comentar | ✅ | ✅ |
| Moderar/excluir comentários | ❌ | ✅ |
| Gerenciar categorias e produtos (ERP) | ❌ | ✅ |
| Definir estágio de roadmap de sugestão publicada | ❌ | ✅ |
| Ver ranking de mais votadas | ✅ | ✅ |

\* Admin interno não computa votos de cliente; a definir se admin também poderá votar em nome da equipe de produto (ponto em aberto).

## 7. Regras de Negócio

### 7.1 Sugestões
- Toda sugestão nova entra com status **"Em moderação"** (não pública, não vota-se nela).
- Admin interno aprova (→ **"Publicada"**) ou rejeita (→ **"Rejeitada"**, com justificativa opcional visível ao autor).
- Sugestão publicada é visível a todos os clientes (multi-tenant compartilhado) e pode receber votos e comentários.
- Campos mínimos: produto (ERP de origem), título, descrição, resultado esperado com a solicitação, categoria, autor, empresa/cliente de origem, data de criação, status. Todos obrigatórios no cadastro.

### 7.2 Votação
- Cada usuário cliente tem no máximo **3 votos ativos simultâneos**, aplicáveis apenas a sugestões **publicadas e ainda não construídas/lançadas** (estágio de roadmap `Lançado` — regra imposta pela API, ver seção 7.6).
- O cliente pode remover um voto de uma sugestão e realocá-lo para outra a qualquer momento.
- Um usuário não pode votar mais de uma vez na mesma sugestão.
- O voto é vinculado ao usuário individual (não à empresa/conta cliente).

### 7.3 Comentários
- Disponível apenas em sugestões publicadas.
- Cliente e Admin podem comentar; Admin pode remover comentários (moderação reativa).

### 7.4 Categorias e Produtos
- Mantidas exclusivamente dentro do portal pelo Admin interno (CRUD simples — incluir, renomear e desativar), sem sincronização com o ERP nesta fase.
- **Produtos** representam os ERPs comercializados pela empresa (ex: AJORS.OOH, AJORS.SIGN) e são obrigatórios no cadastro de sugestão, junto com a categoria.
- "Excluir" categoria/produto é, na prática, uma desativação (fica oculto do cadastro de novas sugestões, mas permanece visível/gerenciável pelo Admin) — exclusão física não é permitida enquanto houver sugestões vinculadas.

### 7.5 Notificações
- Disparadas por e-mail ao autor da sugestão nos eventos: aprovação, rejeição, comentário feito pelo **Admin interno**, e sugestão marcada como **"Lançado"** no roadmap (comentário de outro cliente e demais mudanças de estágio intermediárias não disparam notificação — decisão tomada para evitar excesso de e-mails, resolvendo o ponto em aberto #3 original).

### 7.6 Estágio de Roadmap
- Toda sugestão **publicada** pode receber um estágio de andamento, definido pelo Admin interno: **Em análise → Planejado → Em desenvolvimento → Lançado** (nesta ordem sugerida, mas sem transição forçada — o Admin pode pular ou voltar estágios).
- Antes do Admin definir o primeiro estágio, a sugestão aparece "sem estágio" no ranking (não é o mesmo que "Em análise" — é a ausência de classificação).
- Estágio **"Lançado"** encerra a elegibilidade a novos votos (regra 7.2) e dispara notificação por e-mail ao autor (regra 7.5), uma única vez, na transição.
- Visível a todos os usuários autenticados no ranking (Cliente e Admin), como parte da transparência de priorização (objetivo — seção 2).

## 8. Fluxos Principais

**Fluxo 1 — Cliente cria sugestão**
1. Cliente clica em "Portal de Sugestão" no ERP → abre nova aba autenticada via SSO.
2. Cliente acessa "Nova sugestão" (popup), preenche produto (ERP), título, descrição, resultado esperado e categoria.
3. Sugestão entra como "Em moderação".
4. Admin recebe (fila de moderação), aprova ou rejeita.
5. Cliente é notificado por e-mail do resultado.

**Fluxo 2 — Cliente vota**
1. Cliente navega pela lista/ranking de sugestões publicadas.
2. Clica em "Votar" numa sugestão (se tiver voto disponível, dos 3).
3. Se já usou os 3 votos, sistema oferece opção de remover voto de outra sugestão para realocar.

**Fluxo 3 — Admin modera**
1. Admin acessa fila de sugestões pendentes.
2. Aprova, rejeita (com motivo) ou edita antes de publicar.
3. Após publicada, pode ocultar/mesclar duplicadas posteriormente.

## 9. Arquitetura Técnica

- **Banco de dados**: SQL Server.
- **Backend**: API REST em .NET 8.
- **Frontend**: Angular + DevExtreme (componentes de grid, formulários, dashboard para o ranking).
- **Autenticação**: login (ID da empresa/Login/Senha, tela `/login`) contra a `api_authentication` do ERP — real ou seu simulador local `backend/tools/ErpAuthSimulado` enquanto a real não existe num ambiente acessível (ver `docs/erp-auth-simulador.md`) — seguido de sessão local via **cookie HttpOnly** emitida por `POST /api/auth/sessao` com os dados devolvidos pela `api_authentication`. Revisão de 2026-08-14 do mecanismo definido com o time do ERP em 2026-08-12 (ver `docs/sso-checklist.md`): a abordagem anterior de token simulado via URL (`?token=...`) foi substituída por login direto contra a `api_authentication`. **Importante**: a doc-fonte dessa revisão (`docs/autenticacao-e-api-portal-sugestoes.md`, citada nos comentários do código) não está neste repositório — se o time do ERP fornecer a especificação oficial, ela deve ser conferida contra o que está implementado.
- **Contrato de API**: alinhado ao padrão real da plataforma — todos os endpoints são POST, resposta sempre `{Erro, Mensagem, ...dados}` com HTTP 200 (só falha técnica usa status HTTP real), JSON em PascalCase (ver `docs/api-contract.md`).
- **Integração com ERP**: hoje o Portal não recebe mais um handoff automático via URL do botão do ERP — o usuário loga diretamente pela tela `/login` do Portal, que autentica contra a `api_authentication`. Botão real dentro do ERP e fluxo de abertura automática ainda não implementados/testados com o ERP de verdade.

## 10. Modelo de Dados (entidades principais — visão preliminar)

- **Usuario** (id, nome, email, empresa/cliente de origem, referência ao usuário do ERP, role)
- **Sugestao** (id, produtoId, titulo, descricao, resultadoEsperado, categoriaId, autorId, status, estagioRoadmap, dataCriacao, dataModeracao, motivoRejeicao, moderadorId)
- **Categoria** (id, nome, ativo)
- **Produto** (id, nome, ativo) — ERPs comercializados pela empresa (ex: AJORS.OOH, AJORS.SIGN)
- **Voto** (id, sugestaoId, usuarioId, dataVoto)
- **Comentario** (id, sugestaoId, usuarioId, texto, dataCriacao)
- **NotificacaoLog** (id, usuarioId, tipo, sugestaoId, dataEnvio) — opcional, para auditoria de e-mails enviados

## 11. Requisitos Não-Funcionais

- Multi-tenant compartilhado: performance da listagem/ranking deve suportar todos os clientes do ERP simultaneamente.
- Auditoria básica de ações de moderação (quem aprovou/rejeitou e quando).
- Interface responsiva (acesso via desktop, principal, mas navegável em mobile).
- Tempo de carregamento do ranking de mais votadas deve ser rápido mesmo com grande volume de sugestões/votos (considerar cache/otimização de contagem de votos).

> **Status (2026-08-03)**: testado com carga sintética de 5.000 sugestões/21.000 votos — identificado e corrigido um gargalo de N+1/materialização desnecessária no EF Core, e implementada paginação server-side no ranking (`GET /api/sugestoes`), eliminando o crescimento do payload com o volume de dados. Metodologia e números completos em `docs/performance-report.md`. Responsividade mobile também ajustada e validada (menu hamburguer abaixo de 860px, popups com largura fluida) — sem scroll horizontal da página em nenhuma tela.

## 12. Pontos em Aberto

1. **Mecanismo técnico de SSO** entre ERP e Portal — mecanismo original **definido com o time técnico do ERP em 2026-08-12** (ver `docs/sso-checklist.md`) foi **revisado em 2026-08-14**: em vez do token simulado via URL, o login agora é feito direto contra a `api_authentication` real do ERP (tela `/login` do Portal envia ID/Login/Senha), com sessão local via **cookie HttpOnly** emitido por `POST /api/auth/sessao`. **Ainda em aberto**: (a) a `api_authentication` real não existe num ambiente acessível — hoje só há um simulador local (`backend/tools/ErpAuthSimulado`, ver `docs/erp-auth-simulador.md`); (b) não há mais handoff automático via botão do ERP (o usuário loga manualmente pela tela do Portal); (c) a doc-fonte da revisão de 2026-08-14 (`docs/autenticacao-e-api-portal-sugestoes.md`) não está neste repositório — conferir contra a especificação oficial do time do ERP quando disponível. Ver Fase 6.
2. ~~Status de andamento (Kanban/roadmap público)~~ — **Resolvido (2026-08-08)**: implementado como estágio de roadmap (Em análise/Planejado/Em desenvolvimento/Lançado) na sugestão publicada, gerenciado pelo Admin numa tela dedicada (`/roadmap`) e visível a todos no ranking (ver seção 7.6).
3. ~~Regras de e-mail: quais eventos exatos disparam notificação~~ — **Resolvido**: notifica apenas aprovação, rejeição e comentário feito pelo Admin interno; comentário de outro cliente não notifica (ver seção 7.5).
4. Se Admin interno também poderá votar (representando a equipe de produto) ou fica restrito a clientes. **Implementado como restrito a clientes** (Admin não vota) — revisitar se a necessidade de a equipe de produto votar surgir.
5. Política de duplicidade: como o Admin deve tratar sugestões duplicadas (mesclar votos ao unificar duas sugestões?). Ainda não implementado.

## 13. Métricas de Sucesso

- Número de sugestões cadastradas e aprovadas por período.
- Número de votos totais e usuários únicos votando.
- Taxa de sugestões que avançam de "mais votada" para efetivamente entrarem no roadmap/build.
- Redução de pedidos de funcionalidade recebidos via canais informais (suporte/e-mail).

## 14. Fases de Construção

Ainda que o escopo funcional (seção 5) seja tratado como MVP de fase única, a **construção técnica** será feita de forma incremental, entregando valor testável a cada etapa. Cada fase pressupõe a anterior concluída.

> **Status geral (atualizado em 2026-08-14)**: Fases 0 a 5 concluídas e validadas ponta a ponta (inclui o estágio de roadmap/Kanban, ponto em aberto #2, antecipado da Fase 8); Fase 7 (testes funcionais, performance, paginação e responsividade mobile) concluída, restando apenas a validação com stakeholders/usuários-piloto; Fase 6 (SSO real) em andamento — login revisado pra autenticar direto contra a `api_authentication` do ERP (real ou simulador local) + contrato de API migrado pro padrão real da plataforma (POST-only, envelope `{Erro, Mensagem}`, PascalCase); falta a `api_authentication` real, o botão/handoff automático dentro do ERP e testes de integração em homologação. Repositório: https://github.com/resolveia/portal-sugestao (CI configurado — GitHub Actions roda a suíte completa de testes a cada push/PR pra `master`).

### Fase 0 — Definições e Preparação
- Definir mecanismo técnico de SSO com o time do ERP (ponto em aberto #1).
- Modelagem final do banco de dados (SQL Server) a partir da visão preliminar (seção 10).
- Setup dos repositórios, pipelines de build/deploy e ambientes (dev/homologação).
- Definição do contrato de API (endpoints REST) entre frontend Angular e backend .NET 8.

**Status: ✅ Concluída.** Monorepo com backend .NET 8 (camadas Domain/Application/Infrastructure/Api/Tests) e frontend Angular 22 + DevExtreme. SSO real ainda não definido pelo time do ERP (ponto em aberto #1) — autenticação mock via JWT criada como substituto temporário (ver Fase 1). Contrato de API documentado em `docs/api-contract.md`.

### Fase 1 — Fundação (Autenticação e Cadastro Base)
- Autenticação via SSO funcional (recebendo token/sessão do ERP e validando na API).
- CRUD de **Categorias** pelo Admin interno.
- Cadastro de **Sugestões** pelo cliente (título, descrição, categoria), com status inicial "Em moderação".
- Listagem simples de sugestões (sem ranking/votação ainda).

**Status: ✅ Concluída** (com autenticação mock no lugar do SSO real — ponto em aberto #1). Cadastro de sugestão evoluiu além do previsto originalmente: ganhou os campos **Produto (ERP de origem)** e **Resultado esperado**, ambos obrigatórios (ver seções 5, 7.1 e 10). CRUD de Categorias e Produtos com edição (renomear) e desativação, além de criação — implementado como popup de cadastro no frontend.

### Fase 2 — Moderação
- Fila de moderação para o Admin interno.
- Aprovação (→ "Publicada") e rejeição (→ "Rejeitada", com justificativa).
- Edição de sugestão pelo cliente antes da aprovação.
- Auditoria básica de quem aprovou/rejeitou e quando (RNF seção 11).

**Status: ✅ Concluída.**

### Fase 3 — Votação e Ranking
- Votação em sugestões publicadas, com limite de 3 votos ativos por usuário.
- Realocação de votos (remover de uma sugestão, aplicar em outra).
- Ranking público ordenado por número de votos, com otimização/cache para performance (RNF seção 11).

**Status: ✅ Concluída.** Otimização/paginação do ranking aprofundada na Fase 7 (ver abaixo) depois de testes de carga com grande volume de dados.

### Fase 4 — Comentários
- Thread de comentários em sugestões publicadas.
- Moderação reativa de comentários pelo Admin (remoção).

**Status: ✅ Concluída.**

### Fase 5 — Notificações
- Envio de e-mails ao autor nos eventos definidos (aprovação, rejeição, comentário — conforme resolução do ponto em aberto #3).
- Registro opcional em `NotificacaoLog` para auditoria de envios.

**Status: ✅ Concluída.** Envio via MailKit/SMTP (MailHog local em desenvolvimento); decisão tomada sobre o ponto em aberto #3 — notifica apenas comentário do Admin, não de outros clientes (ver seção 7.5). Falha no envio de e-mail não bloqueia a ação principal (aprovar/rejeitar/comentar).

### Fase 6 — Integração com o ERP
- Botão "Portal de Sugestão" implementado dentro do ERP.
- Abertura do portal em nova aba com SSO real (fim a fim, substituindo mocks de autenticação usados nas fases anteriores).
- Testes de integração entre ERP e Portal em ambiente de homologação.

**Status: 🔶 Em andamento.** Mecanismo original definido com o time do ERP em 2026-08-12 (ponto em aberto #1), **revisado em 2026-08-14** pra autenticar direto contra a `api_authentication` real (em vez do token simulado via URL):
- **Backend**: autenticação por **cookie HttpOnly** (`portal_sugestao_session`, 24h) em vez de Bearer/localStorage — `JwtBearerEvents.OnMessageReceived` lê o cookie (com fallback pro header `Authorization`, usado pelos testes). `POST /api/auth/sessao` recebe os dados já autenticados pela `api_authentication` (Nome, Login, Id, EmpresaId, AdminPortalSugestoes) e cria/atualiza o `Usuario` local + emite o cookie — não valida credencial (quem faz isso é a `api_authentication`). `POST /api/auth/logout` limpa o cookie. As rotas antigas (`mock-login`, `login-token`, `tokens-demo`) e o `ErpTokenSimuladoService` foram removidos.
- **Contrato de API**: todos os endpoints (não só auth) migraram pro padrão POST-only com envelope `{Erro, Mensagem, ...dados}` sempre HTTP 200 e JSON em PascalCase, alinhado ao contrato real da plataforma (ver `docs/api-contract.md`). Autorização por role passou de `[Authorize(Roles=)]` pra checagem manual em cada ação, já que erro de permissão também precisa devolver 200 com `Erro: true`.
- **Frontend**: tela `/login` agora pede ID da empresa/Login/Senha e os envia direto pra `api_authentication` (`environment.authApiUrl`), repassando o resultado pra `POST /api/auth/sessao`. A rota `/login/token` e os botões de simulação de entrada via ERP foram removidos — não há mais handoff automático via URL. `AuthService`, `SugestoesService` e `ComentariosService` migrados pro novo contrato POST/envelope.
- **Simulador local**: como a `api_authentication` real não existe num ambiente acessível, `backend/tools/ErpAuthSimulado` (porta 5112) simula só o endpoint de login, com 2 usuários demo (Admin/Cliente) — ver `docs/erp-auth-simulador.md`. Não é a especificação oficial, só uma simulação a partir do contrato assumido no código.
- Validado ponta a ponta via Playwright (login Admin, login Cliente, senha inválida) e via curl (fluxo completo `api_authentication` → `auth/sessao` → chamada autenticada). 45 testes de backend, 41 de frontend.
- **Falta para concluir**: a `api_authentication` real (hoje só simulada localmente); a doc-fonte oficial do contrato (`docs/autenticacao-e-api-portal-sugestoes.md`, citada no código mas ausente do repositório — conferir com o time do ERP); handoff/botão real dentro do ERP; testes de integração em homologação.

### Fase 7 — Testes, Performance e Homologação
- Testes funcionais ponta a ponta dos fluxos principais (seção 8).
- Testes de carga na listagem/ranking (multi-tenant compartilhado, RNF seção 11).
- Ajustes de responsividade (desktop e mobile).
- Validação com stakeholders/usuários-piloto.

**Status: 🔶 Quase concluída.** Testes funcionais automatizados (suíte de integração no backend + testes unitários no frontend, CI configurado), testes de carga/performance na listagem (`docs/performance-report.md` — gargalo de query no EF Core corrigido, paginação server-side implementada) e ajustes de responsividade mobile (menu hamburguer abaixo de 860px, popups com largura fluida, validado sem scroll horizontal em nenhuma tela) concluídos. Pendente apenas: validação com stakeholders/usuários-piloto (depende de pessoas fora da equipe técnica).

### Fase 8 — Lançamento (Go-live) e Acompanhamento
- Publicação em produção e liberação do botão no ERP para todos os clientes.
- Acompanhamento das métricas de sucesso (seção 13) nas primeiras semanas.
- Coleta de feedback para priorizar evoluções futuras.

> O status tipo Kanban/roadmap público (ponto em aberto #2) originalmente cogitado só para depois do go-live foi antecipado e já está implementado (ver seção 7.6) — não é mais um item pendente desta fase.

**Status: ⏳ Não iniciada.** Depende da Fase 6 (SSO real) e de decisão de hospedagem/CI-CD para produção, ainda não discutida.
