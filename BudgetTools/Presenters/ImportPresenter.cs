using System;
using System.Linq;
using BudgetTools.Classes;
using BudgetTools.Models.DomainModels;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface IImportPresenter
    {
        string GetPage();
        string GetTransactionRows();
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

        public string GetTransactionRows()
        {
            var writer = contentWriter.GetWriter("ROW");
            GetTransactionRows(writer);
            return writer.GetContent();
        }

        public void GetTransactionRows(ITemplateWriter writer)
        {
            var data = this.budgetService.GetBankAccounts<BankAccount>().Where(a => a.IsActive);
            writer.SetMultiSectionFields<BankAccount>(data);
        }

        public string GetPage()
        {
            var writer = SetupMasterPage("HEAD", "BODY");
            writer.SelectProvider("HEAD");
            writer.AppendSection(true);

            writer.SelectProvider("BODY");
            writer.SelectProvider("SELECTOR");
            writer.AppendSection(true);

            writer.SelectSection("ROWS");
            GetTransactionRows(writer);
            writer.AppendAll();

            return writer.GetContent();
        }

    }

}