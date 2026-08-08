# Contrato de API — Fase 0/1/2/3/4

Endpoints implementados no scaffolding inicial do backend (`backend/src/PortalSugestao.Api`). O contrato completo e sempre atualizado também pode ser consultado via Swagger em `/swagger` com a API rodando.

> **Nota**: a autenticação abaixo é um **mock temporário** que simula o token de SSO do ERP. O mecanismo real depende de definição do time técnico do ERP (PRD, seção 12, ponto em aberto #1) e substituirá este endpoint futuramente.

## Autenticação

### `POST /api/auth/mock-login`
Simula o login via SSO do ERP. Cria o usuário no primeiro acesso (upsert por e-mail).

**Request**
```json
{
  "email": "cliente@empresa.com",
  "nome": "Nome do Cliente",
  "empresa": "Empresa Exemplo Ltda",
  "role": "Cliente"
}
```
`role`: `"Cliente"` ou `"AdminInterno"`.

**Response `200 OK`**
```json
{
  "token": "<jwt>",
  "expiresAt": "2026-07-29T04:00:00Z",
  "usuarioId": 1,
  "nome": "Nome do Cliente",
  "email": "cliente@empresa.com",
  "role": "Cliente"
}
```

Use o `token` retornado no header `Authorization: Bearer <token>` das demais chamadas.

## Categorias

### `GET /api/categorias`
Lista categorias **ativas**. Requer autenticação (qualquer role). Usado no seletor de categoria ao criar/editar sugestão.

### `GET /api/categorias/todas`
Lista todas as categorias, ativas e inativas. Requer role `AdminInterno` — usado na tela de gestão de categorias.

### `POST /api/categorias`
Cria uma categoria. Requer role `AdminInterno`.

**Request**
```json
{ "nome": "Financeiro" }
```
**Respostas de erro**: `400` (nome vazio).

### `PUT /api/categorias/{id}`
Renomeia uma categoria. Requer role `AdminInterno`. Mesmo corpo do `POST`.

**Respostas de erro**: `400` (nome vazio), `404` (não existe).

### `DELETE /api/categorias/{id}`
"Exclui" uma categoria — na prática, **desativa** (`ativo = false`), já que sugestões existentes referenciam a categoria (FK `Restrict`). Requer role `AdminInterno`. Retorna `204`.

**Respostas de erro**: `404` (não existe).

## Produtos

Lista de ERPs comercializados pela empresa (hoje `AJORS.OOH` e `AJORS.SIGN`, semeados via migration). Endpoints idênticos aos de Categorias.

### `GET /api/produtos`
Lista produtos **ativos**. Requer autenticação (qualquer role). Usado no seletor de produto ao criar/editar sugestão.

### `GET /api/produtos/todos`
Lista todos os produtos, ativos e inativos. Requer role `AdminInterno` — usado na tela de gestão de produtos.

### `POST /api/produtos`
Cria um produto. Requer role `AdminInterno`.

**Request**
```json
{ "nome": "AJORS.OOH" }
```
**Respostas de erro**: `400` (nome vazio).

### `PUT /api/produtos/{id}`
Renomeia um produto. Requer role `AdminInterno`. Mesmo corpo do `POST`.

**Respostas de erro**: `400` (nome vazio), `404` (não existe).

### `DELETE /api/produtos/{id}`
"Exclui" um produto — na prática, **desativa** (`ativo = false`), já que sugestões existentes referenciam o produto (FK `Restrict`). Requer role `AdminInterno`. Retorna `204`.

**Respostas de erro**: `404` (não existe).

## Sugestões

### `GET /api/sugestoes?skip=0&take=20`
Lista sugestões **publicadas**, ordenadas por total de votos (ranking). Requer autenticação. **Paginado no servidor** (RNF seção 11 — ver `docs/performance-report.md`): `skip` (padrão `0`) e `take` (padrão `20`, máximo `100`) são opcionais.

**Response**
```json
{
  "items": [ /* SugestaoDto[] da página atual */ ],
  "total": 137,
  "votosUsadosPeloUsuarioAtual": 2
}
```
`votosUsadosPeloUsuarioAtual` é o total de votos ativos do usuário autenticado (regra 7.2) — vem à parte porque o limite de 3 é sobre o total, não sobre a página atual, então o frontend não pode mais somar `votadoPorMim` só dos itens carregados.

### `POST /api/sugestoes`
Cria uma nova sugestão com status inicial `EmModeracao` (regra 7.1 do PRD). Requer autenticação. `produtoId`, `titulo`, `descricao`, `resultadoEsperado` e `categoriaId` são obrigatórios.

**Request**
```json
{
  "produtoId": 1,
  "titulo": "Exportar relatório em Excel",
  "descricao": "Permitir exportar o relatório X para .xlsx",
  "resultadoEsperado": "Conseguir baixar o relatório em .xlsx direto da tela de relatórios",
  "categoriaId": 1
}
```
**Respostas de erro**: `400` (algum campo obrigatório vazio, produto inválido ou categoria inválida).

### `PUT /api/sugestoes/{id}`
Edita a própria sugestão, permitido apenas enquanto `status` for `EmModeracao` (regra 7.1). Requer ser o autor.

**Request**: mesmo formato do `POST /api/sugestoes`.

**Respostas de erro**: `403` (não é o autor), `404` (não existe), `409` (já foi moderada).

## Moderação

> Requerem role `AdminInterno`.

### `GET /api/sugestoes/pendentes`
Lista sugestões com `status = EmModeracao`, ordenadas da mais antiga para a mais nova (fila de moderação).

### `PUT /api/sugestoes/{id}/aprovar`
Aprova a sugestão (`status` → `Publicada`), registra `dataModeracao` e o moderador responsável (auditoria — RNF seção 11).

**Respostas de erro**: `404` (não existe), `409` (já foi moderada).

### `PUT /api/sugestoes/{id}/rejeitar`
Rejeita a sugestão (`status` → `Rejeitada`), com justificativa.

**Request**
```json
{ "motivo": "Duplicada da sugestão #12" }
```

**Respostas de erro**: `400` (motivo vazio), `404` (não existe), `409` (já foi moderada).

## Roadmap

> Requer role `AdminInterno`.

### `PUT /api/sugestoes/{id}/roadmap`
Define o estágio de roadmap (status de andamento público — PRD, ponto em aberto #2) de uma sugestão já publicada. Notifica o autor por e-mail só na transição pra `Lancado` (não a cada mudança de estágio intermediária).

**Request**
```json
{ "estagio": "Planejado" }
```
Valores possíveis: `EmAnalise`, `Planejado`, `EmDesenvolvimento`, `Lancado`.

**Respostas de erro**: `404` (não existe), `409` (sugestão ainda não publicada).

> `estagioRoadmap` aparece em toda resposta de `SugestaoDto` (nullable — `null` até o Admin definir o primeiro estágio). Sugestões com `estagioRoadmap = Lancado` não aceitam mais votos (`409` em `POST /api/sugestoes/{id}/votos` — regra 7.2, "sugestões publicadas e ainda não construídas/lançadas").

## Votação

> Requerem role `Cliente` (Admin interno não vota — PRD seção 6).

### `POST /api/sugestoes/{id}/votos`
Vota na sugestão (deve estar `Publicada`). Limite de **3 votos ativos simultâneos** por cliente, um voto por sugestão (regra 7.2).

**Respostas de erro**: `404` (não existe), `409` (não publicada / já lançada / já votou nela / limite de 3 votos atingido).

### `DELETE /api/sugestoes/{id}/votos`
Remove o voto do usuário autenticado nessa sugestão — é assim que se faz a **realocação**: remover de uma e depois `POST` em outra.

**Respostas de erro**: `404` (sugestão não existe, ou usuário não tinha voto nela).

> Todas as respostas de `SugestaoDto` (listagem, criação, edição, moderação, voto) incluem `votadoPorMim: bool`, indicando se o usuário autenticado já votou naquela sugestão. O total de votos usados pelo usuário (pro limite de 3) vem em `votosUsadosPeloUsuarioAtual`, na resposta paginada de `GET /api/sugestoes`.

## Comentários

> Disponíveis apenas em sugestões **publicadas** (regra 7.3).

### `GET /api/sugestoes/{id}/comentarios`
Lista a thread de comentários da sugestão, ordenada da mais antiga para a mais nova. Requer autenticação (Cliente ou Admin).

**Respostas de erro**: `404` (sugestão não existe), `409` (sugestão não publicada).

### `POST /api/sugestoes/{id}/comentarios`
Cria um comentário. Cliente e Admin podem comentar (tabela de permissões, seção 6).

**Request**
```json
{ "texto": "Ótima ideia, também preciso disso!" }
```

**Respostas de erro**: `400` (texto vazio), `404` (não existe), `409` (não publicada).

### `DELETE /api/sugestoes/{id}/comentarios/{comentarioId}`
Remove um comentário — **moderação reativa, só `AdminInterno`** (tabela de permissões, seção 6).

**Respostas de erro**: `403` (não é Admin), `404` (comentário não existe).

## Notificações

Não são endpoints HTTP — são **efeitos colaterais** de ações já existentes (regra 7.5 do PRD). O autor da sugestão recebe um e-mail quando:

- a sugestão é **aprovada** (`PUT /api/sugestoes/{id}/aprovar`);
- a sugestão é **rejeitada** (`PUT /api/sugestoes/{id}/rejeitar`), com o motivo no corpo do e-mail;
- **o Admin** comenta na sugestão (`POST /api/sugestoes/{id}/comentarios`) — comentários de outros clientes **não** geram e-mail, para evitar excesso de notificações (PRD, ponto em aberto #3).

Cada envio também é registrado em `NotificacaoLog` (auditoria). Se o envio de e-mail falhar (ex.: SMTP fora do ar), a ação principal (aprovar/rejeitar/comentar) **continua funcionando normalmente** — a falha só é logada.

Em desenvolvimento, os e-mails vão para um SMTP de teste local (MailHog, subido via `docker-compose.yml`). Veja os e-mails recebidos em `http://localhost:8025`.
