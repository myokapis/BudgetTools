using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BudgetTools.Models;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;

namespace BudgetTools.Controllers
{

    public class BudgetController : Controller
    {
        private readonly IBudgetPresenter budgetPresenter;
        private readonly IBudgetService budgetService;

        public BudgetController(IBudgetPresenter budgetPresenter, IBudgetService budgetService)
        {
            this.budgetPresenter = budgetPresenter;
            this.budgetService = budgetService;
        }

        public async Task<ActionResult> Index(PageScope pageScope)
        {
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();

            return Content(await budgetPresenter.GetPage(pageScope), "text/html");
        }

        public async Task<ActionResult> ChangePageScope(PageScope pageScope)
        {
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();

            var data = new
            {
                html = await budgetPresenter.GetTransactionRows(pageScope)
            };

            return Json(data);
        }

        // TODO: return json with success or failure and message
        public async Task<ActionResult> SaveBudgetLine(PageScope pageScope, int budgetLineId,
            decimal plannedAmount, decimal allocatedAmount, decimal accruedAmount)
        {

            if (!ModelState.IsValid)
                throw new Exception();

            await budgetService.SaveBudgetLine(pageScope.PeriodId, budgetLineId, pageScope.BankAccountId,
                plannedAmount, allocatedAmount, accruedAmount);

            return Json(new { });
        }

    }

}