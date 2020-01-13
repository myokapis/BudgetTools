using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BudgetToolsDAL.Models
{
    public class BudgetGroup
    {
        [Key()]
        public int BudgetGroupId { get; set; }

        [Required, MinLength(0), MaxLength(50)]
        public string BudgetGroupName { get; set; }

        [Required, MinLength(0), MaxLength(255)]
        public string BudgetGroupDesc { get; set; }

        public virtual ICollection<BudgetCategory> BudgetCategories { get; set; }
    }
}