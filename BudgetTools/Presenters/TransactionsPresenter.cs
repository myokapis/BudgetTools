using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetTools.Classes;
using BudgetTools.Models;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface ITransactionsPresenter
    {
        Task<string> GetEditor(int transactionId);
        Task<string> GetEditor(Transaction transaction = null);
        Task<string> GetPage(PageScope pageScope);
        Task<string> GetTransactionRows(PageScope pageScope);
    }

    public class TransactionsPresenter : MasterPresenter, ITransactionsPresenter
    {

        public TransactionsPresenter(IBudgetService budgetService, ITemplateCache templateCache) : base(templateCache, budgetService)
        {
        }

        public async Task<string> GetEditor(int transactionId)
        {
            await SetupContentWriter("Transactions.tpl");
            var transaction = await budgetService.GetTransaction<Transaction>(transactionId);
            return await GetEditor(transaction);
        }

        public async Task<string> GetEditor(Transaction transaction = null)
        {
            await SetupContentWriter("Transactions.tpl");
            var writer = contentWriter.GetWriter("EDITOR", true);
            await WriteEditor(writer, transaction);
            return writer.GetContent(true);
        } 

        private async Task WriteEditor(ITemplateWriter writer, Transaction transaction = null)
        {

            // handle null transaction
            var xact = transaction ?? new Transaction()
            {
                TransactionTypeCode = "S",
                MappedTransactions = new List<MappedTransaction>()
            };

            // setup dropdowns TODO: cache the transaction type and budget line dropdowns
            var transactionTypeDefinitions = new FieldDefinitions();

            transactionTypeDefinitions.SetDropdowns(new DropdownDefinition("TRANSACTION_TYPE", "TransactionTypeCode",
                new List<Option>
                {
                    new Option(){ Text = "Standard", Value = "S" },
                    new Option(){ Text = "Transfer", Value = "X" }
                }));

            var budgetLineDefinitions = new FieldDefinitions();

            budgetLineDefinitions.SetDropdowns(new DropdownDefinition("BUDGET_LINES", "BudgetLineId",
                await budgetService.GetBudgetLineSet<Option>()));

            // set up a default mapped transaction
            var mappedTransactions = xact.MappedTransactions;
            var mappedTransaction = new MappedTransaction();

            writer.SetSectionFields(xact, SectionOptions.Set, transactionTypeDefinitions);
            writer.SelectSection("EDITOR_ROWS");

            // add five rows of budget lines
            for(var i = 0; i < 5; i++)
            {
                var data = (mappedTransactions.Count > i) ? mappedTransactions[i] : mappedTransaction;
                writer.SetSectionFields(data, SectionOptions.AppendOnly, budgetLineDefinitions);
            }

            writer.DeselectSection();
        }

        public async Task<string> GetPage(PageScope pageScope)
        {
            // setup master page and the content page section providers
            await SetupWriters("Master.tpl", "Transactions.tpl");
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

                writer.SelectSection("ROW");
                await WriteTransactionRows(pageScope, writer);
                writer.DeselectSection();

                writer.SelectSection("EDITOR");
                await WriteEditor(writer, null);
            });

            return GetContent();
        }

        public async Task<string> GetTransactionRows(PageScope pageScope)
        {
            await SetupContentWriter("Transactions.tpl");
            var writer = contentWriter.GetWriter("ROW");
            await WriteTransactionRows(pageScope, writer);
            return writer.GetContent(true);
        }

        private async Task WriteTransactionRows(PageScope pageScope, ITemplateWriter writer)
        {
            // get transaction data
            var transactions = await budgetService.GetTransactions<Transaction>(pageScope.BankAccountId, pageScope.PeriodId);

            foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate))
            {
                writer.SetSectionFields(transaction, SectionOptions.Set);
                writer.SetField("Class", transaction.IsMapped ? "transaction-row-mapped" : "transaction-row");
                writer.AppendSection();
            }

        }

    }

}