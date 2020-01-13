using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetToolsDAL.Models
{

    public class PeriodBudgetLine
    {
        [Key()]
        public int BudgetLineId { get; set; }
        public string BudgetLineName { get; set; }
        public string BudgetCategoryName { get; set; }

        [Column(TypeName = "money")]
        public decimal PlannedAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AllocatedAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AccruedAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal ActualAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal RemainingAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal AccruedBalance { get; set; }

        [Column(TypeName = "money")]
        public decimal TotalCashAmount { get; set; }

        public bool IsAccrued { get; set; }
    }

}