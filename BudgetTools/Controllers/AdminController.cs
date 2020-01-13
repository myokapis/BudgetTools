using System.Web.Mvc;
using BudgetTools.Classes;
using BudgetTools.Enums;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;
using BudgetTools.Models;

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

        public void ChangeBankAccount(int bankAccountId)
        {
            (Session["pageScope"] as PageScope).BankAccountId = bankAccountId;
        }

        public ActionResult CloseCurrentPeriod()
        {
            var pageScope = Session["pageScope"] as PageScope;
            var messages = budgetService.CloseCurrentPeriod<Message, PageScope>(ref pageScope);
            return Content(adminPresenter.GetCloseCurrentPeriodMessages(messages));
        }

        public ActionResult GetBalanceTransfer()
        {
            return Content(adminPresenter.GetBalanceTransfer());
        }

        public ActionResult GetCloseCurrentPeriod()
        {
            return Content(adminPresenter.GetCloseCurrentPeriod());
        }

        public JsonResult GetTransferBudgetLines(int bankAccountId)
        {
            (Session["pageScope"] as PageScope).BankAccountId = bankAccountId;

            var data = new
            {
                budgetLinesFrom = adminPresenter.GetTransferBudgetLines(bankAccountId, Direction.From),
                budgetLinesTo = adminPresenter.GetTransferBudgetLines(bankAccountId, Direction.To)
            };

            return Json(data);
        }

        public ActionResult Index()
        {
            return Content(this.adminPresenter.GetPage());
        }

        public JsonResult SaveTransfer(int bankAccountId, int budgetLineFromId,
            int budgetLineToId, decimal amount, string note)
        {
            var isSuccess = false;
            var messages = budgetService.SaveTransfer<Message>(bankAccountId, budgetLineFromId,
                budgetLineToId, amount, note, out isSuccess);

            var data = new
            {
                isSuccess,
                messages = adminPresenter.GetSaveTransferMessages(messages),
                budgetLinesFrom = isSuccess ? adminPresenter.GetTransferBudgetLines(bankAccountId, Direction.From) : "",
                budgetLinesTo = isSuccess ? adminPresenter.GetTransferBudgetLines(bankAccountId, Direction.To) : ""
            };

            return Json(data);
        }

    }
}