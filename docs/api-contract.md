# Contrato de API — api_portal_sugestoes

Endpoints do backend (`backend/src/PortalSugestao.Api`). O contrato completo e sempre atualizado
também pode ser consultado via Swagger em `/swagger` com a API rodando.

> **Convenção da plataforma** (revisão de 2026-08-14, alinhada ao padrão real de
> `api_authentication`/`api_portal_sugestoes` — ver `docs/erp-auth-simulador.md`): **todos os
> endpoints são `POST`**, mesmo os de leitura. A resposta é **sempre `HTTP 200`** com um envelope
> `{ "Erro": bool, "Mensagem": string | null, ...dados }` — erro de negócio, validação ou
> permissão vem com `Erro: true` e a mensagem no campo `Mensagem`, não em status HTTP. Só falha
> técnica (não autenticado) usa status HTTP real (`401`). O JSON é **PascalCase**, igual aos nomes
> das propriedades em C#.
>
> Autenticação: cookie HttpOnly de sessão (`portal_sugestao_session`), emitido por
> `POST /api/auth/sessao` depois de login bem-sucedido contra a `api_authentication` do ERP (real
> ou seu simulador local, `backend/tools/ErpAuthSimulado`). Ver PRD, seção 12, ponto em aberto #1.

## Autenticação

### `POST /api/auth/sessao`
Estabelece a sessão local do Portal a partir dos dados já autenticados pela `api_authentication`
(quem valida usuário/senha é ela, não esta API). Cria o `Usuario` local no primeiro login ou
atualiza nome/role nos seguintes (upsert por `EmpresaId:Id`).

**Request**
```json
{ "Nome": "Cliente Teste", "Login": "cliente.teste", "Id": 1, "EmpresaId": "EMP1", "AdminPortalSugestoes": false }
```

**Response `200 OK`**
```json
{ "Erro": false, "Mensagem": null, "Usuario": { "Id": 1, "Nome": "Cliente Teste", "Email": "cliente.teste@erp.local", "Role": "Cliente" } }
```

### `POST /api/auth/logout`
Limpa o cookie de sessão (só o backend consegue, por ser HttpOnly).

**Response `200 OK`**: `{ "Erro": false }`

## Categorias

> Autenticado (qualquer role) pra leitura; `listartodas`/`salvar`/`editar`/`remover` exigem
> `AdminInterno` — checado dentro da ação, devolvendo `{ Erro: true, Mensagem: "Operação não
> permitida." }` em vez de `403`.

### `POST /api/categorias/listar`
Lista categorias **ativas**. Usado no seletor de categoria ao criar/editar sugestão.

**Response**: `{ "Erro": false, "Mensagem": null, "Categorias": [ { "Id": 1, "Nome": "Financeiro", "Ativo": true } ] }`

### `POST /api/categorias/listartodas`
Lista todas as categorias, ativas e inativas — tela de gestão de categorias (Admin).

### `POST /api/categorias/salvar`
Cria uma categoria. **Request**: `{ "Nome": "Financeiro" }`. **Response**: `{ Erro, Mensagem, Categoria }`.
`Erro: true` se nome vazio.

### `POST /api/categorias/editar/{id}`
Renomeia uma categoria. Mesmo corpo do `salvar`. `Erro: true` se nome vazio ou categoria não existe.

### `POST /api/categorias/remover/{id}`
"Exclui" uma categoria — na prática, **desativa** (`Ativo = false`), já que sugestões existentes
referenciam a categoria (FK `Restrict`). `Erro: true` se não existe.

## Produtos

Lista de ERPs comercializados pela empresa (ex.: `AJORS.OOH`, `AJORS.SIGN`). Endpoints idênticos
aos de Categorias, trocando `categorias` por `produtos` e `Categoria(s)` por `Produto(s)`:
`listar`, `listartodos`, `salvar`, `editar/{id}`, `remover/{id}`.

## Sugestões

### `POST /api/sugestoes/listar`
Lista sugestões **publicadas**, ordenadas por total de votos (ranking). **Paginado no servidor**
(RNF seção 11 — ver `docs/performance-report.md`).

**Request**: `{ "Skip": 0, "Take": 20 }` (ambos opcionais; `Take` máximo 100)

**Response**
```json
{
  "Erro": false,
  "Mensagem": null,
  "Sugestoes": [ /* SugestaoDto[] da página atual, ver formato abaixo */ ],
  "Total": 137,
  "VotosUsadosPeloUsuarioAtual": 2
}
```
`VotosUsadosPeloUsuarioAtual` é o total de votos ativos do usuário autenticado (regra 7.2) — vem à
parte porque o limite de 3 é sobre o total, não sobre a página atual.

**`SugestaoDto`**: `Id, Titulo, Descricao, ResultadoEsperado, ProdutoId, ProdutoNome, CategoriaId,
CategoriaNome, AutorId, AutorNome, Status, EstagioRoadmap (nullable), DataCriacao, TotalVotos,
VotadoPorMim, DataModeracao (nullable), MotivoRejeicao (nullable), ModeradorNome (nullable)`.

### `POST /api/sugestoes/salvar`
Cria uma nova sugestão com status inicial `EmModeracao` (regra 7.1).

**Request**
```json
{
  "ProdutoId": 1,
  "Titulo": "Exportar relatório em Excel",
  "Descricao": "Permitir exportar o relatório X para .xlsx",
  "ResultadoEsperado": "Conseguir baixar o relatório em .xlsx direto da tela de relatórios",
  "CategoriaId": 1
}
```
`Erro: true` se algum campo obrigatório vazio, produto inválido ou categoria inválida.

### `POST /api/sugestoes/editar/{id}`
Edita a própria sugestão, permitido apenas enquanto `Status` for `EmModeracao` (regra 7.1). Mesmo
corpo do `salvar`. `Erro: true` se não é o autor, não existe ou já foi moderada.

## Moderação

> `Erro: true` com "Operação não permitida." se o usuário não for `AdminInterno`.

### `POST /api/sugestoes/pendentes`
Lista sugestões com `Status = EmModeracao`, da mais antiga para a mais nova.

### `POST /api/sugestoes/aprovar/{id}`
Aprova a sugestão (`Status` → `Publicada`), registra `DataModeracao` e o moderador responsável.
`Erro: true` se não existe ou já foi moderada.

### `POST /api/sugestoes/rejeitar/{id}`
Rejeita a sugestão (`Status` → `Rejeitada`), com justificativa.
**Request**: `{ "Motivo": "Duplicada da sugestão #12" }`.
`Erro: true` se motivo vazio, não existe ou já foi moderada.

## Roadmap

### `POST /api/sugestoes/roadmap/{id}`
Define o estágio de roadmap (status de andamento público — PRD, ponto em aberto #2) de uma
sugestão já publicada. Só `AdminInterno`. Notifica o autor por e-mail só na transição pra
`Lancado`.

**Request**: `{ "Estagio": "Planejado" }` — valores: `EmAnalise`, `Planejado`, `EmDesenvolvimento`, `Lancado`.
`Erro: true` se não existe ou sugestão ainda não publicada.

> Sugestões com `EstagioRoadmap = Lancado` não aceitam mais votos (regra 7.2).

## Votação

> Só `Cliente` (Admin interno não vota — PRD seção 6). `Erro: true` com "Operação não permitida."
> caso contrário.

### `POST /api/sugestoes/votar/{id}`
Vota na sugestão (deve estar `Publicada`). Limite de **3 votos ativos simultâneos** por cliente,
um voto por sugestão (regra 7.2). `Erro: true` se não existe, não publicada, já lançada, já votou
nela ou limite de 3 atingido.

### `POST /api/sugestoes/removervoto/{id}`
Remove o voto do usuário autenticado nessa sugestão — é assim que se faz a **realocação**: remover
de uma e depois votar em outra. `Erro: true` se sugestão não existe ou usuário não tinha voto nela.

> Todas as respostas com `Sugestao` incluem `VotadoPorMim: bool`. O total de votos usados pelo
> usuário (pro limite de 3) vem em `VotosUsadosPeloUsuarioAtual`, na resposta de `sugestoes/listar`.

## Comentários

> Disponíveis apenas em sugestões **publicadas** (regra 7.3). Rota aninhada em
> `/api/sugestoes/{sugestaoId}/comentarios`.

### `POST /api/sugestoes/{sugestaoId}/comentarios/listar`
Lista a thread de comentários, da mais antiga para a mais nova. `Erro: true` se sugestão não
existe ou não publicada.

### `POST /api/sugestoes/{sugestaoId}/comentarios/salvar`
Cria um comentário. Cliente e Admin podem comentar (tabela de permissões, seção 6).
**Request**: `{ "Texto": "Ótima ideia, também preciso disso!" }`.
`Erro: true` se texto vazio, sugestão não existe ou não publicada.

### `POST /api/sugestoes/{sugestaoId}/comentarios/remover/{comentarioId}`
Remove um comentário — **moderação reativa, só `AdminInterno`**. `Erro: true` se não é Admin ou
comentário não existe.

## Notificações

Não são endpoints HTTP — são **efeitos colaterais** de ações já existentes (regra 7.5 do PRD). O
autor da sugestão recebe um e-mail quando:

- a sugestão é **aprovada** (`sugestoes/aprovar/{id}`);
- a sugestão é **rejeitada** (`sugestoes/rejeitar/{id}`), com o motivo no corpo do e-mail;
- a sugestão é **lançada** (`sugestoes/roadmap/{id}`, transição pra `Lancado`);
- **o Admin** comenta na sugestão (`comentarios/salvar`) — comentários de outros clientes **não**
  geram e-mail, para evitar excesso de notificações (PRD, ponto em aberto #3).

Cada envio também é registrado em `NotificacaoLog` (auditoria). Se o envio de e-mail falhar (ex.:
SMTP fora do ar), a ação principal **continua funcionando normalmente** — a falha só é logada.

Em desenvolvimento, os e-mails vão para um SMTP de teste local (MailHog, subido via
`docker-compose.yml`). Veja os e-mails recebidos em `http://localhost:8025`.
