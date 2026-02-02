using FinanceManager.WinForms.Models;
using FinanceManager.WinForms.Repositories;
using System;
using System.Collections.Generic;

namespace FinanceManager.WinForms.Services
{
    public class TransactionService
    {
        private readonly TransactionRepository _repo = new TransactionRepository();
        private readonly BudgetService? _budgetService;

        public TransactionService(BudgetService? budgetService = null)
        {
            _budgetService = budgetService;
        }

        // Returns alert messages (e.g., budget warnings) produced by this add
        public List<string> Add(Transaction t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (t.Amount == 0) throw new ArgumentException("Amount cannot be zero");
            if (t.Date == default) t.Date = DateTime.Now;
            var id = _repo.Add(t);

            var alerts = new List<string>();
            try
            {
                if (_budgetService != null && !string.IsNullOrEmpty(t.Category))
                {
                    var month = t.Date.ToString("yyyy-MM");
                    var budget = _budgetService.GetFor(t.Category, month);
                    if (budget != null)
                    {
                        var total = _repo.GetTotalForCategoryMonth(t.Category, month);
                        if (total >= budget.Amount) alerts.Add($"Budget exceeded for {t.Category} ({total:C} / {budget.Amount:C})");
                        else if (total >= budget.Amount * 0.8m) alerts.Add($"You have reached 80% of your budget for {t.Category} ({total:C} / {budget.Amount:C})");
                    }
                }
            }
            catch { }

            return alerts;
        }

        public List<Transaction> GetAll() => _repo.GetAll();
        public void Update(Transaction t) => _repo.Update(t);
        public void Delete(long id) => _repo.Delete(id);
    }
}
