using System.Collections.Generic;

namespace BudgetToolsBLL.Models
{

    //public interface ITransaction
    //{
    //    int TransactionId { get; set; }
    //    string TransactionTypeCode { get; set; }
    //    string Recipient { get; set; }
    //    string Notes { get; set; }
    //    List<IMappedTransaction> MappedTransactions { get; set; }
    //}

    public class MappedTransaction
    {
        public int BudgetLineId { get; set; }
        public decimal Amount { get; set; }
    }

}
