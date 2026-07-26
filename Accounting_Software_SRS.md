# Software Requirements Specification (SRS)
## Accounting Management System

| | |
|---|---|
| **Document Version** | 1.0 |
| **Date** | July 15, 2026 |
| **Prepared For** | Accounting Software Project |
| **Technology Stack** | .NET Core (Web API) / Angular (SPA Frontend) |
| **Status** | Draft |

---

## 1. Introduction

### 1.1 Purpose
This document specifies the functional and non-functional requirements for an Accounting Management System comprising four core modules: **Chart of Accounts**, **Vouchers (Transaction Entry)**, **Ledgers**, and **Reports**. It is intended for use by developers, QA engineers, and project stakeholders as the authoritative reference for scope and behavior.

### 1.2 Scope
The system will allow businesses to:
- Define and maintain a hierarchical chart of accounts
- Record financial transactions through standardized vouchers with double-entry validation
- View and reconcile account-wise and general ledgers
- Generate standard financial statements and operational reports

The system will be delivered as a web application with a .NET Core Web API backend and an Angular single-page application frontend, with architecture that allows a future Flutter mobile client to consume the same API.

### 1.3 Intended Audience
- Backend developers (.NET Core / EF Core)
- Frontend developers (Angular)
- QA / Test engineers
- Project managers / Business stakeholders

### 1.4 Definitions & Acronyms

| Term | Definition |
|---|---|
| COA | Chart of Accounts |
| Voucher | A transaction entry document (payment, receipt, journal, etc.) |
| Ledger | Chronological record of transactions per account |
| Dr / Cr | Debit / Credit |
| Fiscal Period | An accounting period (month/quarter/year) that can be open or closed |
| Control Account | A summary account representing a group of sub-ledger accounts |
| SPA | Single Page Application |

---

## 2. Overall Description

### 2.1 System Architecture
```
Angular SPA  <--HTTPS/REST-->  .NET Core Web API  <--EF Core-->  Relational Database
                                      |
                                SignalR (real-time updates)
                                      |
                            Hangfire/Quartz (background jobs)
```

### 2.2 User Classes and Privileges

| Role | Permissions |
|---|---|
| Administrator | Full access: COA setup, user management, period closing |
| Accountant | Create/edit vouchers, view ledgers/reports |
| Approver | Approve/reject pending vouchers |
| Auditor / Viewer | Read-only access to ledgers and reports |

### 2.3 Assumptions and Dependencies
- Single default currency with optional multi-currency support (configurable)
- Fiscal year and period structure configured at system setup
- Backend and frontend deployed as separate, independently versioned applications
- Authentication via JWT issued by the .NET Core API

---

## 3. Functional Requirements

Each requirement is identified as **FR-[Module]-[Number]** for traceability.

### 3.1 Chart of Accounts (COA)

| ID | Requirement |
|---|---|
| FR-COA-01 | The system shall allow creation of accounts with code, name, type (Asset/Liability/Equity/Income/Expense), and nature (Debit/Credit). |
| FR-COA-02 | The system shall support unlimited hierarchical nesting of accounts under parent groups. |
| FR-COA-03 | The system shall prevent circular parent-child references when assigning a parent account. |
| FR-COA-04 | The system shall block permanent deletion of any account that has associated transactions; only deactivation shall be permitted. |
| FR-COA-05 | The system shall support designation of "Control Accounts" that summarize sub-ledger balances (e.g., Debtors, Creditors). |
| FR-COA-06 | The system shall allow bulk import and export of the chart of accounts via CSV/Excel. |
| FR-COA-07 | The system shall support merging/reclassifying an account's transactions into another account. |
| FR-COA-08 | The system shall display the chart of accounts as a navigable, collapsible tree view. |
| FR-COA-09 | The system shall support account-level access restriction (e.g., restricting visibility of payroll or director loan accounts to specific roles). |
| FR-COA-10 | The system shall support opening balances per account, tagged with Dr/Cr type. |

### 3.2 Vouchers

| ID | Requirement |
|---|---|
| FR-VCH-01 | The system shall support the following voucher types: Payment, Receipt, Journal, Sales, Purchase, Contra, Debit Note, Credit Note. |
| FR-VCH-02 | The system shall auto-generate sequential, gap-free voucher numbers per voucher type and fiscal year. |
| FR-VCH-03 | The system shall enforce double-entry validation: total debit must equal total credit before a voucher can be saved/posted. |
| FR-VCH-04 | The system shall require a minimum of two line items per voucher. |
| FR-VCH-05 | The system shall support tagging each voucher line with a cost center/department/project. |
| FR-VCH-06 | The system shall prevent posting of vouchers dated within a closed fiscal period. |
| FR-VCH-07 | The system shall support a voucher approval workflow: Draft → Pending Approval → Posted / Rejected. |
| FR-VCH-08 | The system shall support voucher reversal by generating an offsetting entry; original posted vouchers shall never be deleted or altered. |
| FR-VCH-09 | The system shall support attaching scanned documents/files to a voucher. |
| FR-VCH-10 | The system shall support recurring voucher templates that auto-generate vouchers on a defined schedule. |
| FR-VCH-11 | The system shall support multi-currency vouchers with exchange rate capture at the transaction date. |
| FR-VCH-12 | The system shall maintain a full audit trail (created by, modified by, timestamps) for every voucher. |
| FR-VCH-13 | The system shall auto-calculate applicable taxes on Sales/Purchase vouchers based on configured tax rules. |

### 3.3 Ledgers

| ID | Requirement |
|---|---|
| FR-LED-01 | The system shall generate a General Ledger view listing all posted transactions across all accounts, filterable by date range and voucher type. |
| FR-LED-02 | The system shall generate an Account Ledger showing opening balance, transaction history, and running balance for a selected account and date range. |
| FR-LED-03 | Ledger data shall be derived (computed) from voucher records, not stored redundantly. |
| FR-LED-04 | The system shall support drill-down from any ledger entry to its originating voucher. |
| FR-LED-05 | The system shall generate customer-wise and supplier-wise sub-ledgers with aging summaries. |
| FR-LED-06 | The system shall support bank reconciliation: importing bank statements (CSV/OFX) and matching entries against the bank ledger. |
| FR-LED-07 | The system shall support both automatic (rule-based) and manual matching during reconciliation. |
| FR-LED-08 | The system shall prevent modification of ledger entries within locked/closed fiscal periods. |

### 3.4 Reports

| ID | Requirement |
|---|---|
| FR-RPT-01 | The system shall generate a Trial Balance as of any given date. |
| FR-RPT-02 | The system shall generate a Profit & Loss Statement for a selected period, with optional prior-period comparison. |
| FR-RPT-03 | The system shall generate a Balance Sheet as of any given date. |
| FR-RPT-04 | The system shall generate a Cash Flow Statement (operating, investing, financing) for a selected period. |
| FR-RPT-05 | The system shall generate Accounts Receivable and Accounts Payable aging reports (30/60/90/120+ day buckets). |
| FR-RPT-06 | The system shall generate a Day Book listing all vouchers for a selected date or range. |
| FR-RPT-07 | The system shall support Budget vs. Actual comparison reporting. |
| FR-RPT-08 | The system shall support export of any report to PDF and Excel formats. |
| FR-RPT-09 | The system shall support drill-down from report line items to underlying ledger/voucher detail. |
| FR-RPT-10 | The system shall support scheduled, automated emailing of reports at defined intervals. |
| FR-RPT-11 | The system shall support cost-center-wise and branch-wise report segmentation for multi-branch entities. |

---

## 4. Non-Functional Requirements

| ID | Category | Requirement |
|---|---|---|
| NFR-01 | Performance | Ledger and report queries shall return results within 2 seconds for datasets up to 1 million voucher lines, using indexed queries and pagination. |
| NFR-02 | Security | All API endpoints shall require JWT-based authentication and role/claims-based authorization. |
| NFR-03 | Auditability | All create/update/delete operations on financial data shall be logged with user, timestamp, and before/after values. |
| NFR-04 | Data Integrity | Posted vouchers shall be immutable; corrections shall only occur via reversal or adjustment entries. |
| NFR-05 | Concurrency | The system shall use optimistic concurrency control (row versioning) to prevent conflicting simultaneous edits. |
| NFR-06 | Scalability | The backend shall be stateless and horizontally scalable behind a load balancer. |
| NFR-07 | Usability | The Angular frontend shall provide real-time inline validation (e.g., debit/credit balance indicator) before allowing form submission. |
| NFR-08 | Availability | The system shall target 99.5% uptime for production deployments. |
| NFR-09 | Compatibility | The frontend shall support the latest two major versions of Chrome, Edge, and Firefox. |
| NFR-10 | Extensibility | The API shall be versioned (e.g., `/api/v1/`) to support non-breaking evolution for future mobile (Flutter) clients. |
| NFR-11 | Localization | The system shall support multi-currency display and be structured to allow future multi-language UI labels. |

---

## 5. System Interfaces

### 5.1 API Summary (representative endpoints)

| Module | Endpoint | Method | Description |
|---|---|---|---|
| COA | `/api/accounts/tree` | GET | Retrieve hierarchical account tree |
| COA | `/api/accounts` | POST | Create new account |
| Vouchers | `/api/vouchers` | POST | Create voucher (validates double-entry) |
| Vouchers | `/api/vouchers/{id}/approve` | POST | Approve pending voucher |
| Vouchers | `/api/vouchers/{id}/reverse` | POST | Reverse a posted voucher |
| Ledgers | `/api/ledgers/account/{accountId}` | GET | Get account ledger for date range |
| Ledgers | `/api/ledgers/bank/{accountId}/reconcile` | POST | Reconcile bank statement lines |
| Reports | `/api/reports/trial-balance` | GET | Retrieve trial balance as of date |
| Reports | `/api/reports/profit-loss` | GET | Retrieve P&L for period |
| Reports | `/api/reports/{type}/export` | GET | Export report as PDF/Excel |

### 5.2 External Interfaces
- Bank statement import: CSV / OFX file formats
- Report export: PDF (via QuestPDF/DinkToPdf), Excel (via ClosedXML/EPPlus)
- Real-time notifications: SignalR (voucher status changes broadcast to connected clients)

---

## 6. Data Model Overview

```
ChartOfAccounts (AccountId, ParentAccountId, Code, Name, Type, Nature, IsActive, IsControlAccount)
VoucherHeader   (VoucherId, VoucherType, VoucherNo, Date, Narration, Status, CreatedBy, RowVersion)
VoucherDetail   (DetailId, VoucherId, AccountId, DebitAmount, CreditAmount, CostCenterId)
FiscalPeriod    (PeriodId, StartDate, EndDate, IsClosed)
CostCenter      (CostCenterId, Name, ParentCostCenterId)
User            (UserId, Name, Role, Email)
AuditLog        (LogId, EntityName, EntityId, Action, OldValue, NewValue, UserId, Timestamp)
```

Ledgers and reports are **derived views**, computed at query time from `VoucherDetail` joined with `ChartOfAccounts` and `VoucherHeader` — not persisted as separate tables, to avoid data duplication and synchronization issues.

---

## 7. Acceptance Criteria (Sample)

| Requirement | Acceptance Criteria |
|---|---|
| FR-VCH-03 | Saving a voucher with unequal debit/credit totals shall be rejected with a clear validation message; the Save button shall remain disabled until balanced. |
| FR-VCH-08 | Reversing a posted voucher shall create a new voucher with opposite debit/credit entries and link it to the original via a reference field; the original voucher shall remain unchanged and visible in history. |
| FR-LED-06 | Importing a bank statement file shall auto-match at least 80% of entries where amount, date (±3 days), and reference align; unmatched entries shall be listed separately for manual review. |
| FR-RPT-01 | The Trial Balance report shall show total debits equal to total credits; any mismatch shall trigger a visible data-integrity warning. |

---

## 8. Out of Scope (Phase 1)
- Payroll processing
- Fixed asset depreciation scheduling (may be added in a later phase)
- Multi-entity consolidation across separate legal entities
- Native mobile app (Flutter) — API will be designed to support this in a future phase, but mobile UI is not part of this release

---

## 9. Assumptions & Constraints
- Single relational database (SQL Server/PostgreSQL) via EF Core; no NoSQL components in Phase 1
- Angular frontend consumes only versioned REST APIs — no direct database access
- All monetary calculations performed using `decimal` types to avoid floating-point rounding errors
- Fiscal period closing is a manual, role-restricted action (Administrator only)

---

## 10. Approval

| Role | Name | Signature | Date |
|---|---|---|---|
| Product Owner | | | |
| Technical Lead | | | |
| QA Lead | | | |
