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
    public class TransferController : Controller
    {
        // GET: Transfer
      public ActionResult Index(string BankAccountId)
        {
          int selectedBankAccountId = -1;
          bool parseResult = int.TryParse(BankAccountId, out selectedBankAccountId);

          BudgetToolsDBContext db = new BudgetToolsDBContext();
          Period currentPeriod = db.CurrentPeriod;
          int periodId = currentPeriod.PeriodId;

          // get the active bank accounts
          var BankAccounts = db.BankAccounts.Where(ba => ba.IsActive == true);
          List<SelectListItem> bankAccountList = new List<SelectListItem>();

          if (selectedBankAccountId <= 0) selectedBankAccountId = BankAccounts.FirstOrDefault().BankAccountId;

          foreach (BankAccount bankAccount in BankAccounts)
          {
            bankAccountList.Add(new SelectListItem() { Text = bankAccount.BankAccountName, Value = bankAccount.BankAccountId.ToString(), Selected = (bankAccount.BankAccountId == selectedBankAccountId) });
          }

          var fromLines = from bl in db.BudgetLines
                          join pb in db.PeriodBalances on bl.BudgetLineId equals pb.BudgetLineId
                          where pb.BankAccountId == selectedBankAccountId
                            && pb.PeriodId == periodId
                            && pb.Balance > 0
                          select bl;

          var toLines = db.BudgetLines;

          ViewData["BankAccounts"] = bankAccountList;
          ViewData["FromLines"] = fromLines;
          ViewData["ToLines"] = toLines;
          ViewData["SelectedBankAccountId"] = selectedBankAccountId;
          return View();
        }
    }
}