using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BudgetTools.Models.DomainModels
{
  
  public class TransactionType
  {
    [Key()]
    [Required()]
    public string TransactionTypeCode { get; set; }
    [MaxLength(255)]
    public string TransactionTypeDesc { get; set; }
  }
}