# Stock / Inventory & Invoice Management — Implementation Plan

## Context

The ERP currently covers Accounts/Vouchers/Ledgers/Reports (`erp backend/`) and its Angular SPA (`erp/`). The user wants a new **Stock/Inventory** module: multiple products, some with size/other variants and some without, multiple units of measure, multi-warehouse (including several warehouses within one store/branch, with transfer between warehouses and between branches) — **plus full Invoice Management** for Purchase and Sale documents (and their returns): due dates/credit terms, outstanding balances, and recording customer receipts / supplier payments against invoices (a light Accounts Receivable / Accounts Payable subledger).

This domain doesn't exist today (confirmed by full repo/SRS review — no Product/Warehouse/Item concept exists anywhere in the backend, and the SRS's Sales/Purchase are purely financial voucher types with no quantity/item linkage, no AR/AP subledger, no due-date/payment-status tracking). It is fully greenfield, but must slot into the existing conventions: feature-folder repositories, `ITenantScoped`/`IBranchScoped` auto-scoping, the `Voucher` header+detail+approval+attachment+reversal template, `ApiResponse<T>`/`AppException`/`ResponseMessage`/`RightCodes` patterns on the backend, and the `core/<feature>` + `pages/<feature>` + signal-based state + `RightCode`/nav-group patterns on the frontend.

**Binding decisions made with the user up front**:
- Full GL auto-posting: every stock/invoice transaction with financial impact automatically creates and posts a linked accounting voucher, via a configurable account-mapping table pointing at existing Chart-of-Accounts nodes.
- Purchase and Sale documents **are formal invoices** (carrying due date, payment terms, amount paid, outstanding balance, payment status), and their Returns follow the same shape.
- **Suppliers and Customers share one common table** (`BusinessPartner`), distinguished by a `PartnerType` enum, instead of two separate entities.
- **All six order/invoice-like documents (Purchase Order, Purchase Invoice, Purchase Return, Sales Order, Sales Invoice, Sale Return) share one common table** (`InvoiceHeader`/`InvoiceLine`), distinguished by an `InvoiceType` enum, instead of six separate entity pairs. Purchase Orders and Sales Orders were folded in on top of the original four — they turn out to need almost the same shape (partner, warehouse, lines, a Draft→Approval workflow), just without any GL posting and with a "how much of this has been turned into an actual invoice yet" progress concept instead of a payment-progress concept. Customer Receipts and Supplier Payments are likewise unified into one `PartnerPaymentHeader`/`PartnerPaymentAllocation` pair, distinguished by a `Direction` enum — this is the same consolidation and lets a payment allocation carry one clean FK to the single `InvoiceHeader` table instead of two separate nullable FKs.
- **Orders and their downstream invoices/returns are chained through one pair of self-referencing FKs, not type-specific ones**: `InvoiceHeader.ReferenceInvoiceId` and `InvoiceLine.ReferenceInvoiceLineId` now mean "the row this one was created from," with the specific meaning depending on `InvoiceType` — a Purchase Invoice's `ReferenceInvoiceId` points at the Purchase Order it was raised against, a Purchase Return's points at the Purchase Invoice being returned, and so on. This replaces the separate `PurchaseOrderId`/`SalesOrderId` columns from the original 4-type design and — because both ends of the reference are now the same table — lets `ReferenceInvoiceLineId` become a real DB foreign key for the first time (previously the order-line reference had to be an unenforced polymorphic column, since it could point at either of two separate tables).
- Products model variants (e.g. size) as an optional child of a Product master — a product with no sizing still gets one implicit default variant, so every stock transaction always keys off a variant id, never a bare product id. A separate UOM + conversion-factor table supports buying in one unit and selling/issuing in another.

**Note on CLAUDE.md**: exploration found the frontend already has a branch-selector UI, `X-Branch-Id` interceptor, and `TenancyService` — the "frontend gap" noted in CLAUDE.md is stale. This module doesn't need to build that wiring; every new service just calls `HttpClient` normally and the existing `branch.interceptor.ts` attaches the header automatically.

---

## Key design decisions

- **Costing method: moving average**, tracked per `(ProductVariantId, WarehouseId)` pair. Simplest correct method for a first phase — no FIFO lot-queue bookkeeping — and per-warehouse costing allows legitimate value differences by location (e.g. freight-in) without extra tables. COGS is captured at the moment of issue from the then-current average cost of that variant/warehouse pair.
- **Warehouse is `IBranchScoped`** (belongs to exactly one Branch), with potentially several Warehouse rows per Branch (e.g. "Main", "Damaged Goods", "Display Floor"). Reuses the existing Branch scoping/header-enforcement infrastructure rather than inventing a parallel dimension. Cross-branch transfer is just a transfer between two Warehouse rows whose `BranchId` differs.
- **Product + optional Variant + UOM conversion**: a `Product` master optionally has child `ProductVariant` rows (e.g. Size); products with no sizing get a single auto-created default variant so all stock quantities/transactions always key off a variant id. A separate `UnitOfMeasure` + `UOMConversion` table lets a product be purchased in one unit and sold/issued in another via a conversion factor to its base unit.
- **One `BusinessPartner` table, not `Supplier`+`Customer`**: `PartnerType` enum (`Supplier`/`Customer`/`Both`) distinguishes them. A `Both` partner (a company that both buys from and sells to the tenant) is representable without a duplicate record. All FKs that would otherwise point to `Supplier` or `Customer` point to `BusinessPartner`.
- **One `InvoiceHeader`/`InvoiceLine` table, not six separate document types**: `InvoiceType` enum (`PurchaseOrder`/`PurchaseInvoice`/`PurchaseReturn`/`SalesOrder`/`SalesInvoice`/`SaleReturn`) drives which fields are populated and which validation/GL-posting branch the shared repository takes. This is a real trade-off — some columns are only meaningful for a subset of types (e.g. `UnitCostAtSale` only for `SalesInvoice`/`SaleReturn` lines, `PaymentMode`/`AmountPaid`/`OutstandingAmount`/`PaymentStatus` only meaningful for the four invoice/return types and always zero/blank for the two order types, `DueDate` means "payment due date" for invoices but "expected/requested delivery date" for orders) — accepted deliberately for the requested consolidation; see Open Questions.
- **One `PartnerPaymentHeader`/`PartnerPaymentAllocation` table, not separate Customer-Receipt/Supplier-Payment entities**: `Direction` enum (`CustomerReceipt`/`SupplierPayment`) distinguishes them, and — since Invoice is now one table — each allocation carries a single clean FK to `InvoiceHeader.Id` (no more "which of two invoice tables does this point to" ambiguity). Only the four invoice/return types ever have money allocated against them — a payment can never reference an order-type row (there's nothing to pay yet).
- **Invoices carry payment state; Orders carry fulfillment state — same shape, different meaning, same columns where possible**: every `InvoiceHeader` has `PaymentMode` (Cash/Credit), `PaymentTermDays`, `DueDate`, `AmountPaid`, `OutstandingAmount`, and `PaymentStatus` (`Unpaid`/`PartiallyPaid`/`Paid`/`Overdue`) — meaningful for `PurchaseInvoice`/`PurchaseReturn`/`SalesInvoice`/`SaleReturn`. A **Cash** invoice is marked fully `Paid` at posting time (no AR/AP balance is ever created). A **Credit** invoice posts to the Accounts Receivable/Payable control account and stays `Unpaid`/`PartiallyPaid` until a `PartnerPaymentHeader` allocates money against it. Orders (`PurchaseOrder`/`SalesOrder`) instead use a new `FulfillmentStatus` enum (`Open`/`PartiallyFulfilled`/`Fulfilled`/`Cancelled`) at the header, and each line's new `FulfilledBaseQty` tracks how much of that line has been converted into a downstream invoice so far — updated automatically whenever an Invoice-type row is created/approved with `ReferenceInvoiceId` pointing back at this order (the same "referencing row updates the thing it references" mechanism a `PartnerPaymentAllocation` already uses against an invoice's `OutstandingAmount`).
- **GL auto-posting** happens only on **Approve**, and only for the four invoice/return types (never Draft save, never for `PurchaseOrder`/`SalesOrder` — an order is a planning/commitment document, not yet a financial transaction, so `Approve` on an order just flips `Status` to `Posted` with no stock movement and no linked voucher). Every invoice/return repository call resolves accounts via `StockAccountMapping`, performs the stock movement, and builds+posts a linked `VoucherHeader` in the same DB transaction, mirroring the existing `VoucherHeader.ReversalOfVoucherId` self-link pattern via a new nullable `LinkedVoucherId` FK.

---

## Entity Model (new `Models/*.cs`)

All new tenant-wide masters implement `ITenantScoped`; branch-anchored/transactional entities implement `IBranchScoped`, following `Models/Abstractions/ITenantScoped.cs`/`IBranchScoped.cs` exactly as `VoucherHeader`/`VoucherDetail` do. No existing table is modified — only new FKs pointing *into* existing tables (`Account`, `CostCenter`).

### Master data
- **`ProductCategory`** (`ITenantScoped`): hierarchical (`ParentProductCategoryId`), same shape as `CostCenter`.
- **`UnitOfMeasure`** (`ITenantScoped`): `Code`, `Name`, `IsActive`.
- **`Product`** (`ITenantScoped`): `SKU`, `Name`, `ProductCategoryId?`, `BaseUnitOfMeasureId`, `HasVariants` (bool), `IsStockTracked` (bool, default true — false for pure services), `TracksBatches`/`TracksExpiry` (reserved flags, no batch table yet — don't expose in UI until built), `ReorderLevel?`, `IsActive`.
- **`ProductVariant`** (`ITenantScoped`): `ProductId`, `Name` (e.g. "Large / Red"), `VariantCode` (unique per product), `Barcode?`, `IsDefault`, `IsActive`. Invariant enforced in the repository: every Product always has ≥1 Variant; a `HasVariants=false` product auto-gets one `IsDefault=true, Name="Default"` row in the same transaction as product creation.
- **`UOMConversion`** (`ITenantScoped`): `ProductId`, `UnitOfMeasureId` (the alternate unit, e.g. Carton), `ConversionFactor` (base-units per 1 of this unit, e.g. 24). Unique index `(TenantId, ProductId, UnitOfMeasureId)`.
- **`ProductVariantPrice`** (`ITenantScoped`) — one common table for every price a variant can have, distinguished by a `PriceType` enum (`Purchase`/`Retail`/`Sale`) rather than three separate flat columns, following the same "common table + enum" shape as `BusinessPartner`/`InvoiceHeader`: `ProductVariantId` (FK `ProductVariant`), `PriceType` enum, `Amount` (decimal(18,4)). Unique index `(TenantId, ProductVariantId, PriceType)` — exactly one current value per variant per price type (plain update-in-place, no effective-dating/history in phase 1 — see Open Questions if scheduled/promotional pricing is needed later, at which point this is the natural place to add an `EffectiveFromUtc` column). Since every `ProductVariant` already gets its own row (including the auto-created "Default" variant for non-sized products), **this already answers "can different sizes have different prices" — yes, because each size is a distinct `ProductVariantId` with its own set of price rows.**
  - `PriceType.Retail` — the listed/MRP price (what's printed on a tag, shown as a reference "was" price).
  - `PriceType.Sale` — the actual default selling price used to prefill a new Sales Invoice/Sales Order line's `UnitAmount` (still overridable per transaction — e.g. a negotiated discount).
  - `PriceType.Purchase` — a manually-maintained "standard/last cost" reference used only to prefill a new Purchase Invoice/Purchase Order line's `UnitAmount`. **This is deliberately separate from, and never overwrites, `StockBalance.AverageCost`** (the real moving-average figure the costing engine computes from actual transaction history and uses for COGS) — `ProductVariantPrice.Purchase` is pure data-entry convenience, `StockBalance.AverageCost` is the accounting truth.
  - In all three cases, the stored price is only ever a **default that pre-fills a new line** — the actual `InvoiceLine.UnitAmount` on a posted transaction is what was really charged/paid on that specific document and is never retroactively changed by a later price-master edit, exactly like the historical-integrity principle already applied to `UnitCostAtSale`.
- **`BusinessPartner`** (`ITenantScoped`) — replaces separate `Supplier`/`Customer` entities: `PartnerType` enum (`Supplier`/`Customer`/`Both`), `Code` (unique per tenant), `Name`, `ContactPerson`, `Phone`, `Email`, `Address`, `DefaultPaymentTermDays` (int, default 0 = cash, pre-fills new invoices' due-date term), `CreditLimit?` (only meaningful when `PartnerType` includes Customer), `IsActive`.
- **`Warehouse`** (`IBranchScoped`): `Code`, `Name`, `CostCenterId?` (reuses the existing `CostCenter` entity so stock can be tagged with the branch's cost-center dimension), `IsDefault` (one per branch), `IsActive`. Unique index `(TenantId, BranchId, Code)`.

### GL mapping config
- **`StockAccountMapping`** (`ITenantScoped`): keyed by `ProductCategoryId?` (null = tenant-wide fallback row). Holds `InventoryAssetAccountId`, `COGSAccountId`, `AccountsPayableAccountId` (control account, Cr side for credit purchases), `SalesRevenueAccountId`, `AccountsReceivableAccountId` (control account, Dr side for credit sales), `CashOrBankAccountId?` (default cash/bank account — overridable per-transaction), `InputTaxAccountId?`, `OutputTaxAccountId?`, `StockAdjustmentVarianceAccountId`, `OpeningBalanceEquityAccountId` (Cr side specifically for `ReasonCode == OpeningBalance` adjustments — deliberately a **separate** account from `StockAdjustmentVarianceAccountId`, since an opening balance represents pre-existing owned stock being recorded for the first time, an equity/balancing entry, not a gain/loss/variance event the way a physical-count correction is) — all FKs into the existing `Account` table. "At most one global row" (`ProductCategoryId IS NULL`) must be enforced in the **repository**, not a DB unique index, since SQL Server treats NULLs as distinct — flag this explicitly during migration review.

### Inventory ledger / balances
- **`StockLedgerEntry`** (`IBranchScoped`): append-only audit trail — `ProductVariantId`, `WarehouseId`, `TransactionDate`, `MovementType` enum (`PurchaseReceipt`, `PurchaseReturnIssue`, `SaleIssue`, `SaleReturnReceipt`, `TransferOut`, `TransferIn`, `AdjustmentIncrease`, `AdjustmentDecrease`, `OpeningBalance`), `QuantityIn`/`QuantityOut` (base UOM), `UnitCost`, `TotalCostSigned`, `RunningQuantity`/`RunningValue`, a loose `SourceDocumentType` enum (`PurchaseOrder`, `Invoice`, `StockTransfer`, `StockAdjustment`) + `SourceDocumentId` (unenforced polymorphic reference — a deliberate trade-off over sparse nullable FK columns, acceptable since documents are never hard-deleted, only cancelled). Never UPDATE/DELETE — corrections post new offsetting rows, same immutability principle as posted Vouchers.
- **`StockBalance`** (`IBranchScoped`): `ProductVariantId`, `WarehouseId`, `QuantityOnHand`, `AverageCost`, `ReorderLevel?`, `LastMovementAtUtc`. Unique index `(TenantId, BranchId, ProductVariantId, WarehouseId)` — the fast-path "current on-hand" table so reports don't sum the whole ledger; updated transactionally alongside every `StockLedgerEntry` insert.

### The consolidated Invoice/Order (all header+detail, mirroring `VoucherHeader`/`VoucherDetail`: `Status` state machine, `[Timestamp] RowVersion`, `CreatedBy`/`CreatedAtUtc`, `ApprovedBy`/`ApprovedAtUtc`, `LinkedVoucherId`)

This single table now covers the full purchase-side chain (Purchase Order → Purchase Invoice → Purchase Return) and the full sales-side chain (Sales Order → Sales Invoice → Sale Return), six `InvoiceType` values total.

- **`InvoiceHeader`** (`IBranchScoped`): `InvoiceType` enum (`PurchaseOrder`/`PurchaseInvoice`/`PurchaseReturn`/`SalesOrder`/`SalesInvoice`/`SaleReturn`), `InvoiceNo` (unique per `TenantId`+`BranchId`+`InvoiceType` — each of the 6 types numbers independently, e.g. `PO-2026-0001` vs `PINV-2026-0001`), `ExternalReferenceNo?` (the counterparty's own document number — e.g. supplier's invoice #; unused for sales/orders), `PartnerId` (FK `BusinessPartner`; must be `Supplier`/`Both` for the three Purchase-side types, `Customer`/`Both` for the three Sales-side types), `ReferenceInvoiceId?` (self-referencing FK to `InvoiceHeader` — the one column that now threads the whole chain: a `PurchaseInvoice`'s points at the `PurchaseOrder` it was raised against, a `PurchaseReturn`'s points at the `PurchaseInvoice` being returned, and symmetrically for the sales-side chain; null for the two Order types themselves, since nothing precedes them), `WarehouseId`, `Date`, `PaymentMode` enum (`Cash`/`Credit` — meaningful only for the four invoice/return types), `PaymentTermDays`, `DueDate` (= payment due date for invoices/returns; = expected/requested delivery date for orders — same column, deliberately dual-purpose, see Open Questions), `AmountPaid`, `OutstandingAmount`, `PaymentStatus` enum (`Unpaid`/`PartiallyPaid`/`Paid`/`Overdue` — meaningful only for invoices/returns, always `Unpaid`/0 for orders), `FulfillmentStatus?` enum (`Open`/`PartiallyFulfilled`/`Fulfilled`/`Cancelled` — meaningful only for the two Order types, null for invoices/returns), `Status` enum (`Draft/PendingApproval/Posted/Rejected/Cancelled` — the document's own approval workflow, uniform across all 6 types; for an Order, `Posted` means "approved and open for fulfillment," not "posted to GL").
- **`InvoiceLine`** (`IBranchScoped`): `InvoiceHeaderId`, `ProductVariantId`, `UnitOfMeasureId`, `Qty`, `BaseQty` (via UOM conversion), `UnitAmount` (the transaction's per-unit cost for purchase-side types, or per-unit selling price for sale-side types — one generically-named column instead of separate `UnitCost`/`UnitPrice`; serves as both an Order's agreed price and an Invoice's actual price), `UnitCostAtSale?` (populated only for `SalesInvoice`/`SaleReturn` lines — the COGS basis, captured at posting), `TaxRateId?`/`TaxAmount`, `LineTotal`, `FulfilledBaseQty` (default 0 — meaningful only on Order-type lines: how much of this ordered quantity has been converted into a downstream Invoice-type line so far, incremented automatically whenever such a line is created referencing this one), `ReferenceInvoiceLineId?` (self-referencing FK, mirroring the header's `ReferenceInvoiceId` — a Purchase Invoice line points at the Purchase Order line it was raised from, a Return line points at the original Invoice line; now a **real DB foreign key**, since both ends are the same table — this removes the earlier `SourceOrderLineId` column's "polymorphic, no DB FK" trade-off entirely for the order-to-invoice link, leaving only `StockLedgerEntry.SourceDocumentId` still using that pattern).

### Stock movement documents (no invoice/payment tracking)
- **`StockTransferHeader`/`Line`**: `TransferNo`, `SourceWarehouseId`, `DestinationWarehouseId`, `DestinationBranchId` (denormalized for query clarity), `Status` (`Draft/PendingApproval/PendingReceipt/Completed/Rejected/Cancelled`), `ReceivedBy`/`ReceivedAtUtc` (destination-side confirmation); line has `Qty`/`BaseQty`, `UnitCostAtTransfer` (captured from source's `AverageCost` at Approve), `ReceivedBaseQty` (supports partial receipt). `TenantId`/`BranchId` on the header = the **source** branch.
- **`StockAdjustmentHeader`/`Line`**: `AdjustmentNo`, `WarehouseId`, `ReasonCode` enum (`Damage/Expiry/Loss/CountIncrease/CountDecrease/OpeningBalance/Other`), `Status`; line has `Direction` enum (`Increase`/`Decrease`), `BaseQty`, `UnitCost?` (required for Increase, Decrease always values at current `AverageCost`). **This is also how a new item's opening stock balance gets entered** — no separate "Opening Balance" document/table; it's a `StockAdjustment` with `ReasonCode = OpeningBalance` and `Direction = Increase` per line, one line per (ProductVariant, Warehouse) being seeded, `UnitCost` set to the item's known/costed value at go-live. The repository blocks a line with `ReasonCode == OpeningBalance` if `StockLedgerEntry` already has any row for that (ProductVariantId, WarehouseId) — opening balance only makes sense before any other movement exists for that combination — and the resulting `StockLedgerEntry.MovementType` is stamped `OpeningBalance` (already an existing enum value there) rather than `AdjustmentIncrease`, so reports can tell "this is how the item's history started" apart from a later physical-count correction. GL posting also differs from an ordinary count adjustment (see GL posting below).

### The consolidated invoice payment (Accounts Receivable / Accounts Payable)
- **`PartnerPaymentHeader`** (`IBranchScoped`): `Direction` enum (`CustomerReceipt`/`SupplierPayment`), `PaymentNo` (unique per `TenantId`+`BranchId`+`Direction`), `PartnerId` (FK `BusinessPartner`), `Date`, `BankOrCashAccountId` (FK `Account`, chosen per-transaction), `TotalAmount` (sum of its allocations), `Narration`, `Status` (`Draft/PendingApproval/Posted/Rejected`), `LinkedVoucherId?`.
- **`PartnerPaymentAllocation`** (`IBranchScoped`): `PartnerPaymentHeaderId`, `InvoiceHeaderId` (FK `InvoiceHeader` — one clean FK, possible only because Invoice is now a single table), `AllocatedAmount`. One payment can have many allocations across many invoices; validated so Σ`AllocatedAmount` per invoice never exceeds that invoice's `OutstandingAmount` at allocation time.

---

## Backend structure

### Folder layout
Sibling folders (each with `IFooRepository`/`FooRepository`/`Foo/Dtos/*`), grouped by related aggregate the same way `Vouchers/` holds both `VoucherRepository` and `RecurringVoucherTemplateRepository`:

| Folder | Repositories |
|---|---|
| `ProductCategories/` | `IProductCategoryRepository` |
| `Products/` | `IProductRepository` (owns `Product` + child `ProductVariant`) |
| `UnitsOfMeasure/` | `IUnitOfMeasureRepository` (owns `UnitOfMeasure` + `UOMConversion`) |
| `Partners/` | `IBusinessPartnerRepository` (one repository, `PartnerType` filter) |
| `Warehouses/` | `IWarehouseRepository` |
| `Invoices/` | **`IInvoiceRepository`/`InvoiceRepository`** — the single shared engine handling all 6 `InvoiceType`s (`PurchaseOrder`/`PurchaseInvoice`/`PurchaseReturn`/`SalesOrder`/`SalesInvoice`/`SaleReturn`; state machine, costing, GL posting, and order-fulfillment tracking all branch internally on type) |
| `Transfers/` | `IStockTransferRepository` |
| `StockAdjustments/` | `IStockAdjustmentRepository` |
| `PartnerPayments/` | **`IPartnerPaymentRepository`/`PartnerPaymentRepository`** — single shared engine for both `Direction`s |
| `Inventory/` | `IStockAccountMappingRepository`, read-only `IStockLedgerRepository` (ledger/on-hand/low-stock/AR-AP-aging queries), the shared **`IStockMovementService`/`StockMovementService`** posting engine, `StockDocumentNumberGenerator` (static, mirrors `Vouchers/VoucherNumberGenerator.cs`) |

Each folder gets a `Dtos/` subfolder; repositories never return raw entities, exactly like `Vouchers/Dtos/VoucherResponse.cs`.

### `IStockMovementService` — the shared posting engine (unchanged shape)
Every stock-affecting document repository calls into this rather than duplicating moving-average math:
```
Task<StockMovementResult> ReceiveAsync(productVariantId, warehouseId, baseQty, unitCost, date,
    sourceType, sourceDocumentId, sourceDocumentLineId?, narration, username);   // increases qty, re-bases moving average

Task<StockMovementResult> IssueAsync(productVariantId, warehouseId, baseQty, date,
    sourceType, sourceDocumentId, sourceDocumentLineId?, narration, username, allowNegativeStock = false);
    // decreases qty at current average, throws BadRequestException(InsufficientStock) unless allowed
```
`StockMovementResult { StockLedgerEntryId, NewQuantityOnHand, NewAverageCost, MovementValue }`. It does **not** open its own transaction — the calling document repository owns the transaction boundary. `PartnerPaymentRepository` never calls this service — it only moves money/allocations, never stock.

### `IInvoiceRepository` — the consolidated engine
One repository, one controller (`InvoicesController`), branching internally on all 6 `InvoiceType` values:
- `CreateAsync(request)`: request carries `InvoiceType` explicitly. Validates the type-appropriate reference: `ReferenceInvoiceId` is optional for `PurchaseInvoice`/`SalesInvoice` (set only when raised against a prior Order — must point at a `Posted`, not-yet-`Fulfilled` row of the matching Order type) and required for the two Return types (must point at a `Posted` invoice of the corresponding non-return type); always null for the two Order types themselves. Validates `PartnerId`'s `PartnerType` is compatible with the type (`Supplier`/`Both` for the three Purchase-side types, `Customer`/`Both` for the three Sales-side types). For each line referencing a prior order line (`ReferenceInvoiceLineId` set), validates `Qty` doesn't push the order line's `FulfilledBaseQty` past its `BaseQty`.
- `ApproveAsync(id)` (mirrors `VoucherRepository.ApproveAsync`), branching into two very different paths depending on `InvoiceType`:
  - **`PurchaseOrder`/`SalesOrder`**: check `Status == PendingApproval`; `IFiscalPeriodGuard.IsDateInClosedPeriodAsync` against `Date`; set `Status = Posted`, `FulfillmentStatus = Open`. **No stock movement, no linked voucher** — an order is a commitment, not yet a financial or stock transaction.
  - **`PurchaseInvoice`/`PurchaseReturn`/`SalesInvoice`/`SaleReturn`** (the original 4-type flow, unchanged):
    1. Check `Status == PendingApproval`; `IFiscalPeriodGuard.IsDateInClosedPeriodAsync(invoice.Date)`.
    2. Open transaction. Resolve accounts via `StockAccountMappingRepository` (category-specific, falling back to tenant-wide).
    3. Stock movement: `PurchaseInvoice`→`ReceiveAsync` per line; `SalesInvoice`→`IssueAsync` per line (capturing `UnitCostAtSale` from the returned `StockMovementResult`); `PurchaseReturn`→`IssueAsync`; `SaleReturn`→`ReceiveAsync` (at the original invoice's `UnitCostAtSale`).
    4. Set `DueDate = Date.AddDays(PaymentTermDays)`; if `PaymentMode == Cash`, `AmountPaid = Total, OutstandingAmount = 0, PaymentStatus = Paid` immediately; if `Credit`, `AmountPaid = 0, OutstandingAmount = Total, PaymentStatus = Unpaid`.
    5. Build the linked `VoucherHeader`/`VoucherDetail` with the type-specific legs (see GL posting below), number via `VoucherNumberGenerator`, set `Posted` immediately.
    6. **If `ReferenceInvoiceId` is set** (this invoice was raised against an order): in the same transaction, for each line with a `ReferenceInvoiceLineId`, add this line's `BaseQty` to the referenced order line's `FulfilledBaseQty`; then recompute the referenced order header's `FulfillmentStatus` (`Fulfilled` if every line's `FulfilledBaseQty == BaseQty`, `PartiallyFulfilled` if some but not all lines have any progress, else unchanged `Open`) — the exact same "referencing row updates what it references" shape `PartnerPaymentRepository.ApproveAsync` already uses against an invoice's `OutstandingAmount`, just one link further up the chain.
    7. Stamp `LinkedVoucherId`, commit, audit log.
- `CancelAsync(id)`: meaningful mainly for the two Order types (also usable for a Draft/PendingApproval invoice/return as an alternative to Reject before it's ever posted). Requires `Status ∈ {Draft, PendingApproval, Posted}`; for a `Posted` order additionally requires `FulfillmentStatus ∈ {Open, PartiallyFulfilled}` (a fully `Fulfilled` order has nothing left to cancel). Sets `Status = Cancelled`.
- For Return types, also increments/decrements the referenced original invoice's effective outstanding tracking if needed (a Return against an already-fully-paid invoice doesn't reopen it — Returns settle independently via their own `PaymentStatus`/allocations).

### `IPartnerPaymentRepository` — the consolidated engine
One repository, one controller (`PartnerPaymentsController`), branching on `Direction`:
1. `CreateAsync`: validates every allocation's `InvoiceHeader.PartnerId == request.PartnerId` and the invoice's `InvoiceType` is compatible with `Direction` (`CustomerReceipt` → `SalesInvoice`/`SaleReturn`; `SupplierPayment` → `PurchaseInvoice`/`PurchaseReturn`), and `AllocatedAmount <= invoice.OutstandingAmount`.
2. `ApproveAsync`: re-validates allocation amounts against current `OutstandingAmount` (guards a race between two payments against the same invoice, combined with `RowVersion` optimistic concurrency on `InvoiceHeader`). For each allocation: `invoice.AmountPaid += AllocatedAmount`, `invoice.OutstandingAmount -= AllocatedAmount`, recompute `PaymentStatus`. Builds one `VoucherHeader` with a **single aggregate** AR-or-AP line (the subledger detail lives in `PartnerPaymentAllocation`, not in the GL). Stamps `LinkedVoucherId`, commits, audit logs.

### GL posting per transaction (concrete legs)
- **`InvoiceType.PurchaseOrder` / `InvoiceType.SalesOrder`**: **no GL voucher, ever** — `LinkedVoucherId` stays null for the lifetime of an order row; only its downstream Invoice/Return rows post to the GL.
- **`InvoiceType.PurchaseInvoice`**: Dr Inventory Asset (+ Dr Input Tax) / Cr Accounts Payable (Credit) or Cr Cash/Bank (Cash).
- **`InvoiceType.PurchaseReturn`**: mirror image (Dr Accounts Payable-or-Cash/Bank / Cr Inventory, Cr Input Tax).
- **`InvoiceType.SalesInvoice`**: Dr Accounts Receivable (Credit) or Dr Cash/Bank (Cash) (+ Cr Output Tax) / Cr Sales Revenue, **plus** Dr COGS / Cr Inventory Asset at `UnitCostAtSale`.
- **`InvoiceType.SaleReturn`**: mirror image, reversing at the original `UnitCostAtSale`.
- **`Direction.CustomerReceipt`**: Dr Bank/Cash / Cr Accounts Receivable (aggregate).
- **`Direction.SupplierPayment`**: Dr Accounts Payable (aggregate) / Cr Bank/Cash.
- **Stock Adjustment**: Increase = Dr Inventory / Cr Variance; Decrease = Dr Variance / Cr Inventory — **except** `ReasonCode == OpeningBalance` (always `Direction == Increase`), which posts Dr Inventory / **Cr Opening Balance Equity** instead of Cr Variance, since this is seeding pre-existing stock into the books for the first time, not recording a gain.
- **Stock Transfer**: **no GL voucher in phase 1** — value stays on the same tenant-wide Inventory Asset account regardless of warehouse (`Warehouse.CostCenterId` is reserved for a future per-branch visibility retrofit without a schema break).

### Controllers, messages, rights, DI
Controllers flat in `Controllers/*.cs`: `ProductCategoriesController`, `ProductsController` (incl. `/{id}/variants`), `UnitsOfMeasureController` (incl. `/{id}/conversions`), `BusinessPartnersController` (`?partnerType=` filter), `WarehousesController`, `InvoicesController` (`?invoiceType=` filter covering all 6 types — `GET/POST` + `/{id}/submit`, `/approve`, `/reject`, `/cancel`, `GET /{id}/outstanding`), `PartnerPaymentsController` (`?direction=` filter), `StockTransfersController` (+ `POST /{id}/receive`), `StockAdjustmentsController`, `StockAccountMappingsController`, `StockLedgerController` (read-only: ledger inquiry, on-hand, low-stock, AR aging, AP aging). Identical `[ApiController][Route][Authorize]` + try/catch skeleton as `VouchersController.cs` throughout. There is no longer a separate `PurchaseOrdersController`/`SalesOrdersController` — Orders are just two more `InvoiceType` values served by the same `InvoicesController`.

### Pagination — a deliberate new pattern for this module, not a retrofit of the rest of the app
`VouchersController.GetAll` (and every other existing list endpoint in this codebase) returns its full filtered result set as a plain `List<T>`, with no `pageNumber`/`pageSize` concept anywhere in the backend today — PrimeNG's `p-table` just paginates whatever already landed in memory. That's fine for small, bounded master data (Chart of Accounts, Cost Centers), but several of this module's lists have no natural bound — an `Invoice` list spans every purchase/sale/order/return ever made, and `StockLedgerEntry` has one row per unit of stock movement, so both only ever grow. Loading either "everything at once" would get slower every month. So the **transactional, ever-growing list endpoints get real server-side paging**, while small bounded master-data lists (Products, Warehouses, BusinessPartners, ProductCategories, UOM — realistically hundreds of rows, not millions) stay exactly like the rest of the app (full list, client-side paging), consistent with existing convention rather than paginating everything reflexively.

- **Endpoints that paginate**: `GET /api/invoices` (all 6 `InvoiceType`s funnel through here, by far the highest-volume list), `GET /api/partner-payments`, `GET /api/stock-transfers`, `GET /api/stock-adjustments`, and `StockLedgerController`'s ledger-inquiry endpoint (potentially the single largest table in the whole schema).
- **Shape**: each such action adds `[FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 25` alongside its existing filters (`invoiceType`, `partnerId`, `status`, date range, etc.), and returns `ApiResponse<PagedResult<T>>` instead of `ApiResponse<List<T>>`, where `PagedResult<T> { List<T> Items, int TotalCount, int PageNumber, int PageSize }` is a new small generic type (`Models/PagedResult.cs`, sits alongside `ApiResponse<T>`, reusable by any future paginated endpoint).
- **Repository shape**: `GetAllAsync(..., pageNumber, pageSize)` runs the filtered query with `.OrderByDescending(x => x.Date)` (or `.Id` as a tiebreaker), a `.CountAsync()` for `TotalCount`, then `.Skip((pageNumber - 1) * pageSize).Take(pageSize)` for the page itself, with `.AsNoTracking()` since these are always read-only list views.
- **Frontend**: the shared `invoice-list`/`partner-payment-list`/`stock-ledger` components use PrimeNG `p-table`'s `[lazy]="true"` mode with `(onLazyLoad)="onPageChange($event)"` instead of loading everything into a plain signal array up front — each page turn (or filter change, which resets to page 1) issues a fresh `GET` with the current `pageNumber`/`pageSize`/filters, and `[totalRecords]="totalCount()"` drives the paginator UI from the response's `TotalCount`. This is a different loading pattern from `pages/vouchers/voucher-list/voucher-list.ts` (which loads its full filtered list into a signal today) — intentional, scoped only to this module's high-volume lists, not a change to the existing Vouchers page.

**Right codes stay fully granular per business type + action, even though the API is shared.** The backend API/table/controller is consolidated (one `InvoicesController`, one `PartnerPaymentsController`, one `BusinessPartnersController`), but permissions are not — the user wants a distinct, independently assignable right per real business page and action (e.g. a role can approve Sales Invoices without being able to approve Purchase Invoices, or manage Customers without managing Suppliers), matching the frontend's per-page rights list:

```
PurchaseInvoicesCreate / Edit / Submit / Approve / Reject
SalesInvoicesCreate / Edit / Submit / Approve / Reject
PurchaseReturnsCreate / Edit / Submit / Approve / Reject
SaleReturnsCreate / Edit / Submit / Approve / Reject
CustomerReceiptsCreate / Submit / Approve / Reject
SupplierPaymentsCreate / Submit / Approve / Reject
SuppliersCreate / Edit
CustomersCreate / Edit
PurchaseOrdersCreate / Edit / Submit / Approve / Reject / Cancel
SalesOrdersCreate / Edit / Submit / Approve / Reject / Cancel
StockTransfersCreate / Submit / Approve / Reject / Receive
StockAdjustmentsCreate / Submit / Approve / Reject
ProductCategoriesCreate / Edit, ProductsCreate / Edit, UnitsOfMeasureCreate / Edit,
WarehousesCreate / Edit, StockAccountMappingsManage
```

(`ProductsEdit` also covers managing a variant's `ProductVariantPrice` rows — pricing isn't a distinct approval workflow, just another editable attribute of the product, so it doesn't get its own right.)

**Why this needs a different mechanism than the rest of the app**: every other controller in this codebase gates an action with a single, statically-known `[Authorize(Policy = RightPolicies.Prefix + RightCodes.X)]` attribute, because the right never depends on the request's *content* — only on which action method is running. `InvoicesController`/`PartnerPaymentsController`/`BusinessPartnersController` break that assumption: the same `POST /api/invoices` action backs four different business processes, and *which* right applies depends on the `InvoiceType`/`Direction`/`PartnerType` value inside the request (or, for Update/Submit/Approve/Reject, inside the already-persisted row being acted on). A static attribute can't express "check `SalesInvoicesApprove` if this row's `InvoiceType == SalesInvoice`, else check `PurchaseInvoicesApprove`."

So these three controllers add one extra step at the top of every mutating action, resolving the concrete right *in code* and checking it *programmatically* instead of via the attribute:
- A small static resolver per shared resource — `InvoiceRightResolver.Resolve(InvoiceType type, InvoiceAction action) → string rightCode` (`Invoices/InvoiceRightResolver.cs`), now covering all **six** `InvoiceType` values (including `PurchaseOrder`/`SalesOrder`, which additionally support a `Cancel` action the four invoice/return types don't), and equivalents `PartnerPaymentRightResolver.Resolve(Direction, PaymentAction)` and `BusinessPartnerRightResolver.Resolve(PartnerType, PartnerAction)`.
- For **Create**: resolve from the `InvoiceType`/`Direction`/`PartnerType` field already present in the incoming request DTO.
- For **Update/Submit/Approve/Reject/Receive/Cancel**: fetch the entity first, resolve from *its* persisted type field (never trust a client-supplied type for an existing row — `InvoiceType`/`Direction`/`PartnerType` are immutable after creation anyway).
- **`PartnerType.Both`** is the one case needing two rights at once: creating or editing a `Both` partner requires *both* the Supplier-side and Customer-side right (e.g. both `SuppliersCreate` and `CustomersCreate`) — a user must be trusted to manage both sides to create a dual-role record.
- The actual check: inject `IAuthorizationService` (standard ASP.NET Core DI, no new infrastructure) and call `await _authorizationService.AuthorizeAsync(User, RightPolicies.Prefix + resolvedCode)`; on failure, return the same `403`/`ApiResponse<T>.Fail(...)` shape a failed `[Authorize(Policy=...)]` attribute would have produced, so the failure mode is indistinguishable from every other controller's from the client's point of view.
- Class-level `[Authorize]` (plain authentication, no policy) stays on these controllers so unauthenticated requests are still rejected before reaching the resolver step. Read (`GET`) actions are **not** gated by any of this — consistent with every other module, reads stay open beyond basic authentication.

This is strictly more backend code than the earlier "one coarse right per shared resource" idea, but it's what makes the frontend's "different rights per page" requirement (below) actually enforceable server-side rather than purely a UI-hiding convenience.

`Messages/ResponseMessage.cs` gets one `// --- Section ---` banner per sub-domain (Products, Business Partners, Invoices, Partner Payments, Transfers, Adjustments, etc.) following the exact `NotFound`/`Created`/`NotDraftForX`/`Submitted`/`Approved`/`Rejected` shape the `--- Vouchers ---` section already uses, plus invoice-specific ones (`AllocationExceedsOutstandingBalance`, `InvoiceAlreadyFullyPaid`, `AllocationPartnerMismatch`, `InvoiceTypeReferenceMismatch`) — don't hand-enumerate every message here, replicate the existing pattern per domain.

`Program.cs` gets one `AddScoped<IFooRepository, FooRepository>()` line per repository listed above, appended after the existing Vouchers registration block.

`AppDbContext.cs` needs new `DbSet<T>` properties for every entity above plus `OnModelCreating` blocks (decimal column types, `HasMany().WithOne().OnDelete(Cascade)` for header→lines, `OnDelete(Restrict)` for FKs into `Account`/`CostCenter`/`Product`/`ProductVariant`/`Warehouse`/`BusinessPartner`) — no changes needed to the existing reflection-loop query-filter logic, it picks up every new `ITenantScoped`/`IBranchScoped` type automatically.

### Migration considerations
All new tables, so the CLAUDE.md warning about `defaultValue` on non-nullable columns mostly doesn't apply here (nothing is backfilling existing rows). Verify: every FK into an existing table uses `OnDelete(Restrict)`; `StockAccountMapping`'s "one global row" rule is enforced in the repository, not assumed from a DB constraint; `InvoiceHeader.InvoiceNo`'s uniqueness is scoped to `(TenantId, BranchId, InvoiceType)` (each type numbers independently, e.g. `PINV-2026-0001` vs `SINV-2026-0001`), not just `(TenantId, BranchId)`.

---

## Database Schema — Full Table Reference

Conventions applied throughout: every table has an `Id int IDENTITY PK`. Tables marked **(Tenant)** implement `ITenantScoped` (add `TenantId int NOT NULL FK→Tenant`, auto-filtered/stamped). Tables marked **(Branch)** implement `IBranchScoped` (add `TenantId`+`BranchId int NOT NULL FK→Branch`, auto-filtered/stamped). Money columns are `decimal(18,2)`; quantity/factor/cost columns are `decimal(18,4)` (or `decimal(18,6)` for `ConversionFactor`). All header→line relationships are `ON DELETE CASCADE`; all FKs into existing/master tables (`Account`, `CostCenter`, `Product`, `ProductVariant`, `Warehouse`, `BusinessPartner`, `VoucherHeader`) are `ON DELETE RESTRICT`.

### Master data

**`ProductCategory`** (Tenant)
| Column | Type | Constraints |
|---|---|---|
| Name | nvarchar(200) | not null |
| ParentProductCategoryId | int | FK→ProductCategory, nullable (self-referencing) |
| IsActive | bit | default 1 |

**`UnitOfMeasure`** (Tenant)
| Column | Type | Constraints |
|---|---|---|
| Code | nvarchar(20) | not null, unique per TenantId |
| Name | nvarchar(100) | not null |
| IsActive | bit | default 1 |

**`Product`** (Tenant)
| Column | Type | Constraints |
|---|---|---|
| SKU | nvarchar(50) | not null, unique per TenantId |
| Name | nvarchar(200) | not null |
| ProductCategoryId | int | FK→ProductCategory, nullable |
| BaseUnitOfMeasureId | int | FK→UnitOfMeasure, not null |
| HasVariants | bit | default 0 |
| IsStockTracked | bit | default 1 |
| TracksBatches | bit | default 0 (reserved, unimplemented) |
| TracksExpiry | bit | default 0 (reserved, unimplemented) |
| ReorderLevel | decimal(18,4) | nullable |
| IsActive | bit | default 1 |
| CreatedAtUtc | datetime2 | not null |

**`ProductVariant`** (Tenant)
| Column | Type | Constraints |
|---|---|---|
| ProductId | int | FK→Product, not null, cascade |
| Name | nvarchar(200) | not null |
| VariantCode | nvarchar(50) | not null, unique per (TenantId, ProductId) |
| Barcode | nvarchar(50) | nullable |
| IsDefault | bit | default 0 |
| IsActive | bit | default 1 |

**`UOMConversion`** (Tenant)
| Column | Type | Constraints |
|---|---|---|
| ProductId | int | FK→Product, not null |
| UnitOfMeasureId | int | FK→UnitOfMeasure, not null |
| ConversionFactor | decimal(18,6) | not null (base-units per 1 of this unit) |
| — | — | unique index (TenantId, ProductId, UnitOfMeasureId) |

**`BusinessPartner`** (Tenant) — replaces separate Supplier/Customer tables
| Column | Type | Constraints |
|---|---|---|
| PartnerType | enum (int) | Supplier / Customer / Both |
| Code | nvarchar(30) | not null, unique per TenantId |
| Name | nvarchar(200) | not null |
| ContactPerson | nvarchar(150) | nullable |
| Phone | nvarchar(30) | nullable |
| Email | nvarchar(150) | nullable |
| Address | nvarchar(300) | nullable |
| DefaultPaymentTermDays | int | default 0 |
| CreditLimit | decimal(18,2) | nullable (meaningful only when PartnerType includes Customer) |
| IsActive | bit | default 1 |
| CreatedAtUtc | datetime2 | not null |

**`Warehouse`** (Branch)
| Column | Type | Constraints |
|---|---|---|
| Code | nvarchar(30) | not null |
| Name | nvarchar(150) | not null |
| CostCenterId | int | FK→CostCenter, nullable |
| IsDefault | bit | default 0 |
| IsActive | bit | default 1 |
| CreatedAtUtc | datetime2 | not null |
| — | — | unique index (TenantId, BranchId, Code) |

### GL mapping config

**`StockAccountMapping`** (Tenant)
| Column | Type | Constraints |
|---|---|---|
| ProductCategoryId | int | FK→ProductCategory, nullable (null = tenant-wide default row; "at most one null row" enforced in repository, not DB) |
| InventoryAssetAccountId | int | FK→Account, not null |
| COGSAccountId | int | FK→Account, not null |
| AccountsPayableAccountId | int | FK→Account, not null |
| SalesRevenueAccountId | int | FK→Account, not null |
| AccountsReceivableAccountId | int | FK→Account, not null |
| CashOrBankAccountId | int | FK→Account, nullable (default; overridable per-transaction) |
| InputTaxAccountId | int | FK→Account, nullable |
| OutputTaxAccountId | int | FK→Account, nullable |
| StockAdjustmentVarianceAccountId | int | FK→Account, not null |
| OpeningBalanceEquityAccountId | int | FK→Account, not null (Cr side for `ReasonCode == OpeningBalance` adjustments only — kept separate from the variance account, see Entity Model) |
| IsActive | bit | default 1 |

### Inventory ledger / balances

**`StockLedgerEntry`** (Branch) — append-only, never updated/deleted
| Column | Type | Constraints |
|---|---|---|
| ProductVariantId | int | FK→ProductVariant, not null |
| WarehouseId | int | FK→Warehouse, not null |
| TransactionDate | datetime2 | not null |
| MovementType | enum (int) | PurchaseReceipt / PurchaseReturnIssue / SaleIssue / SaleReturnReceipt / TransferOut / TransferIn / AdjustmentIncrease / AdjustmentDecrease / OpeningBalance |
| QuantityIn | decimal(18,4) | default 0 |
| QuantityOut | decimal(18,4) | default 0 |
| UnitCost | decimal(18,4) | not null |
| TotalCostSigned | decimal(18,2) | not null |
| RunningQuantity | decimal(18,4) | not null |
| RunningValue | decimal(18,2) | not null |
| SourceDocumentType | enum (int) | Invoice (covers all 4 stock-moving `InvoiceType`s — the two Order types never appear here, since orders never move stock) / StockTransfer / StockAdjustment |
| SourceDocumentId | int | not null, **no DB FK** (polymorphic, app-enforced) |
| SourceDocumentLineId | int | nullable |
| Narration | nvarchar(500) | nullable |
| CreatedBy | nvarchar(100) | not null |
| CreatedAtUtc | datetime2 | not null |

**`StockBalance`** (Branch) — fast-path current on-hand
| Column | Type | Constraints |
|---|---|---|
| ProductVariantId | int | FK→ProductVariant, not null |
| WarehouseId | int | FK→Warehouse, not null |
| QuantityOnHand | decimal(18,4) | not null, default 0 |
| AverageCost | decimal(18,4) | not null, default 0 |
| ReorderLevel | decimal(18,4) | nullable |
| LastMovementAtUtc | datetime2 | nullable |
| — | — | unique index (TenantId, BranchId, ProductVariantId, WarehouseId) |

### The consolidated invoice/order

All six document types (`PurchaseOrder`, `PurchaseInvoice`, `PurchaseReturn`, `SalesOrder`, `SalesInvoice`, `SaleReturn`) live in these two tables.

**`InvoiceHeader`** (Branch)
| Column | Type | Constraints |
|---|---|---|
| InvoiceType | enum (int) | PurchaseOrder / PurchaseInvoice / PurchaseReturn / SalesOrder / SalesInvoice / SaleReturn |
| InvoiceNo | nvarchar(30) | not null, unique per (TenantId, BranchId, InvoiceType) — each of the 6 types numbers independently |
| ExternalReferenceNo | nvarchar(50) | nullable (counterparty's own document number; unused for orders) |
| PartnerId | int | FK→BusinessPartner, restrict |
| ReferenceInvoiceId | int | FK→InvoiceHeader (self), restrict, nullable — null for the two Order types; for `PurchaseInvoice`/`SalesInvoice` optionally points at the Order it was raised against; for the two Return types points at the Invoice being returned |
| WarehouseId | int | FK→Warehouse, restrict |
| Date | datetime2 | not null |
| PaymentMode | enum (int) | Cash / Credit — meaningful only for the 4 invoice/return types |
| PaymentTermDays | int | default 0 |
| DueDate | datetime2 | not null — payment due date (invoices/returns) or expected/requested delivery date (orders); same column, dual meaning by type |
| AmountPaid | decimal(18,2) | default 0 — always 0 for order types |
| OutstandingAmount | decimal(18,2) | not null — always equal to total (never paid down) for order types |
| PaymentStatus | enum (int) | Unpaid / PartiallyPaid / Paid / Overdue — always Unpaid for order types |
| FulfillmentStatus | enum (int) | nullable; Open / PartiallyFulfilled / Fulfilled / Cancelled — meaningful only for the two Order types, null for invoices/returns |
| Status | enum (int) | Draft / PendingApproval / Posted / Rejected / Cancelled — the document's own workflow, shared across all 6 types |
| Narration | nvarchar(500) | nullable |
| LinkedVoucherId | int | FK→VoucherHeader, restrict, nullable — always null for the two Order types (they never post to GL) |
| CreatedBy / CreatedAtUtc | nvarchar(100) / datetime2 | not null |
| ApprovedBy / ApprovedAtUtc | nvarchar(100) / datetime2 | nullable |
| RowVersion | rowversion | `[Timestamp]` |

**`InvoiceLine`** (Branch)
| Column | Type | Constraints |
|---|---|---|
| InvoiceHeaderId | int | FK→InvoiceHeader, cascade |
| ProductVariantId | int | FK→ProductVariant, restrict |
| UnitOfMeasureId | int | FK→UnitOfMeasure, restrict |
| Qty | decimal(18,4) | not null |
| BaseQty | decimal(18,4) | not null |
| UnitAmount | decimal(18,4) | not null (unit cost for purchase-side types, unit price for sale-side types — also the agreed price on an Order line) |
| UnitCostAtSale | decimal(18,4) | nullable (populated only for SalesInvoice/SaleReturn — COGS basis) |
| TaxRateId | int | FK→TaxRate, restrict, nullable |
| TaxAmount | decimal(18,2) | default 0 |
| LineTotal | decimal(18,2) | not null |
| FulfilledBaseQty | decimal(18,4) | default 0 — meaningful only on Order-type lines: how much of this line has been converted into a downstream invoice line so far |
| ReferenceInvoiceLineId | int | FK→InvoiceLine (self), restrict, nullable — mirrors the header's `ReferenceInvoiceId`; now a real DB FK since both ends are the same table |

### Product pricing

**`ProductVariantPrice`** (Tenant)
| Column | Type | Constraints |
|---|---|---|
| ProductVariantId | int | FK→ProductVariant, not null |
| PriceType | enum (int) | Purchase / Retail / Sale |
| Amount | decimal(18,4) | not null |
| — | — | unique index (TenantId, ProductVariantId, PriceType) |

### Stock movement documents

**`StockTransferHeader`** (Branch — `BranchId` = source branch)
| Column | Type | Constraints |
|---|---|---|
| TransferNo | nvarchar(30) | not null, unique per (TenantId, BranchId) |
| SourceWarehouseId | int | FK→Warehouse, restrict |
| DestinationWarehouseId | int | FK→Warehouse, restrict |
| DestinationBranchId | int | FK→Branch, restrict (denormalized for query clarity) |
| Date | datetime2 | not null |
| Status | enum (int) | Draft / PendingApproval / PendingReceipt / Completed / Rejected / Cancelled |
| Narration | nvarchar(500) | nullable |
| CreatedBy / CreatedAtUtc | nvarchar(100) / datetime2 | not null |
| ApprovedBy / ApprovedAtUtc | nvarchar(100) / datetime2 | nullable |
| ReceivedBy | nvarchar(100) | nullable |
| ReceivedAtUtc | datetime2 | nullable |
| RowVersion | rowversion | `[Timestamp]` |

**`StockTransferLine`** (Branch)
| Column | Type | Constraints |
|---|---|---|
| StockTransferHeaderId | int | FK→StockTransferHeader, cascade |
| ProductVariantId | int | FK→ProductVariant, restrict |
| UnitOfMeasureId | int | FK→UnitOfMeasure, restrict |
| Qty | decimal(18,4) | not null |
| BaseQty | decimal(18,4) | not null |
| UnitCostAtTransfer | decimal(18,4) | not null (captured from source AverageCost at Approve) |
| ReceivedBaseQty | decimal(18,4) | default 0 (supports partial receipt) |

**`StockAdjustmentHeader`** (Branch)
| Column | Type | Constraints |
|---|---|---|
| AdjustmentNo | nvarchar(30) | not null, unique per (TenantId, BranchId) |
| WarehouseId | int | FK→Warehouse, restrict |
| Date | datetime2 | not null |
| ReasonCode | enum (int) | Damage / Expiry / Loss / CountIncrease / CountDecrease / OpeningBalance / Other |
| Status | enum (int) | Draft / PendingApproval / Posted / Rejected |
| Narration | nvarchar(500) | nullable |
| LinkedVoucherId | int | FK→VoucherHeader, restrict, nullable |
| CreatedBy / CreatedAtUtc | nvarchar(100) / datetime2 | not null |
| ApprovedBy / ApprovedAtUtc | nvarchar(100) / datetime2 | nullable |
| RowVersion | rowversion | `[Timestamp]` |

**`StockAdjustmentLine`** (Branch)
| Column | Type | Constraints |
|---|---|---|
| StockAdjustmentHeaderId | int | FK→StockAdjustmentHeader, cascade |
| ProductVariantId | int | FK→ProductVariant, restrict |
| Direction | enum (int) | Increase / Decrease |
| BaseQty | decimal(18,4) | not null |
| UnitCost | decimal(18,4) | nullable (required for Increase; Decrease values at current AverageCost) |
| LineValue | decimal(18,2) | not null |

### The consolidated invoice payment

**`PartnerPaymentHeader`** (Branch)
| Column | Type | Constraints |
|---|---|---|
| Direction | enum (int) | CustomerReceipt / SupplierPayment |
| PaymentNo | nvarchar(30) | not null, unique per (TenantId, BranchId, Direction) |
| PartnerId | int | FK→BusinessPartner, restrict |
| Date | datetime2 | not null |
| BankOrCashAccountId | int | FK→Account, restrict |
| TotalAmount | decimal(18,2) | not null (= Σ allocations) |
| Narration | nvarchar(500) | nullable |
| Status | enum (int) | Draft / PendingApproval / Posted / Rejected |
| LinkedVoucherId | int | FK→VoucherHeader, restrict, nullable |
| CreatedBy / CreatedAtUtc | nvarchar(100) / datetime2 | not null |
| ApprovedBy / ApprovedAtUtc | nvarchar(100) / datetime2 | nullable |
| RowVersion | rowversion | `[Timestamp]` |

**`PartnerPaymentAllocation`** (Branch)
| Column | Type | Constraints |
|---|---|---|
| PartnerPaymentHeaderId | int | FK→PartnerPaymentHeader, cascade |
| InvoiceHeaderId | int | FK→InvoiceHeader, restrict |
| AllocatedAmount | decimal(18,2) | not null (≤ invoice's OutstandingAmount at allocation time) |

### Entity-relationship summary (text form)

```
ProductCategory 1──* Product *──1 UnitOfMeasure (base)
Product 1──* ProductVariant
Product 1──* UOMConversion *──1 UnitOfMeasure (alternate)
ProductCategory 1──* StockAccountMapping (nullable = tenant default)

Warehouse *──1 Branch
Warehouse 1──* StockBalance *──1 ProductVariant
Warehouse 1──* StockLedgerEntry *──1 ProductVariant

ProductVariant 1──* ProductVariantPrice   (PriceType = Purchase | Retail | Sale)

BusinessPartner 1──* InvoiceHeader (InvoiceType = PurchaseOrder | PurchaseInvoice | PurchaseReturn | SalesOrder | SalesInvoice | SaleReturn)
InvoiceHeader *──0..1 InvoiceHeader (self, ReferenceInvoiceId)
   PurchaseOrder  ◄── PurchaseInvoice ◄── PurchaseReturn   (each stage's ReferenceInvoiceId points at the stage before it)
   SalesOrder     ◄── SalesInvoice    ◄── SaleReturn
   (PurchaseOrder/SalesOrder themselves have ReferenceInvoiceId = null — nothing precedes them)
InvoiceHeader 1──* InvoiceLine *──1 ProductVariant
InvoiceLine *──0..1 InvoiceLine (self, ReferenceInvoiceLineId)   (mirrors the header chain above, one line at a time)

InvoiceHeader 1──* PartnerPaymentAllocation *──1 PartnerPaymentHeader *──1 BusinessPartner
   (PartnerPaymentHeader.Direction = CustomerReceipt for Sales-side invoices, SupplierPayment for Purchase-side invoices)

StockTransferHeader *──1 Warehouse (source) , *──1 Warehouse (destination)
StockAdjustmentHeader 1──* StockAdjustmentLine *──1 ProductVariant

InvoiceHeader / StockAdjustmentHeader / PartnerPaymentHeader
    each 0..1──1 VoucherHeader (LinkedVoucherId, existing Accounts module table)
```

---

## Frontend

**Core services stay one-per-shared-resource** (unchanged from before): `core/<feature>/` per sub-domain (`<feature>.models.ts` + `<feature>.service.ts`, `@Injectable({providedIn:'root'})`, `inject(HttpClient)`, `baseUrl = \`${environment.apiUrl}/api/<route>\``, every method returns `Observable<ApiResponse<T>>`, branch header automatic via the existing `branch.interceptor.ts`). `core/invoices/invoice.service.ts` (`getAll({invoiceType, ...filter})`, `create`/`update`/`submit`/`approve`/`reject` all taking `invoiceType` as part of the payload/id lookup), `core/partner-payments/partner-payment.service.ts` (`getAll({direction, ...filter})`), `core/business-partners/business-partner.service.ts` (`getAll({partnerType, ...filter})`) — one service each, matching the one shared backend API each.

**Components are a shared base + thin per-type wrappers**, one level up from the previous "separate pages calling a shared service" idea — this is the piece that changes: instead of independently duplicating the list/form logic four times for Purchase Invoice/Sales Invoice/Purchase Return/Sale Return (and twice for Customer Receipt/Supplier Payment, twice for Supplier/Customer), build **one common component per shared resource**, parameterized by a config object, and let each business page be a thin wrapper that only supplies its own config:

- **`shared/invoices/invoice-list/invoice-list.component.ts`** and **`shared/invoices/invoice-form/invoice-form.component.ts`** (not routed directly) — carry all the actual logic: the `FormArray` line-editor (`pages/vouchers/voucher-form/voucher-form.ts`'s `VoucherLineFormControls`/`VoucherLineGroup` pattern), the `DueDate`/`OutstandingAmount`/`PaymentStatus` columns and `<p-tag>` severity mapping, and the Submit/Approve/Reject workflow buttons. Internally, an `@switch` (or simple `@if` chain) on `config.invoiceType` shows the one reference picker that's actually relevant (`PurchaseOrderId` picker for `PurchaseInvoice`, `SalesOrderId` picker for `SalesInvoice`, a "Return against" invoice picker for the two Return types) — the same modest branching the backend's `IInvoiceRepository` already does, just mirrored in the template.
  - `@Input() config: InvoiceTypeConfig` (`core/invoices/invoice.models.ts`):
    ```ts
    interface InvoiceTypeConfig {
      invoiceType: 'PurchaseOrder' | 'PurchaseInvoice' | 'PurchaseReturn'
                 | 'SalesOrder' | 'SalesInvoice' | 'SaleReturn';
      title: string;                 // "Purchase Orders", "Purchase Invoices", ...
      listRoute: string; formRoute: string;
      createRight: RightCode; editRight: RightCode; submitRight: RightCode;
      approveRight: RightCode; rejectRight: RightCode; cancelRight?: RightCode;
    }
    ```
  - Action buttons use `*appHasRight="config.approveRight"` etc. — the directive doesn't care that the code came from an `@Input` instead of a literal `RightCode.X`, so this is a drop-in use of the existing `has-right.directive.ts`. For the two Order types, the shared component hides the Submit/Approve GL-posting language and instead shows fulfillment progress (a `<p-tag>` for `FulfillmentStatus`, and a per-line progress column comparing `FulfilledBaseQty`/`BaseQty`) plus the Cancel button (`*appHasRight="config.cancelRight"`) instead of Reject.
- **Thin routed wrappers**, one pair per business page, each just building its fixed `config` and rendering the shared component: `pages/purchase-orders/purchase-order-list.component.ts`, `pages/purchase-invoices/purchase-invoice-list.component.ts` (`config = { invoiceType: 'PurchaseInvoice', title: 'Purchase Invoices', approveRight: RightCode.PurchaseInvoicesApprove, ... }`, template is just `<app-invoice-list [config]="config" />`), and matching pairs for `pages/purchase-returns/`, `pages/sales-orders/`, `pages/sales-invoices/`, `pages/sale-returns/` — **six page pairs total, all wrapping the exact same two shared components**. **This is the mechanism that gives each page its own distinct rights** despite sharing 100% of the component logic — `purchase-invoice-list.component.ts` passes `RightCode.PurchaseInvoicesApprove` while `sales-invoice-list.component.ts` passes `RightCode.SalesInvoicesApprove`, so a role holding one but not the other correctly sees the Approve button on only one page, even though both pages render the exact same shared component against the exact same backend endpoint. A Purchase Invoice's form, when created from `pages/purchase-orders/`'s list via a "Raise Invoice" row action, pre-populates `config`'s `invoiceType`/lines from the source order — this is the same mechanism the frontend already needs for "Return against this invoice" from the Purchase/Sales Invoice list.
- Same shared-base + thin-wrapper shape for payments: **`shared/partner-payments/partner-payment-form.component.ts`** (config carries `direction`, `createRight`/`submitRight`/`approveRight`/`rejectRight`, and the outstanding-invoice-allocation grid described below) with `pages/customer-receipts/` and `pages/supplier-payments/` as thin wrappers.
- Same shape for `BusinessPartner`: **`shared/business-partners/business-partner-dialog.component.ts`** (the create/edit dialog + table, config carries `partnerType`, `createRight`, `editRight`) with `pages/suppliers/` and `pages/customers/` as thin single-page wrappers (matching the existing `pages/cost-centers/cost-centers.ts` dialog-CRUD shape internally).
- **`pages/customer-receipts/`**'s wrapper config additionally drives the allocation grid: a table of the selected partner's outstanding Sales Invoices/Sale Returns (`GET /api/invoices?invoiceType=SalesInvoice&partnerId=&status=Unpaid,PartiallyPaid`, plus `SaleReturn` for a full picture) with an editable "Amount to Allocate" column (capped client-side at each row's `OutstandingAmount`), a running total against the entered `TotalAmount`, and a Bank/Cash account select — the shared `partner-payment-form` component renders this the same way regardless of `direction`, just querying `PurchaseInvoice`/`PurchaseReturn` instead when `config.direction === 'SupplierPayment'`.
- **Product pricing**: `pages/products/product-form/` gains a third repeatable section alongside the existing Variants and UOM-Conversions `FormArray`s — a small pricing grid per variant (Purchase / Retail / Sale amount fields, calling `core/products/product-variant-price.service.ts`). The Purchase/Sales Invoice and Order line editors (in the shared `invoice-form` component) look up `ProductVariantPrice` when a variant is selected on a new line, to prefill `UnitAmount` (`Sale` price for sales-side types, `Purchase` price for purchase-side types) — purely a convenience default, never overwriting what the user actually types in, and the `Retail` price (when set) is shown read-only alongside the field as a reference "list price."
- **Reports** (unchanged): `pages/stock-ledger/`, `pages/stock-on-hand/`, `pages/accounts-receivable-aging/`, `pages/accounts-payable-aging/` — PrimeNG tables/stat cards, no charting library installed.
- Routes added flat in `src/app/app.routes.ts` under the guarded `Shell` parent, following the existing `/x`, `/x/new`, `/x/:id/edit`, `/x/:id` (view) convention — one set of routes per thin wrapper, same as if the pages were fully independent; the sharing is invisible at the routing layer.
- New nav groups in `src/app/layout/sidebar/sidebar.ts`: **Inventory Setup** (Products, Product Categories, Units of Measure, Warehouses, Suppliers, Customers, Stock Account Mappings), **Purchasing** (Purchase Orders, Purchase Invoices, Purchase Returns, Supplier Payments, AP Aging), **Sales** (Sales Orders, Sales Invoices, Sale Returns, Customer Receipts, AR Aging), **Stock** (Stock Transfers, Stock Adjustments, Stock Ledger, Stock On-Hand).
- **`core/auth/right-code.ts` gets one member per backend `RightCodes` constant listed in Backend Structure** — fully granular, one-to-one with the backend, exactly the existing convention (no consolidation on the frontend either). This granular enum is precisely what lets each thin wrapper component pass a different `RightCode` value into the same shared component's config.

---

## Build order / phasing

1. **Foundations**: `ProductCategory`, `UnitOfMeasure`+conversions, `Product`+`ProductVariant`+`ProductVariantPrice`, `Warehouse`, `BusinessPartner`, `StockAccountMapping`, empty `StockLedgerEntry`/`StockBalance` schema, skeleton `IStockMovementService`.
2. **`InvoiceHeader`/`Line` table + the `InvoiceType.PurchaseOrder`/`SalesOrder` branches first**: no GL posting and no stock movement for these two types, so this is the cheapest place to prove out the shared table, the `InvoiceRightResolver` dynamic-rights mechanism, and the `ReferenceInvoiceId`/`ReferenceInvoiceLineId` self-referencing chain, before layering GL/costing complexity on top in step 3.
3. **`InvoiceType.PurchaseInvoice`, proven with and without a `ReferenceInvoiceId`**: wire `IStockMovementService.ReceiveAsync`, the full GL auto-posting pipeline, the Cash-vs-Credit/`DueDate`/`PaymentStatus` logic, and — when created against a `PurchaseOrder` — the fulfillment-progress update on the referenced order's line/header.
4. **`PartnerPaymentHeader`/`Allocation`, proven on `SupplierPayment` first**: build the allocation mechanism against Purchase Invoices — proves out the AR/AP subledger pattern on the simpler (payable) side.
5. **`InvoiceType.SalesInvoice`**: reuses the proven posting engine (`IssueAsync`), adds COGS-capture-at-issue and the combined revenue+COGS voucher, plus the same Cash-vs-Credit/`DueDate`/`PaymentStatus`/order-fulfillment logic against `SalesOrder`.
6. **`Direction.CustomerReceipt`**: same allocation mechanism as step 4, against Sales Invoices.
7. **`InvoiceType.PurchaseReturn`, `InvoiceType.SaleReturn`**: reverse flows on the same `InvoiceHeader`/`Line` table, reusing existing costing/posting code paths.
8. **`StockTransferHeader`/`Line`**: same-branch (single-approval) case first, then cross-branch (`Approve`→`PendingReceipt`→`Receive`) dual-step case.
9. **`StockAdjustmentHeader`/`Line`**.
10. **Reporting**: Stock Ledger, Stock On-Hand, AR Aging, AP Aging.

---

## Open questions / assumptions (confirm before or while building)

1. **`InvoiceHeader`/`Line` consolidation trades schema purity for fewer tables**: several columns are only meaningful for a subset of `InvoiceType`s (`PaymentStatus`/`AmountPaid` for invoices/returns only, `FulfillmentStatus`/`FulfilledBaseQty` for orders only, `UnitCostAtSale` only for sale-side lines). This is the explicitly requested trade-off; if it later proves confusing in practice (e.g. EF model validation warnings, or awkward nullable-heavy queries), the fallback is splitting back into per-type tables that all still share the `IInvoiceRepository` internal logic.
2. **Resolved**: right codes are fully granular per document type + action (`PurchaseInvoicesApprove`, `SalesInvoicesApprove`, etc. are distinct, independently assignable rights), not consolidated — matching the frontend's need for a different rights list per page. Because the backend API/table stays shared per resource (`InvoicesController`, `PartnerPaymentsController`, `BusinessPartnersController`), the required right must be resolved *dynamically* from the row's `InvoiceType`/`Direction`/`PartnerType` and checked programmatically via `IAuthorizationService`, rather than via a static `[Authorize(Policy=...)]` attribute (see Backend Structure's "Controllers, messages, rights, DI" section for the resolver mechanism). This is more backend code than a coarse single right per resource, but was an explicit requirement, not a default.
3. **Resolved**: `PurchaseOrder`/`SalesOrder` are now merged into the same `InvoiceHeader`/`InvoiceLine` table as the four invoice/return types, as two more `InvoiceType` values. This pushed the trade-off in point 1 further — `PaymentMode`/`AmountPaid`/`OutstandingAmount`/`PaymentStatus`/`LinkedVoucherId` are now always blank/zero/null for order rows, `FulfillmentStatus`/`FulfilledBaseQty` are conversely always null/0 for invoice/return rows, and `DueDate` means two different things depending on type (payment due date vs. expected delivery date). It also had one genuine benefit: the order↔invoice reference (previously `PurchaseOrderId`/`SalesOrderId`, an unenforced polymorphic column for the line-level version) now reuses the same self-referencing `ReferenceInvoiceId`/`ReferenceInvoiceLineId` columns already used for Returns, and — because both ends are now the same table — the line-level reference became a real DB foreign key instead of an app-only convention.
4. **`ProductVariantPrice` has no effective-dating/history** — updating a price overwrites it in place, with no record of what it used to be or a way to schedule a future price change. If promotional pricing, scheduled price increases, or a price-change audit trail turn out to be needed, add an `EffectiveFromUtc` column and stop enforcing the unique index (resolve "current price" as the latest dated row instead) — the same pattern already used for `EmployeeSalaryHistory` in a sibling HR/Payroll plan, not built here since it wasn't asked for.
5. **`PurchaseInvoice` combines goods-receipt + supplier invoice** in phase 1 — no separate "goods received, not yet invoiced" step.
6. **Negative stock is blocked by default** (`IssueAsync` throws unless `allowNegativeStock` is explicitly passed) — no per-warehouse configurable toggle yet.
7. **Tax posts to one control account per type**, not per-`TaxRate`, consistent with how Vouchers already treat tax as amount-only.
8. **`StockAccountMapping`'s "one global row" invariant is app-level, not DB-level.**
9. **Warehouse is branch-scoped** (one Branch → many Warehouses) — does not support one warehouse serving multiple branches.
10. **No early-payment discounts, late-payment penalty interest, or invoice PDF/print template** are scoped for phase 1.
11. **Batch/lot and expiry-date tracking are out of scope** for phase 1 — `Product.TracksBatches`/`TracksExpiry` are reserved columns only.
12. **`StockLedgerEntry.SourceDocumentId`'s polymorphic reference has no DB-level FK** — the equivalent order/invoice-chain reference (`InvoiceLine.ReferenceInvoiceLineId`) no longer has this problem now that Orders share the Invoice table (see point 3) — this is the one remaining unenforced reference, for stock ledger entries pointing at whichever document type triggered them. Referential integrity there is an application-level guarantee only, mitigated by documents never being hard-deleted (only cancelled/rejected).
13. **Multi-currency is out of scope.**
14. **Opening balance entries and `IFiscalPeriodGuard`**: an opening balance is often entered "as of" a go-live/cutover date that, by the time someone gets around to data entry, may already fall in a period the fiscal calendar considers closed for ordinary transactions. As designed, `StockAdjustment.ApproveAsync` runs the same `IsDateInClosedPeriodAsync` check for every `ReasonCode` including `OpeningBalance` — meaning a late opening-balance entry could get blocked by the same rule meant to stop someone editing last quarter's real sales. If that turns out to be a problem in practice, the fix is a small carve-out in the guard check (skip it specifically when `ReasonCode == OpeningBalance`), not a bigger change — flagging now rather than building it speculatively.

---

## Verification

1. `dotnet build` after each entity/migration batch; `dotnet ef migrations add <Name>` then hand-verify the generated migration (per CLAUDE.md's existing warning) — check every FK into an existing table uses `OnDelete(Restrict)`, and that `InvoiceHeader.InvoiceNo`'s unique index is scoped by `InvoiceType` correctly.
2. Exercise each `InvoiceType` through Swagger (`dotnet run` in Development launches it): Create → Submit → Approve, and confirm a `VoucherHeader` appears linked and balanced (Dr = Cr). Confirm `StockBalance`/`StockLedgerEntry` update correctly.
3. **Invoice-lifecycle-specific test**: create a Credit `SalesInvoice`, confirm it posts Dr AR/Cr Revenue+COGS and shows `PaymentStatus = Unpaid`; create a `PartnerPaymentHeader` (`Direction = CustomerReceipt`) allocating a partial amount against it, confirm `PaymentStatus` flips to `PartiallyPaid` and the GL posts Dr Bank/Cr AR; allocate the remainder via a second receipt and confirm `Paid`/`OutstandingAmount = 0`. Repeat for a Cash `SalesInvoice` (immediately `Paid`, no AR touched) and mirror both cases for `PurchaseInvoice`/`SupplierPayment`.
4. Confirm `BusinessPartner` type-compatibility validation: attempt to create a `SalesInvoice` against a `Supplier`-only partner and confirm it's rejected; same for a `PurchaseInvoice` against a `Customer`-only partner.
5. **Order-to-invoice fulfillment chain**: create a `PurchaseOrder` with one line for 100 units, Approve it (confirm `FulfillmentStatus = Open`, no `LinkedVoucherId`, no stock movement). Create a `PurchaseInvoice` referencing that order for 40 units, Approve it (confirm normal GL posting + stock receipt happens, **and** the order line's `FulfilledBaseQty` becomes 40 and the order header's `FulfillmentStatus` becomes `PartiallyFulfilled`). Create a second Purchase Invoice against the same order for the remaining 60 units and confirm the order flips to `Fulfilled`. Mirror on the Sales Order → Sales Invoice side.
6. **Product pricing**: set `Purchase`/`Retail`/`Sale` prices on two different variants (sizes) of the same product, confirm they're independent; start a new Sales Invoice line for each variant and confirm `UnitAmount` prefills from that variant's `Sale` price (not the other variant's), and confirm editing a variant's price afterward does not change `UnitAmount` on an already-posted invoice line.
7. `npm test` (Vitest) for new component specs; `npm start` and manually walk each new page's CRUD + workflow buttons in the browser, confirming `*appHasRight` gating and branch-switching (top bar) correctly scope Warehouse/Invoice/Transfer lists.
8. Test the cross-branch Stock Transfer flow end-to-end: create from Branch A, Approve, switch to Branch B, confirm pending receipt, Receive it, confirm stock moved correctly.
