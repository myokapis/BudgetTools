using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTools.Models.ViewModels
{
  public class AllocationViewModel
  {
    [Key, Column(Order = 0)]
    public int PeriodId { get; set; }
    [Key, Column(Order = 1)]
    public int BudgetLineId { get; set; }
    [Required, DisplayFormat(DataFormatString="{0:C}")]
    public decimal PlannedAmount { get; set; }
    [Required, DisplayFormat(DataFormatString = "{0:C}")]
    public decimal AllocatedAmount { get; set; }
    [Required, DisplayFormat(DataFormatString = "{0:C}")]
    public decimal AccruedAmount { get; set; }
    [Required, DisplayFormat(DataFormatString = "{0:C}")]
    public decimal ActualAmount { get; set; }
    [Required, DisplayFormat(DataFormatString = "{0:C}")]
    public decimal RemainingAmount { get; set; }
    [Required, DisplayFormat(DataFormatString = "{0:C}")]
    public decimal AccruedBalance { get; set; }
    [Required, MinLength(1), MaxLength(100)]
    public string BudgetCategoryName { get; set; }
    [Required, MinLength(1), MaxLength(100)]
    public string BudgetLineName { get; set; }
    [Required]
    public bool IsAccrued { get; set; }
  }
}