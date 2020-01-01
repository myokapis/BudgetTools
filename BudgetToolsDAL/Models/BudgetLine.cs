using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetToolsDAL.Models
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

        [Column(TypeName = "Money")]
        public decimal Balance { get; set; }
    }
    // TODO: rename this once the conflict is gone
    public class BudgetLine1
    {
        [Key()]
        public int BudgetLineId { get; set; }
        public string DisplayName { get; set; }
        public bool IsAccrued { get; set; }
        public int BudgetCategoryId { get; set; }
    }
}