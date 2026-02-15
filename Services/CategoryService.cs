using FinanceManager.WinForms.Models;
using FinanceManager.WinForms.Repositories;
using System.Collections.Generic;

namespace FinanceManager.WinForms.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _repo = new CategoryRepository();
        public List<Category> GetAll() => _repo.GetAll();
        public long Add(string name)
        {
            var c = new Category { Name = name };
            return _repo.Add(c);
        }
        public void Delete(long id) => _repo.Delete(id);
        public void Edit(long id, string name) => _repo.Update(id, name);
    }
}
