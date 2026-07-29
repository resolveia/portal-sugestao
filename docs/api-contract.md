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
Lista categorias ativas. Requer autenticação (qualquer role).

### `POST /api/categorias`
Cria uma categoria. Requer role `AdminInterno`.

**Request**
```json
{ "nome": "Financeiro" }
```

## Sugestões

### `GET /api/sugestoes`
Lista sugestões **publicadas**, ordenadas por total de votos (ranking). Requer autenticação.

### `POST /api/sugestoes`
Cria uma nova sugestão com status inicial `EmModeracao` (regra 7.1 do PRD). Requer autenticação.

**Request**
```json
{
  "titulo": "Exportar relatório em Excel",
  "descricao": "Permitir exportar o relatório X para .xlsx",
  "categoriaId": 1
}
```

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

## Votação

> Requerem role `Cliente` (Admin interno não vota — PRD seção 6).

### `POST /api/sugestoes/{id}/votos`
Vota na sugestão (deve estar `Publicada`). Limite de **3 votos ativos simultâneos** por cliente, um voto por sugestão (regra 7.2).

**Respostas de erro**: `404` (não existe), `409` (não publicada / já votou nela / limite de 3 votos atingido).

### `DELETE /api/sugestoes/{id}/votos`
Remove o voto do usuário autenticado nessa sugestão — é assim que se faz a **realocação**: remover de uma e depois `POST` em outra.

**Respostas de erro**: `404` (sugestão não existe, ou usuário não tinha voto nela).

> Todas as respostas de `SugestaoDto` (listagem, criação, edição, moderação, voto) incluem `votadoPorMim: bool`, indicando se o usuário autenticado já votou naquela sugestão — o frontend soma esses `true` para saber quantos dos 3 votos já foram usados, sem precisar de outro endpoint.

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

## Próximos endpoints (Fase 5, ainda não implementados)
- Notificações: disparo assíncrono de e-mail (sem endpoint HTTP direto).
