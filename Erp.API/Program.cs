using Erp.Repository.Data;
using Erp.Repository.Implementation.Account;
using Erp.Repository.Implementation.Master;
using Erp.Repository.Implementation.Production;
using Erp.Repository.Implementation.Purchase;
using Erp.Repository.Implementation.Reports;
using Erp.Repository.Interfaces.Account;
using Erp.Repository.Interfaces.Master;
using Erp.Repository.Interfaces.Production;
using Erp.Repository.Interfaces.Purchase;
using Erp.Repository.Interfaces.Reports;
using Erp.Services.Implementation.Account;
using Erp.Services.Implementation.Master;
using Erp.Services.Implementation.Production;
using Erp.Services.Implementation.Purchase;
using Erp.Services.Implementation.Reports;
using Erp.Services.Interfaces.Account;
using Erp.Services.Interfaces.Master;
using Erp.Services.Interfaces.Production;
using Erp.Services.Interfaces.Purchase;
using Erp.Services.Interfaces.Reports;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PdfSharp.Fonts;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

GlobalFontSettings.FontResolver =
    new PdfFontResolver();

//  ✅ Logging decleration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();
Log.Information("starting server.");

builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration.WriteTo.Console();
    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
});

//  ✅ MSSQL server connection strings
builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//  ✅ PostgreSql server connection strings
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection")));


// ✅ Controllers
builder.Services.AddControllers();


// ✅ Use Swashbuckle (SwaggerGen) only
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Erp Management-api", Version = "v1" });

    // JWT Bearer definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token (Bearer <token>)",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // Apply globally

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
   {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
   });
});

// ✅ JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
});

// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCORS", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// ✅ Services and Repositories Middleware
// Add services to the container.
builder.Services.AddTransient<IAccountService,AccountService>();
#region Masters Services
builder.Services.AddTransient<IUomService, UomService>();
builder.Services.AddTransient<IDepartmentService, DepartmentService>();
builder.Services.AddTransient<IDesignationService, DesignationService>();
builder.Services.AddTransient<IEmployeeService, EmployeeService>();
builder.Services.AddTransient<IPaymentModeService, PaymentModeService>();
builder.Services.AddTransient<IPaymentTermsService, PaymentTermsService>();
builder.Services.AddTransient<ICompanyService, CompanyService>();
builder.Services.AddTransient<IBankDetailsService, BankDetailsService>();
builder.Services.AddTransient<ISalesTypeService, SalesTypeService>();
builder.Services.AddTransient<IProductGroupService, ProductGroupService>();
builder.Services.AddTransient<IChargeService, ChargeService>();
builder.Services.AddTransient<ISupplierService, SupplierService>();
builder.Services.AddTransient<IItemService, ItemService>();
builder.Services.AddTransient<IStyleService, StyleService>();
builder.Services.AddTransient<IColorService, ColorService>();
builder.Services.AddTransient<ISizeService, SizeService>();
#endregion
#region Production Services
builder.Services.AddTransient<IJobReceiptService, JobReceiptService>();
builder.Services.AddTransient<IProductionBatchService, ProductionBatchService>();
builder.Services.AddTransient<IMontlyReportService, MontlyReportService>();
builder.Services.AddTransient<IPurchaseSupplierOutstandingService, PurchaseSupplierOutstandingService>();
builder.Services.AddTransient<IProductionCostService, ProductionCostService>();
builder.Services.AddTransient<IDespatchService, DespatchService>();
builder.Services.AddTransient<IPaymentVoucherService, PaymentVoucherService>();
builder.Services.AddTransient<IBillingService, BillingService>();
builder.Services.AddTransient<IBillingPdfService, BillingPdfService>();
builder.Services.AddTransient<IBillingReceiptService, BillingReceiptService>();
builder.Services.AddTransient<IDashboardService, DashboardService>();
builder.Services.AddTransient<IWagesService, WagesService>();
builder.Services.AddTransient<IReportService, ReportService>();
builder.Services.AddTransient<IQuotationService, QuotationService>();
builder.Services.AddTransient<IJobOrderService, JobOrderService>();
builder.Services.AddTransient<IInwardService, InwardService>();
builder.Services.AddTransient<IOutwardService, OutwardService>();
builder.Services.AddTransient<IInvoiceService, InvoiceService>();
builder.Services.AddTransient<IPaymentService, PaymentService>();
builder.Services.AddTransient<IPurchaseService, PurchaseService>();
builder.Services.AddTransient<IPurchaseReceiptService, PurchaseReceiptService>();
#endregion

// Add Repository to the container.
builder.Services.AddTransient<IAccountRepository, AccountRepository>();
#region Masters Repository
builder.Services.AddTransient<IUomRepository, UomRepository>();
builder.Services.AddTransient<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddTransient<IDesignationRepository, DesignationRepository>();
builder.Services.AddTransient<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddTransient<IPaymentModeRepository, PaymentModeRepository>();
builder.Services.AddTransient<IPaymentTermsRepository, PaymentTermsRepository>();
builder.Services.AddTransient<ICompanyRepository, CompanyRepository>();
builder.Services.AddTransient<IBankDetailsRepository, BankDetailsRepository>();
builder.Services.AddTransient<ISalesTypeRepository, SalesTypeRepository>();
builder.Services.AddTransient<IProductGroupRepository, ProductGroupRepository>();
builder.Services.AddTransient<IChargeRepository, ChargeRepository>();
builder.Services.AddTransient<ISupplierRepository, SupplierRepository>();
builder.Services.AddTransient<IColorRepository, ColorRepository>();
builder.Services.AddTransient<ISizeRepository, SizeRepository>();
builder.Services.AddTransient<IItemRepository, ItemRepository>();
builder.Services.AddTransient<IStyleRepository, StyleRepository>();
#endregion 
#region Production Repository
builder.Services.AddTransient<IJobReceiptRepository, JobReceiptRepository>();
builder.Services.AddTransient<IProductionBatchRepository, ProductionBatchRepository>();
builder.Services.AddTransient<IMontlyReportRepository, MontlyReportRepository>();
builder.Services.AddTransient<IPurchaseSupplierOutstandingRepository, PurchaseSupplierOutstandingRepository>();
builder.Services.AddTransient<IProductionCostRepository, ProductionCostRepository>();
builder.Services.AddTransient<IDespatchRepository, DespatchRepository>();
builder.Services.AddTransient<IPaymentVoucherRepository, PaymentVoucherRepository>();
builder.Services.AddTransient<IBillingRepository, BillingRepository>();
builder.Services.AddTransient<IBillingReceiptRepository, BillingReceiptRepository>();
builder.Services.AddTransient<IDashboardRepository, DashboardRepository>();
builder.Services.AddTransient<IWagesRepository,WagesRepository>();
builder.Services.AddTransient<IReportsRepository, ReportsRepository>();
builder.Services.AddTransient<IQuotationRepository, QuotationRepository>();
builder.Services.AddTransient<IJobOrderRepository, JobOrderRepository>();
builder.Services.AddTransient<IInwardRepository, InwardRepository>();
builder.Services.AddTransient<IOutwardRepository, OutwardRepository>();
builder.Services.AddTransient<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddTransient<IPaymentRepository, PaymentRepository>();
builder.Services.AddTransient<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddTransient<IPurchaseReceiptRepository, PurchaseReceiptRepository>();
#endregion

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//  ✅ Swagger UI
// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//  ✅ Important: enable authentication before authorization
app.UseCors("AllowCORS");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
