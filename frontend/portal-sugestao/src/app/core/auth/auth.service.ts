import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MockLoginRequest, MockLoginResponse } from '../models/usuario.model';

const STORAGE_KEY = 'portal-sugestao.auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly usuarioSignal = signal<MockLoginResponse | null>(this.readFromStorage());
  readonly usuario = this.usuarioSignal.asReadonly();

  constructor(private readonly http: HttpClient) {}

  login(request: MockLoginRequest): Observable<MockLoginResponse> {
    return this.http.post<MockLoginResponse>(`${environment.apiUrl}/auth/mock-login`, request).pipe(
      tap((response) => {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
        this.usuarioSignal.set(response);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.usuarioSignal.set(null);
  }

  getToken(): string | null {
    return this.usuarioSignal()?.token ?? null;
  }

  isAuthenticated(): boolean {
    return this.getToken() !== null;
  }

  private readFromStorage(): MockLoginResponse | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as MockLoginResponse) : null;
  }
}
