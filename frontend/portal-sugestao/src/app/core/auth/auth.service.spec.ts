import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { LoginTokenResponse, MockLoginResponse, TokensDemoResponse, UsuarioLogado } from '../models/usuario.model';
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

const USUARIO_ESPERADO: UsuarioLogado = {
  id: 1,
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

  it('login() grava o usuário no localStorage e atualiza o signal usuario, com withCredentials', () => {
    const service = TestBed.inject(AuthService);

    service.login({ email: RESPOSTA_MOCK.email, nome: RESPOSTA_MOCK.nome, empresa: 'Empresa', role: 'Cliente' })
      .subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/mock-login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.withCredentials).toBe(true);
    req.flush(RESPOSTA_MOCK);

    expect(service.usuario()).toEqual(USUARIO_ESPERADO);
    expect(JSON.parse(localStorage.getItem(STORAGE_KEY)!)).toEqual(USUARIO_ESPERADO);
    expect(service.isAuthenticated()).toBe(true);
  });

  it('loginViaToken() com sucesso grava o usuário e atualiza o signal', () => {
    const service = TestBed.inject(AuthService);
    const resposta: LoginTokenResponse = { erro: false, mensagem: null, usuario: USUARIO_ESPERADO };

    service.loginViaToken('token-simulado-do-erp').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login-token`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ token: 'token-simulado-do-erp' });
    req.flush(resposta);

    expect(service.usuario()).toEqual(USUARIO_ESPERADO);
    expect(service.isAuthenticated()).toBe(true);
  });

  it('loginViaToken() com erro não grava usuário nenhum', () => {
    const service = TestBed.inject(AuthService);
    const resposta: LoginTokenResponse = { erro: true, mensagem: 'Token inválido.', usuario: null };

    service.loginViaToken('token-invalido').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login-token`);
    req.flush(resposta);

    expect(service.usuario()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });

  it('tokensDemo() busca os tokens de demonstração', () => {
    const service = TestBed.inject(AuthService);
    const resposta: TokensDemoResponse = { admin: 'token-admin', cliente: 'token-cliente' };
    let recebido: TokensDemoResponse | undefined;

    service.tokensDemo().subscribe((r) => (recebido = r));

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/tokens-demo`);
    expect(req.request.method).toBe('GET');
    req.flush(resposta);

    expect(recebido).toEqual(resposta);
  });

  it('logout() chama a API, limpa o signal e o localStorage', () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(USUARIO_ESPERADO));
    const service = TestBed.inject(AuthService);
    expect(service.isAuthenticated()).toBe(true);

    service.logout();

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/logout`);
    expect(req.request.withCredentials).toBe(true);
    req.flush({});

    expect(service.usuario()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
  });

  it('limparSessaoLocal() limpa o signal e o localStorage sem chamar a API', () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(USUARIO_ESPERADO));
    const service = TestBed.inject(AuthService);

    service.limparSessaoLocal();

    httpMock.expectNone(`${environment.apiUrl}/auth/logout`);
    expect(service.usuario()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
  });

  it('lê uma sessão já existente no localStorage ao construir', () => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(USUARIO_ESPERADO));

    const service = TestBed.inject(AuthService);

    expect(service.usuario()).toEqual(USUARIO_ESPERADO);
    expect(service.isAuthenticated()).toBe(true);
  });

  it('sem sessão salva, começa deslogado', () => {
    const service = TestBed.inject(AuthService);

    expect(service.usuario()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });
});
