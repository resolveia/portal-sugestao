export type RoleUsuario = 'Cliente' | 'AdminInterno';

export interface UsuarioLogado {
  id: number;
  nome: string;
  email: string;
  role: RoleUsuario;
}
