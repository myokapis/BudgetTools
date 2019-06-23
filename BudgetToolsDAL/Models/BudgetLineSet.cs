using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetToolsDAL.Models
{

    [Table("vwBudgetLineSets")]
    public class BudgetLineSet
    {
        [Key]
        [Column(Order = 1)]
        public int BudgetLineSetId { get; set; }
        [Key]
        [Column(Order = 2)]
        public int BudgetLineId { get; set; }
        public DateTime EffInDate { get; set; }
        public DateTime? EffOutDate { get; set; }
        public string DisplayName { get; set; }
        public string BudgetLineName { get; set; }
        public string BudgetLineDesc { get; set; }
        public bool IsAccrued { get; set; }
        public int BudgetCategoryId { get; set; }
        public string BudgetCategoryName { get; set; }
        public string BudgetCategoryDesc { get; set; }
        public int BudgetGroupId { get; set; }
        public string BudgetGroupName { get; set; }
        public string BudgetGroupDesc { get; set; }
    }

}
