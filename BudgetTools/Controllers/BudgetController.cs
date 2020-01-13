using System;
using System.Web.Mvc;
using BudgetTools.Classes;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;

namespace BudgetTools.Controllers
{

    public class BudgetController : Controller
    {
        protected IBudgetPresenter budgetPresenter;
        protected IBudgetService budgetService;

        public BudgetController(IBudgetPresenter budgetPresenter, IBudgetService budgetService)
        {
            this.budgetPresenter = budgetPresenter;
            this.budgetService = budgetService;
        }

        public ActionResult Index()
        {
            return Content(budgetPresenter.GetPage());
        }

        public ActionResult ChangeBankAccount(int bankAccountId)
        {
            IPageScope pageScope = (IPageScope)this.Session.Contents["pageScope"];
            pageScope.BankAccountId = bankAccountId;

            return Content(budgetPresenter.GetTransactionRows());
        }

        public ActionResult ChangePeriod(int periodId)
        {
            IPageScope pageScope = (IPageScope)this.Session.Contents["pageScope"];
            pageScope.PeriodId = periodId;

            return Content(budgetPresenter.GetTransactionRows());
        }

        public void SaveBudgetLine(int periodId, int budgetLineId, int bankAccountId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount)
        {
            budgetService.SaveBudgetLine(periodId, budgetLineId, bankAccountId,
                plannedAmount, allocatedAmount, accruedAmount);
        }

    }

}