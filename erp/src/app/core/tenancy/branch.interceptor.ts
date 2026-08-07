import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TenancyService } from './tenancy.service';

export const branchInterceptor: HttpInterceptorFn = (req, next) => {
  const tenancyService = inject(TenancyService);
  const overrideTenantId = tenancyService.overrideTenantId();
  const branchId = overrideTenantId != null ? tenancyService.overrideBranchId() : tenancyService.currentBranchId();

  const headers: Record<string, string> = {};
  if (overrideTenantId != null) {
    headers['X-Tenant-Id'] = String(overrideTenantId);
  }
  if (branchId != null) {
    headers['X-Branch-Id'] = String(branchId);
  }

  const scopedReq = Object.keys(headers).length > 0 ? req.clone({ setHeaders: headers }) : req;

  return next(scopedReq);
};
