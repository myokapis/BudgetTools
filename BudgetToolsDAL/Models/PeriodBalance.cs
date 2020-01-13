using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetToolsDAL.Models
{

    public class PeriodBalance
    {
        [Key, Column(Order=0)]
        public int PeriodId { get; set; }

        [Key, Column(Order = 1)]
        public int BankAccountId { get; set; }

        [Key, Column(Order = 2)]
        public int BudgetLineId { get; set; }

        public string BankAccountName { get; set; }
        public string BudgetGroupName { get; set; }
        public string BudgetCategoryName { get; set; }
        public string BudgetLineName { get; set; }

        [Required, Column(TypeName = "Money")]
        public decimal PreviousBalance { get; set; }

        [Required, Column(TypeName = "Money")]
        public decimal Balance { get; set; }

        [Required, Column(TypeName = "Money")]
        public decimal ProjectedBalance { get; set; }
  }

}