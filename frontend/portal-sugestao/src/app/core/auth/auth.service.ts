import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map, switchMap, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { UsuarioLogado } from '../models/usuario.model';

const STORAGE_KEY = 'portal-sugestao.auth';

/** Formato devolvido pela api_authentication real (docs/autenticacao-e-api-portal-sugestoes.md). */
interface LoginErpResponse {
  Erro: boolean;
  Mensagem?: string | null;
  Usuario?: {
    Nome: string;
    Login: string;
    Id: number;
    EmpresaId: string;
    AdminPortalSugestoes: boolean;
  };
}

/** Formato do nosso próprio backend (dublê local de api_portal_sugestoes). */
interface SessaoResponse {
  Erro: boolean;
  Mensagem?: string | null;
  Usuario?: { Id: number; Nome: string; Email: string; Role: 'Cliente' | 'AdminInterno' };
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly usuarioSignal = signal<UsuarioLogado | null>(this.readFromStorage());
  readonly usuario = this.usuarioSignal.asReadonly();

  constructor(private readonly http: HttpClient) {}

  /**
   * Login contra a api_authentication real (ou o simulador local dela, environment.authApiUrl) e,
   * em seguida, estabelece a sessão local no nosso backend (dublê de api_portal_sugestoes) com os
   * dados devolvidos — ver docs/autenticacao-e-api-portal-sugestoes.md.
   */
  login(empresaId: string, login: string, senha: string): Observable<UsuarioLogado> {
    const idioma = encodeURIComponent(navigator.language);

    return this.http
      .post<LoginErpResponse>(
        `${environment.authApiUrl}/authentication/logar?idioma=${idioma}`,
        { EmpresaID: empresaId, Login: login, Senha: senha, Modulo: '' },
        { withCredentials: true }
      )
      .pipe(
        switchMap((respErp) => {
          if (respErp.Erro || !respErp.Usuario) {
            return throwError(() => new Error(respErp.Mensagem ?? 'Não foi possível autenticar.'));
          }

          const u = respErp.Usuario;
          return this.http.post<SessaoResponse>(
            `${environment.apiUrl}/auth/sessao`,
            { Nome: u.Nome, Login: u.Login, Id: u.Id, EmpresaId: u.EmpresaId, AdminPortalSugestoes: u.AdminPortalSugestoes },
            { withCredentials: true }
          );
        }),
        map((respSessao) => {
          if (respSessao.Erro || !respSessao.Usuario) {
            throw new Error(respSessao.Mensagem ?? 'Não foi possível iniciar a sessão local.');
          }

          const usuario: UsuarioLogado = {
            id: respSessao.Usuario.Id,
            nome: respSessao.Usuario.Nome,
            email: respSessao.Usuario.Email,
            role: respSessao.Usuario.Role
          };
          this.armazenarUsuario(usuario);
          return usuario;
        })
      );
  }

  logout(): void {
    this.http.post(`${environment.apiUrl}/auth/logout`, {}, { withCredentials: true }).subscribe();
    localStorage.removeItem(STORAGE_KEY);
    this.usuarioSignal.set(null);
  }

  /** Limpa só o estado local (sem chamar /logout) — usado quando a API já respondeu 401. */
  limparSessaoLocal(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.usuarioSignal.set(null);
  }

  isAuthenticated(): boolean {
    return this.usuarioSignal() !== null;
  }

  private armazenarUsuario(usuario: UsuarioLogado): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(usuario));
    this.usuarioSignal.set(usuario);
  }

  private readFromStorage(): UsuarioLogado | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as UsuarioLogado) : null;
  }
}
