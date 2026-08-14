import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Categoria, CreateSugestaoRequest, EstagioRoadmap, Produto, Sugestao, SugestoesPaginadas } from '../models/sugestao.model';

/**
 * Contrato alinhado ao api_portal_sugestoes real (docs/autenticacao-e-api-portal-sugestoes.md):
 * tudo POST, resposta sempre { Erro, Mensagem?, ...dados } em PascalCase. Esta camada traduz
 * pros modelos internos (camelCase) já usados pelo resto do app, que não muda nada.
 */
function mapCategoria(c: any): Categoria {
  return { id: c.Id, nome: c.Nome, ativo: c.Ativo };
}

function mapProduto(p: any): Produto {
  return { id: p.Id, nome: p.Nome, ativo: p.Ativo };
}

function mapSugestao(s: any): Sugestao {
  return {
    id: s.Id,
    titulo: s.Titulo,
    descricao: s.Descricao,
    resultadoEsperado: s.ResultadoEsperado,
    produtoId: s.ProdutoId,
    produtoNome: s.ProdutoNome,
    categoriaId: s.CategoriaId,
    categoriaNome: s.CategoriaNome,
    autorId: s.AutorId,
    autorNome: s.AutorNome,
    status: s.Status,
    estagioRoadmap: s.EstagioRoadmap,
    dataCriacao: s.DataCriacao,
    totalVotos: s.TotalVotos,
    votadoPorMim: s.VotadoPorMim,
    dataModeracao: s.DataModeracao,
    motivoRejeicao: s.MotivoRejeicao,
    moderadorNome: s.ModeradorNome
  };
}

@Injectable({ providedIn: 'root' })
export class SugestoesService {
  constructor(private readonly http: HttpClient) {}

  listarPaginado(skip: number, take: number): Observable<SugestoesPaginadas> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/sugestoes/listar`, { skip, take }), (r) => ({
      items: (r.Sugestoes ?? []).map(mapSugestao),
      total: r.Total,
      votosUsadosPeloUsuarioAtual: r.VotosUsadosPeloUsuarioAtual
    }));
  }

  listarPendentes(): Observable<Sugestao[]> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/sugestoes/pendentes`, {}), (r) => (r.Sugestoes ?? []).map(mapSugestao));
  }

  criar(request: CreateSugestaoRequest): Observable<Sugestao> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/sugestoes/salvar`, request), (r) => mapSugestao(r.Sugestao));
  }

  editar(id: number, request: CreateSugestaoRequest): Observable<Sugestao> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/sugestoes/editar/${id}`, request), (r) => mapSugestao(r.Sugestao));
  }

  aprovar(id: number): Observable<Sugestao> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/sugestoes/aprovar/${id}`, {}), (r) => mapSugestao(r.Sugestao));
  }

  rejeitar(id: number, motivo: string): Observable<Sugestao> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/sugestoes/rejeitar/${id}`, { motivo }), (r) => mapSugestao(r.Sugestao));
  }

  votar(id: number): Observable<Sugestao> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/sugestoes/votar/${id}`, {}), (r) => mapSugestao(r.Sugestao));
  }

  removerVoto(id: number): Observable<Sugestao> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/sugestoes/removervoto/${id}`, {}), (r) => mapSugestao(r.Sugestao));
  }

  atualizarEstagioRoadmap(id: number, estagio: EstagioRoadmap): Observable<Sugestao> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/sugestoes/roadmap/${id}`, { estagio }), (r) => mapSugestao(r.Sugestao));
  }

  listarCategorias(): Observable<Categoria[]> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/categorias/listar`, {}), (r) => (r.Categorias ?? []).map(mapCategoria));
  }

  listarTodasCategorias(): Observable<Categoria[]> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/categorias/listartodas`, {}), (r) => (r.Categorias ?? []).map(mapCategoria));
  }

  criarCategoria(nome: string): Observable<Categoria> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/categorias/salvar`, { nome }), (r) => mapCategoria(r.Categoria));
  }

  editarCategoria(id: number, nome: string): Observable<Categoria> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/categorias/editar/${id}`, { nome }), (r) => mapCategoria(r.Categoria));
  }

  removerCategoria(id: number): Observable<void> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/categorias/remover/${id}`, {}), () => undefined);
  }

  listarProdutos(): Observable<Produto[]> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/produtos/listar`, {}), (r) => (r.Produtos ?? []).map(mapProduto));
  }

  listarTodosProdutos(): Observable<Produto[]> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/produtos/listartodos`, {}), (r) => (r.Produtos ?? []).map(mapProduto));
  }

  criarProduto(nome: string): Observable<Produto> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/produtos/salvar`, { nome }), (r) => mapProduto(r.Produto));
  }

  editarProduto(id: number, nome: string): Observable<Produto> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/produtos/editar/${id}`, { nome }), (r) => mapProduto(r.Produto));
  }

  removerProduto(id: number): Observable<void> {
    return this.handle(this.http.post<any>(`${environment.apiUrl}/produtos/remover/${id}`, {}), () => undefined);
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
