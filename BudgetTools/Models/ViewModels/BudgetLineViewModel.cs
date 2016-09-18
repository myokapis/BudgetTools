using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BudgetTools.Models.ViewModels
{
  public class BudgetLineViewModel
  {
    public int BudgetLineId { get; set; }
    public int BankAccountId { get; set; }
    public decimal AccruedAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal PlannedAmount { get; set; }
  }
}