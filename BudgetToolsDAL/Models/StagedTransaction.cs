using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace BudgetToolsDAL.Models
{

  public class StagedTransaction
  {
    [Key()]
    public int StageTransactionId { get; set; }
    public int BankAccountId { get; set; }
    [MaxLength(255)]
    public string TransactionNo { get; set; }
    public DateTime TransactionDate { get; set; }
    [MaxLength(255)]
    public string TransactionDesc { get; set; }
    public int? CheckNo { get; set; }
    public double Amount { get; set; }
  }

}