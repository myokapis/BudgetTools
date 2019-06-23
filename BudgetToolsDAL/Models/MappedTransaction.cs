using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetToolsDAL.Models
{
  public class MappedTransaction
  {
    [Key]
    public int MappedTransactionId { get; set; }
    [Required]
    public int TransactionId { get; set; }
    [Required]
    public int BudgetLineId { get; set; }
    [Required, Column(TypeName="money")]
    public decimal Amount { get; set; }
    public virtual BudgetLine BudgetLine { get; set; }
  }
}