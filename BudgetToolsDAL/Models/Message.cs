using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetToolsDAL.Models
{

    [NotMapped]
    public class Message
    {
        public int ErrorLevel { get; set; }
        public string MessageText { get; set; }
    }

}
