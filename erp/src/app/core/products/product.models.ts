export type PriceType = 'Purchase' | 'Retail' | 'Sale';

export interface ProductVariantPrice {
  id: number;
  priceType: PriceType;
  amount: number;
}

export interface ProductVariantPriceRequest {
  priceType: PriceType;
  amount: number;
}

export interface ProductVariant {
  id: number;
  productId: number;
  name: string;
  variantCode: string;
  barcode: string | null;
  isDefault: boolean;
  isActive: boolean;
  prices: ProductVariantPrice[];
}

export interface ProductVariantRequest {
  name: string;
  variantCode: string;
  barcode: string | null;
  isDefault: boolean;
}

export interface UOMConversion {
  id: number;
  unitOfMeasureId: number;
  unitOfMeasureCode: string;
  conversionFactor: number;
}

export interface UOMConversionRequest {
  unitOfMeasureId: number;
  conversionFactor: number;
}

export interface Product {
  id: number;
  sku: string;
  name: string;
  productCategoryId: number | null;
  productCategoryName: string | null;
  baseUnitOfMeasureId: number;
  baseUnitOfMeasureCode: string;
  hasVariants: boolean;
  isStockTracked: boolean;
  reorderLevel: number | null;
  isActive: boolean;
  createdAtUtc: string;
  variants: ProductVariant[];
  uomConversions: UOMConversion[];
}

export interface ProductRequest {
  sku: string;
  name: string;
  productCategoryId: number | null;
  baseUnitOfMeasureId: number;
  hasVariants: boolean;
  isStockTracked: boolean;
  reorderLevel: number | null;
  variants: ProductVariantRequest[];
  uomConversions: UOMConversionRequest[];
}
