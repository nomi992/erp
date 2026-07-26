import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TenancyService } from './tenancy.service';

export const branchInterceptor: HttpInterceptorFn = (req, next) => {
  const tenancyService = inject(TenancyService);
  const branchId = tenancyService.currentBranchId();

  const scopedReq = branchId != null
    ? req.clone({ setHeaders: { 'X-Branch-Id': String(branchId) } })
    : req;

  return next(scopedReq);
};
