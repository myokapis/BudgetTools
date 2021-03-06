﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetToolsDAL.Models
{
  public class Allocation
  {
    [Key, Column(Order=0)]
    public int PeriodId { get; set; }
    [Key, Column(Order = 1)]
    public int BudgetLineId { get; set; }
    [Key, Column(Order = 2)]
    public int BankAccountId { get; set; }
    [Required, Column(TypeName = "Money")]
    public decimal PlannedAmount { get; set; }
    [Required, Column(TypeName="Money")]
    public decimal AllocatedAmount { get; set; }
    [Required, Column(TypeName="Money")]
    public decimal AccruedAmount { get; set; }

    public virtual BudgetLine BudgetLine { get; set;  }
  }
}