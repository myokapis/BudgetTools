using System.Linq;
using System.Threading.Tasks;
using BudgetTools.Models;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface IImportPresenter
    {
        Task<string> GetPage(PageScope pageScope);
    }

    public class ImportPresenter : MasterPresenter, IImportPresenter
    {

        public ImportPresenter(IBudgetService budgetService, ITemplateCache templateCache) :
            base(templateCache, budgetService)
        {
        }

        // TODO: change the bank account and period queries to accept an optional condition
        protected async Task GetBankAccountRows(ITemplateWriter writer)
        {
            var bankAccounts = await budgetService.GetBankAccounts<BankAccount>();
            writer.SetMultiSectionFields("ROW", bankAccounts.Where(a => a.IsActive));
        }

        public async Task<string> GetPage(PageScope pageScope)
        {
            // setup master page and the content page section providers
            await SetupWriters("Master.tpl", "Import.tpl");
            SetupMasterPage("HEAD", "BODY");

            // write head
            WriteMasterSection("HEAD");

            // write body
            await WriteMasterSection("BODY", async (writer) =>
            {
                writer.SetSectionFields(pageScope, SectionOptions.Set);
                await GetBankAccountRows(writer);
            });

            return GetContent();
        }

    }

}