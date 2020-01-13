using System.Collections.Generic;
using System.Linq;
using BudgetTools.Classes;
using BudgetTools.Enums;
using BudgetTools.Models;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface IAdminPresenter
    {
        string GetBalanceTransfer();
        string GetCloseCurrentPeriod();
        string GetCloseCurrentPeriodMessages(IEnumerable<Message> messages);
        string GetSaveTransferMessages(IEnumerable<Message> messages);
        string GetTransferBudgetLines(int bankAccountId, Direction direction);
        string GetPage();
    }

    public class AdminPresenter : MasterPresenter, IAdminPresenter
    {

        public AdminPresenter(IBudgetService budgetService, IPageScope pageScope, ITemplateCache templateCache,
            IWebCache webCache)
        {
            this.budgetService = budgetService;
            this.pageScope = pageScope;
            this.templateCache = templateCache;
            this.webCache = webCache;

            contentWriter = GetTemplateWriter("Admin.tpl");
        }

        public string GetBalanceTransfer()
        {
            var writer = contentWriter.GetWriter("TRANSFER_BALANCE", true);
            var sectionNames = new string[] { "From", "To" };

            var budgetLines =
                budgetService.GetBudgetLineBalances<BudgetLineBalance>(pageScope.PeriodId, pageScope.BankAccountId)
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

            writer.AppendAll();
            return writer.GetContent();
        }

        public string GetCloseCurrentPeriod()
        {
            var writer = contentWriter.GetWriter("CLOSE_PERIOD", true);
            writer.AppendSection();
            return writer.GetContent();
        }

        public string GetCloseCurrentPeriodMessages(IEnumerable<Message> messages)
        {
            var writer = contentWriter.GetWriter("MESSAGE", true);
            writer.SetMultiSectionFields(messages);
            return writer.GetContent();
        }

        public string GetPage()
        {
            var writer = SetupMasterPage("HEAD", "BODY");
            var selectorWriter = GetTemplateWriter("Common.tpl").GetWriter("SELECTOR");
            contentWriter.RegisterFieldProvider("BODY", "SELECTOR", selectorWriter);

            writer.SelectProvider("HEAD");
            writer.AppendSection(true);

            writer.SelectProvider("BODY");
            writer.SelectProvider("SELECTOR");
            GetSelector(writer);
            writer.AppendAll();

            return writer.GetContent();
        }

        public string GetSaveTransferMessages(IEnumerable<Message> messages)
        {
            var writer = contentWriter.GetWriter("TRANSFER_MESSAGE", true);
            writer.SetMultiSectionFields(messages);
            return writer.GetContent();
        }

        public string GetTransferBudgetLines(int bankAccountId, Direction direction)
        {
            var writer = contentWriter.GetWriter("ROW", true);
            var budgetLines =
                budgetService.GetBudgetLineBalances<BudgetLineBalance>(pageScope.CurrentPeriodId, bankAccountId)
                .Where(b => direction == Direction.To || b.Balance > 0m)
                .OrderBy(l => l.BudgetLineName);

            writer.SetMultiSectionFields(budgetLines);
            return writer.GetContent();
        }

    }

}