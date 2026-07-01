import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'admin',
    canActivate: [roleGuard(['Admin'])],
    loadComponent: () =>
      import('./features/admin/admin.component').then((m) => m.AdminComponent),
  },
  {
    path: 'cashier',
    canActivate: [roleGuard(['Admin', 'Cashier'])],
    loadComponent: () =>
      import('./features/cashier/cashier.component').then((m) => m.CashierComponent),
  },
  {
    path: 'kitchen',
    canActivate: [roleGuard(['Admin', 'Cook', 'Expediter'])],
    loadComponent: () =>
      import('./features/kitchen/kitchen-display.component').then(
        (m) => m.KitchenDisplayComponent
      ),
  },
  {
    path: 'order-history',
    canActivate: [roleGuard(['Admin'])],
    loadComponent: () =>
      import('./features/order-history/order-history.component').then(
        (m) => m.OrderHistoryComponent
      ),
  },
  { path: '', pathMatch: 'full', redirectTo: 'login' },
  { path: '**', redirectTo: 'login' },
];
