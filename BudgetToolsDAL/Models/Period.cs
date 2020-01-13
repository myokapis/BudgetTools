using System;
using System.ComponentModel.DataAnnotations;

namespace BudgetToolsDAL.Models
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