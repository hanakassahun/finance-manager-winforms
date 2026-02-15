namespace FinanceManager.WinForms.Models
{
    public class Budget
    {
        public long Id { get; set; }
        public string Category { get; set; } = string.Empty;
        // Month in format YYYY-MM
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
namespace FinanceManager.WinForms.Models
{
    public class Budget
    {
        public long Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty; // YYYY-MM
        public decimal Amount { get; set; }
    }
}
