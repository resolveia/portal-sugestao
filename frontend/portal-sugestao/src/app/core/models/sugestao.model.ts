export type StatusSugestao = 'EmModeracao' | 'Publicada' | 'Rejeitada';

export interface Sugestao {
  id: number;
  titulo: string;
  descricao: string;
  categoriaId: number;
  categoriaNome: string;
  autorId: number;
  autorNome: string;
  status: StatusSugestao;
  dataCriacao: string;
  totalVotos: number;
}

export interface CreateSugestaoRequest {
  titulo: string;
  descricao: string;
  categoriaId: number;
}

export interface Categoria {
  id: number;
  nome: string;
  ativo: boolean;
}
