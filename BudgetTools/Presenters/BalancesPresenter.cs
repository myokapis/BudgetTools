using System.Linq;
using System.Threading.Tasks;
using BudgetTools.Classes;
using BudgetTools.Models;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface IBalancesPresenter
    {
        Task<string> GetPage(PageScope pageScope);
        Task<string> GetTransactionRows(int periodId);
    }

    public class BalancesPresenter : MasterPresenter, IBalancesPresenter
    {

        public BalancesPresenter(IBudgetService budgetService, ITemplateCache templateCache) : base(templateCache, budgetService)
        { }

        public async Task<string> GetPage(PageScope pageScope)
        {
            // setup master page and the content page section providers
            await SetupWriters("Master.tpl", "Balances.tpl");
            SetupMasterPage("HEAD", "BODY");

            // get common template and register it
            var tplCommon = await GetTemplateWriter("Common.tpl");
            contentWriter.RegisterFieldProvider("BODY", "SELECTOR", tplCommon.GetWriter("SELECTOR"));

            // write head
            WriteMasterSection("HEAD");

            // write body
            await WriteMasterSection("BODY", async (writer) =>
            {
                await WriteSelector(writer, pageScope);
                writer.SelectSection("CONTENT");
                await GetTransactionRows(pageScope.PeriodId, writer);
                //writer.DeselectSection();
            });

            return GetContent();
        }

        public async Task<string> GetTransactionRows(int periodId)
        {
            await SetupContentWriter("Balances.tpl");
            var writer = contentWriter.GetWriter("CONTENT", true);
            await GetTransactionRows(periodId, writer);
            writer.AppendAll(); // added this
            return writer.GetContent();
        }

        private async Task GetTransactionRows(int periodId, ITemplateWriter writer = null)
        {

            // get budget records for the period
            var records = await budgetService.GetPeriodBalancesWithSummary<PeriodBalance>(periodId);

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

            foreach (var record in details)
            {
                writer.SelectSection("ROWS");
                var sectionName = record.Level == 1 ? "ROW_S" : "ROW_D";
                writer.SetSectionFields(sectionName, record, SectionOptions.AppendDeselect);
                writer.AppendSection(true);
            }

            //writer.DeselectSection();
            //writer.AppendSection();
        }

    }

}