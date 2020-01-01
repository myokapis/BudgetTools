using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetTools.Models
{

    public class UploadPath
    {
        [Key()]
        [ForeignKey("BankAccount")]
        public int BankAccountId { get; set; }
        public string BankAccountName { get; set; }
        public string FilePath { get; set; }

        public BankAccount BankAccount { get; set; }
    }

}