import { Component, OnInit, computed, signal } from '@angular/core';
import { DxDataGridModule, DxTemplateModule } from 'devextreme-angular';
import { DxTextBoxModule, DxSelectBoxModule, DxButtonModule } from 'devextreme-angular';
import { AuthService } from '../../core/auth/auth.service';
import { SugestoesService } from '../../core/sugestoes/sugestoes.service';
import { Categoria, Sugestao } from '../../core/models/sugestao.model';
import { Router } from '@angular/router';
import { Comentarios } from './comentarios/comentarios';

const LIMITE_VOTOS = 3;

@Component({
  selector: 'app-sugestoes-list',
  standalone: true,
  imports: [DxDataGridModule, DxTemplateModule, DxTextBoxModule, DxSelectBoxModule, DxButtonModule, Comentarios],
  templateUrl: './sugestoes-list.html',
  styleUrl: './sugestoes-list.scss'
})
export class SugestoesList implements OnInit {
  readonly sugestoes = signal<Sugestao[]>([]);
  readonly categorias = signal<Categoria[]>([]);
  readonly erro = signal<string | null>(null);

  readonly limiteVotos = LIMITE_VOTOS;
  readonly votosUsados = computed(() => this.sugestoes().filter((s) => s.votadoPorMim).length);
  readonly votosDisponiveis = computed(() => this.limiteVotos - this.votosUsados());

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
          // Sugestão entra "Em moderação" — só aparece no ranking após aprovação pelo Admin.
        },
        error: () => this.erro.set('Não foi possível criar a sugestão.')
      });
  }

  votar(id: number): void {
    this.sugestoesService.votar(id).subscribe({
      next: () => {
        this.erro.set(null);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível registrar o voto.')
    });
  }

  removerVoto(id: number): void {
    this.sugestoesService.removerVoto(id).subscribe({
      next: () => {
        this.erro.set(null);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível remover o voto.')
    });
  }

  irParaModeracao(): void {
    this.router.navigate(['/moderacao']);
  }

  irParaCategorias(): void {
    this.router.navigate(['/categorias']);
  }

  sair(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
