import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { SugestoesService } from './sugestoes.service';
import { environment } from '../../../environments/environment';

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

  it('listarPaginado() faz GET em /sugestoes com skip e take', () => {
    service.listarPaginado(20, 10).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes?skip=20&take=10`);
    expect(req.request.method).toBe('GET');
    req.flush({ items: [], total: 0, votosUsadosPeloUsuarioAtual: 0 });
  });

  it('listarPendentes() faz GET em /sugestoes/pendentes', () => {
    service.listarPendentes().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/pendentes`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('criar() faz POST em /sugestoes com o corpo certo', () => {
    const request = { produtoId: 1, titulo: 'T', descricao: 'D', resultadoEsperado: 'R', categoriaId: 1 };
    service.criar(request).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({});
  });

  it('editar() faz PUT em /sugestoes/{id}', () => {
    const request = { produtoId: 1, titulo: 'T', descricao: 'D', resultadoEsperado: 'R', categoriaId: 1 };
    service.editar(5, request).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(request);
    req.flush({});
  });

  it('aprovar() faz PUT em /sugestoes/{id}/aprovar', () => {
    service.aprovar(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/aprovar`);
    expect(req.request.method).toBe('PUT');
    req.flush({});
  });

  it('rejeitar() faz PUT em /sugestoes/{id}/rejeitar com o motivo', () => {
    service.rejeitar(5, 'Duplicada').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/rejeitar`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ motivo: 'Duplicada' });
    req.flush({});
  });

  it('votar() faz POST em /sugestoes/{id}/votos', () => {
    service.votar(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/votos`);
    expect(req.request.method).toBe('POST');
    req.flush({});
  });

  it('removerVoto() faz DELETE em /sugestoes/{id}/votos', () => {
    service.removerVoto(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/votos`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('listarCategorias() faz GET em /categorias', () => {
    service.listarCategorias().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categorias`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('listarTodasCategorias() faz GET em /categorias/todas', () => {
    service.listarTodasCategorias().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categorias/todas`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('criarCategoria() faz POST em /categorias com o nome', () => {
    service.criarCategoria('Financeiro').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categorias`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ nome: 'Financeiro' });
    req.flush({});
  });

  it('editarCategoria() faz PUT em /categorias/{id} com o nome', () => {
    service.editarCategoria(5, 'Novo nome').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categorias/5`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ nome: 'Novo nome' });
    req.flush({});
  });

  it('removerCategoria() faz DELETE em /categorias/{id}', () => {
    service.removerCategoria(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/categorias/5`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('listarProdutos() faz GET em /produtos', () => {
    service.listarProdutos().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/produtos`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('listarTodosProdutos() faz GET em /produtos/todos', () => {
    service.listarTodosProdutos().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/produtos/todos`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('criarProduto() faz POST em /produtos com o nome', () => {
    service.criarProduto('AJORS.OOH').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/produtos`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ nome: 'AJORS.OOH' });
    req.flush({});
  });

  it('editarProduto() faz PUT em /produtos/{id} com o nome', () => {
    service.editarProduto(5, 'Novo nome').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/produtos/5`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ nome: 'Novo nome' });
    req.flush({});
  });

  it('removerProduto() faz DELETE em /produtos/{id}', () => {
    service.removerProduto(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/produtos/5`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
