using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTools.Models.DomainModels
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

    //[Association("Period", "PeriodId", "PeriodId")]
    //public Period Period { get; set; }
    //[Association("BudgetLine", "BudgetLineId", "BudgetLineId")]
    //public BudgetLine BudgetLine { get; set; }
  }
}