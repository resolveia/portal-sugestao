import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DxDataGridModule, DxTextBoxModule, DxButtonModule } from 'devextreme-angular';
import { SugestoesService } from '../../core/sugestoes/sugestoes.service';
import { Categoria } from '../../core/models/sugestao.model';

@Component({
  selector: 'app-categorias-list',
  standalone: true,
  imports: [DxDataGridModule, DxTextBoxModule, DxButtonModule],
  templateUrl: './categorias-list.html',
  styleUrl: './categorias-list.scss'
})
export class CategoriasList implements OnInit {
  readonly categorias = signal<Categoria[]>([]);
  readonly erro = signal<string | null>(null);
  novoNome = '';

  constructor(
    private readonly sugestoesService: SugestoesService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.sugestoesService.listarCategorias().subscribe({
      next: (dados) => this.categorias.set(dados),
      error: () => this.erro.set('Não foi possível carregar as categorias.')
    });
  }

  criarCategoria(): void {
    if (!this.novoNome.trim()) {
      this.erro.set('Informe o nome da categoria.');
      return;
    }

    this.sugestoesService.criarCategoria(this.novoNome).subscribe({
      next: () => {
        this.novoNome = '';
        this.erro.set(null);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível criar a categoria.')
    });
  }

  voltar(): void {
    this.router.navigate(['/sugestoes']);
  }
}
