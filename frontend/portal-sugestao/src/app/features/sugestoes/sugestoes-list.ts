import { Component, OnInit, computed, signal } from '@angular/core';
import { DxDataGridModule, DxTemplateModule } from 'devextreme-angular';
import { DxTextBoxModule, DxSelectBoxModule, DxButtonModule, DxPopupModule } from 'devextreme-angular';
import CustomStore from 'devextreme/data/custom_store';
import DataSource from 'devextreme/data/data_source';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { SugestoesService } from '../../core/sugestoes/sugestoes.service';
import { Categoria, Produto } from '../../core/models/sugestao.model';
import { Comentarios } from './comentarios/comentarios';

const LIMITE_VOTOS = 3;
const TAMANHO_PAGINA = 20;

@Component({
  selector: 'app-sugestoes-list',
  standalone: true,
  imports: [DxDataGridModule, DxTemplateModule, DxTextBoxModule, DxSelectBoxModule, DxButtonModule, DxPopupModule, Comentarios],
  templateUrl: './sugestoes-list.html',
  styleUrl: './sugestoes-list.scss'
})
export class SugestoesList implements OnInit {
  readonly categorias = signal<Categoria[]>([]);
  readonly produtos = signal<Produto[]>([]);
  readonly erro = signal<string | null>(null);

  readonly limiteVotos = LIMITE_VOTOS;
  // Vem do servidor a cada carregamento de página — o limite de 3 votos é sobre o total do
  // usuário, não sobre os itens da página atual (por isso não dá pra somar votadoPorMim aqui).
  readonly votosUsados = signal(0);
  readonly votosDisponiveis = computed(() => this.limiteVotos - this.votosUsados());

  readonly mostrarNovaSugestao = signal(false);

  // Paginação server-side (RNF seção 11 — docs/performance-report.md): a grid busca só a
  // página atual via CustomStore, em vez de carregar todas as sugestões publicadas de uma vez.
  readonly sugestoesDataSource = new DataSource({
    store: new CustomStore({
      key: 'id',
      load: (loadOptions) => {
        const skip = loadOptions.skip ?? 0;
        const take = loadOptions.take ?? TAMANHO_PAGINA;
        return firstValueFrom(this.sugestoesService.listarPaginado(skip, take))
          .then((resposta) => {
            this.votosUsados.set(resposta.votosUsadosPeloUsuarioAtual);
            this.erro.set(null);
            return { data: resposta.items, totalCount: resposta.total };
          })
          .catch((erro) => {
            this.erro.set('Não foi possível carregar as sugestões.');
            throw erro;
          });
      }
    }),
    paginate: true,
    pageSize: TAMANHO_PAGINA
  });

  novoProdutoId: number | null = null;
  novoTitulo = '';
  novaDescricao = '';
  novoResultadoEsperado = '';
  novaCategoriaId: number | null = null;

  constructor(
    private readonly sugestoesService: SugestoesService,
    protected readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    this.carregarReferencias();
  }

  carregarReferencias(): void {
    this.sugestoesService.listarCategorias().subscribe({
      next: (dados) => this.categorias.set(dados),
      error: () => this.erro.set('Não foi possível carregar as categorias.')
    });

    this.sugestoesService.listarProdutos().subscribe({
      next: (dados) => this.produtos.set(dados),
      error: () => this.erro.set('Não foi possível carregar os produtos.')
    });
  }

  criarSugestao(): void {
    if (!this.novoProdutoId || !this.novoTitulo || !this.novaDescricao || !this.novoResultadoEsperado || !this.novaCategoriaId) {
      this.erro.set('Preencha produto, título, descrição, resultado esperado e categoria.');
      return;
    }

    this.sugestoesService
      .criar({
        produtoId: this.novoProdutoId,
        titulo: this.novoTitulo,
        descricao: this.novaDescricao,
        resultadoEsperado: this.novoResultadoEsperado,
        categoriaId: this.novaCategoriaId
      })
      .subscribe({
        next: () => {
          this.novoProdutoId = null;
          this.novoTitulo = '';
          this.novaDescricao = '';
          this.novoResultadoEsperado = '';
          this.novaCategoriaId = null;
          this.erro.set(null);
          this.mostrarNovaSugestao.set(false);
          // Sugestão entra "Em moderação" — só aparece no ranking após aprovação pelo Admin.
        },
        error: () => this.erro.set('Não foi possível criar a sugestão.')
      });
  }

  votar(id: number): void {
    this.sugestoesService.votar(id).subscribe({
      next: () => {
        this.erro.set(null);
        this.sugestoesDataSource.reload();
      },
      error: () => this.erro.set('Não foi possível registrar o voto.')
    });
  }

  removerVoto(id: number): void {
    this.sugestoesService.removerVoto(id).subscribe({
      next: () => {
        this.erro.set(null);
        this.sugestoesDataSource.reload();
      },
      error: () => this.erro.set('Não foi possível remover o voto.')
    });
  }

}
