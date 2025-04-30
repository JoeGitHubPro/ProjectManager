using ProjectManager.Constants;
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Models.CapitalManagement
{
    public class CashFlowModel
    {
        [Key]
        public int TransactionId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionType Type { get; set; } // Income or Expense
    }


}
