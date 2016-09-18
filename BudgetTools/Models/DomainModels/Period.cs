using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BudgetTools.Models.DomainModels
{
  public class Period
  {
    [Key]
    public int PeriodId { get; set; }
    [Required]
    public DateTime PeriodStartDate { get; set; }
    [Required]
    public DateTime PeriodEndDate { get; set; }
    [Required]
    public bool IsOpen { get; set; }
  }
}