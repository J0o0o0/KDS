import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Role } from '../models';

/**
 * Usage in routes: { path: 'admin', canActivate: [roleGuard(['Admin'])], ... }
 */
export function roleGuard(allowed: Role[]): CanActivateFn {
  return () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isLoggedIn()) {
      router.navigate(['/login']);
      return false;
    }

    if (auth.hasRole(...allowed)) return true;

    // Logged in but wrong role — send them to their own home instead of login.
    router.navigate([auth.homeRouteForCurrentUser()]);
    return false;
  };
}
