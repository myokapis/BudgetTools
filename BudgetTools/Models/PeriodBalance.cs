using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TemplateEngine.Formats;

namespace BudgetTools.Models
{

    [Table("vwPeriodBalances")]
    public class PeriodBalance
    {
        public int PeriodId { get; set; }
        public int BankAccountId { get; set; }
        public string BankAccountName { get; set; }
        public int BudgetLineId { get; set; }
        public string BudgetLineName { get; set; }
        public string BudgetCategoryName { get; set; }

        [FormatCurrency(2)]
        public decimal PreviousBalance { get; set; }

        [FormatCurrency(2)]
        public decimal Balance { get; set; }

        [FormatCurrency(2)]
        public decimal ProjectedBalance { get; set; }

        public int Level { get; set; }
    }

}