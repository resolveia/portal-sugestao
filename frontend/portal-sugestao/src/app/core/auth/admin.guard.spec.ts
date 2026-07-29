import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { adminGuard } from './admin.guard';
import { AuthService } from './auth.service';
import { MockLoginResponse } from '../models/usuario.model';

describe('adminGuard', () => {
  let usuario: ReturnType<typeof vi.fn>;
  let navigate: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    usuario = vi.fn();
    navigate = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { usuario } },
        { provide: Router, useValue: { navigate } }
      ]
    });
  });

  function runGuard(): boolean {
    return TestBed.runInInjectionContext(() => adminGuard({} as any, {} as any)) as boolean;
  }

  it('libera quando o usuário é AdminInterno', () => {
    usuario.mockReturnValue({ role: 'AdminInterno' } as MockLoginResponse);

    expect(runGuard()).toBe(true);
    expect(navigate).not.toHaveBeenCalled();
  });

  it('bloqueia e redireciona para /sugestoes quando o usuário é Cliente', () => {
    usuario.mockReturnValue({ role: 'Cliente' } as MockLoginResponse);

    expect(runGuard()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/sugestoes']);
  });

  it('bloqueia quando não há usuário logado', () => {
    usuario.mockReturnValue(null);

    expect(runGuard()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/sugestoes']);
  });
});
