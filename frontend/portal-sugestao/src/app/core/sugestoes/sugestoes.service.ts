import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Categoria, CreateSugestaoRequest, Produto, RejeitarRequest, Sugestao, SugestoesPaginadas } from '../models/sugestao.model';

@Injectable({ providedIn: 'root' })
export class SugestoesService {
  constructor(private readonly http: HttpClient) {}

  listarPaginado(skip: number, take: number): Observable<SugestoesPaginadas> {
    return this.http.get<SugestoesPaginadas>(`${environment.apiUrl}/sugestoes`, { params: { skip, take } });
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

  votar(id: number): Observable<Sugestao> {
    return this.http.post<Sugestao>(`${environment.apiUrl}/sugestoes/${id}/votos`, {});
  }

  removerVoto(id: number): Observable<Sugestao> {
    return this.http.delete<Sugestao>(`${environment.apiUrl}/sugestoes/${id}/votos`);
  }

  listarCategorias(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(`${environment.apiUrl}/categorias`);
  }

  listarTodasCategorias(): Observable<Categoria[]> {
    return this.http.get<Categoria[]>(`${environment.apiUrl}/categorias/todas`);
  }

  criarCategoria(nome: string): Observable<Categoria> {
    return this.http.post<Categoria>(`${environment.apiUrl}/categorias`, { nome });
  }

  editarCategoria(id: number, nome: string): Observable<Categoria> {
    return this.http.put<Categoria>(`${environment.apiUrl}/categorias/${id}`, { nome });
  }

  removerCategoria(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/categorias/${id}`);
  }

  listarProdutos(): Observable<Produto[]> {
    return this.http.get<Produto[]>(`${environment.apiUrl}/produtos`);
  }

  listarTodosProdutos(): Observable<Produto[]> {
    return this.http.get<Produto[]>(`${environment.apiUrl}/produtos/todos`);
  }

  criarProduto(nome: string): Observable<Produto> {
    return this.http.post<Produto>(`${environment.apiUrl}/produtos`, { nome });
  }

  editarProduto(id: number, nome: string): Observable<Produto> {
    return this.http.put<Produto>(`${environment.apiUrl}/produtos/${id}`, { nome });
  }

  removerProduto(id: number): Observable<void> {
    return this.http.delete<void>(`${environment.apiUrl}/produtos/${id}`);
  }
}
