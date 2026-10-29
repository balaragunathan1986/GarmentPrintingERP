using Erp.Domain.DTO;
using Erp.Domain.DTO.ProductionAndLogistics;
using Erp.Domain.DTO.ProductionAndLogistics.Purchase;
using Erp.Domain.DTO.Reports;
using Erp.Domain.Model.Account;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Erp.Repository.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        // Map your entity to a database table
        //public DbSet<Product> Products { get; set; }
        public DbSet<UserDTO> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserPermissions> UserPermissions { get; set; }
        public DbSet<ApplicationMenus> ApplicationMenus { get; set; }
        public DbSet<EmailSetup> EmailSetup { get; set; }
        public DbSet<VerifyCode> VerifyCode { get; set; }

        #region Masters
        public DbSet<UomDTO> Uom { get; set; }
        public DbSet<DepartmentDTO> Department { get; set; }
        public DbSet<DesignationDTO> Designation { get; set; }
        public DbSet<EmployeeDTO> Employee { get; set; }
        public DbSet<PaymentModeDTO> PaymentMode { get; set; }
        public DbSet<PaymentTermsDTO> PaymentTerms { get; set; }
        public DbSet<CompanyDTO> Company { get; set; }
        public DbSet<BankDetailsDTO> BankDetails { get; set; }
        public DbSet<SalesTypeDTO> SalesType { get; set; }
        public DbSet<ProductGroupDTO> ProductGroup { get; set; }
        public DbSet<ChargeDTO> Charge { get; set; }
        public DbSet<SupplierDTO> Supplier { get; set; }
        public DbSet<ItemDTO> Item { get; set; }

        public DbSet<ColorDTO> Color { get; set; }
        public DbSet<SizeDTO> Size { get; set; }

        #endregion

        #region Production
        public DbSet<JobReceiptDTO> JobReceipt { get; set; }
        public DbSet<JobReceiptItemDTO> JobReceiptItems { get; set; }
        public DbSet<JobReceiptListDTO> JobReceiptList { get; set; }
        public DbSet<JobReceiptChargeDTO> JobReceiptCharge { get; set; }
        public DbSet<ProductionBatchDTO> ProductionBatch { get; set; }
        public DbSet<ProductionBatchDetailDTO> ProductionBatchDetails { get; set; }
        public DbSet<ProductionBatchListDTO> ProductionBatchList { get; set; }
        public DbSet<ProductionCostDTO> ProductionCost { get; set; }
        public DbSet<ProductionCostChargeDTO> ProductionCostCharges { get; set; }
        public DbSet<ProductionCostScreeningDTO> ProductionCostScreening { get; set; }
        public DbSet<ProductionCostListDTO> ProductionCostList { get; set; }
        public DbSet<DespatchDTO> Despatch { get; set; }
        public DbSet<PaymentVoucherDTO> PaymentVoucher { get; set; }
        public DbSet<BillingMasterDTO> BillingMaster { get; set; }
        public DbSet<BillingDetailDTO> BillingDetails { get; set; }
        public DbSet<BillingReceiptMasterDTO> BillingReceiptMaster { get; set; }
        public DbSet<BillingReceiptDetailDTO> BillingReceiptDetails { get; set; }

        public DbSet<WagesDTO> Wages { get; set; }
        public DbSet<WagesChargesDTO> WagesCharges { get; set; }

        public DbSet<MontlyReportDTO> MontlyReport { get; set; }
        public DbSet<PurchaseSupplierOutstandingDTO> PurchaseSupplierOutstanding { get; set; }
        public DbSet<QuotationMasterDTO> QuotationMaster { get; set; }
        public DbSet<QuotationDetailDTO> QuotationDetails { get; set; }
        public DbSet<JobOrderMasterDTO> JobOrderMaster { get; set; }
        public DbSet<JobOrderDetailDTO> JobOrderDetails { get; set; }

        public DbSet<InwardMasterDTO> InwardMaster { get; set; }
        public DbSet<InwardItemDTO> InwardItems { get; set; }
        public DbSet<InwardStockDTO> InwardStocks { get; set; }
        public DbSet<OutwardDTO> Outward { get; set; }
        public DbSet<OutwardDetailDTO> OutwardDetails { get; set; }
        public DbSet<InvoiceMasterDTO> InvoiceMaster { get; set; }
        public DbSet<InvoiceDetailDTO> InvoiceDetails { get; set; }
        public DbSet<InvoiceChargeDTO> InvoiceCharges { get; set; }
        public DbSet<PaymentMasterDTO> PaymentMaster { get; set; }
        public DbSet<PaymentDetailDTO> PaymentDetails { get; set; }
        public DbSet<PaymentChargeDTO> PaymentCharges { get; set; }
        public DbSet<PurchaseMasterDTO> PurchaseMaster { get; set; }
        public DbSet<PurchaseDetailDTO> PurchaseDetails { get; set; }
        public DbSet<PurchaseChargeDTO> PurchaseCharges { get; set; }
        public DbSet<PurchaseReceiptMasterDTO> PurchaseReceiptMaster { get; set; }
        public DbSet<PurchaseReceiptDetailDTO> PurchaseReceiptDetails { get; set; }
        public DbSet<StockDetailDTO> StockDetails { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserDTO>();
            modelBuilder.Entity<Roles>();
            modelBuilder.Entity<EmailSetup>();
            modelBuilder.Entity<VerifyCode>();

            modelBuilder.Entity<UomDTO>();
            modelBuilder.Entity<DepartmentDTO>();
            modelBuilder.Entity<DesignationDTO>();
            modelBuilder.Entity<EmployeeDTO>();
            modelBuilder.Entity<PaymentModeDTO>();
            modelBuilder.Entity<CompanyDTO>();
            modelBuilder.Entity<BankDetailsDTO>();
            modelBuilder.Entity<SupplierDTO>();
            modelBuilder.Entity<ColorDTO>();
            modelBuilder.Entity<SizeDTO>();
            modelBuilder.Entity<ItemDTO>();
            modelBuilder.Entity<QuotationMasterDTO>();
            modelBuilder.Entity<QuotationDetailDTO>();
            modelBuilder.Entity<JobOrderMasterDTO>();
            modelBuilder.Entity<JobOrderDetailDTO>();
            modelBuilder.Entity<InwardMasterDTO>();
            modelBuilder.Entity<InwardItemDTO>();
            modelBuilder.Entity<InwardStockDTO>();
            modelBuilder.Entity<OutwardDTO>();
            modelBuilder.Entity<OutwardDetailDTO>();
            modelBuilder.Entity<InvoiceMasterDTO>();
            modelBuilder.Entity<InvoiceDetailDTO>();
            modelBuilder.Entity<InvoiceChargeDTO>();
            modelBuilder.Entity<PaymentMasterDTO>();
            modelBuilder.Entity<PaymentDetailDTO>();
            modelBuilder.Entity<PaymentChargeDTO>();
            modelBuilder.Entity<PurchaseMasterDTO>();
            modelBuilder.Entity<PurchaseDetailDTO>();
            modelBuilder.Entity<PurchaseChargeDTO>();
            modelBuilder.Entity<PurchaseReceiptMasterDTO>();
            modelBuilder.Entity<PurchaseReceiptDetailDTO>();
            modelBuilder.Entity<StockDetailDTO>();

            modelBuilder.Entity<ProductionBatchListDTO>().HasNoKey();
            modelBuilder.Entity<JobReceiptListDTO>().HasNoKey();
            // modelBuilder.Entity<ProductionCostListDTO>().HasNoKey();
            modelBuilder.Entity<MontlyReportDTO>()
      .HasNoKey();
                        modelBuilder.Entity<PurchaseSupplierOutstandingDTO>().HasNoKey();
        }
    }
}
