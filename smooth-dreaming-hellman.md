# Rights & Roles (RBAC) feature

## Context

Today access control is a single `User.Role` string (`"User"`/`"Admin"`/`"SystemAdmin"`, `Auth/AppRoles.cs` constants) checked via ASP.NET's built-in `[Authorize(Roles="...")]`/JWT `ClaimTypes.Role`. There is no notion of granular permissions. The user wants: a fixed catalog of **Rights**, one per real functionality; full **CRUD of Roles**, where a Role is an assignable bundle of Rights; and assigning a **User** a Role grants them all of that Role's Rights.

Confirmed product decisions (already asked/answered):
1. **Rights are a fixed, code-seeded catalog** — not user-creatable. Read-only `GET /api/rights`.
2. **Infrastructure only this pass** — existing business controllers (`CostCentersController`, `AccountsController`, etc., all still gated by `[Authorize(Roles="Admin")]`) are **not** touched. Only the new Roles/Rights/Users surfaces adopt the new mechanism.
3. **Roles are per-tenant**, except the 3 existing roles become **protected global built-ins** (`IsSystemRole=true`, `TenantId=null`, not renamable/deletable — but their Rights ARE still editable).

This plan builds `Right`/`Role`/`RoleRight` end-to-end (DB, JWT, authorization, CRUD API, Angular pages) while leaving every other controller's authorization untouched, and preserves 100% of existing login/seeding/escalation behavior.

---

## Backend (`erp backend/erp backend/`)

### New entities (`Models/Right.cs`, `Models/Role.cs`, `Models/RoleRight.cs`)

- `Right { Id, Code (unique), Module, Description }` — implements neither `ITenantScoped` nor `IBranchScoped` (global catalog, like `Tenant`/`Branch`).
- `Role { Id, TenantId (int?), Tenant?, Name, Description?, IsSystemRole, CreatedAtUtc, RoleRights }` — implements **neither** marker interface either. `TenantId == null` means global built-in role (visible/assignable everywhere); non-null means a tenant-owned custom role. This can't reuse the reflection-based `ApplyTenantBranchQueryFilters` loop in `Data/AppDbContext.cs` (its `SetTenantScopedFilter<T>` assumes exact non-nullable equality), so add directly in `Role`'s own `OnModelCreating` block:
  ```csharp
  entity.HasQueryFilter(r => r.TenantId == null || r.TenantId == _tenantContext.TenantId);
  entity.HasIndex(r => new { r.TenantId, r.Name }).IsUnique();
  entity.HasOne(r => r.Tenant).WithMany().HasForeignKey(r => r.TenantId).OnDelete(DeleteBehavior.Restrict);
  ```
  Note for whoever writes `RoleRepository`: because `Role` implements neither interface, `ApplyTenantBranchStamping` never auto-stamps `TenantId` — set it explicitly on create (the repo already needs to resolve `targetTenantId` for other checks, same as `UserRepository.CreateAsync` today).
- `RoleRight { RoleId, RightId }` — composite key, `Role` cascade-delete, `Right` restrict-delete. Add all three as `DbSet`s in `Data/AppDbContext.cs` (next to the existing `DbSet<User>` etc.) plus their `modelBuilder.Entity<...>` Fluent blocks, following the exact style already used for `UserBranch`/`Branch` in that file.

### Right catalog: `Auth/RightCatalog.cs` (new)

A `RightCodes` static class of string constants (one per real guarded functionality, audited from every existing `[Authorize(Roles="Admin")]` action — Vouchers, Accounts, CostCenters, TaxRates, FiscalPeriods, Budgets, ReportSchedules, RecurringVoucherTemplates, Branches, Tenants — plus `Roles.Manage` for the new feature itself) and a `RightCatalog.All` list of `(Code, Module, Description)`. Only `Roles.Manage` is actually enforced anywhere in this pass; the rest exist so the "assign rights" picker is meaningful day one.

Seeding — two different mechanisms for two different lifetimes:
- **The 3 built-in Roles** → EF `HasData` in the migration that creates the `Roles` table, with a **fixed literal** `CreatedAtUtc` (not `DateTime.UtcNow` — `HasData` needs a deterministic value or every future `migrations add` will think the seed changed). This must be `HasData` (not `Program.cs` runtime code) because the `User.Role`→`RoleId` backfill migration (below) needs these rows to already exist *at migration-apply time*, which happens before any `Program.cs` code runs.
- **The evolving `RightCatalog.All`** → an idempotent upsert-by-`Code` helper called from `Program.cs`, **unconditionally** (outside the `if (app.Environment.IsDevelopment())` block — Rights must exist in every environment, not just dev). Using `HasData` here would force a new migration every time a right is added.
- Also seed **built-in Admin's `RoleRight` grants** = every catalog right, idempotently, also unconditional — this is what makes tenant Admins' day-one access to the new Roles/Rights endpoints genuinely data-driven instead of a second hardcoded bypass.

### `User.Role` (string) → `User.RoleId` (FK) — two migrations

**Migration A (`AddRightsAndRoles`)**: pure additive — creates `Rights`, `Roles`, `RoleRights` tables + the 3 built-in Role rows via `HasData`. Does not touch `Users`.

**Migration B (`AddUserRoleId`)** — high-risk, must be hand-edited after `dotnet ef migrations add` (per CLAUDE.md's standing warning that EF's autogenerated `defaultValue` for new non-nullable columns is untrustworthy for backfills):
1. Add `RoleId` to `Users` as **nullable, no default**.
2. Hand-written `migrationBuilder.Sql(...)` backfill: `UPDATE u SET u.RoleId = r.Id FROM Users u INNER JOIN Roles r ON r.TenantId IS NULL AND r.Name = u.Role;` (safe because Migration A guarantees these rows exist).
3. Hand-written guard: `IF EXISTS (SELECT 1 FROM Users WHERE RoleId IS NULL) THROW 51000, '...', 1;` — fail loudly rather than silently corrupt data.
4. Alter `RoleId` to `NOT NULL`, add index + FK to `Roles` (Restrict).
5. Drop the old `Role` string column.
6. `Down()`: symmetric reverse (re-add `Role` column, backfill from `Roles.Name` via `Users.RoleId`, drop FK/column).

Edit `Models/User.cs` first (`Role` string → `RoleId int` + `Role? Role` nav, mirroring the existing `TenantId`/`Tenant` pattern) so EF's diff proposes the right shape, then hand-edit the generated migration into the exact order above.

### `Program.cs` changes

- Restructure so the Rights-catalog + built-in-role-rights seeding runs unconditionally (both dev and non-dev), right after `Database.Migrate()`.
- Dev-only admin/superadmin seeding (lines ~128-169 today) changes only at the `User` construction: look up `adminRoleId`/`systemAdminRoleId` from `db.Roles.IgnoreQueryFilters()` first, then `Role = "Admin"` → `RoleId = adminRoleId` (same for `AppRoles.SystemAdmin` → `systemAdminRoleId`). The existing `platformTenant` logic for `superadmin`'s `TenantId` is untouched.

### `Roles/` feature folder (new, mirrors `Users/UserRepository.cs`'s "inject `AppDbContext` directly" style, not the generic-`IRepository<T>` style, since it needs custom tenant/uniqueness/protection queries)

- `IRoleRepository`/`RoleRepository`: `GetAllAsync(tenantId?)`, `GetByIdAsync(id)`, `CreateAsync(RoleRequest)`, `UpdateAsync(id, RoleRequest)`, `DeleteAsync(id)`.
  - Reuses the exact `IsSystemAdmin`/`XyzQuery()` pattern from `UserRepository.cs:27-29` (`_httpContextAccessor.HttpContext?.User.IsInRole(AppRoles.SystemAdmin)`).
  - `CreateAsync`: resolve `targetTenantId` exactly like `UserRepository.CreateAsync` (`UserRepository.cs:55`); validate `RightIds` all exist (→ `RightNotFound`); enforce name uniqueness among `{null, targetTenantId}` (→ `RoleNameExists`); always `IsSystemRole = false` (API can never create a system role); explicitly set `TenantId` (no auto-stamping safety net, see above).
  - `UpdateAsync`: if `role.IsSystemRole && request.Name != role.Name` → new `RoleNameNotEditableForSystemRole`; Rights replacement (delete-all-then-reinsert `RoleRight` rows) is always allowed, even for system roles.
  - `DeleteAsync`: `IsSystemRole` → `RoleIsSystemRoleCannotDelete`; in-use check (`_context.Users.IgnoreQueryFilters().AnyAsync(u => u.RoleId == id)`) → new `RoleInUse`.
- `Dtos/RoleRequest.cs` (`TenantId?`, `Name`, `Description?`, `RightIds: List<int>`), `Dtos/RoleResponse.cs` (adds `TenantId?`, `TenantName?`, `IsSystemRole`, `Rights: List<RightResponse>`, static `FromEntity`).
- `Controllers/RolesController.cs`: `[Authorize(Policy = RightPolicies.RolesManage)]` at class level (see below), full CRUD, same `try/catch(AppException)/catch(Exception)` → `ApiResponse<T>` shape as every other controller (use `CostCentersController.cs` as the structural template).

### `Rights/` feature folder (new, separate from `Roles/` — one domain per folder, matching repo convention)

- `IRightRepository`/`RightRepository`: injects the generic `IRepository<Right>` (no tenant scoping needed — same simple template as `CostCenterRepository.cs`), one method `GetAllGroupedAsync()` → group by `Module`.
- `Dtos/RightResponse.cs`, `Dtos/RightModuleGroupResponse.cs` ({Module, Rights: List<RightResponse>}).
- `Controllers/RightsController.cs`: `[Authorize(Policy = RightPolicies.RolesManage)]`, single `GET /api/rights`.

### `Users/` changes

- `Dtos/CreateUserRequest`/`UpdateUserRequest`: `Role` (string) → `RoleId` (int).
- `Dtos/UserResponse`: add `RoleId`; keep `Role` (string, from `entity.Role.Name`) so the existing frontend contract shape for display is unaffected structurally (frontend plan below changes it to a nested object, but the backend field name can stay `Role` if populated from the name — confirm exact shape against the frontend plan when implementing).
- `UserRepository.cs`: add `.Include(u => u.Role)` everywhere `UsersQuery()`/`GetByIdAsync` is used; `CreateAsync`/`UpdateAsync` resolve the target `Role` by `RoleId` (`NotFoundException(RoleNotFound)` if missing); **escalation guard preserved exactly** — replace the raw string compares at `UserRepository.cs:57` and `UserRepository.cs:110-112` with `role.IsSystemRole && role.Name == AppRoles.SystemAdmin` (create) / `targetRole.Name == AppRoles.SystemAdmin && user.Role?.Name != AppRoles.SystemAdmin` (update), same `ForbiddenException(OnlySystemAdminCanAssignSystemAdmin)` on failure — zero behavior change for existing callers.

### JWT + granular authorization

- `Auth/JwtTokenService.cs`: change `GenerateToken(User user)` → `GenerateToken(User user, string roleName, IReadOnlyCollection<string> rightCodes)`; keep `new Claim(ClaimTypes.Role, roleName)` (built-in `Role.Name` values are exactly `"Admin"`/`"SystemAdmin"`/`"User"`, so every existing `[Authorize(Roles="Admin")]` on untouched controllers keeps working unchanged); add one `new Claim("right", code)` per right code.
- `Auth/AuthRepository.cs` (`LoginAsync`, line 58): add `.Include(u => u.Role)` to the user lookup; query `RoleRight`/`Right` joined on `user.RoleId` for the code list; pass `user.Role!.Name` + codes into `GenerateToken`. `LoginResponse.Role` stays a string (`user.Role.Name`) — no shape change. Add `LoginResponse.Rights: List<string>` (new field) so the SPA can gate UI without decoding the JWT.
- New `Auth/RightPolicies.cs` (`Prefix = "Right:"`, `RolesManage = Prefix + RightCodes.RolesManage`), `Auth/RightRequirement.cs` (`IAuthorizationRequirement` wrapping a right code), `Auth/RightAuthorizationHandler.cs` (`AuthorizationHandler<RightRequirement>`: succeed if `context.User.IsInRole(AppRoles.SystemAdmin)` OR has the matching `"right"` claim — SystemAdmin bypass is unconditional here, mirroring how it already bypasses tenant filters elsewhere via `IgnoreQueryFilters()`), `Auth/RightPolicyProvider.cs` (`IAuthorizationPolicyProvider` that intercepts `"Right:"`-prefixed policy names and delegates everything else to a wrapped `DefaultAuthorizationPolicyProvider`).
- Why this is safe: `GetPolicyAsync` is only consulted for `[Authorize(Policy="...")]`; bare `[Authorize]` and `[Authorize(Roles="...")]` never go through it (confirmed against ASP.NET Core's `AuthorizationPolicy.CombineAsync`), so **no existing controller's authorization changes**.
- `Program.cs`: register `AddSingleton<IAuthorizationPolicyProvider, RightPolicyProvider>()`, `AddSingleton<IAuthorizationHandler, RightAuthorizationHandler>()` before the existing bare `AddAuthorization()` call; register `IRoleRepository`/`IRightRepository` alongside the other `AddScoped<I...>` lines.

### `Messages/ResponseMessage.cs`

Add a new `// --- Roles / Rights ---` group: `RoleNotFound`, `RoleNameExists`, `RoleNameNotEditableForSystemRole`, `RoleIsSystemRoleCannotDelete`, `RoleInUse`, `RoleTenantMismatch`, `RoleCreated`, `RoleUpdated`, `RoleDeleted`, `RightNotFound` — same `[Description("...")]` + enum-member style as the existing "Users (administration)" group at line 252. Reuse the existing `OnlySystemAdminCanAssignSystemAdmin` unchanged.

---

## Frontend (`erp/`)

### `core/rights/` (new) — read-only reference data, separate from `core/roles/`

- `right.models.ts`: `RightSummary { id, code, name, description }`, `RightGroup { module, rights: RightSummary[] }`.
- `rights.service.ts`: single `getAll(): Observable<ApiResponse<RightGroup[]>>` against `${environment.apiUrl}/api/rights`.

### `core/roles/` (new)

- `role.models.ts`: `RoleSummary { id, name, isSystemRole }` (slim shape for the Users-page dropdown, same idea as `BranchSummary`), `RoleResponse { id, name, description, isSystemRole, rights: RightSummary[] }`, `RoleRequest { name, description, rightIds: number[] }`.
- `role.service.ts`: `getAll()`, `getById(id)`, `create(req)`, `update(id, req)`, `delete(id)` against `.../api/roles` — same `Injectable({providedIn:'root'})` + `inject(HttpClient)` shape as `user.service.ts`/`tenant.service.ts`.

### `pages/admin/roles/` (new) — `roles.ts`/`roles.html`/`roles.scss`, same triad/toolbar/dialog structure as `pages/admin/tenants/` and `pages/admin/users/users.ts` (already read as the template: `loading`/`saving` signals, `FormBuilder` reactive group for `name`/`description`, `ConfirmationService` for delete, `extractErrorMessage` private helper copied verbatim per existing per-page convention — no shared base class exists in this repo).

- State: `roles = signal<RoleResponse[]>([])`, `rightGroups = signal<RightGroup[]>([])` (loaded once via `RightsService.getAll()`), `dialogVisible`, `editingRole`, and `selectedRightIds = signal<Set<number>>(...)` driving a **grouped-checkbox** rights picker (one `p-fieldset`/section per `RightGroup.module`, `p-checkbox` per right) — chosen over `p-multiSelect`/`p-picklist` because the backend already shapes rights by module and a flat/reorderable control would lose that grouping.
- `openEditDialog(role)`: if `role.isSystemRole`, disable the `name` form control (mirrors how `tenants.ts` disables `code` on edit) — description and rights stay editable per the "built-ins protected but rights still editable" decision.
- Delete button `[disabled]="role.isSystemRole"` (+ defensive early-return in the method).
- Route in `app.routes.ts`, inserted after `admin/users`: `{ path: 'admin/roles', component: Roles, canActivate: [roleGuard], data: { roles: ['Admin','SystemAdmin'] } }` (role-gated like the other 3 admin routes today — no right-gating required for the initial cut).
- Sidebar (`layout/sidebar/sidebar.ts`): add `{ label: 'Roles', icon: 'pi pi-shield', route: '/admin/roles', roles: ['Admin', 'SystemAdmin'] }` to the existing `Administration` group, right after `Users` — `isVisible()`/`isGroupVisible()` need no changes, the existing `roles?: string[]` mechanism already covers it.

### `core/users/` updates

- `user.models.ts`: `AdminUser.role: string` → `role: RoleSummary`; `CreateUserRequest.role: string` → `roleId: number`; `UpdateUserRequest.role: string` → `roleId: number`.
- `pages/admin/users/users.ts`: delete the hardcoded `NON_SYSTEM_ADMIN_ROLES`/`ALL_ROLES` arrays (lines 27-32 today); inject `RoleService`; add `allRoles = signal<RoleSummary[]>([])` loaded unconditionally in `ngOnInit` (not gated behind `isSystemAdmin()` the way the tenants call is, since every tenant Admin needs role options); replace the `roleOptions` computed with one built from `allRoles()`, filtering out the `'SystemAdmin'` role name unless `isSystemAdmin()` (preserves today's exact gate semantics, just data-driven instead of a hardcoded array); update `createForm`/`editForm`'s `role` control → `roleId` (`fb.control<number|null>(null, [Validators.required])`, since it's no longer safe to default to a literal like `'User'`); update `openEditDialog`/`openCreateDialog` reset payloads accordingly.
- `pages/admin/users/users.html`: `{{ user.role }}` → `{{ user.role.name }}`; `formControlName="role"` → `formControlName="roleId"` in both dialogs.

### `core/auth/` updates

- `auth.models.ts`: add `rights: string[]` to `LoginResponse`.
- `auth.service.ts`: add `rights: string[]` to the private `StoredAuth` interface; in `readStoredAuth()`, defensively coalesce `{ ...parsed, rights: parsed.rights ?? [] }` so a pre-migration `localStorage` session (no `rights` key) doesn't break; add `readonly rights = computed(() => this.auth()?.rights ?? [])` and `hasRight(code: string): boolean` next to the existing `role`/`token`/`username` computeds; `setAuth()` gains `rights: data.rights` in the constructed `StoredAuth` object. No other consumer (`auth.guard.ts`, `auth.interceptor.ts`, `role.guard.ts`, `sidebar.ts`) needs changes from this alone.
- `role.guard.ts`: extend (don't add a sibling file) to also read `route.data['rights']: string[]` and AND it with the existing `roles` check via `authService.hasRight(...)`. Fully backward compatible — the 3 existing routes only set `data.roles`, so `requiredRights` is `undefined` and behavior is identical to today.

---

## Verification

1. **Backend build**: `dotnet build` in `erp backend/erp backend/` — must be clean.
2. **Migration correctness**: `dotnet ef database update` against a fresh dev DB (drop and recreate the local `erp` DB first to test the full seed path from zero), then again against a DB with existing `admin`/`superadmin` users (via `dotnet run`, since `Program.cs` runs `Database.Migrate()` itself in Development) to confirm the backfill migration correctly maps their string `Role` to the new `RoleId` and the app starts without the `THROW` guard firing.
3. **Login regression**: log in as `admin`/`Admin@123` and `superadmin`/`SuperAdmin@123` via `POST /api/auth/login` (Swagger) — confirm `role` in the response is unchanged (`"Admin"`/`"SystemAdmin"`) and a new `rights` array is present.
4. **Existing endpoints untouched**: hit one existing `[Authorize(Roles="Admin")]` endpoint (e.g. `POST /api/cost-centers`) with the `admin` token — must still succeed exactly as before.
5. **New endpoints**: as `admin`, call `GET /api/rights` (grouped catalog), `POST /api/roles` (create a custom role with a subset of rights), `PUT /api/users/{id}` with the new role — confirm `RoleId` persists and `GET /api/users/{id}` reflects it. As a non-SystemAdmin `Admin`, attempt to assign the `SystemAdmin` role to a user — must still 403 with `OnlySystemAdminCanAssignSystemAdmin`. Attempt to delete/rename the built-in `Admin` role — must 400 with the new system-role-protection messages.
6. **Frontend**: `npm start`, log in as `admin`, navigate to the new `/admin/roles` page — verify list, create/edit dialog with the grouped rights picker, system-role rows have disabled name/delete; navigate to `/admin/users` — verify the role dropdown is now populated from the API and user create/edit still works end-to-end.
7. **Frontend unit tests**: `npm test` — must stay green (no `.spec.ts` exists today for `pages/admin/*` or `core/auth`/`core/users`, so none are expected to need updates, but confirm nothing else broke from the model shape changes).
