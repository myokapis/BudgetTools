using System.Linq;
using BudgetTools.Classes;
using BudgetTools.Models;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface IBudgetPresenter
    {
        string GetPage();
        string GetTransactionRows();
    }

    public class BudgetPresenter : MasterPresenter, IBudgetPresenter
    {

        public BudgetPresenter(IBudgetService budgetService, IPageScope pageScope, ITemplateCache templateCache)
        {
            this.budgetService = budgetService;
            this.pageScope = pageScope;
            this.templateCache = templateCache;

            contentWriter = GetTemplateWriter("Budget.tpl");
        }

        public string GetTransactionRows()
        {
            var writer = contentWriter.GetWriter("TBODY", true);
            GetTransactionRows(writer);
            writer.AppendAll();
            return writer.GetContent();
        }

        private void GetTransactionRows(ITemplateWriter writer = null)
        {

            // get budget records for the period
            var records = this.budgetService
                .GetPeriodBudgetWithSummary<PeriodBudgetLine>(this.pageScope.PeriodId, this.pageScope.BankAccountId)
                .OrderBy(l => l.BudgetCategoryName)
                .ThenBy(l => l.IsDetail)
                .ThenBy(l => l.BudgetLineName);

            foreach (var record in records)
            {
                writer.SelectSection("ROWS");
                var sectionName = !record.IsDetail ? "ROW_S" : record.IsAccrued ? "ROW_A" : "ROW_D";
                writer.SetSectionFields(sectionName, record, SectionOptions.AppendDeselect);
                writer.AppendSection(true);
            }

        }

        public string GetPage()
        {
            // setup master page and the content page section providers
            var writer = SetupMasterPage("HEAD", "BODY");
            var selectorWriter = GetTemplateWriter("Common.tpl").GetWriter("SELECTOR");
            contentWriter.RegisterFieldProvider("BODY", "SELECTOR", selectorWriter);

            // TODO: include a sectionoptions param in SelectProvider and use it to simplify this to a one liner
            writer.SelectProvider("HEAD");
            writer.AppendSection(true);

            writer.SelectProvider("BODY");
            writer.SelectProvider("SELECTOR");
            GetSelector(writer);
            writer.SelectSection("TBODY");
            GetTransactionRows(writer);
            writer.AppendAll();

            return writer.GetContent();
        }

    }

}