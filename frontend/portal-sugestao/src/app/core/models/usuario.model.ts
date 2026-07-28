export type RoleUsuario = 'Cliente' | 'AdminInterno';

export interface MockLoginRequest {
  email: string;
  nome: string;
  empresa: string;
  role: RoleUsuario;
}

export interface MockLoginResponse {
  token: string;
  expiresAt: string;
  usuarioId: number;
  nome: string;
  email: string;
  role: RoleUsuario;
}
