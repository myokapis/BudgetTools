using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BudgetTools.Models;
using BudgetTools.Models.DomainModels;
using BudgetTools.Models.ViewModels;

namespace BudgetTools.Controllers
{
    public class BudgetController : Controller
    {
      //
      // GET: /Budget/
      public ActionResult Index(string BankAccountId)
      {
        int selectedBankAccountId = -1;
        bool parseResult = int.TryParse(BankAccountId, out selectedBankAccountId);

        BudgetToolsDBContext db = new BudgetToolsDBContext();

        // get the active bank accounts
        var BankAccounts = db.BankAccounts.Where(ba => ba.IsActive == true);
        List<SelectListItem> bankAccountList = new List<SelectListItem>();
        bankAccountList.Add(new SelectListItem() { Text = "(All)", Value = "0", Selected = (selectedBankAccountId == 0) });

        foreach(BankAccount bankAccount in BankAccounts)
        {
          bankAccountList.Add(new SelectListItem() { Text = bankAccount.BankAccountName, Value = bankAccount.BankAccountId.ToString(), Selected = (bankAccount.BankAccountId == selectedBankAccountId) });
        }

        ViewData["BankAccounts"] = bankAccountList;
        ViewData["SelectedBankAccountId"] = selectedBankAccountId;

        return View("Index", AllocationMapper.Map(selectedBankAccountId));
      }

      public ActionResult Balances(string PeriodId = null, bool ClosePeriod = false)
      {
        BudgetToolsDBContext db = new BudgetToolsDBContext();
        Period currentPeriod = db.CurrentPeriod;
        int periodId = (PeriodId == null) ? currentPeriod.PeriodId : int.Parse(PeriodId);
        DateTime periodStartDate = new DateTime(periodId / 100, periodId % (periodId / 100), 1);
        DateTime firstPeriodStartDate = periodStartDate.AddMonths(-12);

        db.UpdatePeriodBalances(periodId, false);

        var periods = db.Periods.Where(p => p.PeriodStartDate >= firstPeriodStartDate)
                        .OrderBy(p => p.PeriodId)
                        .Take(24);

        List<SelectListItem> periodList = new List<SelectListItem>();
        foreach (var period in periods)
        {
          periodList.Add(new SelectListItem { Text = period.PeriodId.ToString(), Value = period.PeriodId.ToString(), Selected = (period.PeriodId == periodId) });
        }
        ViewData["Period"] = periodList;

        var balances = db.PeriodBalances.Include("BankAccount")
                         .Where(p => p.PeriodId == periodId);

        return View("Balances", balances);
      }
	
      [HttpPost]
      public ActionResult SaveBudgetLine(BudgetLineViewModel viewModel)
      {
        BudgetToolsDBContext db = new BudgetToolsDBContext();
        Period currentPeriod = db.CurrentPeriod;

        // save the data
        var allocation = db.Allocations
          .Single(a => a.PeriodId == currentPeriod.PeriodId
            && a.BudgetLineId == viewModel.BudgetLineId
            && a.BankAccountId == viewModel.BankAccountId);

        allocation.AccruedAmount = decimal.Round(viewModel.AccruedAmount, 2);
        allocation.AllocatedAmount = decimal.Round(viewModel.AllocatedAmount, 2);
        allocation.PlannedAmount = decimal.Round(viewModel.PlannedAmount, 2);
        db.SaveChanges();

        ViewData["SelectedBankAccountId"] = viewModel.BankAccountId;

        return PartialView("_BudgetCategory", AllocationMapper.Map(viewModel.BankAccountId, allocation.BudgetLine.BudgetCategoryId));
      }
    
    }
}