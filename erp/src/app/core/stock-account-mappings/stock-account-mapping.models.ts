export interface StockAccountMapping {
  id: number;
  productCategoryId: number | null;
  productCategoryName: string | null;
  inventoryAssetAccountId: number;
  cogsAccountId: number;
  accountsPayableAccountId: number;
  salesRevenueAccountId: number;
  accountsReceivableAccountId: number;
  cashOrBankAccountId: number | null;
  inputTaxAccountId: number | null;
  outputTaxAccountId: number | null;
  stockAdjustmentVarianceAccountId: number;
  openingBalanceEquityAccountId: number;
  isActive: boolean;
}

export interface StockAccountMappingRequest {
  productCategoryId: number | null;
  inventoryAssetAccountId: number;
  cogsAccountId: number;
  accountsPayableAccountId: number;
  salesRevenueAccountId: number;
  accountsReceivableAccountId: number;
  cashOrBankAccountId: number | null;
  inputTaxAccountId: number | null;
  outputTaxAccountId: number | null;
  stockAdjustmentVarianceAccountId: number;
  openingBalanceEquityAccountId: number;
}
