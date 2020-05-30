using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AutoMapper;
using BudgetToolsDAL.Accessors;
using BudgetToolsBLL.Models;

namespace BudgetToolsBLL.Services
{

    // TODO: decide if a mapper should be required for instantiating the service
    public interface IBudgetService
    {
        List<T1> CloseCurrentPeriod<T1, T2>(ref T2 pageScope);
        List<T> GetBankAccounts<T>(bool activeOnly = false);
        List<T> GetBudgetLineBalances<T>(int periodId, int bankAccountId);
        List<T> GetBudgetLineSet<T>(DateTime? effectiveDate = null);
        List<T> GetPeriodBudget<T>(int BankAccountId, int PeriodId);
        List<T> GetPeriodBalancesWithSummary<T>(int periodId);
        List<T2> GetPeriodBudgetWithSummary<T1, T2>(T1 scope);
        List<T> GetPeriods<T>(bool openOnly = false);
        T GetTransaction<T>(int transactionId);
        List<T> GetTransactions<T>(int bankAccountId, int periodId);
        void ImportFile(int bankAccountId, Stream stream);

        void SaveBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount);

        List<T> SaveTransfer<T>(int bankAccountId, int budgetLineFromId,
            int budgetLineToId, decimal amount, string note, out bool isSuccess);

        void SetPageScope<T>(ref T pageScope);

        void UpdateTransaction<T>(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<T> mappedTransactions);
    }

    public class BudgetService : IBudgetService
    {
        protected IBudgetToolsAccessor budgetToolsAccessor;
        protected IImportService importService;

        public BudgetService(IBudgetToolsAccessor budgetToolsAccessor, IImportService importService)
        {
            this.budgetToolsAccessor = budgetToolsAccessor;
            this.importService = importService;
        }

        public List<T1> CloseCurrentPeriod<T1, T2>(ref T2 pageScope)
        {
            var dataScope = Mapper.Map<DataScope>(pageScope);
            var data = budgetToolsAccessor.CloseCurrentPeriod<T1>(out bool isSuccess);

            if(isSuccess)
            {
                SetPageScope(ref pageScope);
                budgetToolsAccessor.CreateAllocations(dataScope.CurrentPeriodId);
            }
            
            return data;
        }

        public List<T> GetBankAccounts<T>(bool activeOnly = false)
        {
            return this.budgetToolsAccessor.GetBankAccounts()
                .Where(a => !activeOnly || a.IsActive)
                .Select(a => Mapper.Map<T>(a)).ToList();
        }

        public List<T> GetBudgetLineBalances<T>(int periodId, int bankAccountId)
        {
            return budgetToolsAccessor.GetBudgetLineBalances<T>(periodId, bankAccountId);
        }

        public List<T> GetBudgetLineSet<T>(DateTime? effectiveDate = null)
        {
            return budgetToolsAccessor.GetBudgetLineSet<T>(effectiveDate ?? DateTime.Now);
        }

        public List<T> GetPeriodBalancesWithSummary<T>(int periodId)
        {
            var balances = budgetToolsAccessor.GetPeriodBalances<PeriodBalance>(periodId)
                .Select(b => new
                {
                    b.PeriodId,
                    b.BankAccountId,
                    b.BankAccountName,
                    b.BudgetLineId,
                    b.BudgetLineName,
                    b.BudgetCategoryName,
                    b.PreviousBalance,
                    b.Balance,
                    b.ProjectedBalance,
                    Level = 2
                });

            var accountTotals = balances.GroupBy(b => b.BankAccountId)
                .Select(k => new
                {
                    k.First().PeriodId,
                    BankAccountId = k.Key,
                    k.First().BankAccountName,
                    BudgetLineId = 0,
                    BudgetLineName = "",
                    k.First().BudgetCategoryName,
                    PreviousBalance = k.Sum(i => i.PreviousBalance),
                    Balance = k.Sum(i => i.Balance),
                    ProjectedBalance = k.Sum(i => i.ProjectedBalance),
                    Level = 1
                });

            var grandTotal = accountTotals.GroupBy(k => 0)
                .Select(k => new
                {
                    k.First().PeriodId,
                    BankAccountId = 0,
                    BankAccountName = "All Accounts",
                    BudgetLineId = 0,
                    BudgetLineName = "",
                    k.First().BudgetCategoryName,
                    PreviousBalance = k.Sum(i => i.PreviousBalance),
                    Balance = k.Sum(i => i.Balance),
                    ProjectedBalance = k.Sum(i => i.ProjectedBalance),
                    Level = 0
                });

            return balances.Union(accountTotals).Union(grandTotal)
                .Select(b => Mapper.Map<T>(b)).ToList();
        }

        public List<T> GetPeriodBudget<T>(int bankAccountId, int periodId)
        {

            // get the planned and allocated amounts for each budget line
            return budgetToolsAccessor.GetPeriodBudget<T>(periodId, bankAccountId)
                .Select(d => Mapper.Map<T>(d)).ToList();

        }

        public List<T2> GetPeriodBudgetWithSummary<T1, T2>(T1 scope)
        {
            var dataScope = Mapper.Map<DataScope>(scope);

            if (dataScope.PeriodId > dataScope.CurrentPeriodId)
                budgetToolsAccessor.CreateAllocations(dataScope.PeriodId);

            var lines = budgetToolsAccessor.GetPeriodBudget<PeriodBudgetLine>(dataScope.PeriodId, dataScope.BankAccountId)
                .Select(l => new
                {
                    l.BudgetLineId,
                    l.BudgetLineName,
                    l.BudgetCategoryName,
                    l.PlannedAmount,
                    l.AllocatedAmount,
                    l.AccruedAmount,
                    l.ActualAmount,
                    l.RemainingAmount,
                    l.AccruedBalance,
                    l.TotalCashAmount,
                    l.IsAccrued,
                    IsDetail = true
                }).ToList();

            var summaryLines = lines.GroupBy(l => l.BudgetCategoryName)
                .Select(g => new
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

            return lines.Select(l => Mapper.Map<T2>(l)).ToList();
        }

        public List<T> GetPeriods<T>(bool openOnly = false)
        {
            return this.budgetToolsAccessor.GetPeriods()
                .Where(p => !openOnly || p.IsOpen)
                .Select(p => Mapper.Map<T>(p)).ToList();
        }

        public T GetTransaction<T>(int transactionId)
        {
            return budgetToolsAccessor.GetTransaction<T>(transactionId);
        }

        public List<T> GetTransactions<T>(int bankAccountId, int periodId)
        {
            int year = periodId / 100;
            int month = periodId - year * 100;
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddMilliseconds(-3);

            return budgetToolsAccessor.GetTransactions<T>(bankAccountId, startDate, endDate);
        }

        // TODO: create a parser config table and associate its records with each bank account
        // TODO: can add transaction order, BOM, encoding, etc. as needed
        public void ImportFile(int bankAccountId, Stream stream)
        {
            var bankAccount = this.budgetToolsAccessor.GetBankAccount(bankAccountId);

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
            budgetToolsAccessor.SaveStagedTransactions(bankAccountId, stagedTransactions, parserConfig.IsSortDesc);
        }

        public void SaveBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount)
        {
            budgetToolsAccessor.UpdateBudgetLine(periodId, budgetLineId, bankAccountId,
                plannedAmount, allocatedAmount, accruedAmount);
        }

        public List<T> SaveTransfer<T>(int bankAccountId, int budgetLineFromId,
            int budgetLineToId, decimal amount, string note, out bool isSuccess)
        {
            return budgetToolsAccessor.SaveTransfer<T>(bankAccountId, budgetLineFromId,
                budgetLineToId, amount, note, out isSuccess);
        }

        public void SetPageScope<T>(ref T pageScope)
        {
            var dataScope = Mapper.Map<DataScope>(pageScope);
            var currentPeriodId = budgetToolsAccessor.GetCurrentPeriodId();

            if (dataScope.BankAccountId <= 0)
                dataScope.BankAccountId = budgetToolsAccessor.GetBankAccounts().First().BankAccountId;

            dataScope.CurrentPeriodId = currentPeriodId;
            if (dataScope.PeriodId < currentPeriodId) dataScope.PeriodId = currentPeriodId;

            Mapper.Map(dataScope, pageScope);
        }

        public void UpdateTransaction<T>(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<T> mappedTransactions)
        {
            var mappedXacts = mappedTransactions.Select(m => Mapper.Map<IMappedTransaction>(m)).ToList();

            // verify that each budget line is only used once
            var dups = mappedXacts.GroupBy(x => x.BudgetLineId).Select(g => g.Count()).Where(c => c > 1).ToList();
            if (dups.Count() > 0) throw new ArgumentException("Only one mapped transaction id may be saved for each budget line.");

            budgetToolsAccessor.UpdateTransaction<T>(transactionId, transactionTypeCode, recipient, notes,
                mappedTransactions);

        }

    }

}