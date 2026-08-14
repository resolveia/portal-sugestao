import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DxTextBoxModule, DxButtonModule } from 'devextreme-angular';
import { AuthService } from '../../core/auth/auth.service';

/**
 * Login real contra a api_authentication (ou o simulador local dela) — ver
 * docs/autenticacao-e-api-portal-sugestoes.md. Não existe mais cadastro/login livre pelo Portal:
 * o usuário já precisa existir do lado do ERP.
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [DxTextBoxModule, DxButtonModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  empresaId = '';
  login = '';
  senha = '';

  readonly erro = signal<string | null>(null);
  readonly carregando = signal(false);

  constructor(private readonly authService: AuthService, private readonly router: Router) {}

  entrar(): void {
    this.erro.set(null);

    if (!this.empresaId || !this.login || !this.senha) {
      this.erro.set('Preencha ID, login e senha.');
      return;
    }

    this.carregando.set(true);
    this.authService.login(this.empresaId, this.login, this.senha).subscribe({
      next: () => {
        this.carregando.set(false);
        this.router.navigate(['/sugestoes']);
      },
      error: (erro: Error) => {
        this.carregando.set(false);
        this.erro.set(erro.message || 'Não foi possível entrar.');
      }
    });
  }
}
