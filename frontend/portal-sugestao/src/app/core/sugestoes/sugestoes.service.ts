import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Categoria, CreateSugestaoRequest, RejeitarRequest, Sugestao } from '../models/sugestao.model';

@Injectable({ providedIn: 'root' })
export class SugestoesService {
  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Sugestao[]> {
    return this.http.get<Sugestao[]>(`${environment.apiUrl}/sugestoes`);
  }

  listarPendentes(): Observable<Sugestao[]> {
    return this.http.get<Sugestao[]>(`${environment.apiUrl}/sugestoes/pendentes`);
  }

  criar(request: CreateSugestaoRequest): Observable<Sugestao> {
    return this.http.post<Sugestao>(`${environment.apiUrl}/sugestoes`, request);
  }

  editar(id: number, request: CreateSugestaoRequest): Observable<Sugestao> {
    return this.http.put<Sugestao>(`${environment.apiUrl}/sugestoes/${id}`, request);
  }

  aprovar(id: number): Observable<Sugestao> {
    return this.http.put<Sugestao>(`${environment.apiUrl}/sugestoes/${id}/aprovar`, {});
  }

  rejeitar(id: number, motivo: string): Observable<Sugestao> {
    const request: RejeitarRequest = { motivo };
    return this.http.put<Sugestao>(`${environment.apiUrl}/sugestoes/${id}/rejeitar`, request);
  }

  listarCategorias(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(`${environment.apiUrl}/categorias`);
  }
}
