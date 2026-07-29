import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  let httpMock: HttpTestingController;
  let getToken: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    getToken = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: { getToken } }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('anexa o header Authorization quando há token', () => {
    getToken.mockReturnValue('token-fake');
    const http = TestBed.inject(HttpClient);

    http.get('/qualquer-rota').subscribe();

    const req = httpMock.expectOne('/qualquer-rota');
    expect(req.request.headers.get('Authorization')).toBe('Bearer token-fake');
    req.flush({});
  });

  it('não anexa header nenhum quando não há token', () => {
    getToken.mockReturnValue(null);
    const http = TestBed.inject(HttpClient);

    http.get('/qualquer-rota').subscribe();

    const req = httpMock.expectOne('/qualquer-rota');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });
});
