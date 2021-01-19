using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;
using BudgetTools.Models;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace BudgetTools.Controllers
{
    public class ImportController : Controller
    {

        private readonly IImportPresenter importPresenter;
        private readonly IBudgetService budgetService;

        public ImportController(IImportPresenter importPresenter, IBudgetService budgetService)
        {
            this.importPresenter = importPresenter;
            this.budgetService = budgetService;
        }

        public async Task<ActionResult> Index(PageScope pageScope)
        {
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();

            return Content(await importPresenter.GetPage(pageScope), "text/html");
        }

        // TODO: return json with success or failure and message
        public async Task<ActionResult> ImportFile(int bankAccountId)
        {
            if (bankAccountId == 0)
                throw new Exception("Invalid bank accounn id.");

            var file = Request.Form.Files[0];

            using(var stream = file.OpenReadStream())
            {
                await budgetService.ImportFile(bankAccountId, stream);
            }

            return Json("");
        }

    }
}