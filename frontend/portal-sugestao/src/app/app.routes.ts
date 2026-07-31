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
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./core/layout/app-shell').then((m) => m.AppShell),
    children: [
      {
        path: 'sugestoes',
        loadComponent: () => import('./features/sugestoes/sugestoes-list').then((m) => m.SugestoesList)
      },
      {
        path: 'moderacao',
        canActivate: [adminGuard],
        loadComponent: () => import('./features/moderacao/moderacao-list').then((m) => m.ModeracaoList)
      },
      {
        path: 'categorias',
        canActivate: [adminGuard],
        loadComponent: () => import('./features/categorias/categorias-list').then((m) => m.CategoriasList)
      },
      {
        path: 'produtos',
        canActivate: [adminGuard],
        loadComponent: () => import('./features/produtos/produtos-list').then((m) => m.ProdutosList)
      }
    ]
  }
];
