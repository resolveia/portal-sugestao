import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Categoria, CreateSugestaoRequest, Sugestao } from '../models/sugestao.model';

@Injectable({ providedIn: 'root' })
export class SugestoesService {
  constructor(private readonly http: HttpClient) {}

  listar(): Observable<Sugestao[]> {
    return this.http.get<Sugestao[]>(`${environment.apiUrl}/sugestoes`);
  }

  criar(request: CreateSugestaoRequest): Observable<Sugestao> {
    return this.http.post<Sugestao>(`${environment.apiUrl}/sugestoes`, request);
  }

  listarCategorias(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(`${environment.apiUrl}/categorias`);
  }
}
