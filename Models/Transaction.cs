using System;

namespace FinanceManager.WinForms.Models
{
    public class Transaction
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = "Expense";
    }
}
