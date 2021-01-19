using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using LazyCache;
using BudgetToolsDAL.Accessors;
using BudgetToolsBLL.Models;

namespace BudgetToolsBLL.Services
{

    public interface IBudgetService
    {
        Task<(TScope PageScope, List<TMessage> Messages)> CloseCurrentPeriod<TScope, TMessage>(TScope pageScope);
        Task<List<T>> GetBankAccounts<T>(bool activeOnly = false);
        Task<List<T>> GetBudgetLineBalances<T>(int periodId, int bankAccountId);
        Task<List<T>> GetBudgetLineSet<T>(DateTime? effectiveDate = null);
        Task<T> GetPageScope<T>();
        Task<List<T>> GetPeriodBudget<T>(int BankAccountId, int PeriodId);
        Task<List<T>> GetPeriodBalancesWithSummary<T>(int periodId);
        Task<List<T2>> GetPeriodBudgetWithSummary<T1, T2>(T1 scope);
        Task<List<T>> GetPeriods<T>(bool openOnly = false);
        Task<T> GetTransaction<T>(int transactionId);
        Task<List<T>> GetTransactions<T>(int bankAccountId, int periodId);
        Task ImportFile(int bankAccountId, Stream stream);

        Task<int> SaveBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount);

        Task<(bool IsSuccess, List<T> Messages)> SaveTransfer<T>(int bankAccountId, int budgetLineFromId,
            int budgetLineToId, decimal amount, string note);
        Task<int> UpdateTransaction<T>(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<T> mappedTransactions);
    }

    public class BudgetService : IBudgetService
    {
        private readonly IBudgetToolsAccessor budgetToolsAccessor;
        private readonly IAppCache cache;
        private readonly IImportService importService;
        private readonly IMapper mapper;

        public BudgetService(IBudgetToolsAccessor budgetToolsAccessor, IImportService importService,
            IMapper mapper, IAppCache cache)
        {
            this.budgetToolsAccessor = budgetToolsAccessor;
            this.importService = importService;
            this.mapper = mapper;
            this.cache = cache;
        }

        public async Task<(TScope PageScope, List<TMessage> Messages)> CloseCurrentPeriod<TScope, TMessage>(TScope pageScope)
        {
            var dataScope = mapper.Map<DataScope>(pageScope);
            var result = await budgetToolsAccessor.CloseCurrentPeriod<TMessage>();

            if(result.IsSuccess)
            {
                var currentScope = await GetPageScope<DataScope>();
                dataScope.CurrentPeriodId = currentScope.CurrentPeriodId;

                if (currentScope.PeriodId > dataScope.PeriodId) dataScope.PeriodId = currentScope.PeriodId;

                await budgetToolsAccessor.CreateAllocations(currentScope.CurrentPeriodId);
            }

            return (mapper.Map<TScope>(dataScope), result.Messages);
        }

        public async Task<List<T>> GetBankAccounts<T>(bool activeOnly = false)
        {
            var bankAccounts = await budgetToolsAccessor.GetBankAccounts();
            
            return bankAccounts.Where(a => !activeOnly || a.IsActive)
                .Select(a => mapper.Map<T>(a)).ToList();
        }

        public async Task<List<T>> GetBudgetLineBalances<T>(int periodId, int bankAccountId)
        {
            return await budgetToolsAccessor.GetBudgetLineBalances<T>(periodId, bankAccountId);
        }

        public async Task<List<T>> GetBudgetLineSet<T>(DateTime? effectiveDate = null)
        {
            return await budgetToolsAccessor.GetBudgetLineSet<T>(effectiveDate ?? DateTime.Now);
        }

        public async Task<List<T>> GetCachedBudgetLineSet<T>(bool reloadCache = false)
        {
            return await budgetToolsAccessor.GetBudgetLineSet<T>(DateTime.Now);
        }

        public async Task<T> GetPageScope<T>()
        {
            var bankAccounts = await budgetToolsAccessor.GetBankAccounts();
            var currentPeriodId = await budgetToolsAccessor.GetCurrentPeriodId();

            var dataScope = new DataScope
            {
                BankAccountId = bankAccounts.First().BankAccountId,
                CurrentPeriodId = currentPeriodId,
                PeriodId = currentPeriodId
            };

            return mapper.Map<T>(dataScope);
        }

        public async Task<List<T>> GetPeriodBalancesWithSummary<T>(int periodId)
        {
            var balances = await budgetToolsAccessor.GetPeriodBalances<PeriodBalance>(periodId);

            var accountTotals = balances.GroupBy(b => b.BankAccountId)
                .Select(k => new PeriodBalance
                {
                    PeriodId = k.First().PeriodId,
                    BankAccountId = k.Key,
                    BankAccountName = k.First().BankAccountName,
                    BudgetLineId = 0,
                    BudgetLineName = "",
                    BudgetCategoryName = k.First().BudgetCategoryName,
                    PreviousBalance = k.Sum(i => i.PreviousBalance),
                    Balance = k.Sum(i => i.Balance),
                    ProjectedBalance = k.Sum(i => i.ProjectedBalance),
                    Level = 1
                });

            var grandTotal = accountTotals.GroupBy(k => 0)
                .Select(k => new PeriodBalance
                {
                    PeriodId = k.First().PeriodId,
                    BankAccountId = 0,
                    BankAccountName = "All Accounts",
                    BudgetLineId = 0,
                    BudgetLineName = "",
                    BudgetCategoryName = k.First().BudgetCategoryName,
                    PreviousBalance = k.Sum(i => i.PreviousBalance),
                    Balance = k.Sum(i => i.Balance),
                    ProjectedBalance = k.Sum(i => i.ProjectedBalance),
                    Level = 0
                });

            return balances.Union(accountTotals).Union(grandTotal)
                .Select(b => mapper.Map<T>(b)).ToList();
        }

        public async Task<List<T>> GetPeriodBudget<T>(int bankAccountId, int periodId)
        {

            // get the planned and allocated amounts for each budget line
            var budget = await budgetToolsAccessor.GetPeriodBudget<T>(periodId, bankAccountId);

            return budget.Select(d => mapper.Map<T>(d)).ToList();

        }

        public async Task<List<T2>> GetPeriodBudgetWithSummary<T1, T2>(T1 scope)
        {
            var dataScope = mapper.Map<DataScope>(scope);

            if (dataScope.PeriodId > dataScope.CurrentPeriodId)
                await budgetToolsAccessor.CreateAllocations(dataScope.PeriodId);

            var lines = (await budgetToolsAccessor.GetPeriodBudget<PeriodBudgetLine>(dataScope.PeriodId, dataScope.BankAccountId));

            var summaryLines = lines.GroupBy(l => l.BudgetCategoryName)
                .Select(g => new PeriodBudgetLine
                {
                    BudgetLineId = -1,
                    BudgetLineName = g.Key,
                    BudgetCategoryName = g.Key,
                    PlannedAmount = g.Sum(l => l.PlannedAmount),
                    AllocatedAmount = g.Sum(l => l.AllocatedAmount),
                    AccruedAmount = g.Sum(l => l.AccruedAmount),
                    ActualAmount = g.Sum(l => l.ActualAmount),
                    RemainingAmount = g.Sum(l => l.RemainingAmount),
                    AccruedBalance = g.Sum(l => l.AccruedBalance),
                    TotalCashAmount = g.Sum(l => l.TotalCashAmount),
                    IsAccrued = false,
                    IsDetail = false,
                });

            lines.AddRange(summaryLines);

            return lines.Select(l => mapper.Map<T2>(l)).ToList();
        }

        public async Task<List<T>> GetPeriods<T>(bool openOnly = false)
        {
            var periods = await budgetToolsAccessor.GetPeriods(); // TODO: make get periods accept criteria
            
            return periods.Where(p => !openOnly || p.IsOpen)
                .Select(p => mapper.Map<T>(p)).ToList();
        }

        public async Task<T> GetTransaction<T>(int transactionId)
        {
            return await budgetToolsAccessor.GetTransaction<T>(transactionId);
        }

        public async Task<List<T>> GetTransactions<T>(int bankAccountId, int periodId)
        {
            int year = periodId / 100;
            int month = periodId - year * 100;
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddMilliseconds(-3); // TODO: change this; don't like the millisecond thing

            return await budgetToolsAccessor.GetTransactions<T>(bankAccountId, startDate, endDate);
        }

        // TODO: create a parser config table and associate its records with each bank account
        // TODO: can add transaction order, BOM, encoding, etc. as needed
        public async Task ImportFile(int bankAccountId, Stream stream)
        {
            var bankAccount = await budgetToolsAccessor.GetBankAccount(bankAccountId);

            // TODO: get this from config in the db
            // create a parser config
            var parserConfig = new ParserConfig
            {
                BankAccountId = bankAccountId,
                Delimiter = ',',
                HeaderRows = 4,
                Id = 1,
                IsSortDesc = true,
                ParserName = "FirstCommunity",
                ValidationPattern = @"(?:\:\s*)(\S*)",
                ValidationRowNo = 2,
                ValidationValue = bankAccount.UploadValidator
            };

            // convert the raw row data to staged transactions
            var stagedTransactions = importService.ParseStream<StagedTransaction>(stream, parserConfig);

            // write the file data to a staging table and process it
            await budgetToolsAccessor.SaveStagedTransactions(bankAccountId, stagedTransactions, parserConfig.IsSortDesc);
        }

        public void RemoveCachedItem(string key)
        {
            cache.Remove(key); // TODO: see if there is an async remove
        }

        public async Task<int> SaveBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount)
        {
            return await budgetToolsAccessor.UpdateBudgetLine(periodId, budgetLineId, bankAccountId,
                plannedAmount, allocatedAmount, accruedAmount);
        }

        public async Task<(bool IsSuccess, List<T> Messages)> SaveTransfer<T>(int bankAccountId, int budgetLineFromId,
            int budgetLineToId, decimal amount, string note)
        {
            return await budgetToolsAccessor.SaveTransfer<T>(bankAccountId, budgetLineFromId,
                budgetLineToId, amount, note);
        }

        public async Task<int> UpdateTransaction<T>(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<T> mappedTransactions)
        {
            var mappedXacts = mappedTransactions.Select(m => mapper.Map<MappedTransaction>(m)).ToList();

            // verify that each budget line is only used once
            var dups = mappedXacts.GroupBy(x => x.BudgetLineId).Select(g => g.Count()).Where(c => c > 1).ToList();
            if (dups.Count() > 0) throw new ArgumentException("Only one mapped transaction id may be saved for each budget line.");

            return await budgetToolsAccessor.UpdateTransaction<T>(transactionId, transactionTypeCode, recipient, notes,
                mappedTransactions);

        }

    }

}