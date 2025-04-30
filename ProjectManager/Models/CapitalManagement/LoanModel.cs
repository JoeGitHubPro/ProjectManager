using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProjectManager.Models.CapitalManagement
{
    public class LoanModel
    {
        [Key]
        public int LoanId { get; set; }
        public string LenderId { get; set; }  // Foreign Key to IdentityUser
        public string LenderName { get; set; }
        public decimal AmountLoaned { get; set; }
        public DateTime LoanDate { get; set; }
        public decimal InterestRate { get; set; } // Optional
        public DateTime DueDate { get; set; }
        public decimal OutstandingBalance { get; set; }
        public bool IsRepaid { get; set; }

        public virtual IdentityUser Lender { get; set; }  // Navigation property
    }

}
