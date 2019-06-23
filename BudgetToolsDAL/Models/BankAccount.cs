using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BudgetToolsDAL.Models
{

    public class BankAccount
    {
        [Key()]
        public int BankAccountId { get; set; }
        [Required, StringLength(50)]
        public string BankAccountName { get; set; }
        [Required, StringLength(1)]
        public string BankAccountType { get; set; }
        [Required, StringLength(50)]
        public string BankName { get; set; }
        [Required, StringLength(10)]
        public string BankRoutingNumber { get; set; }
        [Required, StringLength(20)]
        public string BankAccountNumber { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public string UploadValidator { get; set; }
    }

}