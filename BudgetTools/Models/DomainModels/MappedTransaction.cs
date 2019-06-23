using TemplateEngine.Formats;

namespace BudgetTools.Models.DomainModels
{

    public class MappedTransaction
    {
        public int MappedTransactionId { get; set; }
        public int TransactionId { get; set; }
        public int BudgetLineId { get; set; }
        public decimal Amount { get; set; }
    }

}