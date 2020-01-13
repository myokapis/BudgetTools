using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetToolsDAL.Models
{
  public class Transaction
  {
    public Transaction()
    {
      TransactionTypeCode = "S";
      IsMapped = false;
    }

    [Key]
    public int TransactionId { get; set; }
    public int BankAccountId { get; set; }
    [MaxLength(255)]
    public string TransactionNo { get; set; }
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
    public DateTime TransactionDate { get; set; }
    [MaxLength(255)]
    public string TransactionDesc { get; set; }
    public int? CheckNo { get; set; }
    [Column(TypeName = "Money")]
    public decimal Amount { get; set; }
    [Required]
    [MinLength(1)]
    [MaxLength(1)]
    public string TransactionTypeCode { get; set; }
    [MaxLength(255)]
    public string Recipient { get; set; }
    [MaxLength(255)]
    public string Notes { get; set; }
    [Required]
    public bool IsMapped { get; set; }
    public virtual List<MappedTransaction> MappedTransactions { get; set; }
  }
}