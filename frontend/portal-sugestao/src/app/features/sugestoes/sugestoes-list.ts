import { Component, OnInit, signal } from '@angular/core';
import { DxDataGridModule } from 'devextreme-angular';
import { DxTextBoxModule, DxSelectBoxModule, DxButtonModule } from 'devextreme-angular';
import { AuthService } from '../../core/auth/auth.service';
import { SugestoesService } from '../../core/sugestoes/sugestoes.service';
import { Categoria, Sugestao } from '../../core/models/sugestao.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sugestoes-list',
  standalone: true,
  imports: [DxDataGridModule, DxTextBoxModule, DxSelectBoxModule, DxButtonModule],
  templateUrl: './sugestoes-list.html',
  styleUrl: './sugestoes-list.scss'
})
export class SugestoesList implements OnInit {
  readonly sugestoes = signal<Sugestao[]>([]);
  readonly categorias = signal<Categoria[]>([]);
  readonly erro = signal<string | null>(null);

  novoTitulo = '';
  novaDescricao = '';
  novaCategoriaId: number | null = null;

  constructor(
    private readonly sugestoesService: SugestoesService,
    protected readonly authService: AuthService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.sugestoesService.listar().subscribe({
      next: (dados) => this.sugestoes.set(dados),
      error: () => this.erro.set('Não foi possível carregar as sugestões.')
    });

    this.sugestoesService.listarCategorias().subscribe({
      next: (dados) => this.categorias.set(dados),
      error: () => this.erro.set('Não foi possível carregar as categorias.')
    });
  }

  criarSugestao(): void {
    if (!this.novoTitulo || !this.novaDescricao || !this.novaCategoriaId) {
      this.erro.set('Preencha título, descrição e categoria.');
      return;
    }

    this.sugestoesService
      .criar({ titulo: this.novoTitulo, descricao: this.novaDescricao, categoriaId: this.novaCategoriaId })
      .subscribe({
        next: () => {
          this.novoTitulo = '';
          this.novaDescricao = '';
          this.novaCategoriaId = null;
          this.erro.set(null);
          // Sugestão entra "Em moderação" — só aparece no ranking após aprovação (Fase 2).
        },
        error: () => this.erro.set('Não foi possível criar a sugestão.')
      });
  }

  sair(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
