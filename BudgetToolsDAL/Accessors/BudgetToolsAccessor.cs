using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using AutoMapper;
using BudgetToolsDAL.Contexts;
using BudgetToolsDAL.Models;

namespace BudgetToolsDAL.Accessors
{

    public interface IBudgetToolsAccessor
    {
        List<T> CloseCurrentPeriod<T>(out bool isSuccess);

        void CreateAllocations(int periodId);

        BankAccount GetBankAccount(int bankAccountId);

        List<BankAccount> GetBankAccounts();

        List<T> GetBudgetLineBalances<T>(int periodId, int bankAccountId);

        List<T> GetBudgetLineSet<T>(DateTime effectiveDate);

        int GetCurrentPeriodId();

        List<T> GetPeriodBalances<T>(int periodId);

        List<T> GetPeriodBudget<T>(int PeriodId, int BankAccountId);

        List<Period> GetPeriods();

        T GetTransaction<T>(int transactionId);

        List<T> GetTransactions<T>(int bankAccountId, DateTime startDate, DateTime endDate);

        void SaveStagedTransactions<T>(int bankAccountId, IEnumerable<T> stagedTransactions, bool isSortDesc=true);

        List<T> SaveTransfer<T>(int bankAccountId, int budgetLineFromId, int budgetLineToId,
            decimal amount, string note, out bool isSuccess);

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
            db = budgetToolsDBContext;
        }

        public List<T> CloseCurrentPeriod<T>(out bool isSuccess)
        {
            var periodId = GetCurrentPeriodId();

            // ensure balances are up to date
            var messages = UpdatePeriodBalances(periodId, out isSuccess)
                .Select(m => Mapper.Map<T>(m)).ToList();

            // bail out if processing failed
            if (!isSuccess) return messages;

            // attempt to close the period
            var returnValue = ReturnValue;
            messages = db.Database.SqlQuery<Message>("exec @ReturnValue = dbo.CloseCurrentPeriod", returnValue)
                .Select(m => Mapper.Map<T>(m))
                .ToList();

            isSuccess = returnValue.Value.Equals(0);
            return messages;
        }

        public void CreateAllocations(int periodId)
        {
            db.Database.ExecuteSqlCommand("exec dbo.uspCreateAllocations @PeriodId",
                new SqlParameter("@PeriodId", periodId));
        }

        public BankAccount GetBankAccount(int bankAccountId)
        {
            return db.BankAccounts.FirstOrDefault(a => a.BankAccountId == bankAccountId);
        }

        public List<BankAccount> GetBankAccounts()
        {
            return db.BankAccounts.ToList();
        }

        public List<T> GetBudgetLineBalances<T>(int periodId, int bankAccountId)
        {
            bool isSuccess = false;

            // ensure balances are current
            UpdatePeriodBalances(periodId, out isSuccess, true);

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

        public int GetCurrentPeriodId()
        {
            return db.Periods
                .Where(p => p.IsOpen)
                .OrderBy(p => p.PeriodId)
                .First().PeriodId;
        }

        public List<T> GetPeriodBalances<T>(int periodId)
        {
            bool isSuccess = false;

            // ensure allocations are up to date
            CreateAllocations(periodId);

            // ensure balances are up to date
            UpdatePeriodBalances(periodId, out isSuccess, true);

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

        public List<T> GetPeriodBudget<T>(int PeriodId, int BankAccountId)
        {

            var lines = db.Database.SqlQuery<PeriodBudgetLine>("exec dbo.uspPeriodBudgetGet @PeriodId, @BankAccountId",
                new SqlParameter("@PeriodID", PeriodId),
                new SqlParameter("@BankAccountId", BankAccountId));

            return lines.Select(l => Mapper.Map<T>(l)).ToList();

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
                    && t.TransactionDate >= startDate
                    && t.TransactionDate <= endDate
                    && t.TransactionTypeCode != "I")
                .ToList();

            return transactions.Select(t => Mapper.Map<T>(t)).ToList();
        }

        private SqlParameter ReturnValue => new SqlParameter("@ReturnValue", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        public void SaveStagedTransactions<T>(int bankAccountId, IEnumerable<T> stagedTransactions, bool isSortDesc = true)
        {
            var st = (stagedTransactions is IEnumerable<StagedTransaction>) ?
                stagedTransactions as IEnumerable<StagedTransaction> :
                stagedTransactions.Select(t => Mapper.Map<StagedTransaction>(t)).ToList();

            db.DeleteStagedTransactions(bankAccountId);
            db.StageTransactions.AddRange(st);
            db.SaveChanges();
            db.ImportTransactions(bankAccountId, isSortDesc);
        }

        public List<T> SaveTransfer<T>(int bankAccountId, int budgetLineFromId, int budgetLineToId,
            decimal amount, string note, out bool isSuccess)
        {
            var returnValue = ReturnValue;

            var paramNames = string.Join(", ", new string[]
            {
                "@BankAccountID",
                "@BudgetLineFromID",
                "@BudgetLineToID",
                "@Amount",
                "@Note"
            });

            var messages = db.Database.SqlQuery<Message>($"exec @ReturnValue = dbo.CreateInternalTransfer {paramNames}",
                new SqlParameter("@BankAccountID", bankAccountId),
                new SqlParameter("@BudgetLineFromID", budgetLineFromId),
                new SqlParameter("@BudgetLineToID", budgetLineToId),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@Note", note),
                returnValue).ToList();

            isSuccess = returnValue.Value.Equals(0);

            return messages.Select(m => Mapper.Map<T>(m)).ToList();
        }

        public void UpdateBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount)
        {
            var allocation = db.Allocations.Single(a =>
                a.PeriodId == periodId && a.BudgetLineId == budgetLineId && a.BankAccountId == bankAccountId);

            allocation.PlannedAmount = plannedAmount;
            allocation.AllocatedAmount = allocatedAmount;
            allocation.AccruedAmount = accruedAmount;

            db.SaveChanges();
        }

        private List<Message> UpdatePeriodBalances(int periodId, out bool isSuccess, bool skipValidations = false)
        {
            var returnValue = ReturnValue;

            // ensure balances are up to date
            var messages = db.Database.SqlQuery<Message>("exec @ReturnValue = dbo.UpdatePeriodBalances @PeriodId",
                new SqlParameter("@PeriodId", periodId),
                returnValue).ToList();

            isSuccess = returnValue.Value.Equals(0);

            return messages;
        }

        // TODO: change this pattern to accept a partial object and merge it into the existing object
        //       rename the method to MergeTransaction
        public void UpdateTransaction<T>(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<T> mappedTransactions)
        {

            // get the transaction
            var transaction = db.Transactions.First(t => t.TransactionId == transactionId);

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

            db.SaveChanges();

        }

    }

}