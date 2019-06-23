using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BudgetToolsDAL.Models
{
  public class BudgetCategory
  {
    [Key]
    public int BudgetCategoryId { get; set; }

    [Required, MinLength(0), MaxLength(50)]
    public string BudgetCategoryName { get; set; }
    
    [Required, MinLength(0), MaxLength(255)]
    public string BudgetCategoryDesc { get; set; }
    
    [Required]
    public int BudgetGroupId { get; set; }
    
    public virtual ICollection<BudgetLine> BudgetLines { get; set; }

  }
}