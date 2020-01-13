using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BudgetTools.Classes;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;
using BudgetTools.Models;

namespace BudgetTools.Controllers
{
    public class TransactionsController : Controller
    {
        protected ITransactionsPresenter transactionsPresenter;
        protected IBudgetService budgetService;

        public TransactionsController(ITransactionsPresenter transactionsPresenter, IBudgetService budgetService)
        {
            this.budgetService = budgetService;
            this.transactionsPresenter = transactionsPresenter;
        }

        public ActionResult Index()
        {
            return Content(this.transactionsPresenter.GetPage());
        }

        public JsonResult ChangeBankAccount(int bankAccountId)
        {
            IPageScope pageScope = (IPageScope)this.Session.Contents["pageScope"];
            pageScope.BankAccountId = bankAccountId;

            var output = new
            {
                transactions = this.transactionsPresenter.GetTransactionRows(),
                editor = this.transactionsPresenter.GetEditor(null)
            };

            return Json(output);
        }

        public JsonResult ChangePeriod(int periodId)
        {
            IPageScope pageScope = (IPageScope)this.Session.Contents["pageScope"];
            pageScope.PeriodId = periodId;

            var output = new
            {
                transactions = this.transactionsPresenter.GetTransactionRows(),
                editor = this.transactionsPresenter.GetEditor(null)
            };

            return Json(output);
        }

        public JsonResult GetTransaction(int transactionId)
        {
            var transaction = budgetService.GetTransaction<Transaction>(transactionId);

            var data = new
            {
                amount = transaction.Amount,
                isMapped = transaction.IsMapped,
                mappedTransactions = transaction.MappedTransactions.Select(m => new
                {
                    amount = m.Amount,
                    budgetLineId = m.BudgetLineId
                }),
                notes = transaction.Notes,
                recipient = transaction.Recipient,
                transactionId = transaction.TransactionId,
                transactionType = transaction.TransactionTypeCode
            };

            return Json(data);
        }

        public void UpdateTransaction(int transactionId, string transactionTypeCode, string recipient,
            string notes, List<MappedTransaction> mappedTransactions)
        {
            budgetService.UpdateTransaction(transactionId, transactionTypeCode, recipient, notes,
                mappedTransactions);
        }

    }

}