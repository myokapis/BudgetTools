﻿using System.Collections.Generic;

namespace BudgetToolsBLL.Models
{

    public interface ITransaction
    {
        int TransactionId { get; set; }
        string TransactionTypeCode { get; set; }
        string Recipient { get; set; }
        string Notes { get; set; }
        List<IMappedTransaction> MappedTransactions { get; set; }
    }

    public interface IMappedTransaction
    {
        int BudgetLineId { get; set; }
        decimal Amount { get; set; }
    }

}
