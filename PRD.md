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

1. **Cadastro de sugestões** pelo cliente (título, descrição, categoria/módulo).
2. **Moderação/aprovação** pela equipe interna antes da sugestão ficar pública.
3. **Votação** em sugestões aprovadas, com limite de **3 votos ativos por usuário**, com **realocação** (o cliente pode remover um voto de uma sugestão e aplicá-lo em outra a qualquer momento).
4. **Listagem/ranking de mais votadas** — visão pública ordenada por número de votos.
5. **Comentários** nas sugestões (thread de discussão por sugestão).
6. **Categorias/módulos** cadastrados e mantidos dentro do próprio portal pela equipe interna.
7. **Notificações por e-mail** ao cliente quando houver mudanças relevantes na sugestão (ex: aprovação, rejeição, nova resposta/comentário da equipe).
8. **Autenticação via SSO** com o ERP (mecanismo de token a definir com o time técnico do ERP).
9. **Multi-tenant compartilhado**: sugestões e votos são visíveis e compartilhados entre todos os clientes/empresas que usam o ERP (portal único, não isolado por empresa).

### Fora de escopo (nesta fase)

- Status de andamento tipo Kanban/roadmap público (Em análise, Planejado, Em desenvolvimento, Lançado). Nesta fase, a priorização é comunicada apenas pelo ranking de votos — **ponto em aberto** para futura evolução (ver seção 12).
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
| Gerenciar categorias | ❌ | ✅ |
| Ver ranking de mais votadas | ✅ | ✅ |

\* Admin interno não computa votos de cliente; a definir se admin também poderá votar em nome da equipe de produto (ponto em aberto).

## 7. Regras de Negócio

### 7.1 Sugestões
- Toda sugestão nova entra com status **"Em moderação"** (não pública, não vota-se nela).
- Admin interno aprova (→ **"Publicada"**) ou rejeita (→ **"Rejeitada"**, com justificativa opcional visível ao autor).
- Sugestão publicada é visível a todos os clientes (multi-tenant compartilhado) e pode receber votos e comentários.
- Campos mínimos: título, descrição, categoria, autor, empresa/cliente de origem, data de criação, status.

### 7.2 Votação
- Cada usuário cliente tem no máximo **3 votos ativos simultâneos**, aplicáveis apenas a sugestões **publicadas e ainda não construídas/lançadas**.
- O cliente pode remover um voto de uma sugestão e realocá-lo para outra a qualquer momento.
- Um usuário não pode votar mais de uma vez na mesma sugestão.
- O voto é vinculado ao usuário individual (não à empresa/conta cliente).

### 7.3 Comentários
- Disponível apenas em sugestões publicadas.
- Cliente e Admin podem comentar; Admin pode remover comentários (moderação reativa).

### 7.4 Categorias
- Mantidas exclusivamente dentro do portal pelo Admin interno (CRUD simples), sem sincronização com o ERP nesta fase.

### 7.5 Notificações
- Disparadas por e-mail ao autor da sugestão nos eventos: aprovação, rejeição, novo comentário (a validar se notifica em toda resposta ou apenas respostas do Admin, para evitar excesso de e-mails).

## 8. Fluxos Principais

**Fluxo 1 — Cliente cria sugestão**
1. Cliente clica em "Portal de Sugestão" no ERP → abre nova aba autenticada via SSO.
2. Cliente acessa "Nova sugestão", preenche título, descrição e categoria.
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
- **Autenticação**: SSO com o ERP — mecanismo exato (JWT próprio, OAuth2/OIDC, etc.) **ainda não definido**, a ser especificado junto ao time responsável pela autenticação do ERP.
- **Integração com ERP**: botão no ERP abre o portal em nova aba, repassando token/sessão para autenticação automática (sem novo login).

## 10. Modelo de Dados (entidades principais — visão preliminar)

- **Usuario** (id, nome, email, empresa/cliente de origem, referência ao usuário do ERP)
- **Sugestao** (id, titulo, descricao, categoriaId, autorId, status, dataCriacao, dataModeracao, motivoRejeicao)
- **Categoria** (id, nome, ativo)
- **Voto** (id, sugestaoId, usuarioId, dataVoto)
- **Comentario** (id, sugestaoId, usuarioId, texto, dataCriacao)
- **NotificacaoLog** (id, usuarioId, tipo, sugestaoId, dataEnvio) — opcional, para auditoria de e-mails enviados

## 11. Requisitos Não-Funcionais

- Multi-tenant compartilhado: performance da listagem/ranking deve suportar todos os clientes do ERP simultaneamente.
- Auditoria básica de ações de moderação (quem aprovou/rejeitou e quando).
- Interface responsiva (acesso via desktop, principal, mas navegável em mobile).
- Tempo de carregamento do ranking de mais votadas deve ser rápido mesmo com grande volume de sugestões/votos (considerar cache/otimização de contagem de votos).

## 12. Pontos em Aberto

1. **Mecanismo técnico de SSO** entre ERP e Portal (tipo de token, emissor, validação na API .NET) — definir com time técnico do ERP.
2. **Status de andamento (Kanban/roadmap público)**: hoje fora de escopo, mas é uma evolução natural após o MVP — vale revisitar após o portal estar em produção.
3. Regras de e-mail: quais eventos exatos disparam notificação (todo comentário vs. apenas respostas do Admin).
4. Se Admin interno também poderá votar (representando a equipe de produto) ou fica restrito a clientes.
5. Política de duplicidade: como o Admin deve tratar sugestões duplicadas (mesclar votos ao unificar duas sugestões?).

## 13. Métricas de Sucesso

- Número de sugestões cadastradas e aprovadas por período.
- Número de votos totais e usuários únicos votando.
- Taxa de sugestões que avançam de "mais votada" para efetivamente entrarem no roadmap/build.
- Redução de pedidos de funcionalidade recebidos via canais informais (suporte/e-mail).

## 14. Fases de Construção

Ainda que o escopo funcional (seção 5) seja tratado como MVP de fase única, a **construção técnica** será feita de forma incremental, entregando valor testável a cada etapa. Cada fase pressupõe a anterior concluída.

### Fase 0 — Definições e Preparação
- Definir mecanismo técnico de SSO com o time do ERP (ponto em aberto #1).
- Modelagem final do banco de dados (SQL Server) a partir da visão preliminar (seção 10).
- Setup dos repositórios, pipelines de build/deploy e ambientes (dev/homologação).
- Definição do contrato de API (endpoints REST) entre frontend Angular e backend .NET 8.

### Fase 1 — Fundação (Autenticação e Cadastro Base)
- Autenticação via SSO funcional (recebendo token/sessão do ERP e validando na API).
- CRUD de **Categorias** pelo Admin interno.
- Cadastro de **Sugestões** pelo cliente (título, descrição, categoria), com status inicial "Em moderação".
- Listagem simples de sugestões (sem ranking/votação ainda).

### Fase 2 — Moderação
- Fila de moderação para o Admin interno.
- Aprovação (→ "Publicada") e rejeição (→ "Rejeitada", com justificativa).
- Edição de sugestão pelo cliente antes da aprovação.
- Auditoria básica de quem aprovou/rejeitou e quando (RNF seção 11).

### Fase 3 — Votação e Ranking
- Votação em sugestões publicadas, com limite de 3 votos ativos por usuário.
- Realocação de votos (remover de uma sugestão, aplicar em outra).
- Ranking público ordenado por número de votos, com otimização/cache para performance (RNF seção 11).

### Fase 4 — Comentários
- Thread de comentários em sugestões publicadas.
- Moderação reativa de comentários pelo Admin (remoção).

### Fase 5 — Notificações
- Envio de e-mails ao autor nos eventos definidos (aprovação, rejeição, comentário — conforme resolução do ponto em aberto #3).
- Registro opcional em `NotificacaoLog` para auditoria de envios.

### Fase 6 — Integração com o ERP
- Botão "Portal de Sugestão" implementado dentro do ERP.
- Abertura do portal em nova aba com SSO real (fim a fim, substituindo mocks de autenticação usados nas fases anteriores).
- Testes de integração entre ERP e Portal em ambiente de homologação.

### Fase 7 — Testes, Performance e Homologação
- Testes funcionais ponta a ponta dos fluxos principais (seção 8).
- Testes de carga na listagem/ranking (multi-tenant compartilhado, RNF seção 11).
- Ajustes de responsividade (desktop e mobile).
- Validação com stakeholders/usuários-piloto.

### Fase 8 — Lançamento (Go-live) e Acompanhamento
- Publicação em produção e liberação do botão no ERP para todos os clientes.
- Acompanhamento das métricas de sucesso (seção 13) nas primeiras semanas.
- Coleta de feedback para priorizar evoluções futuras (ex: status tipo Kanban/roadmap público, ponto em aberto #2).
