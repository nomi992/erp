// 1:1 port of erp/src/app/core/models/paged-result.model.ts.
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
