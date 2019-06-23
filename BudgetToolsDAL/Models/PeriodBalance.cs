using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetToolsDAL.Models
{
  public class PeriodBalance
  {
    [Key, Column(Order=0)]
    public int PeriodId { get; set; }
    [Key, Column(Order = 1)]
    public int BankAccountId { get; set; }
    [Key, Column(Order = 2)]
    public int BudgetLineId { get; set; }
    [Required, Column(TypeName = "Money")]
    public decimal PreviousBalance { get; set; }
    [Required, Column(TypeName = "Money")]
    public decimal Balance { get; set; }
    [Required, Column(TypeName = "Money")]
    public decimal ProjectedBalance { get; set; }

    public virtual BudgetLine BudgetLine { get; set; }
    public virtual BankAccount BankAccount { get; set; }
  }
}