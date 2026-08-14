import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Comentario } from '../models/comentario.model';

function mapComentario(c: any): Comentario {
  return {
    id: c.Id,
    sugestaoId: c.SugestaoId,
    usuarioId: c.UsuarioId,
    autorNome: c.AutorNome,
    texto: c.Texto,
    dataCriacao: c.DataCriacao
  };
}

@Injectable({ providedIn: 'root' })
export class ComentariosService {
  constructor(private readonly http: HttpClient) {}

  listar(sugestaoId: number): Observable<Comentario[]> {
    return this.handle(
      this.http.post<any>(`${environment.apiUrl}/sugestoes/${sugestaoId}/comentarios/listar`, {}),
      (r) => (r.Comentarios ?? []).map(mapComentario)
    );
  }

  criar(sugestaoId: number, texto: string): Observable<Comentario> {
    return this.handle(
      this.http.post<any>(`${environment.apiUrl}/sugestoes/${sugestaoId}/comentarios/salvar`, { texto }),
      (r) => mapComentario(r.Comentario)
    );
  }

  remover(sugestaoId: number, comentarioId: number): Observable<void> {
    return this.handle(
      this.http.post<any>(`${environment.apiUrl}/sugestoes/${sugestaoId}/comentarios/remover/${comentarioId}`, {}),
      () => undefined
    );
  }

  private handle<T>(source: Observable<any>, extract: (body: any) => T): Observable<T> {
    return source.pipe(
      map((body) => {
        if (body.Erro) {
          throw new Error(body.Mensagem ?? 'Ocorreu um erro.');
        }
        return extract(body);
      })
    );
  }
}
