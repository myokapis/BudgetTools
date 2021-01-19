using System.Linq;
using System.Threading.Tasks;
using BudgetTools.Classes;
using BudgetTools.Models;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface IBudgetPresenter
    {
        Task<string> GetPage(PageScope pageScope);
        Task<string> GetTransactionRows(PageScope pageScope);
    }

    public class BudgetPresenter : MasterPresenter, IBudgetPresenter
    {

        public BudgetPresenter(IBudgetService budgetService, ITemplateCache templateCache) : base(templateCache, budgetService)
        {
        }

        public async Task<string> GetTransactionRows(PageScope pageScope)
        {
            await SetupContentWriter("Budget.tpl");
            var writer = contentWriter.GetWriter("TBODY", true);
            await GetTransactionRows(pageScope, writer);
            writer.AppendAll();
            return writer.GetContent();
        }

        private async Task GetTransactionRows(PageScope pageScope, ITemplateWriter writer = null)
        {

            // get budget records for the period
            var records = (await budgetService.GetPeriodBudgetWithSummary<PageScope, PeriodBudgetLine>(pageScope))
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

        public async Task<string> GetPage(PageScope pageScope)
        {
            // setup master page and the content page section providers
            await SetupWriters("Master.tpl", "Budget.tpl");
            SetupMasterPage("HEAD", "BODY");

            // get common template and register it
            var tplCommon = await GetTemplateWriter("Common.tpl");
            contentWriter.RegisterFieldProvider("BODY", "SELECTOR", tplCommon.GetWriter("SELECTOR"));

            // write head
            WriteMasterSection("HEAD");


            //writer.SelectProvider("BODY");
            //writer.SelectProvider("SELECTOR");
            //await GetSelector(writer, pageScope);
            //writer.SelectSection("TBODY");
            //await GetTransactionRows(pageScope, writer);
            //writer.AppendAll();

            // write body
            await WriteMasterSection("BODY", async (writer) =>
            {
                await WriteSelector(writer, pageScope);
                writer.SelectSection("TBODY");
                await GetTransactionRows(pageScope, writer);
                //writer.DeselectSection();
            });

            return GetContent();
        }

    }

}