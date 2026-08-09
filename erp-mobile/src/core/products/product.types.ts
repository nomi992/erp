// Subset of erp/src/app/core/products/product.models.ts needed for the Sales Invoice line
// item picker. Unit-of-measure conversions aren't surfaced here — the mobile app invoices
// against each product's base UOM only (see README.md scoping).
export type PriceType = 'Purchase' | 'Retail' | 'Sale';

export interface ProductVariantPrice {
  priceType: PriceType;
  amount: number;
}

export interface ProductVariant {
  id: number;
  productId: number;
  name: string;
  variantCode: string;
  isActive: boolean;
  prices: ProductVariantPrice[];
}

export interface Product {
  id: number;
  sku: string;
  name: string;
  baseUnitOfMeasureId: number;
  baseUnitOfMeasureCode: string;
  isActive: boolean;
  variants: ProductVariant[];
}

/** Flattened product+variant, one row per sellable variant — built client-side for the picker. */
export interface ProductVariantOption {
  variantId: number;
  productId: number;
  label: string;
  variantCode: string;
  unitOfMeasureId: number;
  unitOfMeasureCode: string;
  salePrice: number | null;
}

export function flattenProductVariants(products: Product[]): ProductVariantOption[] {
  return products
    .filter((p) => p.isActive)
    .flatMap((product) =>
      product.variants
        .filter((v) => v.isActive)
        .map((variant) => ({
          variantId: variant.id,
          productId: product.id,
          label: variant.name === product.name ? product.name : `${product.name} — ${variant.name}`,
          variantCode: variant.variantCode,
          unitOfMeasureId: product.baseUnitOfMeasureId,
          unitOfMeasureCode: product.baseUnitOfMeasureCode,
          salePrice: variant.prices.find((p) => p.priceType === 'Sale')?.amount ?? null,
        })),
    );
}
