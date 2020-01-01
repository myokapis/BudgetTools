using System.IO;
using System.Web.Mvc;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;

namespace BudgetTools.Controllers
{
    public class ImportController : Controller
    {

        protected IImportPresenter importPresenter;
        protected IBudgetService budgetService;

        public ImportController(IImportPresenter importPresenter, IBudgetService budgetService)
        {
            this.importPresenter = importPresenter;
            this.budgetService = budgetService;
        }

        public ActionResult Index()
        {
            return Content(this.importPresenter.GetPage());
        }

        public void ImportFile()
        {
            int bankAccountId = int.Parse(Request.Params["bankAccountId"]);
            var file = Request.Files[0];
            string fileName = file.FileName;
            var reader = new StreamReader(file.InputStream);

            this.budgetService.ImportFile(bankAccountId, reader.BaseStream);
        }

    }
}