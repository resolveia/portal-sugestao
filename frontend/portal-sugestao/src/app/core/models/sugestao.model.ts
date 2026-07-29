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
  votadoPorMim: boolean;
  dataModeracao?: string | null;
  motivoRejeicao?: string | null;
  moderadorNome?: string | null;
}

export interface CreateSugestaoRequest {
  titulo: string;
  descricao: string;
  categoriaId: number;
}

export interface RejeitarRequest {
  motivo: string;
}

export interface Categoria {
  id: number;
  nome: string;
  ativo: boolean;
}
