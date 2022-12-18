using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BudgetTools.Models;
using BudgetTools.Presenters;
using BudgetToolsBLL.Services;

public class Bubble
{
    public int Popped { get; set; }
}

namespace BudgetTools.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ITransactionsPresenter transactionsPresenter;
        private readonly IBudgetService budgetService;

        public TransactionsController(ITransactionsPresenter transactionsPresenter, IBudgetService budgetService)
        {
            this.budgetService = budgetService;
            this.transactionsPresenter = transactionsPresenter;
        }

        public async Task<ActionResult> Index(PageScope pageScope)
        {
            if (!ModelState.IsValid)
                pageScope = await budgetService.GetPageScope<PageScope>();

            return Content(await transactionsPresenter.GetPage(pageScope), "text/html");
        }

        public async Task<JsonResult> ChangePageScope(PageScope pageScope)
        {
            if (!ModelState.IsValid) 
                pageScope = await budgetService.GetPageScope<PageScope>();

            var output = new
            {
                transactions = await transactionsPresenter.GetTransactionRows(pageScope),
                editor = await transactionsPresenter.GetEditor(null)
            };

            return Json(output);
        }

        public async Task<JsonResult> GetTransaction(int transactionId)
        {
            var transaction = await budgetService.GetTransaction<Transaction>(transactionId);

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

        public async Task<JsonResult> UpdateTransaction([FromForm]int transactionId, string transactionTypeCode, string recipient,
            string notes, List<MappedTransaction> mappedTransactions)
        {
            //var mappedTransactions = new List<MappedTransaction> { mappedTransaction };
            // TODO: need to return json indicating success or failure w/ message
            await budgetService.UpdateTransaction(transactionId, transactionTypeCode, recipient, notes, mappedTransactions);

            return Json("");
        }

    }

}