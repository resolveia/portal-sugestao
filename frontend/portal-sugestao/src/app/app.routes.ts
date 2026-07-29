import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { adminGuard } from './core/auth/admin.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'sugestoes' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login').then((m) => m.Login)
  },
  {
    path: 'sugestoes',
    canActivate: [authGuard],
    loadComponent: () => import('./features/sugestoes/sugestoes-list').then((m) => m.SugestoesList)
  },
  {
    path: 'moderacao',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/moderacao/moderacao-list').then((m) => m.ModeracaoList)
  }
];
