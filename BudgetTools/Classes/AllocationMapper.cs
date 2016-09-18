using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BudgetTools.Models.DomainModels;
using BudgetTools.Models.ViewModels;

namespace BudgetTools.Models
{
  public class AllocationMapper
  {
    public static List<AllocationViewModel> Map(int BankAccountId)
    {
      return Map(BankAccountId, null);
    }

    public static List<AllocationViewModel> Map(int BankAccountId, int? BudgetCategoryId)
    {
      AllocationViewModel viewModel;
      List<AllocationViewModel> viewModelQ = new List<AllocationViewModel>();
      BudgetToolsDBContext db = new BudgetToolsDBContext();

      // get previous and current period
      Period previousPeriod = db.PreviousPeriod;
      Period currentPeriod = db.CurrentPeriod;

      // get the allocations for the period
      var allocations = db.Allocations.Include("BudgetLine")
        .Where(a => a.PeriodId == currentPeriod.PeriodId
          && a.BudgetLine.BudgetCategoryId == (BudgetCategoryId == null ? a.BudgetLine.BudgetCategoryId : BudgetCategoryId)
          && (a.BankAccountId == BankAccountId || BankAccountId == 0))
        .GroupBy(g => new
        {
          BudgetLineId = g.BudgetLineId,
          BankAccountId = (BankAccountId == 0 ? 0 : g.BankAccountId),
          DisplayName = g.BudgetLine.DisplayName,
          BudgetCategoryName = g.BudgetLine.BudgetCategoryName,
          IsAccrued = g.BudgetLine.IsAccrued
        })
        .Select(r => new
        {
          BudgetLineId = r.Key.BudgetLineId,
          BankAccountId = r.Key.BankAccountId,
          DisplayName = r.Key.DisplayName,
          BudgetCategoryName = r.Key.BudgetCategoryName,
          IsAccrued = r.Key.IsAccrued,
          AllocatedAmount = r.Sum(s => s.AllocatedAmount),
          AccruedAmount = r.Sum(s => s.AccruedAmount),
          PlannedAmount = r.Sum(s => s.PlannedAmount)
        });

      // summarize transactions for the period
      var tranTotals = from m in db.MappedTransactions
                       join t in db.Transactions on m.TransactionId equals t.TransactionId
                       where t.TransactionDate >= currentPeriod.PeriodStartDate
                        && t.TransactionDate <= currentPeriod.PeriodEndDate
                        && m.BudgetLine.BudgetCategoryId == (BudgetCategoryId == null ? m.BudgetLine.BudgetCategoryId : BudgetCategoryId)
                        && (t.BankAccountId == BankAccountId || BankAccountId == 0)
                       group m by new
                       {
                         BudgetLineId = m.BudgetLineId,
                         BankAccountId = (BankAccountId == 0 ? 0 : t.BankAccountId)
                       } into gm
                       select new
                       {
                         BudgetLineId = gm.Key.BudgetLineId,
                         BankAccountId = gm.Key.BankAccountId,
                         Amount = gm.Sum(item => -1 * item.Amount)
                       };

      // summarized the accrued balances for the beginning of the current period
      var accruedBalances = from p in db.PeriodBalances
                            where p.PeriodId == previousPeriod.PeriodId
                              && p.BudgetLine.BudgetCategoryId == (BudgetCategoryId == null ? p.BudgetLine.BudgetCategoryId : BudgetCategoryId)
                              && (p.BankAccountId == BankAccountId || BankAccountId == 0)
                            group p by new
                            {
                              BudgetLineId = p.BudgetLineId,
                              BankAccountId = (BankAccountId == 0 ? 0 : p.BankAccountId)
                            } into gp
                            select new
                            {
                              BudgetLineId = gp.Key.BudgetLineId,
                              BankAccountId = gp.Key.BankAccountId,
                              AccruedBalance = gp.Sum(item => item.Balance)
                            };

      // join the allocations to the summarized transactions and accrued balances
      var lineTotals = from a in allocations
                       join c in tranTotals
                       on new { BudgetLineId = a.BudgetLineId, BankAccountId = a.BankAccountId }
                       equals new { BudgetLineId = c.BudgetLineId, BankAccountId = c.BankAccountId } into ac
                       join b in accruedBalances
                       on new { BudgetLineId = a.BudgetLineId, BankAccountId = a.BankAccountId }
                       equals new { BudgetLineId = b.BudgetLineId, BankAccountId = b.BankAccountId } into ab
                       from lac in ac.DefaultIfEmpty()
                       from lab in ab.DefaultIfEmpty()
                       select new
                       {
                         a.BudgetLineId,
                         a.DisplayName,
                         a.BudgetCategoryName,
                         a.PlannedAmount,
                         a.AllocatedAmount,
                         a.AccruedAmount,
                         ActualAmount = (lac.Amount == null ? 0 : lac.Amount),
                         RemainingAmount = (lac.Amount == null ? a.PlannedAmount : a.PlannedAmount - lac.Amount),
                         AccruedBalance = (lab.AccruedBalance == null ? 0 : lab.AccruedBalance),
                         a.IsAccrued
                       };

      // map the domain model to the view model
      foreach (var lineTotal in lineTotals)
      {
        viewModel = new AllocationViewModel();
        viewModel.BudgetLineId = lineTotal.BudgetLineId;
        viewModel.BudgetLineName = lineTotal.DisplayName;
        viewModel.BudgetCategoryName = lineTotal.BudgetCategoryName;
        viewModel.PlannedAmount = lineTotal.PlannedAmount;
        viewModel.AllocatedAmount = lineTotal.AllocatedAmount;
        viewModel.AccruedAmount = lineTotal.AccruedAmount;
        viewModel.ActualAmount = lineTotal.ActualAmount;
        viewModel.RemainingAmount = lineTotal.RemainingAmount;
        viewModel.AccruedBalance = lineTotal.AccruedBalance;
        viewModel.IsAccrued = lineTotal.IsAccrued;
        viewModelQ.Add(viewModel);
      }

      return viewModelQ;
    }
  
  }
}