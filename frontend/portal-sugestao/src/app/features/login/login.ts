import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DxTextBoxModule, DxSelectBoxModule, DxButtonModule } from 'devextreme-angular';
import { AuthService } from '../../core/auth/auth.service';
import { RoleUsuario } from '../../core/models/usuario.model';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [DxTextBoxModule, DxSelectBoxModule, DxButtonModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  email = '';
  nome = '';
  empresa = '';
  role: RoleUsuario = 'Cliente';
  roles: RoleUsuario[] = ['Cliente', 'AdminInterno'];

  readonly erro = signal<string | null>(null);
  readonly carregando = signal(false);
  readonly simulando = signal(false);

  constructor(private readonly authService: AuthService, private readonly router: Router) {}

  entrar(): void {
    this.erro.set(null);

    if (!this.email || !this.nome || !this.empresa) {
      this.erro.set('Preencha e-mail, nome e empresa.');
      return;
    }

    this.carregando.set(true);
    this.authService
      .login({ email: this.email, nome: this.nome, empresa: this.empresa, role: this.role })
      .subscribe({
        next: () => {
          this.carregando.set(false);
          this.router.navigate(['/sugestoes']);
        },
        error: () => {
          this.carregando.set(false);
          this.erro.set('Não foi possível entrar. Verifique se a API está rodando.');
        }
      });
  }

  /** Simula a entrada automática vinda do botão "Portal de Sugestão" do ERP (token hoje simulado). */
  simularEntradaViaErp(perfil: 'admin' | 'cliente'): void {
    this.erro.set(null);
    this.simulando.set(true);

    this.authService.tokensDemo().subscribe({
      next: (tokens) => {
        this.simulando.set(false);
        const token = perfil === 'admin' ? tokens.admin : tokens.cliente;
        this.router.navigate(['/login/token'], { queryParams: { token } });
      },
      error: () => {
        this.simulando.set(false);
        this.erro.set('Não foi possível simular a entrada via ERP.');
      }
    });
  }
}
