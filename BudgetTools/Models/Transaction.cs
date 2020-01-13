using System;
using System.Collections.Generic;
using TemplateEngine.Formats;

namespace BudgetTools.Models
{

    public class Transaction
    {
        public int TransactionId { get; set; }
        public string TransactionNo { get; set; }
        [FormatDate("d")]
        public DateTime TransactionDate { get; set; }
        public string TransactionDesc { get; set; }
        public int? CheckNo { get; set; }
        [FormatCurrency(2)]
        public decimal Amount { get; set; }
        public string TransactionTypeCode { get; set; }
        public string Recipient { get; set; }
        public string Notes { get; set; }
        public bool IsMapped { get; set; }
        public virtual List<MappedTransaction> MappedTransactions { get; set; }
    }

}