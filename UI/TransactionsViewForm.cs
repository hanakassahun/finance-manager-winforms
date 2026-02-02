using FinanceManager.WinForms.Models;
using FinanceManager.WinForms.Services;
using FinanceManager.WinForms.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FinanceManager.WinForms.UI
{
    public class TransactionsViewForm : Form
    {
        private readonly TransactionService _service;
        private readonly CategoryService _categoryService;
        private BindingList<Transaction> _bindingList = new BindingList<Transaction>();
        private DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };
        private Panel _top = new Panel { Dock = DockStyle.Top, Height = 56 };
        private TextBox _txtSearch = new TextBox { Width = 200, Location = new System.Drawing.Point(8, 16) };
        private ComboBox _cbCategoryFilter = new ComboBox { Width = 160, Location = new System.Drawing.Point(220, 16), DropDownStyle = ComboBoxStyle.DropDownList };
        private ComboBox _cbTypeFilter = new ComboBox { Width = 120, Location = new System.Drawing.Point(392, 16), DropDownStyle = ComboBoxStyle.DropDownList };
        private DateTimePicker _dtFrom = new DateTimePicker { Width = 140, Location = new System.Drawing.Point(528, 16) };
        private DateTimePicker _dtTo = new DateTimePicker { Width = 140, Location = new System.Drawing.Point(676, 16) };
        private Button _btnFilter = new Button { Text = "Filter", Location = new System.Drawing.Point(828, 14) };
        private Button _btnClear = new Button { Text = "Clear", Location = new System.Drawing.Point(900, 14) };

        public TransactionsViewForm(TransactionService service, CategoryService categoryService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            Text = "Transactions";
            Width = 900; Height = 500; StartPosition = FormStartPosition.CenterParent;
            Controls.Add(_grid);
            Controls.Add(_top);
            _top.Controls.Add(_txtSearch);
            _top.Controls.Add(_cbCategoryFilter);
            _top.Controls.Add(_cbTypeFilter);
            _top.Controls.Add(_dtFrom);
            _top.Controls.Add(_dtTo);
            _top.Controls.Add(_btnFilter);
            _top.Controls.Add(_btnClear);

            _cbTypeFilter.Items.AddRange(new[] { "All", "Expense", "Income" });
            _cbTypeFilter.SelectedIndex = 0;
            _btnFilter.Click += (s, e) => RefreshList();
            _btnClear.Click += (s, e) => { _txtSearch.Clear(); _cbCategoryFilter.SelectedIndex = 0; _cbTypeFilter.SelectedIndex = 0; RefreshList(); };

            var cms = new ContextMenuStrip();
            var edit = new ToolStripMenuItem("Edit");
            var del = new ToolStripMenuItem("Delete");
            edit.Click += Edit_Click; del.Click += Del_Click;
            cms.Items.AddRange(new ToolStripItem[] { edit, del });
            _grid.ContextMenuStrip = cms;

            Load += (s, e) => { LoadFilters(); RefreshList(); ThemeManager.Apply(this); };
            _grid.DoubleClick += Grid_DoubleClick;
        }

        private void Grid_DoubleClick(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is Transaction tx) OpenEdit(tx);
        }

        private void Edit_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is Transaction tx) OpenEdit(tx);
        }

        private void OpenEdit(Transaction tx)
        {
            var f = new AddTransactionForm(_service, _categoryService, tx);
            if (f.ShowDialog(this) == DialogResult.OK) RefreshList();
        }

        private void Del_Click(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is Transaction tx)
            {
                if (MessageBox.Show("Delete transaction?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _service.Delete(tx.Id);
                    RefreshList();
                }
            }
        }

        private void LoadFilters()
        {
            _cbCategoryFilter.Items.Clear();
            _cbCategoryFilter.Items.Add("All");
            foreach (var c in _categoryService.GetAll()) _cbCategoryFilter.Items.Add(c.Name);
            _cbCategoryFilter.SelectedIndex = 0;
            _dtFrom.Value = DateTime.Now.AddMonths(-1);
            _dtTo.Value = DateTime.Now;
        }

        private void RefreshList()
        {
            var list = _service.GetAll();
            // Apply search & filters
            var q = list.AsQueryable();
            var txt = _txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(txt)) q = q.Where(t => (t.Description ?? string.Empty).IndexOf(txt, System.StringComparison.OrdinalIgnoreCase) >= 0 || (t.Category ?? string.Empty).IndexOf(txt, System.StringComparison.OrdinalIgnoreCase) >= 0);
            if (_cbCategoryFilter.SelectedIndex > 0)
            {
                var cat = _cbCategoryFilter.SelectedItem?.ToString();
                q = q.Where(t => t.Category == cat);
            }
            if (_cbTypeFilter.SelectedItem != null && _cbTypeFilter.SelectedItem.ToString() != "All")
            {
                var type = _cbTypeFilter.SelectedItem.ToString();
                q = q.Where(t => t.Type == type);
            }
            var from = _dtFrom.Value.Date; var to = _dtTo.Value.Date.AddDays(1).AddTicks(-1);
            q = q.Where(t => t.Date >= from && t.Date <= to);

            _bindingList = new BindingList<Transaction>(q.OrderByDescending(t => t.Date).ToList());
            _grid.DataSource = _bindingList;
            _grid.Columns[0].Visible = false; // hide Id
            _grid.AutoResizeColumns();
        }
    }
}
