import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DxButtonModule } from 'devextreme-angular';
import { AuthService } from '../../../core/auth/auth.service';

/**
 * Rota de login automático: equivalente à URL que o ERP vai abrir com "?token=..." (ver
 * docs/sso-checklist.md). Lê o token da query string, chama o backend e redireciona.
 * Convive com o login manual (/login) — decisão do time do ERP (2026-08-12).
 */
@Component({
  selector: 'app-login-token',
  standalone: true,
  imports: [DxButtonModule],
  templateUrl: './login-token.html',
  styleUrl: './login-token.scss'
})
export class LoginToken implements OnInit {
  readonly erro = signal<string | null>(null);

  constructor(
    private readonly route: ActivatedRoute,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');

    if (!token) {
      this.erro.set('Token não informado na URL.');
      return;
    }

    this.authService.loginViaToken(token).subscribe({
      next: (response) => {
        if (response.erro || !response.usuario) {
          this.erro.set(response.mensagem ?? 'Não foi possível autenticar.');
          return;
        }
        this.router.navigate(['/sugestoes']);
      },
      error: () => this.erro.set('Não foi possível autenticar. Verifique se a API está rodando.')
    });
  }

  voltarParaLoginManual(): void {
    this.router.navigate(['/login']);
  }
}
