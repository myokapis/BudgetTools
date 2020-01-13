using System.Linq;
using BudgetToolsBLL.Services;
using BudgetTools.Classes;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public class MasterPresenter
    {

        protected IBudgetService budgetService;
        protected ITemplateWriter contentWriter;
        protected ITemplateWriter masterWriter;
        protected IPageScope pageScope;
        protected ITemplateCache templateCache;
        protected IWebCache webCache = null;

        // TODO: cache the periods and bank accounts on app start up
        protected void GetSelector(ITemplateWriter writer)
        {
            // get drop down data
            var periods = this.budgetService.GetPeriods<Option>().OrderByDescending(p => p.Value).ToList();
            var bankAccounts = this.budgetService.GetBankAccounts<Option>(true);

            var definitions = new FieldDefinitions();
            definitions.SetDropdowns(
                new DropdownDefinition("BUDGET_PERIODS", "PeriodId", periods),
                new DropdownDefinition("BANK_ACCOUNTS", "BankAccountId", bankAccounts)
            );

            writer.SetSectionFields<IPageScope>(pageScope, SectionOptions.AppendDeselect, definitions);
        }

        protected ITemplateWriter GetTemplateWriter(string fileName = null)
        {
            var tpl = this.templateCache.GetTemplate(fileName);
            return new TemplateWriter(tpl);
        }

        protected ITemplateWriter SetupMasterPage(string headSection = null, string bodySection = null, string tailSection = null)
        {
            var head = headSection != null ? contentWriter.GetWriter(headSection) : null;
            var body = bodySection != null ? contentWriter.GetWriter(bodySection) : null;
            var tail = tailSection != null ? contentWriter.GetWriter(tailSection) : null;
            return SetupMasterPage(head, body, tail);
        }

        protected ITemplateWriter SetupMasterPage(ITemplateWriter head = null, ITemplateWriter body = null, ITemplateWriter tail = null)
        {
            masterWriter = GetTemplateWriter("Master.tpl");
            masterWriter.Reset();

            // reset all providers
            head?.Reset();
            body?.Reset();
            tail?.Reset();

            // setup the master page section providers
            if (head != null) this.masterWriter.RegisterFieldProvider("HEAD", head);
            if (body != null) this.masterWriter.RegisterFieldProvider("BODY", body);
            if (tail != null) this.masterWriter.RegisterFieldProvider("TAIL", tail);

            return masterWriter;
        }

    }

}