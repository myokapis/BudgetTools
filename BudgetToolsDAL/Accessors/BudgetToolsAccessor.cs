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
        List<T> CloseCurrentPeriod<T>();
        BankAccount GetBankAccount(int bankAccountId);
        List<BankAccount> GetBankAccounts();
        List<T> GetBudgetLineBalances<T>(int bankAccountId);
        List<T> GetBudgetLineSet<T>(DateTime effectiveDate);
        List<T> GetPeriodBalances<T>(int periodId);
        List<PeriodBudgetLine> GetPeriodBudget(int PeriodId, int BankAccountId);
        List<Period> GetPeriods();
        T GetTransaction<T>(int transactionId);
        List<T> GetTransactions<T>(int bankAccountId, DateTime startDate, DateTime endDate);
        void SaveStagedTransactions<T>(int bankAccountId, List<T> stagedTransactions);
        List<T> SaveTransfer<T>(int bankAccountFromId, int budgetLineFromId,
            int bankAccountToId, int budgetLineToId, decimal amount, string note);
        void UpdateBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount);
        void UpdateTransaction<T>(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<T> mappedTransactions);
    }

    public class BudgetToolsAccessor : IBudgetToolsAccessor
    {

        protected IBudgetToolsDBContext db;

        public BudgetToolsAccessor(IBudgetToolsDBContext budgetToolsDBContext)
        {
            this.db = budgetToolsDBContext;
        }

        public List<T> CloseCurrentPeriod<T>()
        {
            var messages = db.Database.SqlQuery<Message>("exec dbo.uspUpdatePeriodBalances @ClosePeriod",
                new SqlParameter("@ClosePeriod", true));

            //// TODO: create allocations for new current period
            ////       maybe update balances for the period as well
            //if(messages.All(m => m.ErrorLevel == 0))
            //{
            //    db.Database.ExecuteSqlCommand()
            //}

            return messages.Select(m => Mapper.Map<T>(m)).ToList();
        }

        public BankAccount GetBankAccount(int bankAccountId)
        {
            return this.db.BankAccounts.FirstOrDefault(a => a.BankAccountId == bankAccountId);
        }

        public List<BankAccount> GetBankAccounts()
        {
            return this.db.BankAccounts.ToList();
        }

        public List<T> GetBudgetLineBalances<T>(int bankAccountId)
        {
            // ensure balances are current
            db.Database.ExecuteSqlCommand("dbo.uspUpdatePeriodBalances");

            // query budget line balances
            var budgetLines = db.Database.SqlQuery<BudgetLine>("exec dbo.GetBudgetLinesWithBalances @BankAccountId;",
                new object[] { new SqlParameter("@BankAccountId", bankAccountId) });

            return budgetLines.Select(b => Mapper.Map<T>(b)).ToList();
        }

        public List<T> GetBudgetLineSet<T>(DateTime effectiveDate)
        {
            var lines = db.BudgetLineSets
                .Where(l => l.EffInDate <= effectiveDate && (l.EffOutDate ?? DateTime.MaxValue) > effectiveDate)
                .ToList();

            return lines.Select(l => Mapper.Map<T>(l)).ToList();
        }

        public List<T> GetPeriodBalances<T>(int periodId)
        {
            // ensure balances are up to date
            db.Database.ExecuteSqlCommand("exec dbo.uspUpdatePeriodBalances @PeriodId",
                new SqlParameter("@PeriodId", periodId));

            // get the balances
            var balances = db.PeriodBalances
                .Where(b => b.PeriodId == periodId
                    && (b.PreviousBalance != 0m || b.ProjectedBalance != 0m || b.Balance != 0m))
                .ToList();

            return balances.Select(b => Mapper.Map<T>(new
            {
                b.PeriodId,
                b.BankAccountId,
                b.BudgetLineId,
                b.BankAccountName,
                b.BudgetGroupName,
                b.BudgetCategoryName,
                b.BudgetLineName,
                b.PreviousBalance,
                b.Balance,
                b.ProjectedBalance
            })).ToList();
        }

        // TODO: make this generic with mapping
        public List<PeriodBudgetLine> GetPeriodBudget(int PeriodId, int BankAccountId)
        {

            var lines = db.Database.SqlQuery<PeriodBudgetLine>("exec dbo.uspPeriodBudgetGet @PeriodId, @BankAccountId",
                new SqlParameter("@PeriodID", PeriodId),
                new SqlParameter("@BankAccountId", BankAccountId));

            return lines.ToList();

        }

        public List<Period> GetPeriods()
        {

            return db.Periods.ToList();

        }

        public T GetTransaction<T>(int transactionId)
        {
            var transaction = db.Transactions
                .FirstOrDefault(t => t.TransactionId == transactionId);

            return (transaction == null) ? default(T) : Mapper.Map<T>(transaction);
        }

        public List<T> GetTransactions<T>(int bankAccountId, DateTime startDate, DateTime endDate)
        {
            var transactions = db.Transactions
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
            transaction.IsMapped = true;

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

        public List<T> SaveTransfer<T>(int bankAccountFromId, int budgetLineFromId,
            int bankAccountToId, int budgetLineToId, decimal amount, string note)
        {
            var paramNames = string.Join(", ", new string[]
            {
                "@BankAccountFromID",
                "@BudgetLineFromID",
                "@BankAccountToID",
                "@BudgetLineToID",
                "@Amount",
                "@Note"
            });

            var messages = db.Database.SqlQuery<Message>($"exec dbo.CreateInternalTransfer {paramNames}",
                new SqlParameter("@BankAccountFromID", bankAccountFromId),
                new SqlParameter("@BudgetLineFromID", budgetLineFromId),
                new SqlParameter("@BankAccountToID", bankAccountToId),
                new SqlParameter("@BudgetLineToID", budgetLineToId),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@Note", note));

            return messages.Select(m => Mapper.Map<T>(m)).ToList();
        }

    }

}