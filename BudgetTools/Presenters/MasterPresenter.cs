using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetToolsBLL.Services;
using BudgetTools.Models;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public class MasterPresenter
    {

        protected readonly IBudgetService budgetService;
        protected ITemplateWriter contentWriter;
        protected ITemplateWriter masterWriter;
        protected readonly ITemplateCache templateCache;

        public MasterPresenter(ITemplateCache templateCache, IBudgetService budgetService)
        {
            this.templateCache = templateCache;
            this.budgetService = budgetService;
        }

        protected string GetContent()
        {
            masterWriter.AppendAll();
            return masterWriter.GetContent();
        }

        protected async Task<ITemplateWriter> GetTemplateWriter(string fileName = null)
        {
            var template = await templateCache.GetTemplateAsync(fileName);
            return new TemplateWriter(template);
        }

        protected async Task<ITemplateWriter> SetupContentWriter(string contentFileName)
        {
            contentWriter = new TemplateWriter(await templateCache.GetTemplateAsync(contentFileName));
            return contentWriter;
        }

        protected async Task<ITemplateWriter> SetupWriters(string masterFileName, string contentFileName)
        {
            masterWriter = new TemplateWriter(await templateCache.GetTemplateAsync(masterFileName));
            contentWriter = new TemplateWriter(await templateCache.GetTemplateAsync(contentFileName));

            return masterWriter;
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
            masterWriter.Reset();

            // reset all providers
            head?.Reset();
            body?.Reset();
            tail?.Reset();

            // setup the master page section providers
            if (head != null) masterWriter.RegisterFieldProvider("HEAD", head);
            if (body != null) masterWriter.RegisterFieldProvider("BODY", body);
            if (tail != null) masterWriter.RegisterFieldProvider("TAIL", tail);

            return masterWriter;
        }

        protected void WriteMasterSection(string providerName)
        {
            masterWriter.SelectProvider(providerName);
            masterWriter.AppendSection(true);
        }

        protected async Task WriteMasterSection(string providerName, Func<ITemplateWriter, Task> pageBuilder)
        {
            masterWriter.SelectProvider(providerName);
            await pageBuilder.Invoke(masterWriter);
            masterWriter.AppendSection(true);
        }

        // TODO: cache the periods and bank accounts on app start up
        protected async Task WriteSelector(ITemplateWriter writer, PageScope pageScope)
        {
            writer.SelectProvider("SELECTOR");

            // get drop down data
            var periods = (await budgetService.GetPeriods<Option>()).OrderByDescending(p => p.Value).ToList();
            var bankAccounts = await budgetService.GetBankAccounts<Option>(true);

            var definitions = new FieldDefinitions();
            definitions.SetDropdowns(
                new DropdownDefinition("BUDGET_PERIODS", "PeriodId", periods),
                new DropdownDefinition("BANK_ACCOUNTS", "BankAccountId", bankAccounts)
            );

            writer.SetSectionFields(pageScope, SectionOptions.AppendDeselect, definitions);
        }

        //protected ITemplateWriter GetTemplateWriter(string fileName = null)
        //{
        //    var tpl = this.templateCache.GetTemplate(fileName);
        //    return new TemplateWriter(tpl);
        //}

        //protected ITemplateWriter SetupMasterPage(string headSection = null, string bodySection = null, string tailSection = null)
        //{
        //    var head = headSection != null ? contentWriter.GetWriter(headSection) : null;
        //    var body = bodySection != null ? contentWriter.GetWriter(bodySection) : null;
        //    var tail = tailSection != null ? contentWriter.GetWriter(tailSection) : null;
        //    return SetupMasterPage(head, body, tail);
        //}

        //protected ITemplateWriter SetupMasterPage(ITemplateWriter head = null, ITemplateWriter body = null, ITemplateWriter tail = null)
        //{
        //    masterWriter = GetTemplateWriter("Master.tpl");
        //    masterWriter.Reset();

        //    // reset all providers
        //    head?.Reset();
        //    body?.Reset();
        //    tail?.Reset();

        //    // setup the master page section providers
        //    if (head != null) this.masterWriter.RegisterFieldProvider("HEAD", head);
        //    if (body != null) this.masterWriter.RegisterFieldProvider("BODY", body);
        //    if (tail != null) this.masterWriter.RegisterFieldProvider("TAIL", tail);

        //    return masterWriter;
        //}

    }

}