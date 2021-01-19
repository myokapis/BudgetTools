using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BudgetTools.Classes;
using BudgetTools.Enums;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;
using BudgetTools.Models;

namespace BudgetTools.Controllers
{
    public class AdminController : Controller
    {

        private readonly IAdminPresenter adminPresenter;
        private readonly IBudgetService budgetService;

        public AdminController(IAdminPresenter adminPresenter, IBudgetService budgetService)
        {
            this.budgetService = budgetService;
            this.adminPresenter = adminPresenter;
        }

        public async Task<ActionResult> CloseCurrentPeriod(PageScope pageScope)
        {
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();

            var result = await budgetService.CloseCurrentPeriod<PageScope, Message>(pageScope);
            
            var data = new
            {
                pageScope = result.PageScope,
                html = await adminPresenter.GetCloseCurrentPeriodMessages(result.Messages)
            };

            return Json(data);
        }

        public async Task<ActionResult> Index(PageScope pageScope)
        {
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();
            
            return Content(await adminPresenter.GetPage(pageScope), "text/html");
        }

        public async Task<JsonResult> SaveTransfer(PageScope pageScope, int budgetLineFromId,
            int budgetLineToId, decimal amount, string note)
        {
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();

            var result = await budgetService.SaveTransfer<Message>(pageScope.BankAccountId, budgetLineFromId,
                budgetLineToId, amount, note);

            var data = new
            {
                result.IsSuccess,
                messages = await adminPresenter.GetSaveTransferMessages(result.Messages),
                budgetLinesFrom = result.IsSuccess ? await adminPresenter.GetTransferBudgetLines(pageScope, Direction.From) : "",
                budgetLinesTo = result.IsSuccess ? await adminPresenter.GetTransferBudgetLines(pageScope, Direction.To) : ""
            };

            return Json(data);
        }

        public async Task<ActionResult> ShowBalanceTransfer(PageScope pageScope)
        {
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();

            var data = new
            {
                html = await adminPresenter.GetBalanceTransfer(pageScope)
            };

            return Json(data);
        }

        public async Task<ActionResult> ShowCloseCurrentPeriod()
        {
            var data = new
            {
                html = await adminPresenter.GetCloseCurrentPeriod()
            };

            return Json(data);
        }

        public async Task<JsonResult> ShowTransferBudgetLines(PageScope pageScope)
        {
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();

            var data = new
            {
                budgetLinesFrom = await adminPresenter.GetTransferBudgetLines(pageScope, Direction.From),
                budgetLinesTo = await adminPresenter.GetTransferBudgetLines(pageScope, Direction.To)
            };

            return Json(data);
        }

    }
}