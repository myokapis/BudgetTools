using System;

namespace BudgetToolsBLL.Models
{

    public class StagedTransaction
    {
        public double Amount { get; set; }
        public double Balance { get; set; }
        public int BankAccountId { get; set; }
        public int? CheckNo { get; set; }
        public int RowNo { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionDesc { get; set; }
        public string TransactionNo { get; set; }
    }

}
