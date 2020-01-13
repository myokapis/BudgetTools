using TemplateEngine.Formats;

namespace BudgetTools.Models
{

    public class MappedTransaction
    {
        public int MappedTransactionId { get; set; }
        public int TransactionId { get; set; }
        public int BudgetLineId { get; set; }

        [FormatCurrency]
        public decimal Amount { get; set; }
    }

}