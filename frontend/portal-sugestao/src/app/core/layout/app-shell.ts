import { Component, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { DxButtonModule } from 'devextreme-angular';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, DxButtonModule],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss'
})
export class AppShell {
  readonly menuAberto = signal(false);

  constructor(
    protected readonly authService: AuthService,
    private readonly router: Router
  ) {}

  fecharMenu(): void {
    this.menuAberto.set(false);
  }

  sair(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
