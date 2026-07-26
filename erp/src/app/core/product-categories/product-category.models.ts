export interface ProductCategory {
  id: number;
  name: string;
  parentProductCategoryId: number | null;
  parentProductCategoryName: string | null;
  isActive: boolean;
}

export interface ProductCategoryRequest {
  name: string;
  parentProductCategoryId: number | null;
}
