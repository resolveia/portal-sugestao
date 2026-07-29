import { Component, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { DxDataGridModule, DxTemplateModule, DxTextBoxModule, DxButtonModule } from 'devextreme-angular';
import { SugestoesService } from '../../core/sugestoes/sugestoes.service';
import { Sugestao } from '../../core/models/sugestao.model';

@Component({
  selector: 'app-moderacao-list',
  standalone: true,
  imports: [DxDataGridModule, DxTemplateModule, DxTextBoxModule, DxButtonModule],
  templateUrl: './moderacao-list.html',
  styleUrl: './moderacao-list.scss'
})
export class ModeracaoList implements OnInit {
  readonly pendentes = signal<Sugestao[]>([]);
  readonly erro = signal<string | null>(null);
  readonly rejeitandoId = signal<number | null>(null);
  motivoTexto = '';

  constructor(
    private readonly sugestoesService: SugestoesService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.sugestoesService.listarPendentes().subscribe({
      next: (dados) => this.pendentes.set(dados),
      error: () => this.erro.set('Não foi possível carregar a fila de moderação.')
    });
  }

  aprovar(id: number): void {
    this.sugestoesService.aprovar(id).subscribe({
      next: () => {
        this.erro.set(null);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível aprovar a sugestão.')
    });
  }

  iniciarRejeicao(id: number): void {
    this.rejeitandoId.set(id);
    this.motivoTexto = '';
  }

  cancelarRejeicao(): void {
    this.rejeitandoId.set(null);
    this.motivoTexto = '';
  }

  confirmarRejeicao(id: number): void {
    if (!this.motivoTexto.trim()) {
      this.erro.set('Informe o motivo da rejeição.');
      return;
    }

    this.sugestoesService.rejeitar(id, this.motivoTexto).subscribe({
      next: () => {
        this.erro.set(null);
        this.rejeitandoId.set(null);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível rejeitar a sugestão.')
    });
  }

  voltar(): void {
    this.router.navigate(['/sugestoes']);
  }
}
