import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginTokenResponse, MockLoginRequest, MockLoginResponse, TokensDemoResponse, UsuarioLogado } from '../models/usuario.model';

const STORAGE_KEY = 'portal-sugestao.auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly usuarioSignal = signal<UsuarioLogado | null>(this.readFromStorage());
  readonly usuario = this.usuarioSignal.asReadonly();

  constructor(private readonly http: HttpClient) {}

  /** Login manual (formulário) — continua existindo em paralelo ao login automático via token. */
  login(request: MockLoginRequest): Observable<MockLoginResponse> {
    return this.http.post<MockLoginResponse>(`${environment.apiUrl}/auth/mock-login`, request, { withCredentials: true }).pipe(
      tap((response) =>
        this.armazenarUsuario({ id: response.usuarioId, nome: response.nome, email: response.email, role: response.role })
      )
    );
  }

  /** Login automático via token — equivalente ao fluxo real de SSO do ERP (token hoje simulado). */
  loginViaToken(token: string): Observable<LoginTokenResponse> {
    return this.http.post<LoginTokenResponse>(`${environment.apiUrl}/auth/login-token`, { token }, { withCredentials: true }).pipe(
      tap((response) => {
        if (!response.erro && response.usuario) {
          this.armazenarUsuario(response.usuario);
        }
      })
    );
  }

  /** Tokens de demonstração (Admin/Cliente) pra simular a entrada vinda do ERP — remover quando o SSO real existir. */
  tokensDemo(): Observable<TokensDemoResponse> {
    return this.http.get<TokensDemoResponse>(`${environment.apiUrl}/auth/tokens-demo`);
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
