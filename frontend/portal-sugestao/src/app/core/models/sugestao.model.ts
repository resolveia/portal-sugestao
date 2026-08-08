export type StatusSugestao = 'EmModeracao' | 'Publicada' | 'Rejeitada';

export type EstagioRoadmap = 'EmAnalise' | 'Planejado' | 'EmDesenvolvimento' | 'Lancado';

export const ESTAGIO_ROADMAP_OPCOES: { value: EstagioRoadmap; label: string; classe: string }[] = [
  { value: 'EmAnalise', label: 'Em análise', classe: 'em-analise' },
  { value: 'Planejado', label: 'Planejado', classe: 'planejado' },
  { value: 'EmDesenvolvimento', label: 'Em desenvolvimento', classe: 'em-desenvolvimento' },
  { value: 'Lancado', label: 'Lançado', classe: 'lancado' }
];

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
  estagioRoadmap?: EstagioRoadmap | null;
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
