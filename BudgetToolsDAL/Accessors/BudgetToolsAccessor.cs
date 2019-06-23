using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using BudgetToolsDAL.Contexts;
using BudgetToolsDAL.Models;

namespace BudgetToolsDAL.Accessors
{

    public interface IBudgetToolsAccessor
    {
        BankAccount GetBankAccount(int bankAccountId);
        List<BankAccount> GetBankAccounts();
        List<T> GetBudgetLineSet<T>(DateTime effectiveDate);
        List<PeriodBudgetLine> GetPeriodBudget(int PeriodId, int BankAccountId);
        List<Period> GetPeriods();
        T GetTransaction<T>(int transactionId);
        List<T> GetTransactions<T>(int bankAccountId, DateTime startDate, DateTime endDate);
        void UpdateBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount);
        void UpdateTransaction<T>(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<T> mappedTransactions);
        void SaveStagedTransactions<T>(int bankAccountId, List<T> stagedTransactions);
    }

    public class BudgetToolsAccessor : IBudgetToolsAccessor
    {

        protected IBudgetToolsDBContext db;

        public BudgetToolsAccessor(IBudgetToolsDBContext budgetToolsDBContext)
        {
            this.db = budgetToolsDBContext;
        }

        public BankAccount GetBankAccount(int bankAccountId)
        {
            return this.db.BankAccounts.FirstOrDefault(a => a.BankAccountId == bankAccountId);
        }

        public List<BankAccount> GetBankAccounts()
        {
            return this.db.BankAccounts.ToList();
        }

        public List<T> GetBudgetLineSet<T>(DateTime effectiveDate)
        {
            var lines = db.BudgetLineSets
                .Where(l => l.EffInDate <= effectiveDate && (l.EffOutDate ?? DateTime.MaxValue) > effectiveDate)
                .ToList();

            return lines.Select(l => Mapper.Map<T>(l)).ToList();
        }

        public List<PeriodBudgetLine> GetPeriodBudget(int PeriodId, int BankAccountId)
        {

            var lines = db.Database.SqlQuery<PeriodBudgetLine>("exec dbo.uspPeriodBudgetGet @PeriodId, @BankAccountId",
                new object[]
                {
                    new SqlParameter("@PeriodID", PeriodId),
                    new SqlParameter("@BankAccountId", BankAccountId)
                });

            return lines.ToList();

        }

        public List<Period> GetPeriods()
        {

            return db.Periods.ToList();

        }

        public T GetTransaction<T>(int transactionId)
        {
            var transaction = db.Transactions
                //.Include(t => t.MappedTransactions)
                .FirstOrDefault(t => t.TransactionId == transactionId);

            return (transaction == null) ? default(T) : Mapper.Map<T>(transaction);
        }

        public List<T> GetTransactions<T>(int bankAccountId, DateTime startDate, DateTime endDate)
        {
            var transactions = db.Transactions
                //.Include(t => t.MappedTransactions)
                .Where(t => t.BankAccountId == bankAccountId
                    && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToList();

            return transactions.Select(t => Mapper.Map<T>(t)).ToList();
        }

        public void UpdateBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount)
        {
            var allocation = this.db.Allocations.Single(a =>
                a.PeriodId == periodId && a.BudgetLineId == budgetLineId && a.BankAccountId == bankAccountId);

            allocation.PlannedAmount = plannedAmount;
            allocation.AllocatedAmount = allocatedAmount;
            allocation.AccruedAmount = accruedAmount;

            this.db.SaveChanges();
        }

        // TODO: change this pattern to accept a partial object and merge it into the existing object
        //       rename the method to MergeTransaction
        public void UpdateTransaction<T>(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<T> mappedTransactions)
        {

            // get the transaction
            var transaction = this.db.Transactions.First(t => t.TransactionId == transactionId);

            // set the transaction fields
            transaction.TransactionTypeCode = transactionTypeCode;
            transaction.Recipient = recipient;
            transaction.Notes = notes;

            // map the incoming data
            var inData = mappedTransactions.Select(m => Mapper.Map<MappedTransaction>(m)).ToList();
            var dbData = transaction.MappedTransactions;

            // get the mapped transaction record counts
            var dbCount = dbData.Count();
            var inCount = inData.Count();
            var recordCount = dbCount > inCount ? dbCount : inCount;

            // merge the incoming mapped transactions into the existing data
            for (var i = 0; i < recordCount; i++)
            {
                if (dbCount > i)
                {
                    var dbRecord = dbData[i];

                    if (inCount > i)
                    {
                        var inRecord = inData[i];
                        dbRecord.Amount = inRecord.Amount;
                        dbRecord.BudgetLineId = inRecord.BudgetLineId;
                    }
                    else
                    {
                        dbData.Remove(dbRecord);
                    }
                }
                else
                {
                    var inRecord = inData[i];
                    dbData.Add(inRecord);
                }
            }

            this.db.SaveChanges();

        }

        //public void SaveMappedTransactions<T>(List<T> mappedTransactions)
        //{
        //    // map the incoming data
        //    var inData = mappedTransactions.Select(m => Mapper.Map<MappedTransaction>(m)).ToList();

        //    // get the transaction id
        //    var id = inData.FirstOrDefault()?.TransactionId;
        //    if (id == null) return;

        //    // get the existing data from the database
        //    var dbData = this.db.MappedTransactions.Where(m => m.TransactionId == id.Value).ToList();

        //    // get record counts
        //    var dbCount = dbData.Count();
        //    var inCount = inData.Count();
        //    var recordCount = dbCount > inCount ? dbCount : inCount;

        //    // merge the new data into the existing data
        //    for(var i = 0; i < recordCount; i++)
        //    {
        //        if (dbCount > i)
        //        {
        //            var dbRecord = dbData[i];

        //            if (inCount > i)
        //            {
        //                var inRecord = inData[i];
        //                dbRecord.Amount = inRecord.Amount;
        //                dbRecord.BudgetLineId = inRecord.BudgetLineId;
        //            }
        //            else
        //            {
        //                this.db.MappedTransactions.Remove(dbRecord);
        //            }
        //        }
        //        else
        //        {
        //            var inRecord = inData[i];
        //            this.db.MappedTransactions.Add(inRecord);
        //        }
        //    }

        //    this.db.SaveChanges();
        //}

        public void SaveStagedTransactions<T>(int bankAccountId, List<T> stagedTransactions)
        {
            var st = stagedTransactions.Select(t => Mapper.Map<StagedTransaction>(t)).ToList();
            this.db.DeleteStagedTransactions(bankAccountId);
            this.db.StageTransactions.AddRange(st);
            this.db.SaveChanges();
            this.db.ImportTransactions(bankAccountId);
        }

    }

}