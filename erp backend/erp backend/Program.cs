using System.Text;
using System.Text.Json.Serialization;
using erp_backend.Accounting;
using erp_backend.Accounts;
using erp_backend.Auditing;
using erp_backend.Auth;
using erp_backend.Branches;
using erp_backend.CostCenters;
using erp_backend.Data;
using erp_backend.FiscalPeriods;
using erp_backend.Inventory;
using erp_backend.Invoices;
using erp_backend.Ledgers;
using erp_backend.Middleware;
using erp_backend.Models;
using erp_backend.Partners;
using erp_backend.ProductCategories;
using erp_backend.Products;
using erp_backend.Reporting;
using erp_backend.Repositories;
using erp_backend.Rights;
using erp_backend.Roles;
using erp_backend.TaxRates;
using erp_backend.Tenants;
using erp_backend.UnitsOfMeasure;
using erp_backend.Users;
using erp_backend.Vouchers;
using erp_backend.Warehouses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your token.",
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() },
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IAuditLogger, AuditLogger>();
builder.Services.AddScoped<IFiscalPeriodGuard, FiscalPeriodGuard>();
builder.Services.AddScoped<IReportExporter, ReportExporter>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICostCenterRepository, CostCenterRepository>();
builder.Services.AddScoped<IFiscalPeriodRepository, FiscalPeriodRepository>();
builder.Services.AddScoped<ITaxRateRepository, TaxRateRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IRecurringVoucherTemplateRepository, RecurringVoucherTemplateRepository>();
builder.Services.AddScoped<ILedgerRepository, LedgerRepository>();
builder.Services.AddScoped<ISubLedgerRepository, SubLedgerRepository>();
builder.Services.AddScoped<IBankReconciliationRepository, BankReconciliationRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IReportScheduleRepository, ReportScheduleRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ICurrentTenantContext, CurrentTenantContext>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRightRepository, RightRepository>();

builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IBusinessPartnerRepository, BusinessPartnerRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IStockAccountMappingRepository, StockAccountMappingRepository>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        };
    });

builder.Services.AddSingleton<IAuthorizationPolicyProvider, RightPolicyProvider>();
builder.Services.AddSingleton<IAuthorizationHandler, RightAuthorizationHandler>();
builder.Services.AddAuthorization();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (app.Environment.IsDevelopment())
    {
        db.Database.Migrate();
    }

    // The Right catalog and the built-in Admin role's grants must exist in every environment
    // (not just dev) so a freshly migrated production DB isn't missing them.
    SeedRightCatalogAndBuiltInGrants(db);

    if (app.Environment.IsDevelopment())
    {
        var adminRoleId = db.Roles.First(r => r.TenantId == null && r.Name == AppRoles.Admin).Id;
        var systemAdminRoleId = db.Roles.First(r => r.TenantId == null && r.Name == AppRoles.SystemAdmin).Id;

        var admin = db.Users.IgnoreQueryFilters().FirstOrDefault(u => u.Username == "admin");
        if (admin is null)
        {
            var hasher = new PasswordHasher<User>();
            admin = new User { Username = "admin", RoleId = adminRoleId, TenantId = 1 };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
            db.Users.Add(admin);
            db.SaveChanges();
        }

        var hasDefaultBranchGrant = db.UserBranches.Any(ub => ub.UserId == admin.Id && ub.BranchId == 1);
        if (!hasDefaultBranchGrant)
        {
            db.UserBranches.Add(new UserBranch { UserId = admin.Id, BranchId = 1 });
            db.SaveChanges();
        }

        // SystemAdmin accounts belong to a dedicated internal "platform" tenant, never a real
        // customer tenant, so they never show up in any customer's own Users list.
        var platformTenant = db.Tenants.FirstOrDefault(t => t.Code == "PLATFORM");
        if (platformTenant is null)
        {
            platformTenant = new Tenant { Name = "ERP Platform (Internal)", Code = "PLATFORM", IsActive = true };
            db.Tenants.Add(platformTenant);
            db.SaveChanges();
        }

        var superAdmin = db.Users.IgnoreQueryFilters().FirstOrDefault(u => u.Username == "superadmin");
        if (superAdmin is null)
        {
            var hasher = new PasswordHasher<User>();
            superAdmin = new User { Username = "superadmin", RoleId = systemAdminRoleId, TenantId = platformTenant.Id };
            superAdmin.PasswordHash = hasher.HashPassword(superAdmin, "SuperAdmin@123");
            db.Users.Add(superAdmin);
            db.SaveChanges();
        }
        else if (superAdmin.TenantId != platformTenant.Id)
        {
            // Raw SQL deliberately bypasses AppDbContext's cross-tenant reassignment guard —
            // this is a one-time dev-data correction, not a runtime cross-tenant move.
            db.Database.ExecuteSql($"UPDATE Users SET TenantId = {platformTenant.Id} WHERE Id = {superAdmin.Id}");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseMiddleware<BranchContextMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void SeedRightCatalogAndBuiltInGrants(AppDbContext db)
{
    // Retire coarse "*.Manage" codes from a previous pass into their fine-grained replacements
    // before upserting the current catalog, so existing custom-role grants carry over instead of
    // being silently stranded on a right code that no longer exists.
    MigrateLegacyRightCodes(db);

    var existingCodes = db.Rights.Select(r => r.Code).ToHashSet();
    var newRights = RightCatalog.All
        .Where(r => !existingCodes.Contains(r.Code))
        .Select(r => new Right { Code = r.Code, Module = r.Module, Description = r.Description })
        .ToList();

    if (newRights.Count > 0)
    {
        db.Rights.AddRange(newRights);
        db.SaveChanges();
    }

    var adminRole = db.Roles.First(r => r.TenantId == null && r.Name == AppRoles.Admin);
    var allRightIds = db.Rights.Select(r => r.Id).ToList();
    var grantedRightIds = db.RoleRights.Where(rr => rr.RoleId == adminRole.Id).Select(rr => rr.RightId).ToHashSet();

    var missingGrants = allRightIds
        .Where(rightId => !grantedRightIds.Contains(rightId))
        .Select(rightId => new RoleRight { RoleId = adminRole.Id, RightId = rightId })
        .ToList();

    if (missingGrants.Count > 0)
    {
        db.RoleRights.AddRange(missingGrants);
        db.SaveChanges();
    }

    // These 4 voucher actions were open to any authenticated user before rights existed for them.
    // Default-grant them to the built-in User role so existing "User" accounts keep today's
    // capability; only a custom role deliberately built without one of these is newly restricted.
    var userRole = db.Roles.First(r => r.TenantId == null && r.Name == AppRoles.User);
    var userDefaultCodes = new[]
    {
        RightCodes.VouchersCreate,
        RightCodes.VouchersEdit,
        RightCodes.VouchersSubmit,
        RightCodes.VouchersManageAttachments,
    };
    var userDefaultRightIds = db.Rights.Where(r => userDefaultCodes.Contains(r.Code)).Select(r => r.Id).ToList();
    var userGrantedRightIds = db.RoleRights.Where(rr => rr.RoleId == userRole.Id).Select(rr => rr.RightId).ToHashSet();

    var userMissingGrants = userDefaultRightIds
        .Where(rightId => !userGrantedRightIds.Contains(rightId))
        .Select(rightId => new RoleRight { RoleId = userRole.Id, RightId = rightId })
        .ToList();

    if (userMissingGrants.Count > 0)
    {
        db.RoleRights.AddRange(userMissingGrants);
        db.SaveChanges();
    }
}

static void MigrateLegacyRightCodes(AppDbContext db)
{
    var legacyRemap = new Dictionary<string, string[]>
    {
        ["Accounts.Manage"] = [RightCodes.AccountsCreate, RightCodes.AccountsEdit, RightCodes.AccountsDelete],
        ["CostCenters.Manage"] = [RightCodes.CostCentersCreate, RightCodes.CostCentersEdit, RightCodes.CostCentersDelete],
        ["TaxRates.Manage"] = [RightCodes.TaxRatesCreate, RightCodes.TaxRatesEdit],
        ["FiscalPeriods.Manage"] = [RightCodes.FiscalPeriodsCreate, RightCodes.FiscalPeriodsClose, RightCodes.FiscalPeriodsReopen],
        ["Vouchers.Manage"] =
        [
            RightCodes.VouchersCreate, RightCodes.VouchersEdit, RightCodes.VouchersSubmit, RightCodes.VouchersManageAttachments,
            RightCodes.VouchersApprove, RightCodes.VouchersReject, RightCodes.VouchersReverse,
        ],
        ["RecurringVoucherTemplates.Manage"] =
        [
            RightCodes.RecurringVoucherTemplatesCreate, RightCodes.RecurringVoucherTemplatesEdit,
            RightCodes.RecurringVoucherTemplatesGenerateNow,
        ],
        ["Budgets.Manage"] = [RightCodes.BudgetsCreate, RightCodes.BudgetsEdit, RightCodes.BudgetsDelete],
        ["ReportSchedules.Manage"] =
        [
            RightCodes.ReportSchedulesCreate, RightCodes.ReportSchedulesEdit, RightCodes.ReportSchedulesDelete,
            RightCodes.ReportSchedulesRunNow,
        ],
        ["Branches.Manage"] = [RightCodes.BranchesCreate, RightCodes.BranchesEdit],
        ["Tenants.Manage"] = [],
    };

    var legacyRights = db.Rights.Where(r => legacyRemap.Keys.Contains(r.Code)).ToList();
    if (legacyRights.Count == 0)
    {
        return;
    }

    // The upsert step that normally creates new catalog rows hasn't run yet at this point, so
    // make sure every new code the remap needs to grant already exists as a Right row.
    var neededNewCodes = legacyRemap.Values.SelectMany(codes => codes).Distinct().ToList();
    var existingCodes = db.Rights.Select(r => r.Code).ToHashSet();
    var newRightsForRemap = RightCatalog.All
        .Where(r => neededNewCodes.Contains(r.Code) && !existingCodes.Contains(r.Code))
        .Select(r => new Right { Code = r.Code, Module = r.Module, Description = r.Description })
        .ToList();

    if (newRightsForRemap.Count > 0)
    {
        db.Rights.AddRange(newRightsForRemap);
        db.SaveChanges();
    }

    var newCodeToRightId = db.Rights.Where(r => neededNewCodes.Contains(r.Code)).ToDictionary(r => r.Code, r => r.Id);
    var legacyRightIds = legacyRights.Select(r => r.Id).ToHashSet();

    var affectedRoleIds = db.RoleRights
        .Where(rr => legacyRightIds.Contains(rr.RightId))
        .Select(rr => rr.RoleId)
        .Distinct()
        .ToList();

    foreach (var roleId in affectedRoleIds)
    {
        var legacyCodesGrantedToRole = db.RoleRights
            .Where(rr => rr.RoleId == roleId && legacyRightIds.Contains(rr.RightId))
            .Join(db.Rights, rr => rr.RightId, r => r.Id, (rr, r) => r.Code)
            .ToList();

        var alreadyGranted = db.RoleRights.Where(rr => rr.RoleId == roleId).Select(rr => rr.RightId).ToHashSet();

        var newGrants = legacyCodesGrantedToRole
            .SelectMany(code => legacyRemap[code])
            .Distinct()
            .Where(newCode => !alreadyGranted.Contains(newCodeToRightId[newCode]))
            .Select(newCode => new RoleRight { RoleId = roleId, RightId = newCodeToRightId[newCode] })
            .ToList();

        if (newGrants.Count > 0)
        {
            db.RoleRights.AddRange(newGrants);
        }
    }

    db.SaveChanges();

    // Now safe to remove the legacy RoleRight grants, then the orphaned legacy Right rows themselves.
    var legacyRoleRights = db.RoleRights.Where(rr => legacyRightIds.Contains(rr.RightId)).ToList();
    db.RoleRights.RemoveRange(legacyRoleRights);
    db.SaveChanges();

    db.Rights.RemoveRange(legacyRights);
    db.SaveChanges();
}
