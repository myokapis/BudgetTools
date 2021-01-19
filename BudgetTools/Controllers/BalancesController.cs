using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BudgetTools.Models;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;

namespace BudgetTools.Controllers
{
    public class BalancesController : Controller
    {
        private readonly IBalancesPresenter balancesPresenter;
        private readonly IBudgetService budgetService;

        public BalancesController(IBalancesPresenter balancesPresenter, IBudgetService budgetService)
        {
            this.balancesPresenter = balancesPresenter;
            this.budgetService = budgetService;
        }

        public async Task<ActionResult> Index(PageScope pageScope)
        {
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();

            return Content(await balancesPresenter.GetPage(pageScope), "text/html");
        }

        public async Task<ActionResult> LoadPeriodBalances(PageScope pageScope)
        {
            // seems like non-index methods should throw an error when pagescope is invalid
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();

            var data = new
            {
                html = await balancesPresenter.GetTransactionRows(pageScope.PeriodId)
            };

            return Json(data);
        }

    }
}