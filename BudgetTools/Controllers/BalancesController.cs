using System;
using System.Web.Mvc;
using BudgetTools.Classes;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;

namespace BudgetTools.Controllers
{
    public class BalancesController : Controller
    {
        protected IBalancesPresenter balancesPresenter;
        protected IBudgetService budgetService;

        public BalancesController(IBalancesPresenter balancesPresenter, IBudgetService budgetService)
        {
            this.balancesPresenter = balancesPresenter;
            this.budgetService = budgetService;
        }

        public ActionResult ChangePeriod(int periodId)
        {
            IPageScope pageScope = (IPageScope)this.Session.Contents["pageScope"];
            pageScope.PeriodId = periodId;

            return Content(balancesPresenter.GetTransactionRows());
        }

        public ActionResult Index()
        {
            var pageScope = (IPageScope)this.Session.Contents["pageScope"];
            if (pageScope.BankAccountId == 0)
            {
                pageScope.BankAccountId = 1;
                pageScope.PeriodId = int.Parse(DateTime.Now.ToString("yyyyMM"));
            }

            return Content(balancesPresenter.GetPage());
        }

    }
}