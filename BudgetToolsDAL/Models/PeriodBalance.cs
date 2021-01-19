using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BudgetToolsDAL.Models
{

    [Table("vwPeriodBalances", Schema = "dbo")]
    public class PeriodBalance
    {
        [Key, Column("PeriodID", Order=0)]
        public int PeriodId { get; set; }

        [Key, Column("BankAccountID", Order = 1)]
        public int BankAccountId { get; set; }

        [Key, Column("BudgetLineID", Order = 2)]
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