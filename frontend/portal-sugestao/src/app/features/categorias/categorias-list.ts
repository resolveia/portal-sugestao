import { Component, OnInit, signal } from '@angular/core';
import { DxDataGridModule, DxTemplateModule, DxTextBoxModule, DxButtonModule, DxPopupModule } from 'devextreme-angular';
import { SugestoesService } from '../../core/sugestoes/sugestoes.service';
import { Categoria } from '../../core/models/sugestao.model';

@Component({
  selector: 'app-categorias-list',
  standalone: true,
  imports: [DxDataGridModule, DxTemplateModule, DxTextBoxModule, DxButtonModule, DxPopupModule],
  templateUrl: './categorias-list.html',
  styleUrl: './categorias-list.scss'
})
export class CategoriasList implements OnInit {
  readonly categorias = signal<Categoria[]>([]);
  readonly erro = signal<string | null>(null);
  readonly editandoId = signal<number | null>(null);
  readonly mostrarNovaCategoria = signal(false);
  novoNome = '';
  nomeEditado = '';

  constructor(private readonly sugestoesService: SugestoesService) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.sugestoesService.listarTodasCategorias().subscribe({
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
        this.mostrarNovaCategoria.set(false);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível criar a categoria.')
    });
  }

  iniciarEdicao(categoria: Categoria): void {
    this.editandoId.set(categoria.id);
    this.nomeEditado = categoria.nome;
  }

  cancelarEdicao(): void {
    this.editandoId.set(null);
    this.nomeEditado = '';
  }

  salvarEdicao(id: number): void {
    if (!this.nomeEditado.trim()) {
      this.erro.set('Informe o nome da categoria.');
      return;
    }

    this.sugestoesService.editarCategoria(id, this.nomeEditado).subscribe({
      next: () => {
        this.erro.set(null);
        this.editandoId.set(null);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível editar a categoria.')
    });
  }

  remover(id: number): void {
    this.sugestoesService.removerCategoria(id).subscribe({
      next: () => {
        this.erro.set(null);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível excluir a categoria.')
    });
  }
}
