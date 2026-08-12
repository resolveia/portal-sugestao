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

export interface UsuarioLogado {
  id: number;
  nome: string;
  email: string;
  role: RoleUsuario;
}

export interface LoginTokenResponse {
  erro: boolean;
  mensagem: string | null;
  usuario: UsuarioLogado | null;
}

export interface TokensDemoResponse {
  admin: string;
  cliente: string;
}
