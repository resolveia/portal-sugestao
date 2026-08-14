import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ComentariosService } from './comentarios.service';
import { environment } from '../../../environments/environment';

describe('ComentariosService', () => {
  let service: ComentariosService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(ComentariosService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('listar() faz POST em /sugestoes/{id}/comentarios/listar', () => {
    service.listar(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/comentarios/listar`);
    expect(req.request.method).toBe('POST');
    req.flush({
      Erro: false,
      Mensagem: null,
      Comentarios: [{ Id: 9, SugestaoId: 5, UsuarioId: 1, AutorNome: 'Cliente', Texto: 'Ótima ideia', DataCriacao: '2026-08-14T00:00:00Z' }]
    });
  });

  it('criar() faz POST em /sugestoes/{id}/comentarios/salvar com o texto no corpo', () => {
    let recebido: any;
    service.criar(5, 'Ótima ideia').subscribe((c) => (recebido = c));
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/comentarios/salvar`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ texto: 'Ótima ideia' });
    req.flush({
      Erro: false,
      Mensagem: null,
      Comentario: { Id: 9, SugestaoId: 5, UsuarioId: 1, AutorNome: 'Cliente', Texto: 'Ótima ideia', DataCriacao: '2026-08-14T00:00:00Z' }
    });
    expect(recebido).toEqual({ id: 9, sugestaoId: 5, usuarioId: 1, autorNome: 'Cliente', texto: 'Ótima ideia', dataCriacao: '2026-08-14T00:00:00Z' });
  });

  it('remover() faz POST em /sugestoes/{id}/comentarios/remover/{comentarioId}', () => {
    service.remover(5, 9).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/comentarios/remover/9`);
    expect(req.request.method).toBe('POST');
    req.flush({ Erro: false, Mensagem: null });
  });

  it('propaga a mensagem de erro quando a resposta vem com Erro: true', () => {
    let erroRecebido: Error | undefined;
    service.listar(5).subscribe({ error: (erro) => (erroRecebido = erro) });
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/comentarios/listar`);
    req.flush({ Erro: true, Mensagem: 'Sugestão não encontrada.', Comentarios: null });
    expect(erroRecebido?.message).toBe('Sugestão não encontrada.');
  });
});
