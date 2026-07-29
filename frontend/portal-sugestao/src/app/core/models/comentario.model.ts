export interface Comentario {
  id: number;
  sugestaoId: number;
  usuarioId: number;
  autorNome: string;
  texto: string;
  dataCriacao: string;
}

export interface CreateComentarioRequest {
  texto: string;
}
