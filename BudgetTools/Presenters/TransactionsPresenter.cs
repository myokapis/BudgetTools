using System;
using System.Collections.Generic;
using System.Linq;
using BudgetTools.Classes;
using BudgetTools.Models.DomainModels;
using BudgetToolsBLL.Services;
using TemplateEngine;

namespace BudgetTools.Presenters
{

    public interface ITransactionsPresenter
    {
        string GetEditor(int transactionId);
        string GetEditor(Transaction transaction = null);
        string GetPage();
        string GetTransactionRows();
    }

    public class TransactionsPresenter : MasterPresenter, ITransactionsPresenter
    {

        public TransactionsPresenter(IBudgetService budgetService, IPageScope pageScope, ITemplateCache templateCache,
            IWebCache webCache)
        {
            this.budgetService = budgetService;
            this.pageScope = pageScope;
            this.templateCache = templateCache;
            this.webCache = webCache;

            contentWriter = GetTemplateWriter("Transactions.tpl");
        }

        public string GetEditor(int transactionId)
        {
            var transaction = this.budgetService.GetTransaction<Transaction>(transactionId);
            return GetEditor(transaction);
        }

        public string GetEditor(Transaction transaction = null)
        {
            var writer = contentWriter.GetWriter("EDITOR");
            GetEditor(writer, transaction);
            return writer.GetContent();
        } 

        private void GetEditor(ITemplateWriter writer, Transaction transaction = null)
        {

            // handle null transaction
            var xact = transaction ?? new Transaction()
            {
                TransactionTypeCode = "S",
                MappedTransactions = new List<MappedTransaction>()
            };

            // setup dropdowns TODO: cache the transaction type dropdown
            var budgetLineDefinitions = this.webCache.BudgetLineDefinitions();
            var transactionTypeDefinitions = new FieldDefinitions();

            transactionTypeDefinitions.SetDropdowns(new DropdownDefinition("TRANSACTION_TYPE", "TransactionTypeCode",
                new List<Option>
                {
                    new Option(){ Text = "Standard", Value = "S" },
                    new Option(){ Text = "Transfer", Value = "T" }
                }));

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
            writer.AppendSection();
        }

        public string GetPage()
        {
            var writer = SetupMasterPage("HEAD", "BODY");
            var selectorWriter = GetTemplateWriter("Common.tpl").GetWriter("SELECTOR");
            this.contentWriter.RegisterFieldProvider("BODY", "SELECTOR", selectorWriter);

            writer.SelectProvider("HEAD");
            writer.AppendSection(true);

            writer.SelectProvider("BODY");
            writer.SelectProvider("SELECTOR");
            writer.AppendSection(true);

            writer.SelectSection("ROW");
            GetTransactionRows(writer);
            writer.DeselectSection();

            writer.SelectSection("EDITOR");
            GetEditor(writer, null);

            return writer.GetContent();
        }

        public string GetTransactionRows()
        {
            var writer = contentWriter.GetWriter("ROW");
            writer.SelectSection("ROW");
            return writer.GetContent();
        }

        private void GetTransactionRows(ITemplateWriter writer)
        {
            // get transaction data
            var transactions = budgetService.GetTransactions<Transaction>(this.pageScope.BankAccountId, this.pageScope.PeriodId);

            foreach (var transaction in transactions.OrderByDescending(t => t.TransactionDate))
            {
                writer.SetSectionFields<Transaction>(transaction, SectionOptions.Set);
                writer.SetField("Class", transaction.IsMapped ? "transaction-row-mapped" : "transaction-row");
                writer.AppendSection();
            }

        }

    }

}