using FinanceManager.WinForms.Models;
using FinanceManager.WinForms.Services;
using FinanceManager.WinForms.Utils;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FinanceManager.WinForms.UI
{
    public class MonthlySummaryForm : Form
    {
        private readonly TransactionService _txService;
        private readonly BudgetService _budgetService;
        private DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false };
        private DateTimePicker _dtMonth = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM", ShowUpDown = true, Width = 140, Location = new Point(12, 12) };
        private Button _btnLoad = new Button { Text = "Load", Location = new Point(164, 10) };

        public MonthlySummaryForm(TransactionService txService, BudgetService budgetService)
        {
            _txService = txService ?? throw new ArgumentNullException(nameof(txService));
            _budgetService = budgetService ?? throw new ArgumentNullException(nameof(budgetService));
            Text = "Monthly Summary"; Width = 900; Height = 500; StartPosition = FormStartPosition.CenterParent;
            Controls.Add(_dtMonth); Controls.Add(_btnLoad); Controls.Add(_grid);
            _btnLoad.Click += (s, e) => LoadSummary();
            Load += (s, e) => { _dtMonth.Value = DateTime.Now; SetupGrid(); LoadSummary(); ThemeManager.Apply(this); };
        }

        private void SetupGrid()
        {
            _grid.Columns.Clear();
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Category", DataPropertyName = "Category", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Total", DataPropertyName = "Total", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Budget", DataPropertyName = "Budget", Width = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Status", DataPropertyName = "Status", Width = 180 });
        }

        private void LoadSummary()
        {
            var month = _dtMonth.Value.ToString("yyyy-MM");
            var txs = _txService.GetAll().Where(t => t.Date.ToString("yyyy-MM") == month).ToList();
            var categories = txs.GroupBy(t => string.IsNullOrEmpty(t.Category) ? "Uncategorized" : t.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(x => x.Amount)
                }).ToList();

            var table = new DataTable();
            table.Columns.Add("Category");
            table.Columns.Add("Total");
            table.Columns.Add("Budget");
            table.Columns.Add("Status");

            foreach (var c in categories)
            {
                var budget = _budgetService.GetFor(c.Category, month);
                var budgetAmount = budget?.Amount ?? 0m;
                var status = "OK";
                if (budget != null)
                {
                    if (c.Total >= budgetAmount) status = "Exceeded";
                    else if (c.Total >= budgetAmount * 0.8m) status = "Near limit (>=80%)";
                }
                table.Rows.Add(c.Category, c.Total.ToString("C"), budgetAmount == 0m ? "-" : budgetAmount.ToString("C"), status);
            }

            _grid.DataSource = table;
        }
    }
}
