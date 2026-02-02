using FinanceManager.WinForms.Services;
using FinanceManager.WinForms.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FinanceManager.WinForms.UI
{
    public class MainForm : Form
    {
        private readonly TransactionService _txService;
        private readonly CategoryService _category_service;
        private readonly BudgetService _budgetService;
        private MenuStrip _menu = new MenuStrip();

        public MainForm()
        {
            Text = "Finance Manager";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterScreen;

            // services
            _category_service = new CategoryService();
            _budgetService = new BudgetService();
            _txService = new TransactionService(_budgetService);

            var transactionsMenu = new ToolStripMenuItem("Transactions");
            var add = new ToolStripMenuItem("Add") { ShortcutKeys = Keys.Control | Keys.N };
            add.Click += (s, e) => OpenAddTransaction();
            var view = new ToolStripMenuItem("View") { ShortcutKeys = Keys.Control | Keys.D };
            view.Click += (s, e) => OpenTransactionsView();
            transactionsMenu.DropDownItems.Add(add);
            transactionsMenu.DropDownItems.Add(view);

            var viewMenu = new ToolStripMenuItem("View");
            var themeToggle = new ToolStripMenuItem("Toggle Theme");
            themeToggle.Click += (s, e) => { ThemeManager.Toggle(this); };
            viewMenu.DropDownItems.Add(themeToggle);

            var manageMenu = new ToolStripMenuItem("Manage");
            var categoriesItem = new ToolStripMenuItem("Categories");
            categoriesItem.Click += (s, e) => { var f = new CategoriesForm(_category_service); f.ShowDialog(this); };
            var budgetsItem = new ToolStripMenuItem("Budgets");
            budgetsItem.Click += (s, e) => { var f = new BudgetForm(_budgetService, _category_service); f.ShowDialog(this); };
            var summaryItem = new ToolStripMenuItem("Monthly Summary");
            summaryItem.Click += (s, e) => { var f = new MonthlySummaryForm(_txService, _budgetService); f.ShowDialog(this); };
            manageMenu.DropDownItems.Add(categoriesItem);
            manageMenu.DropDownItems.Add(budgetsItem);
            manageMenu.DropDownItems.Add(summaryItem);

            _menu.Items.Add(transactionsMenu);
            _menu.Items.Add(viewMenu);
            _menu.Items.Add(manageMenu);
            Controls.Add(_menu);

            Font = new Font("Segoe UI", 9);
            var welcome = new Label { Text = "Welcome — use the Transactions menu to get started.", AutoSize = true, Location = new Point(24, 48), Font = new Font("Segoe UI", 12) };
            Controls.Add(welcome);
            Load += (s, e) => ThemeManager.Apply(this);
        }

        private void OpenAddTransaction()
        {
            var f = new AddTransactionForm(_txService, _categoryService);
            f.ShowDialog(this);
        }

        private void OpenTransactionsView()
        {
            var f = new TransactionsViewForm(_txService, _categoryService);
            f.Show();
        }
    }
}
