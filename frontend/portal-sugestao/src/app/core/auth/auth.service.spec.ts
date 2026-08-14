import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { UsuarioLogado } from '../models/usuario.model';
import { environment } from '../../../environments/environment';

const STORAGE_KEY = 'portal-sugestao.auth';

const RESPOSTA_ERP = {
  Erro: false,
  Mensagem: null,
  Usuario: {
    Nome: 'Cliente Teste',
    Login: 'cliente.teste',
    Id: 1,
    EmpresaId: 'EMP1',
    AdminPortalSugestoes: false
  }
};

const RESPOSTA_SESSAO = {
  Erro: false,
  Mensagem: null,
  Usuario: { Id: 1, Nome: 'Cliente Teste', Email: 'cliente.teste@erp.local', Role: 'Cliente' as const }
};

const USUARIO_ESPERADO: UsuarioLogado = {
  id: 1,
  nome: 'Cliente Teste',
  email: 'cliente.teste@erp.local',
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

  it('login() autentica na api_authentication, estabelece a sessão local e grava o usuário', () => {
    const service = TestBed.inject(AuthService);

    service.login('EMP1', 'cliente.teste', 'senha123').subscribe();

    const reqErp = httpMock.expectOne((r) => r.url.startsWith(`${environment.authApiUrl}/authentication/logar`));
    expect(reqErp.request.method).toBe('POST');
    expect(reqErp.request.withCredentials).toBe(true);
    expect(reqErp.request.body).toEqual({ EmpresaID: 'EMP1', Login: 'cliente.teste', Senha: 'senha123', Modulo: '' });
    reqErp.flush(RESPOSTA_ERP);

    const reqSessao = httpMock.expectOne(`${environment.apiUrl}/auth/sessao`);
    expect(reqSessao.request.method).toBe('POST');
    expect(reqSessao.request.withCredentials).toBe(true);
    expect(reqSessao.request.body).toEqual({
      Nome: 'Cliente Teste',
      Login: 'cliente.teste',
      Id: 1,
      EmpresaId: 'EMP1',
      AdminPortalSugestoes: false
    });
    reqSessao.flush(RESPOSTA_SESSAO);

    expect(service.usuario()).toEqual(USUARIO_ESPERADO);
    expect(JSON.parse(localStorage.getItem(STORAGE_KEY)!)).toEqual(USUARIO_ESPERADO);
    expect(service.isAuthenticated()).toBe(true);
  });

  it('login() com erro da api_authentication não grava usuário nenhum', () => {
    const service = TestBed.inject(AuthService);
    let erroRecebido: Error | undefined;

    service.login('EMP1', 'cliente.teste', 'senha-errada').subscribe({
      error: (erro) => (erroRecebido = erro)
    });

    const reqErp = httpMock.expectOne((r) => r.url.startsWith(`${environment.authApiUrl}/authentication/logar`));
    reqErp.flush({ Erro: true, Mensagem: 'Usuário ou senha inválidos.', Usuario: null });

    expect(erroRecebido?.message).toBe('Usuário ou senha inválidos.');
    expect(service.usuario()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
  });

  it('login() com erro ao estabelecer a sessão local não grava usuário nenhum', () => {
    const service = TestBed.inject(AuthService);
    let erroRecebido: Error | undefined;

    service.login('EMP1', 'cliente.teste', 'senha123').subscribe({
      error: (erro) => (erroRecebido = erro)
    });

    const reqErp = httpMock.expectOne((r) => r.url.startsWith(`${environment.authApiUrl}/authentication/logar`));
    reqErp.flush(RESPOSTA_ERP);

    const reqSessao = httpMock.expectOne(`${environment.apiUrl}/auth/sessao`);
    reqSessao.flush({ Erro: true, Mensagem: 'Não foi possível iniciar a sessão local.', Usuario: null });

    expect(erroRecebido?.message).toBe('Não foi possível iniciar a sessão local.');
    expect(service.usuario()).toBeNull();
    expect(service.isAuthenticated()).toBe(false);
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
