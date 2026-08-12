import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  let httpMock: HttpTestingController;
  let limparSessaoLocal: ReturnType<typeof vi.fn>;
  let navigate: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    limparSessaoLocal = vi.fn();
    navigate = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: { limparSessaoLocal } },
        { provide: Router, useValue: { navigate } }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('anexa withCredentials em toda requisição, para o cookie de sessão viajar junto', () => {
    const http = TestBed.inject(HttpClient);

    http.get('/qualquer-rota').subscribe();

    const req = httpMock.expectOne('/qualquer-rota');
    expect(req.request.withCredentials).toBe(true);
    req.flush({});
  });

  it('em 401, limpa a sessão local e redireciona para /login', () => {
    const http = TestBed.inject(HttpClient);

    http.get('/qualquer-rota').subscribe({ error: () => {} });

    const req = httpMock.expectOne('/qualquer-rota');
    req.flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(limparSessaoLocal).toHaveBeenCalled();
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });

  it('em erro diferente de 401, não mexe na sessão', () => {
    const http = TestBed.inject(HttpClient);

    http.get('/qualquer-rota').subscribe({ error: () => {} });

    const req = httpMock.expectOne('/qualquer-rota');
    req.flush({}, { status: 500, statusText: 'Internal Server Error' });

    expect(limparSessaoLocal).not.toHaveBeenCalled();
    expect(navigate).not.toHaveBeenCalled();
  });

  it('propaga o erro original pra quem chamou', () => {
    const http = TestBed.inject(HttpClient);
    let erroRecebido: HttpErrorResponse | undefined;

    http.get('/qualquer-rota').subscribe({ error: (err) => (erroRecebido = err) });

    const req = httpMock.expectOne('/qualquer-rota');
    req.flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(erroRecebido?.status).toBe(401);
  });
});
