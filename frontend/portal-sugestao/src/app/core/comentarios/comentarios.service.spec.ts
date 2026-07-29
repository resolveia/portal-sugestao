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

  it('listar() faz GET em /sugestoes/{id}/comentarios', () => {
    service.listar(5).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/comentarios`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('criar() faz POST com o texto no corpo', () => {
    service.criar(5, 'Ótima ideia').subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/comentarios`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ texto: 'Ótima ideia' });
    req.flush({});
  });

  it('remover() faz DELETE em /sugestoes/{id}/comentarios/{comentarioId}', () => {
    service.remover(5, 9).subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/sugestoes/5/comentarios/9`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });
});
