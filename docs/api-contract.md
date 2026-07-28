# Contrato de API — Fase 0/1

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

## Próximos endpoints (Fases 2–5, ainda não implementados)
- Moderação: `PUT /api/sugestoes/{id}/aprovar`, `PUT /api/sugestoes/{id}/rejeitar`.
- Votação: `POST /api/sugestoes/{id}/votos`, `DELETE /api/sugestoes/{id}/votos` (realocação).
- Comentários: `GET/POST /api/sugestoes/{id}/comentarios`.
- Notificações: disparo assíncrono de e-mail (sem endpoint HTTP direto).
