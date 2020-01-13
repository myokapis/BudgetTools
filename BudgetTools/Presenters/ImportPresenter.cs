using System.Linq;
using BudgetTools.Classes;
using BudgetTools.Models;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface IImportPresenter
    {
        string GetPage();
    }

    public class ImportPresenter : MasterPresenter, IImportPresenter
    {

        public ImportPresenter(IBudgetService budgetService, IPageScope pageScope, ITemplateCache templateCache)
        {
            this.budgetService = budgetService;
            this.pageScope = pageScope;
            this.templateCache = templateCache;

            contentWriter = GetTemplateWriter("Import.tpl");
        }

        protected void GetBankAccountRows(ITemplateWriter writer)
        {
            var data = this.budgetService.GetBankAccounts<BankAccount>().Where(a => a.IsActive);
            writer.SetMultiSectionFields("ROW", data);
        }

        public string GetPage()
        {
            var writer = SetupMasterPage("HEAD", "BODY");
            writer.SelectProvider("HEAD");
            writer.AppendSection(true);

            writer.SelectProvider("BODY");
            GetBankAccountRows(writer);
            writer.AppendAll();

            return writer.GetContent();
        }

    }

}