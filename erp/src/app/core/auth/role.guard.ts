import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const roleGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const allowedRoles = route.data['roles'] as string[] | undefined;
  const requiredRights = route.data['rights'] as string[] | undefined;

  const roleAllowed = !allowedRoles || allowedRoles.includes(authService.role() ?? '');
  const rightsAllowed = !requiredRights || requiredRights.every((right) => authService.hasRight(right));

  return roleAllowed && rightsAllowed ? true : router.createUrlTree(['/dashboard']);
};
