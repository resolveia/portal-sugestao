import { Component, Input, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { DxTextBoxModule, DxButtonModule } from 'devextreme-angular';
import { AuthService } from '../../../core/auth/auth.service';
import { ComentariosService } from '../../../core/comentarios/comentarios.service';
import { Comentario } from '../../../core/models/comentario.model';

@Component({
  selector: 'app-comentarios',
  standalone: true,
  imports: [DatePipe, DxTextBoxModule, DxButtonModule],
  templateUrl: './comentarios.html',
  styleUrl: './comentarios.scss'
})
export class Comentarios implements OnInit {
  @Input({ required: true }) sugestaoId!: number;

  readonly comentarios = signal<Comentario[]>([]);
  readonly erro = signal<string | null>(null);
  novoTexto = '';

  constructor(
    private readonly comentariosService: ComentariosService,
    protected readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    this.carregar();
  }

  carregar(): void {
    this.comentariosService.listar(this.sugestaoId).subscribe({
      next: (dados) => this.comentarios.set(dados),
      error: () => this.erro.set('Não foi possível carregar os comentários.')
    });
  }

  comentar(): void {
    if (!this.novoTexto.trim()) {
      this.erro.set('Escreva um comentário antes de enviar.');
      return;
    }

    this.comentariosService.criar(this.sugestaoId, this.novoTexto).subscribe({
      next: () => {
        this.novoTexto = '';
        this.erro.set(null);
        this.carregar();
      },
      error: () => this.erro.set('Não foi possível enviar o comentário.')
    });
  }

  remover(comentarioId: number): void {
    this.comentariosService.remover(this.sugestaoId, comentarioId).subscribe({
      next: () => this.carregar(),
      error: () => this.erro.set('Não foi possível remover o comentário.')
    });
  }
}
