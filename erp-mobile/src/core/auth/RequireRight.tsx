import React from 'react';
import { useAuth } from './auth.context';
import type { RightCode } from './right-code';

interface RequireRightProps {
  right: RightCode;
  children: React.ReactNode;
}

/**
 * Component equivalent of erp/src/app/core/auth/has-right.directive.ts (*appHasRight) — wrap
 * any action/button/screen that should only render when the signed-in user's role currently
 * grants that right. Renders nothing when the right is missing.
 *
 * Reminder: rights are baked into the JWT at login (see JwtTokenService.cs) and are NOT
 * re-checked live, so a right revoked by an admin on the web Roles page won't hide anything
 * here until this user logs out and back in.
 */
export function RequireRight({ right, children }: RequireRightProps) {
  const { hasRight } = useAuth();
  if (!hasRight(right)) {
    return null;
  }
  return <>{children}</>;
}
