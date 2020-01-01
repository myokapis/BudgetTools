using System;
using System.Web.Mvc;
using BudgetTools.Classes;
using BudgetTools.Enums;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;

namespace BudgetTools.Controllers
{
    public class AdminController : Controller
    {

        protected IAdminPresenter adminPresenter;
        protected IBudgetService budgetService;

        public AdminController(IAdminPresenter adminPresenter, IBudgetService budgetService)
        {
            this.budgetService = budgetService;
            this.adminPresenter = adminPresenter;
        }

        public ActionResult CloseCurrentPeriod()
        {
            return Content(adminPresenter.CloseCurrentPeriod());
        }

        public ActionResult GetBalanceTransfer()
        {
            return Content(adminPresenter.GetBalanceTransfer());
        }

        public ActionResult GetCloseCurrentPeriod()
        {
            return Content(adminPresenter.GetCloseCurrentPeriod());
        }

        public ActionResult GetTransferBudgetLines(int bankAccountId, string direction)
        {
            if (!Enum.TryParse<Direction>(direction, out var directionEnum))
                throw new ArgumentException($"The direction parameter had an invalid value.", "direction");

            return Content(adminPresenter.GetTransferBudgetLines(bankAccountId, directionEnum));
        }

        public ActionResult Index()
        {
            // TODO: create a base controller and move this to a method on the base
            var pageScope = (IPageScope)this.Session.Contents["pageScope"];
            if (pageScope.BankAccountId == 0)
            {
                pageScope.BankAccountId = 1;
                pageScope.PeriodId = int.Parse(DateTime.Now.ToString("yyyyMM"));
            }

            return Content(this.adminPresenter.GetPage());
        }

        public ActionResult SaveTransfer(int bankAccountFromId, int budgetLineFromId,
            int bankAccountToId, int budgetLineToId, decimal amount, string note)
        {
            return Content(adminPresenter.SaveTransfer(bankAccountFromId, budgetLineFromId,
                bankAccountToId, budgetLineToId, amount, note));
        }

    }
}