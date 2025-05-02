namespace ProjectManager.Models.CapitalManagement
{
    public class BusinessBalanceModel
    {
        public int Id { get; set; }

        // The actual cash the business currently holds
        public decimal CashOnHand { get; set; }

        // The total amount loaned to the business but not yet repaid
        public decimal TotalOutstandingLoans { get; set; }

        // Total income from selling software, websites, etc.
        public decimal TotalSalesIncome { get; set; }

        // Total expenses (marketing, sponsorships, salaries, etc.)
        public decimal TotalExpenses { get; set; }

        // Optional: an emergency cash reserve
        public decimal EmergencyReserve { get; set; }

        // Computed property: The net balance = cash - expenses - outstanding loans
        public decimal NetBusinessBalance
        {
            get
            {
                return (CashOnHand + EmergencyReserve) - (TotalOutstandingLoans);
            }
        }

        // Related detailed transactions if you want to track them
        public List<CashFlowModel> CashFlows { get; set; }
        public List<LoanModel> Loans { get; set; }
    }

}
