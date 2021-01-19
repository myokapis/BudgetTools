using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetTools.Classes;
using BudgetTools.Enums;
using BudgetTools.Models;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface IAdminPresenter
    {
        Task<string> GetBalanceTransfer(PageScope pageScope);
        Task<string> GetCloseCurrentPeriod();
        Task<string> GetCloseCurrentPeriodMessages(IEnumerable<Message> messages);
        Task<string> GetSaveTransferMessages(IEnumerable<Message> messages);
        Task<string> GetTransferBudgetLines(PageScope pageScope, Direction direction);
        Task<string> GetPage(PageScope pageScope);
    }

    public class AdminPresenter : MasterPresenter, IAdminPresenter
    {

        public AdminPresenter(IBudgetService budgetService, ITemplateCache templateCache) : base(templateCache, budgetService)
        {
        }

        public async Task<string> GetBalanceTransfer(PageScope pageScope)
        {
            await SetupContentWriter("Admin.tpl");
            var writer = contentWriter.GetWriter("TRANSFER_BALANCE", true);
            var sectionNames = new string[] { "From", "To" };

            // TODO: allow filter and order by to be passed in
            var budgetLines = 
                (await budgetService.GetBudgetLineBalances<BudgetLineBalance>(pageScope.PeriodId, pageScope.BankAccountId))
                .OrderBy(l => l.BudgetLineName);

            // set the From and To sections
            foreach (var sectionName in sectionNames)
            {
                writer.SelectSection("GRID_CONTAINER");
                writer.SetField("FROM_TO", sectionName);
                writer.SetMultiSectionFields("ROW", sectionName == "To" ? budgetLines :
                    budgetLines.Where(b => b.Balance > 0m));
                writer.AppendSection(true);
            }

            return writer.GetContent(true);
        }

        public async Task<string> GetCloseCurrentPeriod()
        {
            await SetupContentWriter("Admin.tpl");
            var writer = contentWriter.GetWriter("CLOSE_PERIOD", true);
            return writer.GetContent(true);
        }

        public async Task<string> GetCloseCurrentPeriodMessages(IEnumerable<Message> messages)
        {
            await SetupContentWriter("Admin.tpl");
            var writer = contentWriter.GetWriter("MESSAGE", true);
            writer.SetMultiSectionFields(messages);
            return writer.GetContent();
        }

        public async Task<string> GetPage(PageScope pageScope)
        {
            // setup master page and the content page section providers
            await SetupWriters("Master.tpl", "Admin.tpl");
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
            });

            return GetContent();
        }

        public async Task<string> GetSaveTransferMessages(IEnumerable<Message> messages)
        {
            await SetupContentWriter("Admin.tpl");
            var writer = contentWriter.GetWriter("TRANSFER_MESSAGE", true);
            writer.SetMultiSectionFields(messages);
            return writer.GetContent();
        }

        public async Task<string> GetTransferBudgetLines(PageScope pageScope, Direction direction)
        {
            await SetupContentWriter("Admin.tpl");
            var writer = contentWriter.GetWriter("ROW", true);

            var budgetLines = (await budgetService.GetBudgetLineBalances<BudgetLineBalance>(pageScope.CurrentPeriodId, pageScope.BankAccountId))
                .Where(b => direction == Direction.To || b.Balance > 0m)
                .OrderBy(l => l.BudgetLineName);

            writer.SetMultiSectionFields(budgetLines);
            return writer.GetContent(true);
        }

    }

}