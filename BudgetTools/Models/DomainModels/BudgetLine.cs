using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BudgetTools.Models.DomainModels
{
  public class BudgetLine
  {
    [Key()]
    public int BudgetLineId { get; set; }
    public string DisplayName { get; set; }
    public bool IsAccrued { get; set; }
    public int BudgetCategoryId { get; set; }
    public string BudgetCategoryName { get; set; }
    public string BudgetCategoryDesc { get; set; }
    public int BudgetGroupId { get; set; }
    public string BudgetGroupName { get; set; }
    public string BudgetGroupDesc { get; set; }
  }
}