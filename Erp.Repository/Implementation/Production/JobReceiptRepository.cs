using Erp.Repository.Data;
using Erp.Repository.Interfaces.Production;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Erp.Utility;
using Erp.Domain.DTO.ProductionAndLogistics;

namespace Erp.Repository.Implementation.Production
{
    public class JobReceiptRepository : IJobReceiptRepository
    {
        private readonly AppDBContext _dbContext;
        public JobReceiptRepository(AppDBContext dBContext) {
            _dbContext = dBContext;
        }

        public async Task<string> GenerateNumber()
        {
            var JobReceiptNumber = await _dbContext.JobReceipt.OrderByDescending(x=>x.Job_Receipt_Number).Select(itm => itm.Job_Receipt_Number).FirstOrDefaultAsync();
            return Utility.CommonFunctions.GenerateDocumentNumber(Utility.Costants.JobReceipt, Utility.CommonFunctions.GetTransactionNumber(JobReceiptNumber));
        }

        public async Task<List<JobReceiptDTO>> GetJobReceipt()
        {
            return await (
                from jobReceipt in _dbContext.JobReceipt
                join supplier in _dbContext.Supplier
                    on jobReceipt.Supplier_Id equals supplier.Supplier_Id into supplierGroup
                from supplier in supplierGroup.DefaultIfEmpty()
                orderby jobReceipt.Id descending
                select new JobReceiptDTO
                {
                    Id = jobReceipt.Id,
                    Job_Receipt_Id = jobReceipt.Job_Receipt_Id,
                    Job_Receipt_Number = jobReceipt.Job_Receipt_Number,
                    Job_Receipt_Date = jobReceipt.Job_Receipt_Date,

                    Supplier_Id = jobReceipt.Supplier_Id,
                    Supplier = supplier != null
                        ? supplier.Supplier_Name
                        : null,

                    Vehicle_Number = jobReceipt.Vehicle_Number,
                    Driver_Phone = jobReceipt.Driver_Phone,
                    Driver_Name = jobReceipt.Driver_Name,

                    First_Weight = jobReceipt.First_Weight,
                    Second_Weight = jobReceipt.Second_Weight,
                    Net_Weight = jobReceipt.Net_Weight,
                    Total_Quantity = jobReceipt.Total_Quantity,
                    Total_Amount = jobReceipt.Total_Amount,
                    Payment_Status = jobReceipt.Payment_Status
                }
            ).ToListAsync();
        }

        public async Task<JobReceiptDTO> GetJobReceipt(Guid Job_Rceipt_Id)
        {
            var jobReceipt =  await _dbContext.JobReceipt.Where(item=>item.Job_Receipt_Id == Job_Rceipt_Id).FirstOrDefaultAsync();
            return jobReceipt != null ? jobReceipt : new JobReceiptDTO();
        }

        public async  Task<JobReceiptDTO> Save(JobReceiptDTO JobReceipt)
        {
            var validateNumber = await GenerateNumber();
            if (validateNumber != null)
            {
                JobReceipt.Job_Receipt_Number = validateNumber == JobReceipt.Job_Receipt_Number ? JobReceipt.Job_Receipt_Number : validateNumber;
            }

            await _dbContext.JobReceipt.AddAsync(JobReceipt);
            await _dbContext.SaveChangesAsync();
            return JobReceipt;
        }
        public async Task<JobReceiptDTO> Update(JobReceiptDTO JobReceipt)
        {
           _dbContext.JobReceipt.Update(JobReceipt);
            await _dbContext.SaveChangesAsync();
            return JobReceipt;
        }

        public async Task<bool> Delete(Guid Job_Receipt_Id)
        {
            var deleteItem = await _dbContext.JobReceipt.Where(x => x.Job_Receipt_Id == Job_Receipt_Id).FirstOrDefaultAsync();
            if (deleteItem != null) _dbContext.JobReceipt.Remove(deleteItem);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<JobReceiptItemDTO>> GetJobReceiptItems()
        {
            return await _dbContext.JobReceiptItems.ToListAsync();
        }

        public async Task<List<JobReceiptItemDTO>> GetJobReceiptItems(Guid Job_Receipt_Id)
        {
            return await _dbContext.JobReceiptItems.Where(x => x.Job_Receipt_Id == Job_Receipt_Id).ToListAsync();
        }
        public async Task<bool> Save(List<JobReceiptItemDTO> JobReceiptItems)
        {
            await _dbContext.JobReceiptItems.AddRangeAsync(JobReceiptItems);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<JobReceiptItemDTO> Save(JobReceiptItemDTO JobReceiptItems)
        {
            await _dbContext.JobReceiptItems.AddAsync(JobReceiptItems);
            await _dbContext.SaveChangesAsync();
            return JobReceiptItems;
        }

        public async Task<bool> Update(List<JobReceiptItemDTO> JobReceiptItems)
        {
             _dbContext.JobReceiptItems.UpdateRange(JobReceiptItems);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<JobReceiptItemDTO> Update(JobReceiptItemDTO JobReceiptItems)
        {
            _dbContext.JobReceiptItems.Update(JobReceiptItems);
            await _dbContext.SaveChangesAsync();
            return JobReceiptItems;
        }
        public async Task<bool> DeleteItems(Guid Job_Receipt_Id)
        {
            var deletedItems = await _dbContext.JobReceiptItems.Where(item => item.Job_Receipt_Id == Job_Receipt_Id).ToListAsync();
            _dbContext.JobReceiptItems.RemoveRange(deletedItems);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteJobReceiptItem(Guid Job_Receipt_Item_Id)
        {
            var deletedItems = await _dbContext.JobReceiptItems.Where(item => item.Job_Receipt_Item_Id == Job_Receipt_Item_Id).ToListAsync();
            _dbContext.JobReceiptItems.RemoveRange(deletedItems);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<JobReceiptChargeDTO>> GetCharges(Guid Job_Receipt_Id)
        {
            return await _dbContext.JobReceiptCharge.Where((item) => item.Job_Receipt_Id == Job_Receipt_Id).ToListAsync();
        }
        public async Task<List<JobReceiptChargeDTO>> SaveCharges(List<JobReceiptChargeDTO> JobReceiptCharges)
        {
            await _dbContext.JobReceiptCharge.AddRangeAsync(JobReceiptCharges);
            await _dbContext.SaveChangesAsync();
            return JobReceiptCharges;
        }

        public async Task<List<JobReceiptChargeDTO>> UpdateCharges(List<JobReceiptChargeDTO> JobReceiptCharges)
        { 
            _dbContext.JobReceiptCharge.UpdateRange(JobReceiptCharges);
            await _dbContext.SaveChangesAsync();
            return JobReceiptCharges;
        }

        public async Task<bool> DeleteCharges(Guid Job_Receipt_Id)
        {
            var deletedItems = await _dbContext.JobReceiptCharge.Where((item) => item.Job_Receipt_Id == Job_Receipt_Id).ToListAsync();
            if (deletedItems.Any())
            {
                _dbContext.JobReceiptCharge.RemoveRange(deletedItems);
                await _dbContext.SaveChangesAsync();
            }
            return true;
        }


        public async Task<JobReceiptAdvancePaymentDTO> GetAdvancePayment(Guid supplierId)
        {
            // Total Payment Voucher amount for the supplier
            var paymentVoucherAmount = await (
                from pv in _dbContext.PaymentVoucher
                join supplier in _dbContext.Supplier
                    on pv.Supplier_Id equals supplier.Id
                where supplier.Supplier_Id == supplierId
                select pv.Total_Amount 
            ).SumAsync();

            // Total Job Receipt amount for the supplier
            var jobReceiptAmount = await (
                from item in _dbContext.JobReceiptItems
                join jobReceipt in _dbContext.JobReceipt
                    on item.Job_Receipt_Id equals jobReceipt.Job_Receipt_Id
                join supplier in _dbContext.Supplier
                    on jobReceipt.Supplier_Id equals supplier.Supplier_Id
                where supplier.Supplier_Id == supplierId
                select item.Amount ?? 0m
            ).SumAsync();

            // Advance Payment = Payment Voucher - Job Receipt
            var advancePayment = paymentVoucherAmount - jobReceiptAmount;

            return new JobReceiptAdvancePaymentDTO
            {
                Total_Amount = advancePayment
            };
        }

    }
}
