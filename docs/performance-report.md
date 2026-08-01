# Relatório de Teste de Performance e Carga — Ranking de Sugestões

Fase 7 do PRD (seção 14): "Testes de carga na listagem/ranking (multi-tenant compartilhado, RNF seção 11)".

## Metodologia

1. **Massa de dados**: semeados diretamente no SQL Server (via script T-SQL, sem passar pela API) **5.000 sugestões publicadas** distribuídas entre as categorias/produtos/autores já existentes, mais **21.000 votos** distribuídos entre elas — simulando "grande volume de sugestões/votos" mencionado na RNF (seção 11), no endpoint que mais concentra esse requisito: `GET /api/sugestoes` (ranking público, ordenado por número de votos).
2. **Baseline**: medição de latência de requisição única (`curl`) e de carga concorrente com [`autocannon`](https://github.com/mcollina/autocannon) (simulando múltiplos clientes/tenants acessando o ranking ao mesmo tempo — cenário "multi-tenant compartilhado" da RNF), em duas concorrências (5 e 20 conexões simultâneas).
3. **Diagnóstico**: comparação entre o tempo da query equivalente rodada direto no SQL Server (via `sqlcmd`) e o tempo da mesma operação pela API, para isolar se o gargalo estava no banco ou na camada .NET/EF Core.
4. **Otimização**: aplicada com base no diagnóstico (ver abaixo).
5. **Nova medição**: mesmos cenários de carga repetidos após a otimização, para comparação direta.
6. **Limpeza**: os dados sintéticos de carga foram removidos do banco ao final — não fazem parte do produto, só existiram para o teste.

## Diagnóstico

Uma única chamada a `GET /api/sugestoes` com os 5.000 registros semeados levava **~2 a 3 segundos**. Rodando a consulta SQL equivalente direto no banco (mesmo JOIN com Produto/Categoria/Autor + contagem de votos por sugestão via subquery), o SQL Server respondia em **258ms**. Ou seja, o banco em si não era o gargalo — o tempo estava sendo perdido na camada do EF Core/API.

Causa raiz: o método `Listar()` (e também `Pendentes()`) usava `.Include(s => s.Votos)` para poder calcular `s.Votos.Count` e `s.Votos.Any(...)` (se o usuário atual já votou). Com `Include` em uma coleção, o EF Core por padrão gera uma única consulta com `JOIN`, e como havia ~21.000 votos distribuídos entre as 5.000 sugestões, o resultado do `JOIN` explode em ~21.000 linhas que o EF precisa materializar, rastrear (change tracking) e "desachatar" de volta em 5.000 objetos `Sugestao` com suas coleções `Votos` — um trabalho de CPU/memória desnecessário, já que o endpoint só precisa do **total** de votos e se **o usuário atual votou ou não**, nunca da lista completa de votos.

## Otimização aplicada

**`backend/src/PortalSugestao.Api/Controllers/SugestoesController.cs`** — `Listar()` e `Pendentes()` reescritos:
- Removido `.Include(s => s.Produto/Categoria/Autor/Votos)` seguido de materialização completa da entidade.
- Substituído por uma projeção direta pro `SugestaoDto` dentro do `.Select(...)` do LINQ — o EF Core traduz `s.Produto!.Nome`, `s.Votos.Count` e `s.Votos.Any(v => ...)` em `JOIN`s/subqueries eficientes no SQL gerado, sem nunca materializar a entidade `Sugestao` completa nem a coleção de votos em memória.
- Adicionado `.AsNoTracking()` — essas são consultas somente-leitura (listagem), não precisam do overhead de change tracking do EF Core.

**`backend/src/PortalSugestao.Infrastructure/Data/PortalSugestaoDbContext.cs`** — adicionado índice em `Sugestoes.Status` (migration `AddIndexSugestaoStatus`), já que tanto o ranking quanto a fila de moderação filtram por esse campo em toda listagem.

Essas mudanças não alteram o contrato da API (mesmo formato de `SugestaoDto` na resposta) — validado com a suíte de testes completa (37 testes de backend, 33 de frontend, todos passando) e uma checagem manual do JSON retornado.

## Resultados

### Requisição única (sequencial, sem concorrência)

| Métrica | Antes | Depois |
|---|---|---|
| Latência típica | ~2.000–2.900 ms | ~300–470 ms (regime estável) |

### Carga concorrente — 5 conexões simultâneas, 30s

| Métrica | Antes | Depois |
|---|---|---|
| Latência média | 3.013 ms | 757 ms |
| Latência p97.5 | 4.081 ms | 1.301 ms |
| Requisições/seg (média) | 1,6 | 6,57 |
| Total de requisições em 30s | 53 | 202 |

### Carga concorrente — 20 conexões simultâneas, 30s

| Métrica | Antes | Depois |
|---|---|---|
| Resultado | **Nenhuma requisição completada em 30s** (todas travadas/timeout) | Latência média 2.497 ms, p97.5 3.788 ms |
| Requisições/seg (média) | 0 (sistema travado) | 7,64 |
| Total de requisições em 30s | 0 completas (20 pendentes) | 249 |

**Resumo**: a otimização trouxe um ganho de ~4x em latência e throughput no cenário de carga moderada (5 conexões), e resolveu um travamento completo no cenário de carga mais alta (20 conexões) — antes da mudança, o sistema não conseguia nem terminar uma única requisição dentro de 30 segundos com 20 acessos simultâneos ao ranking.

## Ponto em aberto — recomendação para volumes ainda maiores

Mesmo após a otimização, com 20 conexões simultâneas cada requisição ainda transfere o payload completo de ~5.000 sugestões (a resposta chega a ~2,4 MB por chamada nesse volume de teste), o que é inerente a um endpoint **sem paginação**. Em produção, com múltiplos clientes do ERP acessando o mesmo ranking ao mesmo tempo (RNF "multi-tenant compartilhado"), esse volume de dados por resposta tende a ser o próximo limitador — não é mais um problema de consulta ineficiente, e sim de volume de payload por natureza.

**Recomendação**: se o volume real de sugestões publicadas crescer para a casa dos milhares, vale avaliar paginação server-side no `GET /api/sugestoes` (o `dx-data-grid` do frontend já suporta paginação remota nativamente). Isso é uma mudança de contrato de API e de comportamento da grid no frontend, por isso não foi implementada nesta rodada — fica registrada aqui como próximo passo a decidir, não como parte deste teste de performance.

## Atualização (2026-08-01) — paginação implementada

A recomendação acima foi implementada a pedido do usuário. `GET /api/sugestoes` agora aceita `skip`/`take` (padrão 20, máximo 100) e responde `{ items, total, votosUsadosPeloUsuarioAtual }` em vez de um array — ver seção "Sugestões" em `docs/api-contract.md`. O frontend (`sugestoes-list.ts`) passou a usar um `CustomStore`/`DataSource` do DevExtreme com `remoteOperations.paging = true`, então a grid busca só a página visível.

**Efeito no payload**: com 44 sugestões publicadas no banco de teste, uma página de 20 itens responde em **9,6 KB**; forçando o máximo de 100 itens numa única resposta (equivalente ao comportamento antigo, sem paginação) o payload já dobra para 21,3 KB só com 44 registros. Extrapolando para o volume testado antes (5.000 registros, ~2,4 MB numa resposta só) — com paginação o payload de qualquer página **permanece constante em torno de 10 KB**, independente do total de sugestões publicadas. Isso elimina o próximo gargalo identificado na seção anterior (volume de payload crescendo com o total de dados), sem depender de nenhuma otimização adicional de query.

`votosUsadosPeloUsuarioAtual` resolve o problema de contabilizar o limite de 3 votos ativos por cliente (regra 7.2) sem precisar carregar todas as sugestões publicadas: o backend calcula isso com uma consulta separada e leve (`COUNT` na tabela `Votos` filtrada por usuário), independente da paginação do ranking.
