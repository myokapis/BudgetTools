using System.Linq;
using BudgetTools.Classes;
using BudgetTools.Models;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface IBalancesPresenter
    {
        string GetPage();
        string GetTransactionRows();
    }

    public class BalancesPresenter : MasterPresenter, IBalancesPresenter
    {

        public BalancesPresenter(IBudgetService budgetService, IPageScope pageScope, ITemplateCache templateCache)
        {
            this.budgetService = budgetService;
            this.pageScope = pageScope;
            this.templateCache = templateCache;

            contentWriter = GetTemplateWriter("Balances.tpl");
        }

        public string GetPage()
        {
            // setup master page and the content page section providers
            var writer = SetupMasterPage("HEAD", "BODY");
            var selectorWriter = GetTemplateWriter("Common.tpl").GetWriter("SELECTOR");
            contentWriter.RegisterFieldProvider("BODY", "SELECTOR", selectorWriter);

            writer.SelectProvider("HEAD");
            writer.AppendSection(true);

            writer.SelectProvider("BODY");
            writer.SelectProvider("SELECTOR");
            GetSelector(writer);
            writer.SelectSection("CONTENT");
            GetTransactionRows(writer);
            writer.DeselectSection();
            writer.AppendAll();

            return writer.GetContent();
        }

        public string GetTransactionRows()
        {
            var writer = contentWriter.GetWriter("CONTENT", true);
            GetTransactionRows(writer);
            return writer.GetContent();
        }

        private void GetTransactionRows(ITemplateWriter writer = null)
        {

            // get budget records for the period
            var records = this.budgetService
                .GetPeriodBalancesWithSummary<PeriodBalance>(this.pageScope.PeriodId);

            var grandTotal = records.FirstOrDefault(r => r.Level == 0) ?? new PeriodBalance
            {
                Balance = 0m,
                PreviousBalance = 0m,
                ProjectedBalance = 0m
            };

            var details = records.Where(r => r.Level > 0)
                .OrderBy(l => l.BankAccountId)
                .ThenBy(l => l.Level)
                .ThenBy(l => l.BudgetLineName);

            writer.SelectSection("ALL_ACCOUNTS");
            writer.SetSectionFields(grandTotal, SectionOptions.AppendDeselect);
            writer.SelectSection("ROWS");

            foreach (var record in details)
            {
                var sectionName = record.Level == 1 ? "ROW_S" : "ROW_D";
                writer.SetSectionFields(sectionName, record, SectionOptions.AppendDeselect);
                writer.AppendSection(true);
                writer.SelectSection("ROWS");
            }

            writer.DeselectSection();
            writer.AppendSection();
        }

    }

}