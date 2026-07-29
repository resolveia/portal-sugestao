import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Comentario, CreateComentarioRequest } from '../models/comentario.model';

@Injectable({ providedIn: 'root' })
export class ComentariosService {
  constructor(private readonly http: HttpClient) {}

  listar(sugestaoId: number): Observable<Comentario[]> {
    return this.http.get<Comentario[]>(`${environment.apiUrl}/sugestoes/${sugestaoId}/comentarios`);
  }

  criar(sugestaoId: number, texto: string): Observable<Comentario> {
    const request: CreateComentarioRequest = { texto };
    return this.http.post<Comentario>(`${environment.apiUrl}/sugestoes/${sugestaoId}/comentarios`, request);
  }

  remover(sugestaoId: number, comentarioId: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/sugestoes/${sugestaoId}/comentarios/${comentarioId}`);
  }
}
