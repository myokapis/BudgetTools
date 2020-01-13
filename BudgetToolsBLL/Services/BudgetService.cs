﻿using System;
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
        List<T> GetPeriodBudgetWithSummary<T>(int bankAccountId, int periodId);
        List<T> GetPeriods<T>(bool openOnly = false);
        T GetTransaction<T>(int transactionId);
        List<T> GetTransactions<T>(int bankAccountId, int periodId);
        void ImportFile(int bankAccountId, Stream stream);
        void ImportTransactions(int bankAccountId, List<StagedTransaction> stagedTransactions);

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

        public BudgetService(IBudgetToolsAccessor budgetToolsRepository)
        {
            this.budgetToolsAccessor = budgetToolsRepository;
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
            return budgetToolsAccessor.GetPeriodBudget(bankAccountId, periodId)
                .Select(d => Mapper.Map<T>(d)).ToList<T>();

        }

        // TODO: refactor this to use a bll model and get a generic collection from the dal
        public List<T> GetPeriodBudgetWithSummary<T>(int bankAccountId, int periodId)
        {

            var lines = budgetToolsAccessor.GetPeriodBudget(bankAccountId, periodId)
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

            return lines.Select(l => Mapper.Map<T>(l)).ToList();
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

        // TODO: move upload validator check into file parser. just pass in the value to check
        //       make this a boolean method
        public void ImportFile(int bankAccountId, Stream stream)
        {
            var (uploadValidator, stagedTransactions) = ParseFile(bankAccountId, stream);
            var bankAccount = this.budgetToolsAccessor.GetBankAccount(bankAccountId);

            // ensure file is for the specified bank account
            if(bankAccount.UploadValidator != uploadValidator)
            {
                throw new Exception("UploadValidator was invalid for this bank account.");
            }

            ImportTransactions(bankAccountId, stagedTransactions);
        }

        public void ImportTransactions(int bankAccountId, List<StagedTransaction> stagedTransactions)
        {
            budgetToolsAccessor.SaveStagedTransactions<StagedTransaction>(bankAccountId, stagedTransactions);
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

        #region Protected Methods
        // TODO: move these into a file parser class along with all the junk in StagedTransaction.
        //       better design plus some day i may have another bank account that has a different export format
        protected (string UploadValidator, List<StagedTransaction> StagedTransactions) ParseFile(int bankAccountId, Stream stream)
        {
            int skipLines = 4;
            int lineNo = 0;
            var stagedTransactions = new List<StagedTransaction>();
            var fileValidator = "";

            using (var reader = new StreamReader(stream))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    lineNo++;

                    // TODO: bail out here if validator is false
                    if (lineNo == 2) fileValidator = ParseValidator(line);

                    if (lineNo > skipLines)
                    {
                        var data = line.Split(new char[] { ',' }).Select(v => v.Replace("\"", "")).ToArray();
                        var st = new StagedTransaction(bankAccountId, data);
                        stagedTransactions.Add(st);
                    }
                }

            }

            return (fileValidator, stagedTransactions);
        }

        protected string ParseValidator(string line)
        {
            var regex = new Regex(@"(?:\:\s*)(\S*)");
            var match = regex.Match(line.Replace("\"", ""));
            return match.Groups[1].Value;
        }

        #endregion

    }

}