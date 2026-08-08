import { Component, signal } from '@angular/core';
import { DxDataGridModule, DxTemplateModule, DxSelectBoxModule } from 'devextreme-angular';
import CustomStore from 'devextreme/data/custom_store';
import DataSource from 'devextreme/data/data_source';
import { firstValueFrom } from 'rxjs';
import { SugestoesService } from '../../core/sugestoes/sugestoes.service';
import { ESTAGIO_ROADMAP_OPCOES, EstagioRoadmap } from '../../core/models/sugestao.model';

const TAMANHO_PAGINA = 20;

@Component({
  selector: 'app-roadmap-list',
  standalone: true,
  imports: [DxDataGridModule, DxTemplateModule, DxSelectBoxModule],
  templateUrl: './roadmap-list.html',
  styleUrl: './roadmap-list.scss'
})
export class RoadmapList {
  readonly erro = signal<string | null>(null);
  readonly estagioOpcoes = ESTAGIO_ROADMAP_OPCOES;

  // Mesmo dataset paginado do ranking público (GET /api/sugestoes já só devolve publicadas).
  readonly sugestoesDataSource = new DataSource({
    store: new CustomStore({
      key: 'id',
      load: (loadOptions) => {
        const skip = loadOptions.skip ?? 0;
        const take = loadOptions.take ?? TAMANHO_PAGINA;
        return firstValueFrom(this.sugestoesService.listarPaginado(skip, take))
          .then((resposta) => {
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

  constructor(private readonly sugestoesService: SugestoesService) {}

  alterarEstagio(id: number, estagio: EstagioRoadmap): void {
    this.sugestoesService.atualizarEstagioRoadmap(id, estagio).subscribe({
      next: () => {
        this.erro.set(null);
        this.sugestoesDataSource.reload();
      },
      error: () => this.erro.set('Não foi possível atualizar o estágio.')
    });
  }
}
