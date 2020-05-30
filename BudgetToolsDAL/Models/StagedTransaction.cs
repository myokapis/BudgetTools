using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetToolsDAL.Models
{

    public class StagedTransaction
    {
        [Key()]
        public int StagedTransactionId { get; set; }
        public int BankAccountId { get; set; }
        [MaxLength(255)]
        public string TransactionNo { get; set; }
        public DateTime TransactionDate { get; set; }
        [MaxLength(255)]
        public string TransactionDesc { get; set; }
        public int? CheckNo { get; set; }
        public double Amount { get; set; }
        public double Balance { get; set; }
        public int RowNo { get; set; }
    }

}