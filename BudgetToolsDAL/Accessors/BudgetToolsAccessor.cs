using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using LazyCache;
using BudgetToolsDAL.Contexts;
using BudgetToolsDAL.Models;
using System.Runtime.CompilerServices;

// TODO: revise to use named parameters

namespace BudgetToolsDAL.Accessors
{

    public interface IBudgetToolsAccessor
    {
        Task<(bool IsSuccess, List<T> Messages)> CloseCurrentPeriod<T>();

        Task<int> CreateAllocations(int periodId);

        Task<BankAccount> GetBankAccount(int bankAccountId);

        Task<List<BankAccount>> GetBankAccounts();

        Task<List<T>> GetBudgetLineBalances<T>(int periodId, int bankAccountId);

        Task<List<T>> GetBudgetLineSet<T>(DateTime effectiveDate, bool fromCache = false);

        Task<T> GetCurrentPeriod<T>();

        Task<int> GetCurrentPeriodId();

        Task<List<T>> GetPeriodBalances<T>(int periodId);

        Task<List<T>> GetPeriodBudget<T>(int PeriodId, int BankAccountId);

        Task<List<Period>> GetPeriods();

        Task<T> GetPreviousPeriod<T>();

        Task<T> GetTransaction<T>(int transactionId);

        Task<List<T>> GetTransactions<T>(int bankAccountId, DateTime startDate, DateTime endDate);

        Task SaveStagedTransactions<T>(int bankAccountId, IEnumerable<T> stagedTransactions, bool isSortDesc=true);

        Task<(bool IsSuccess, List<T> Messages)> SaveTransfer<T>(int bankAccountId, int budgetLineFromId, int budgetLineToId,
            decimal amount, string note);

        Task<int> UpdateBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount);

        Task<int> UpdateTransaction<T>(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<T> mappedTransactions);
    }

    public class BudgetToolsAccessor : IBudgetToolsAccessor
    {
        private readonly IAppCache cache;
        private readonly BudgetToolsDBContext db;
        private readonly IMapper mapper;

        public BudgetToolsAccessor(BudgetToolsDBContext budgetToolsDBContext, IAppCache cache, IMapper mapper)
        {
            db = budgetToolsDBContext;
            this.mapper = mapper;
            this.cache = cache;
        }

        public async Task<(bool IsSuccess, List<T> Messages)> CloseCurrentPeriod<T>()
        {
            var period = await GetCurrentPeriod<Period>();

            // ensure balances are up to date
            var result = await UpdatePeriodBalances(period.PeriodId);

            // bail out if processing failed
            if (!result.IsSuccess) return (false, result.Messages.Select(m => mapper.Map<T>(m)).ToList());

            // attempt to close the period
            var returnValue = ReturnValue;

            var messages = await db.Set<Message>()
                .FromSqlRaw("exec @ReturnValue = dbo.CloseCurrentPeriod", returnValue)
                .AsNoTracking()
                .Select(m => mapper.Map<T>(m))
                .ToListAsync();

            return (returnValue.Value.Equals(0), messages);
        }

        public async Task<int> CreateAllocations(int periodId)
        {
            return await db.Database.ExecuteSqlRawAsync("exec dbo.uspCreateAllocations @PeriodId",
                new SqlParameter("@PeriodId", periodId));
        }

        public async Task<BankAccount> GetBankAccount(int bankAccountId)
        {
            return await db.BankAccounts.FirstOrDefaultAsync(a => a.BankAccountId == bankAccountId);
        }

        public async Task<List<BankAccount>> GetBankAccounts()
        {
            return await db.BankAccounts.ToListAsync();
        }

        public async Task<List<T>> GetBudgetLineBalances<T>(int periodId, int bankAccountId)
        {
            // ensure balances are current
            await UpdatePeriodBalances(periodId, true);

            // query budget line balances
            var budgetLines = await db.BudgetLines.FromSqlRaw("exec dbo.GetBudgetLinesWithBalances @BankAccountId;",
                new SqlParameter("@BankAccountId", bankAccountId)).ToListAsync();

            return budgetLines.Select(b => mapper.Map<T>(b)).ToList();
        }

        public async Task<List<T>> GetBudgetLineSet<T>(DateTime effectiveDate, bool fromCache = false)
        {
            Func<Task<List<BudgetLineSet>>> dataFunc = () => db.BudgetLineSets
                .Where(l => l.EffInDate <= effectiveDate && (l.EffOutDate ?? DateTime.MaxValue) > effectiveDate)
                .ToListAsync();

            var lines = fromCache ? await cache.GetOrAddAsync("BudgetLineDefinitions", dataFunc) : await dataFunc();

            return lines.OrderBy(l => l.BudgetLineName).Select(l => mapper.Map<T>(l)).ToList();
        }

        public async Task<T> GetCurrentPeriod<T>()
        {
            var period = await db.Periods
                .Where(p => p.IsOpen == true)
                .OrderBy(o => o.PeriodId)
                .FirstAsync();

            return mapper.Map<T>(period);
        }

        public async Task<int> GetCurrentPeriodId()
        {
            var period = await GetCurrentPeriod<Period>();
            return period.PeriodId;
        }

        public async Task<List<T>> GetPeriodBalances<T>(int periodId)
        {
            // ensure allocations are up to date
            await CreateAllocations(periodId);

            // ensure balances are up to date
            await UpdatePeriodBalances(periodId, true);

            // get the balances
            var balances = await db.PeriodBalances
                .Where(b => b.PeriodId == periodId
                    && (b.PreviousBalance != 0m || b.ProjectedBalance != 0m || b.Balance != 0m))
                .ToListAsync();

            return balances.Select(b => mapper.Map<T>(b)).ToList();
        }

        public async Task<List<T>> GetPeriodBudget<T>(int PeriodId, int BankAccountId)
        {

            var lines = await db.Set<PeriodBudgetLine>()
                .FromSqlRaw("exec dbo.uspPeriodBudgetGet @PeriodId, @BankAccountId",
                    new SqlParameter("@PeriodID", PeriodId),
                    new SqlParameter("@BankAccountId", BankAccountId))
                .ToListAsync();

            return lines.Select(l => mapper.Map<T>(l)).ToList();

        }

        public async Task<List<Period>> GetPeriods()
        {
            return await db.Periods.ToListAsync();
        }

        public async Task<T> GetPreviousPeriod<T>()
        {
            var period = await db.Periods
            .Where(p => p.IsOpen == false)
            .OrderByDescending(o => o.PeriodId)
            .FirstAsync();

            return mapper.Map<T>(period);
        }

        public async Task<T> GetTransaction<T>(int transactionId)
        {
            var transaction = await db.Transactions
                .Include(t => t.MappedTransactions)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

            return (transaction == null) ? default(T) : mapper.Map<T>(transaction);
        }

        public async Task<List<T>> GetTransactions<T>(int bankAccountId, DateTime startDate, DateTime endDate)
        {
            var transactions = await db.Transactions
                .Where(t => t.BankAccountId == bankAccountId
                    && t.TransactionDate >= startDate
                    && t.TransactionDate <= endDate
                    && t.TransactionTypeCode != "I")
                .ToListAsync();

            return transactions.Select(t => mapper.Map<T>(t)).ToList();
        }

        private SqlParameter ReturnValue => new SqlParameter("@ReturnValue", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        public async Task SaveStagedTransactions<T>(int bankAccountId, IEnumerable<T> stagedTransactions, bool isSortDesc = true)
        {
            var st = (stagedTransactions is IEnumerable<StagedTransaction>) ?
                stagedTransactions as IEnumerable<StagedTransaction> :
                stagedTransactions.Select(t => mapper.Map<StagedTransaction>(t)).ToList();

            await DeleteStagedTransactions(bankAccountId);
            db.StagedTransactions.AddRange(st);
            await db.SaveChangesAsync();
            await ImportTransactions(bankAccountId, isSortDesc);
        }

        public async Task<(bool IsSuccess, List<T> Messages)> SaveTransfer<T>(int bankAccountId, int budgetLineFromId, int budgetLineToId,
            decimal amount, string note)
        {
            var returnValue = ReturnValue;

            //var paramNames = string.Join(", ", new string[]
            //{
            //    "@BankAccountID",
            //    "@BudgetLineFromID",
            //    "@BudgetLineToID",
            //    "@Amount",
            //    "@Note"
            //});

            //var messages = await db.Set<Message>()
            //    .FromSqlProc("dbo.CreateInternalTransfer", returnValue,
            //        new SqlParameter("@BankAccountID", bankAccountId),
            //        new SqlParameter("@BudgetLineFromID", budgetLineFromId),
            //        new SqlParameter("@BudgetLineToID", budgetLineToId),
            //        new SqlParameter("@Amount", amount),
            //        new SqlParameter("@Note", note))
            //    .ToListAsync();

            try
            {

                // TODO: reinstate @Note parameter
                var messages = await db.Set<Message>()
                    .FromSqlRaw("exec @ReturnValue = dbo.CreateInternalTransfer @BankAccountID, @BudgetLineFromID, @BudgetLineToID, @Amount",
                        returnValue,
                        new SqlParameter("@BankAccountID", bankAccountId),
                        new SqlParameter("@BudgetLineFromID", budgetLineFromId),
                        new SqlParameter("@BudgetLineToID", budgetLineToId),
                        new SqlParameter("@Amount", amount)
                        //new SqlParameter("@Note", note)
                        )
                    .ToListAsync();

                return (0.Equals(returnValue.Value), messages.Select(m => mapper.Map<T>(m)).ToList());
            }
            catch(Exception ex)
            {
                return (false, null);
            }

        }

        public async Task<int> UpdateBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount)
        {
            var allocation = await db.Allocations.SingleAsync(a =>
                a.PeriodId == periodId && a.BudgetLineId == budgetLineId && a.BankAccountId == bankAccountId);

            allocation.PlannedAmount = plannedAmount;
            allocation.AllocatedAmount = allocatedAmount;
            allocation.AccruedAmount = accruedAmount;

            return await db.SaveChangesAsync();
        }

        private async Task<(bool IsSuccess, List<Message> Messages)> UpdatePeriodBalances(int periodId, bool skipValidations = false)
        {
            var returnValue = ReturnValue;

            // ensure balances are up to date
            var messages = await db.Set<Message>()
                .FromSqlRaw("exec @ReturnValue = dbo.UpdatePeriodBalances @PeriodId",
                    new SqlParameter("@PeriodId", periodId),
                    returnValue)
                .ToListAsync();

            return (returnValue.Value.Equals(0), messages);
        }

        // TODO: change this pattern to accept a partial object and merge it into the existing object
        //       rename the method to MergeTransaction
        public async Task<int> UpdateTransaction<T>(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<T> mappedTransactions)
        {

            // get the transaction
            var transaction = await db.Transactions
                .Include(t => t.MappedTransactions)
                .FirstAsync(t => t.TransactionId == transactionId);

            // set the transaction fields
            transaction.TransactionTypeCode = transactionTypeCode;
            transaction.Recipient = recipient;
            transaction.Notes = notes;
            transaction.IsMapped = true;

            // map the incoming data
            var inData = mappedTransactions.Select(m => mapper.Map<MappedTransaction>(m)).ToList();
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

            return await db.SaveChangesAsync();

        }

        private async Task DeleteStagedTransactions(int bankAccountId)
        {
            await db.Database.ExecuteSqlRawAsync("delete dbo.StagedTransactions where BankAccountId = @BankAccountId;",
                new SqlParameter("@BankAccountId", bankAccountId));
        }

        private async Task ImportTransactions(int bankAccountId, bool isSortDesc = true)
        {
            await db.Database.ExecuteSqlRawAsync("dbo.uspImportTransactions @BankAccountId", 
                new SqlParameter("@BankAccountId", bankAccountId),
                new SqlParameter("@IsSortDesc", isSortDesc));
        }

        //private async Task<IEnumerable<T>> LookupBudgetLineDefinitions<T>()
        //{
        //    db.BudgetLines
        //}

        //private async Task UpdatePeriodBalances(int PeriodId, bool ClosePeriod)
        //{
        //    await db.Database.ExecuteSqlRawAsync("dbo.uspUpdatePeriodBalances @PeriodID, @ClosePeriod",
        //        new SqlParameter("@PeriodID", PeriodId),
        //        new SqlParameter("@ClosePeriod", ClosePeriod));
        //}

    }

}