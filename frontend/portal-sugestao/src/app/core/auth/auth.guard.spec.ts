import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';

describe('authGuard', () => {
  let isAuthenticated: ReturnType<typeof vi.fn>;
  let navigate: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    isAuthenticated = vi.fn();
    navigate = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { isAuthenticated } },
        { provide: Router, useValue: { navigate } }
      ]
    });
  });

  function runGuard(): boolean {
    return TestBed.runInInjectionContext(() => authGuard({} as any, {} as any)) as boolean;
  }

  it('libera quando o usuário está autenticado', () => {
    isAuthenticated.mockReturnValue(true);

    expect(runGuard()).toBe(true);
    expect(navigate).not.toHaveBeenCalled();
  });

  it('bloqueia e redireciona para /login quando não está autenticado', () => {
    isAuthenticated.mockReturnValue(false);

    expect(runGuard()).toBe(false);
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });
});
