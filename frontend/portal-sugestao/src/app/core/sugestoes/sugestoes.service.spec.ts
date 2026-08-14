import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { SugestoesService } from './sugestoes.service';
import { environment } from '../../../environments/environment';

const SUGESTAO_API = {
  Id: 5,
  Titulo: 'T',
  Descricao: 'D',
  ResultadoEsperado: 'R',
  ProdutoId: 1,
  ProdutoNome: 'AJORS.OOH',
  CategoriaId: 1,
  CategoriaNome: 'Financeiro',
  AutorId: 2,
  AutorNome: 'Cliente Teste',
  Status: 'Publicada',
  EstagioRoadmap: null,
  DataCriacao: '2026-08-14T00:00:00Z',
  TotalVotos: 3,
  VotadoPorMim: false,
  DataModeracao: null,
  MotivoRejeicao: null,
  ModeradorNome: null
};

const SUGESTAO_ESPERADA = {
  id: 5,
  titulo: 'T',
  descricao: 'D',
  resultadoEsperado: 'R',
  produtoId: 1,
  produtoNome: 'AJORS.OOH',
  categoriaId: 1,
  categoriaNome: 'Financeiro',
  autorId: 2,
  autorNome: 'Cliente Teste',
  status: 'Publicada',
  estagioRoadmap: null,
  dataCriacao: '2026-08-14T00:00:00Z',
  totalVotos: 3,
  votadoPorMim: false,
  dataModeracao: null,
  motivoRejeicao: null,
  moderadorNome: null
};

describe('SugestoesService', () => {
  let service: SugestoesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(SugestoesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('listarPaginado() faz POST em /sugestoes/listar com skip e take', () => {
    let recebido: any;
    service.listarPaginado(20, 10).subscribe((r) => (recebido = r));
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/listar`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ skip: 20, take: 10 });
    req.flush({ Erro: false, Mensagem: null, Sugestoes: [SUGESTAO_API], Total: 1, VotosUsadosPeloUsuarioAtual: 2 });
    expect(recebido).toEqual({ items: [SUGESTAO_ESPERADA], total: 1, votosUsadosPeloUsuarioAtual: 2 });
  });

  it('listarPendentes() faz POST em /sugestoes/pendentes', () => {
    service.listarPendentes().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/pendentes`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null, Sugestoes: [SUGESTAO_API] });
  });

  it('criar() faz POST em /sugestoes/salvar com o corpo certo', () => {
    const request = { produtoId: 1, titulo: 'T', descricao: 'D', resultadoEsperado: 'R', categoriaId: 1 };
    service.criar(request).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/salvar`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({ Erro: false, Mensagem: null, Sugestao: SUGESTAO_API });
  });

  it('editar() faz POST em /sugestoes/editar/{id}', () => {
    const request = { produtoId: 1, titulo: 'T', descricao: 'D', resultadoEsperado: 'R', categoriaId: 1 };
    service.editar(5, request).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/editar/5`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({ Erro: false, Mensagem: null, Sugestao: SUGESTAO_API });
  });

  it('aprovar() faz POST em /sugestoes/aprovar/{id}', () => {
    service.aprovar(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/aprovar/5`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null, Sugestao: SUGESTAO_API });
  });

  it('rejeitar() faz POST em /sugestoes/rejeitar/{id} com o motivo', () => {
    service.rejeitar(5, 'Duplicada').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/rejeitar/5`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ motivo: 'Duplicada' });
    req.flush({ Erro: false, Mensagem: null, Sugestao: SUGESTAO_API });
  });

  it('votar() faz POST em /sugestoes/votar/{id}', () => {
    service.votar(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/votar/5`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null, Sugestao: SUGESTAO_API });
  });

  it('removerVoto() faz POST em /sugestoes/removervoto/{id}', () => {
    service.removerVoto(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/removervoto/5`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null, Sugestao: SUGESTAO_API });
  });

  it('atualizarEstagioRoadmap() faz POST em /sugestoes/roadmap/{id} com o estagio', () => {
    service.atualizarEstagioRoadmap(5, 'Planejado').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/roadmap/5`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ estagio: 'Planejado' });
    req.flush({ Erro: false, Mensagem: null, Sugestao: SUGESTAO_API });
  });

  it('listarCategorias() faz POST em /categorias/listar', () => {
    service.listarCategorias().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categorias/listar`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null, Categorias: [] });
  });

  it('listarTodasCategorias() faz POST em /categorias/listartodas', () => {
    service.listarTodasCategorias().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categorias/listartodas`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null, Categorias: [] });
  });

  it('criarCategoria() faz POST em /categorias/salvar com o nome', () => {
    service.criarCategoria('Financeiro').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categorias/salvar`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ nome: 'Financeiro' });
    req.flush({ Erro: false, Mensagem: null, Categoria: { Id: 1, Nome: 'Financeiro', Ativo: true } });
  });

  it('editarCategoria() faz POST em /categorias/editar/{id} com o nome', () => {
    service.editarCategoria(5, 'Novo nome').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categorias/editar/5`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ nome: 'Novo nome' });
    req.flush({ Erro: false, Mensagem: null, Categoria: { Id: 5, Nome: 'Novo nome', Ativo: true } });
  });

  it('removerCategoria() faz POST em /categorias/remover/{id}', () => {
    service.removerCategoria(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categorias/remover/5`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null });
  });

  it('listarProdutos() faz POST em /produtos/listar', () => {
    service.listarProdutos().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/produtos/listar`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null, Produtos: [] });
  });

  it('listarTodosProdutos() faz POST em /produtos/listartodos', () => {
    service.listarTodosProdutos().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/produtos/listartodos`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null, Produtos: [] });
  });

  it('criarProduto() faz POST em /produtos/salvar com o nome', () => {
    service.criarProduto('AJORS.OOH').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/produtos/salvar`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ nome: 'AJORS.OOH' });
    req.flush({ Erro: false, Mensagem: null, Produto: { Id: 1, Nome: 'AJORS.OOH', Ativo: true } });
  });

  it('editarProduto() faz POST em /produtos/editar/{id} com o nome', () => {
    service.editarProduto(5, 'Novo nome').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/produtos/editar/5`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ nome: 'Novo nome' });
    req.flush({ Erro: false, Mensagem: null, Produto: { Id: 5, Nome: 'Novo nome', Ativo: true } });
  });

  it('removerProduto() faz POST em /produtos/remover/{id}', () => {
    service.removerProduto(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/produtos/remover/5`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null });
  });

  it('propaga a mensagem de erro quando a resposta vem com Erro: true', () => {
    let erroRecebido: Error | undefined;
    service.votar(5).subscribe({ error: (erro) => (erroRecebido = erro) });
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/votar/5`);
    req.flush({ Erro: true, Mensagem: 'Limite de votos atingido.', Sugestao: null });
    expect(erroRecebido?.message).toBe('Limite de votos atingido.');
  });
});
