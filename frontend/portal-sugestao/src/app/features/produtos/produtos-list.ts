import { Component, OnInit, signal } from '@angular/core';
import { DxDataGridModule, DxTemplateModule, DxTextBoxModule, DxButtonModule, DxPopupModule } from 'devextreme-angular';
import { SugestoesService } from '../../core/sugestoes/sugestoes.service';
import { Produto } from '../../core/models/sugestao.model';

@Component({
  selector: 'app-produtos-list',
  standalone: true,
  imports: [DxDataGridModule, DxTemplateModule, DxTextBoxModule, DxButtonModule, DxPopupModule],
  templateUrl: './produtos-list.html',
  styleUrl: './produtos-list.scss'
})
export class ProdutosList implements OnInit {
  readonly produtos = signal<Produto[]>([]);
  readonly erro = signal<string | null>(null);
  readonly editandoId = signal<number | null>(null);
  readonly mostrarNovoProduto = signal(false);
  novoNome = '';
  nomeEditado = '';

  constructor(private readonly sugestoesService: SugestoesService) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.sugestoesService.listarTodosProdutos().subscribe({
      next: (dados) => this.produtos.set(dados),
      error: () => this.erro.set('Não foi possível carregar os produtos.')
    });
  }

  criarProduto(): void {
    if (!this.novoNome.trim()) {
      this.erro.set('Informe o nome do produto.');
      return;
    }

    this.sugestoesService.criarProduto(this.novoNome).subscribe({
      next: () => {
        this.novoNome = '';
        this.erro.set(null);
        this.mostrarNovoProduto.set(false);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível criar o produto.')
    });
  }

  iniciarEdicao(produto: Produto): void {
    this.editandoId.set(produto.id);
    this.nomeEditado = produto.nome;
  }

  cancelarEdicao(): void {
    this.editandoId.set(null);
    this.nomeEditado = '';
  }

  salvarEdicao(id: number): void {
    if (!this.nomeEditado.trim()) {
      this.erro.set('Informe o nome do produto.');
      return;
    }

    this.sugestoesService.editarProduto(id, this.nomeEditado).subscribe({
      next: () => {
        this.erro.set(null);
        this.editandoId.set(null);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível editar o produto.')
    });
  }

  remover(id: number): void {
    this.sugestoesService.removerProduto(id).subscribe({
      next: () => {
        this.erro.set(null);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível excluir o produto.')
    });
  }
}
