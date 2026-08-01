export type StatusSugestao = 'EmModeracao' | 'Publicada' | 'Rejeitada';

export interface Sugestao {
  id: number;
  titulo: string;
  descricao: string;
  resultadoEsperado: string;
  produtoId: number;
  produtoNome: string;
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

export interface SugestoesPaginadas {
  items: Sugestao[];
  total: number;
  votosUsadosPeloUsuarioAtual: number;
}

export interface CreateSugestaoRequest {
  produtoId: number;
  titulo: string;
  descricao: string;
  resultadoEsperado: string;
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

export interface Produto {
  id: number;
  nome: string;
  ativo: boolean;
}
