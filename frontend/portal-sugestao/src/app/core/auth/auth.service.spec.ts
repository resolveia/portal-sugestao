import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { MockLoginResponse } from '../models/usuario.model';
import { environment } from '../../../environments/environment';

const STORAGE_KEY = 'portal-sugestao.auth';

const RESPOSTA_MOCK: MockLoginResponse = {
  token: 'token-fake',
  expiresAt: '2026-01-01T00:00:00Z',
  usuarioId: 1,
  nome: 'Cliente Teste',
  email: 'cliente@empresa.com',
  role: 'Cliente'
};

describe('AuthService', () => {
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('login() grava no localStorage e atualiza o signal usuario', () => {
    const service = TestBed.inject(AuthService);

    service.login({ email: RESPOSTA_MOCK.email, nome: RESPOSTA_MOCK.nome, empresa: 'Empresa', role: 'Cliente' })
      .subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/mock-login`);
    expect(req.request.method).toBe('POST');
    req.flush(RESPOSTA_MOCK);

    expect(service.usuario()).toEqual(RESPOSTA_MOCK);
    expect(JSON.parse(localStorage.getItem(STORAGE_KEY)!)).toEqual(RESPOSTA_MOCK);
    expect(service.getToken()).toBe('token-fake');
    expect(service.isAuthenticated()).toBe(true);
  });

  it('logout() limpa o signal e o localStorage', () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(RESPOSTA_MOCK));
    const service = TestBed.inject(AuthService);
    expect(service.isAuthenticated()).toBe(true);

    service.logout();

    expect(service.usuario()).toBeNull();
    expect(service.getToken()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
  });

  it('lê uma sessão já existente no localStorage ao construir', () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(RESPOSTA_MOCK));

    const service = TestBed.inject(AuthService);

    expect(service.usuario()).toEqual(RESPOSTA_MOCK);
    expect(service.isAuthenticated()).toBe(true);
  });

  it('sem sessão salva, começa deslogado', () => {
    const service = TestBed.inject(AuthService);

    expect(service.usuario()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });
});
