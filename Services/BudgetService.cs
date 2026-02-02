using FinanceManager.WinForms.Models;
using FinanceManager.WinForms.Repositories;
using System.Collections.Generic;

namespace FinanceManager.WinForms.Services
{
    public class BudgetService
    {
        private readonly BudgetRepository _repo = new BudgetRepository();
        public long AddOrUpdate(Budget b) => _repo.AddOrUpdate(b);
        public List<Budget> GetAll() => _repo.GetAll();
        public Budget? GetFor(string category, string month) => _repo.GetFor(category, month);
        public void Delete(long id) => _repo.Delete(id);
    }
}
