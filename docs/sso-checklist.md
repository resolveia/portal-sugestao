# Checklist — Definição do SSO entre ERP e Portal de Sugestões

> Referente ao **ponto em aberto #1** do PRD (seção 12) e bloqueio da **Fase 6 — Integração com o ERP** (seção 14).
> Objetivo: levar essas perguntas ao time técnico responsável pela autenticação do ERP para destravar a implementação do SSO real, hoje substituído por um login mock (`POST /api/auth/mock-login`).

## 1. Protocolo / mecanismo de autenticação

- [ ] O ERP vai emitir um **JWT próprio** (assinado com chave/certificado compartilhado com o Portal)?
- [ ] Ou o SSO será feito via padrão **OAuth2 / OpenID Connect**, com um Identity Provider centralizado?
- [ ] Existe algum mecanismo de SSO já usado por **outros módulos/satélites** do ERP que devemos seguir (para manter consistência)?

## 2. Emissão e validação do token

- [ ] Quem é o **emissor (issuer)** do token — o próprio ERP, ou um serviço de autenticação separado?
- [ ] Como a API do Portal (.NET) deve **validar** o token: chave pública/certificado (JWKS?), endpoint de introspecção, ou outro mecanismo?
- [ ] Qual o **algoritmo de assinatura** (RS256, HS256, etc.)?
- [ ] Existe rotação de chaves? Se sim, como o Portal deve acompanhar isso?

## 3. Conteúdo do token (claims)

- [ ] Quais dados do usuário virão no token? Precisamos de, no mínimo:
  - [ ] Id do usuário no ERP
  - [ ] Nome
  - [ ] E-mail
  - [ ] Empresa/cliente de origem
  - [ ] Role/perfil (para diferenciar **Cliente** de **Admin interno** — ver seção 6 do PRD)
- [ ] Esses dados já existem hoje em algum token/sessão do ERP, ou precisam ser adicionados?

## 4. Handoff (abertura do Portal a partir do ERP)

- [ ] Como o botão **"Portal de Sugestão"** dentro do ERP vai repassar o token para a nova aba?
  - Ex: query string, header customizado, endpoint intermediário que troca um código de curta duração por um token (mais seguro — evita vazar token na URL/histórico do navegador).
- [ ] Existe alguma preocupação de segurança específica do time do ERP quanto a esse repasse (CORS, domínio, etc.)?

## 5. Expiração e renovação de sessão

- [ ] Qual a validade do token emitido pelo ERP?
- [ ] Existe mecanismo de **refresh**, ou a sessão do Portal simplesmente expira junto com a do ERP (usuário refaz o fluxo pelo botão)?
- [ ] O que deve acontecer no Portal quando o token expirar durante o uso (logout automático, redirecionar de volta ao ERP)?

## 6. Ambiente de homologação

- [ ] O ERP tem um **ambiente de teste/homologação** com esse mecanismo de SSO já configurado ou configurável, para validarmos a integração ponta a ponta antes de produção?
- [ ] Há um responsável técnico do lado do ERP disponível para apoiar esse teste conjunto (Fase 6 do PRD prevê testes de integração em homologação)?

## Próximos passos após essa definição

Assim que os pontos acima estiverem respondidos, a Fase 6 do Portal pode ser implementada — ela é relativamente curta do lado do Portal (o trabalho pesado é integrar contra o mecanismo real definido aqui). Isso inclui:

1. Substituir a autenticação mock (`/api/auth/mock-login`) pela validação real do token do ERP.
2. Implementar o botão "Portal de Sugestão" dentro do ERP, repassando o token conforme definido na seção 4.
3. Testes de integração ponta a ponta em homologação (seção 6).
