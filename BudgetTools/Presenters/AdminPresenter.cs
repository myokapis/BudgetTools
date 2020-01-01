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
        string CloseCurrentPeriod();
        string GetBalanceTransfer();
        string GetCloseCurrentPeriod();
        string GetTransferBudgetLines(int bankAccountId, Direction direction);
        string GetPage();
        string SaveTransfer(int bankAccountFromId, int budgetLineFromId,
            int bankAccountToId, int budgetLineToId, decimal amount, string note);
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

        public string CloseCurrentPeriod()
        {
            var data = budgetService.CloseCurrentPeriod<Option>();
            var writer = contentWriter.GetWriter("MESSAGE", true);
            writer.SetMultiSectionFields(data);
            return writer.GetContent();
        }

        public string GetBalanceTransfer()
        {
            var writer = contentWriter.GetWriter("TRANSFER_BALANCE", true);
            var sectionNames = new string[] { "From", "To" };
            var bankAccounts = this.budgetService.GetBankAccounts<Option>(true);
            var bankAccountId = int.Parse(bankAccounts.First().Value); // TODO: cache bank accounts and budget lines

            var budgetLines = budgetService.GetBudgetLineBalances<BudgetLineBalance>((int)bankAccountId)
                .OrderBy(l => l.BudgetLineName);

            // set the From and To sections
            foreach(var sectionName in sectionNames)
            {
                writer.SelectSection("GRID_CONTAINER");
                writer.SetField("FROM_TO", sectionName);
                writer.SetOptionFields("ACCOUNT", bankAccounts, bankAccounts.First().Value);
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

        public string GetPage()
        {
            var writer = SetupMasterPage("HEAD", "BODY");

            writer.SelectProvider("HEAD");
            writer.AppendSection(true);

            writer.SelectProvider("BODY");
            writer.AppendAll();

            return writer.GetContent();
        }

        public string GetTransferBudgetLines(int bankAccountId, Direction direction)
        {
            var writer = contentWriter.GetWriter("ROW", true);

            var budgetLines = budgetService.GetBudgetLineBalances<BudgetLineBalance>((int)bankAccountId)
                .Where(b => direction == Direction.To || b.Balance > 0m)
                .OrderBy(l => l.BudgetLineName);

            writer.SetMultiSectionFields(budgetLines);
            //writer.AppendAll();
            return writer.GetContent();
        }

        public string SaveTransfer(int bankAccountFromId, int budgetLineFromId,
            int bankAccountToId, int budgetLineToId, decimal amount, string note)
        {
            var data = budgetService.SaveTransfer<Option>(bankAccountFromId, budgetLineFromId,
                bankAccountToId, budgetLineToId, amount, note);

            var writer = contentWriter.GetWriter("TRANSFER_MESSAGE", true);
            writer.SetMultiSectionFields(data);
            return writer.GetContent();
        }

    }

}