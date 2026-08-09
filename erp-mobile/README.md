# erp-mobile

React Native (Expo, TypeScript) companion app to [`erp/`](../erp) (the Angular web app) and
[`erp backend/erp backend/`](../erp%20backend/erp%20backend) (the ASP.NET Core API). Same
backend, same auth/tenancy model, own independently-versioned `package.json` — no code is
shared between the three apps; types here are hand-ported to match the backend's DTOs.

## Running it

```bash
cd erp-mobile
npm start          # opens Expo Dev Tools; press a/i/w or scan the QR code with Expo Go
npm run android
npm run ios         # macOS only
npm run web
```

### Pointing it at the backend

The backend must be running (`dotnet run` from `erp backend/erp backend/`). By default this
app talks to its **plain-HTTP** launch profile on port `5023`, not the HTTPS one on `7002`
(see `src/core/config.ts`) — React Native won't trust the ASP.NET Core dev cert the way a
browser will, so HTTP sidesteps that entirely for local development.

- **Android emulator**: works out of the box (`10.0.2.2` aliases your host machine).
- **iOS simulator**: works out of the box (`localhost` reaches the host directly).
- **Physical device**: neither of the above can reach your dev machine. Copy `.env.example`
  to `.env`, set `EXPO_PUBLIC_API_URL` to your machine's LAN IP (e.g.
  `http://192.168.1.20:5023`), and restart `npm start`.

## Architecture

Mirrors the shape of `erp/src/app/`, adapted for React Native:

```
src/
  core/
    config.ts              API base URL resolution (see above)
    api/client.ts          axios instance + interceptors — port of auth.interceptor.ts +
                            branch.interceptor.ts (attaches Bearer token + X-Branch-Id,
                            force-logout on 401)
    models/                 ApiResponse<T>, PagedResult<T> — ports of core/models/*
    auth/                   auth.types.ts, right-code.ts (port of RightCode), auth.context.tsx
                            (AuthProvider/useAuth — AsyncStorage-backed, mirrors
                            core/auth/auth.service.ts), RequireRight.tsx (port of
                            *appHasRight)
    tenancy/                tenancy.types.ts, tenancy.context.tsx (TenancyProvider/useTenancy
                            — port of core/tenancy/tenancy.service.ts, minus the SystemAdmin
                            cross-tenant "override" feature, which is out of scope here)
  navigation/
    RootNavigator.tsx        Login stack vs. Main tabs, gated on isAuthenticated
    MainTabs.tsx              Home + placeholder tabs for Vouchers/Invoices/Inventory/Admin
  screens/
    auth/LoginScreen.tsx
    home/HomeScreen.tsx       shows signed-in user + a branch switcher when >1 branch
    placeholder/ComingSoonScreen.tsx
```

`AuthProvider` must be rendered inside `TenancyProvider` (see `App.tsx`) — same dependency
direction as the web app's `AuthService` injecting `TenancyService`, so a successful login can
hand branch data off directly.

## Scope — all phases built

1. **Auth + branch** — `screens/auth/LoginScreen.tsx`, branch switcher in `screens/home/HomeScreen.tsx`.
2. **Inventory reporting** — `screens/inventory/InventoryScreen.tsx` (segmented On Hand /
   Ledger): `StockOnHandScreen` (`GET /api/stockledger/on-hand`, warehouse filter, low-stock
   toggle) and `StockLedgerScreen` (`GET /api/stockledger`, paged movement history).
3. **Payment + Receipt vouchers** — `screens/vouchers/VouchersScreen.tsx`: segmented
   Payment/Receipt list, `VoucherFormModal` (double-entry line editor, blocks saving until
   debit == credit), `VoucherDetailModal` (read-only). `POST/GET /api/vouchers`.
4. **Users admin** — `screens/admin/UsersScreen.tsx` + `UserFormModal.tsx`: create/edit a
   user's Role and branch access, activate/deactivate. Hides the `SystemAdmin` role option
   unless the signed-in user is themselves a SystemAdmin (server enforces this regardless;
   this is just matching UX to the real constraint). **Rights themselves stay web-only**:
   this system has no per-user rights, only per-Role (`Role → RoleRight → Right`), and
   editing a Role's rights is left to the existing Angular `pages/admin/roles` screen. Whole
   tab is gated behind the `Users.View` right (`MainTabs.tsx`), same as the web route guard.
5. **Sales Invoice** — `screens/invoices/InvoicesScreen.tsx` + `InvoiceFormModal.tsx`: paged
   list, create against a Customer/Warehouse/product-variant line editor (unit price
   defaults from the variant's `Sale` price, tax total is a client-side estimate for display
   — the backend response is the source of truth). `POST/GET /api/invoices`, `invoiceType` =
   `SalesInvoice`.

Every create action (voucher/user/invoice) is gated behind its matching `RightCode` via
`useAuth().hasRight()`, mirroring `RequireRight.tsx`.

Explicitly out of scope: offline/sync, push notifications, attachment upload, voucher/invoice
approve/reject workflow on mobile (approvals stay a web-only action), biometric auth,
Purchase-side documents (PO/PurchaseInvoice/Returns), UOM conversions on invoice lines
(invoicing always uses a product's base UOM here).

### Known limitations (inherited from the backend, not mobile-specific)

- **Rights apply at next login only.** They're baked into the JWT at login time and never
  re-checked live — if an admin changes a user's Role or a Role's rights on the web, this app
  (and the web app) won't reflect it until that user signs out and back in. There is no
  refresh/`me` endpoint.
- Only a `SystemAdmin` can assign the `SystemAdmin` role to a user (enforced server-side in
  `Users/UserRepository.cs`); the Users admin screen should hide that option client-side for
  non-SystemAdmins as a UX nicety, but the real enforcement is on the backend.
