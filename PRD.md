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
- **Autenticação**: SSO com o ERP via **token na URL** (`?token=...`) + **cookie de sessão HttpOnly** emitido pela própria API do Portal (24h, sem renovação) — definido com o time do ERP em 2026-08-12 (ver `docs/sso-checklist.md`). Algoritmo/chave de criptografia do token real ainda em aberto (ponto #1); simulado até lá.
- **Integração com ERP**: botão no ERP abre o portal em nova aba com o token na URL; o Portal chama sua própria rota de login automático (`/login/token`), que valida o token e autentica via cookie (sem novo login manual). O login manual (`/login`) continua existindo em paralelo, para acesso direto/testes.

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

1. **Mecanismo técnico de SSO** entre ERP e Portal — **definido com o time técnico do ERP em 2026-08-12** (ver `docs/sso-checklist.md`): a API de login é implementada pelo próprio time do Portal (não é um serviço central); o token vem via query string (`?token=...`) na URL que o ERP abre; o token é um dado criptografado (algoritmo/chave ainda não definidos) usado para identificar o usuário; a sessão no Portal é um **cookie HttpOnly** (24h de validade, sem renovação — em 401 o usuário é levado de volta ao login); Portal e API ficam no mesmo domínio (sem problema de cookie cross-site). **Ainda em aberto**: o algoritmo/chave de criptografia real do token. Até isso ser definido, o token continua **simulado** (`ErpTokenSimuladoService`, base64 sem criptografia real) — ver Fase 6.
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

> **Status geral (atualizado em 2026-08-12)**: Fases 0 a 5 concluídas e validadas ponta a ponta (inclui o estágio de roadmap/Kanban, ponto em aberto #2, antecipado da Fase 8); Fase 7 (testes funcionais, performance, paginação e responsividade mobile) concluída, restando apenas a validação com stakeholders/usuários-piloto; Fase 6 (SSO real) em andamento — mecanismo definido com o time do ERP e arquitetura-alvo (cookie HttpOnly + login automático via token) implementada com token simulado, falta o algoritmo/chave real de criptografia e o botão de verdade dentro do ERP. Repositório: https://github.com/resolveia/portal-sugestao (CI configurado — GitHub Actions roda a suíte completa de testes a cada push/PR pra `master`).

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

**Status: 🔶 Em andamento (parcialmente concluída em 2026-08-12).** Mecanismo de SSO definido com o time do ERP (ponto em aberto #1) e a arquitetura-alvo já implementada e simulada:
- **Backend**: autenticação por **cookie HttpOnly** (`portal_sugestao_session`, 24h, `SameSite=Lax`) em vez de Bearer/localStorage — `JwtBearerEvents.OnMessageReceived` lê o cookie (com fallback pro header `Authorization`, usado pelos testes). Nova rota `POST /api/auth/login-token` (equivalente à que o ERP vai chamar com o token da URL), que hoje decodifica um token **simulado** (`ErpTokenSimuladoService`, base64 sem criptografia real — placeholder até o algoritmo/chave reais serem definidos), identifica/cria o usuário e autentica. `POST /api/auth/logout` limpa o cookie (só o backend consegue, por ser HttpOnly). `GET /api/auth/tokens-demo` gera tokens simulados de demonstração para os dois perfis (Admin/Cliente), só para popular o fluxo automático sem o ERP real.
- **Frontend**: nova rota `/login/token` (lê `?token=...` da URL, chama `login-token`, redireciona) simulando a URL que o ERP vai abrir — convive com o login manual (`/login`), que ganhou 2 botões ("Entrar como Admin/Cliente via ERP") para simular essa entrada automática sem precisar montar a URL manualmente. `AuthService`/interceptor migrados de Bearer+localStorage para `withCredentials`+cookie; 401 limpa a sessão local e redireciona pro login.
- Validado ponta a ponta via Playwright (login manual, login automático Admin, login automático Cliente, token inválido) e via curl (cookie setado, autenticação por cookie, 401 sem cookie, logout limpando o cookie). 46 testes de backend (3 novos), 40 de frontend (6 novos).
- **Falta para concluir**: algoritmo/chave real de criptografia do token (ponto em aberto #1 ainda parcialmente aberto), decodificação real substituindo `ErpTokenSimuladoService`, botão real dentro do ERP, e testes de integração em homologação com o ERP de verdade.

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
