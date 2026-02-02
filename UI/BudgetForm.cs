using FinanceManager.WinForms.Services;
using FinanceManager.WinForms.Utils;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FinanceManager.WinForms.UI
{
    public class BudgetForm : Form
    {
        private readonly BudgetService _service;
        private readonly CategoryService _categoryService;
        private ListBox _list = new ListBox { Dock = DockStyle.Left, Width = 320 };
        private ComboBox _cbCategory = new ComboBox { Location = new Point(340, 20), Width = 200, DropDownStyle = ComboBoxStyle.DropDown };
        private DateTimePicker _dtMonth = new DateTimePicker { Location = new Point(340, 56), Width = 120, Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM", ShowUpDown = true };
        private TextBox _txtAmount = new TextBox { Location = new Point(340, 96), Width = 120 };
        private Button _btnSave = new Button { Text = "Save", Location = new Point(340, 132) };
        private Button _btnDelete = new Button { Text = "Delete", Location = new Point(420, 132) };

        public BudgetForm(BudgetService service, CategoryService categoryService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            Text = "Budgets"; Width = 600; Height = 420; StartPosition = FormStartPosition.CenterParent;
            Controls.Add(_list); Controls.Add(_cbCategory); Controls.Add(_dtMonth); Controls.Add(_txtAmount); Controls.Add(_btnSave); Controls.Add(_btnDelete);
            _btnSave.Click += BtnSave_Click; _btnDelete.Click += BtnDelete_Click;
            Load += (s, e) => { LoadCategories(); RefreshList(); ThemeManager.Apply(this); };
            _list.DoubleClick += (s, e) => { if (_list.SelectedItem is string sname) LoadSelected(); };
        }

        private void LoadCategories()
        {
            _cbCategory.Items.Clear();
            foreach (var c in _categoryService.GetAll()) _cbCategory.Items.Add(c.Name);
        }

        private void RefreshList()
        {
            _list.Items.Clear();
            foreach (var b in _service.GetAll()) _list.Items.Add($"{b.Month} — {b.Category} — {b.Amount:C}");
        }

        private void LoadSelected()
        {
            var sel = _list.SelectedItem as string; if (string.IsNullOrEmpty(sel)) return;
            // parse naive
            var parts = sel.Split('—').Select(p => p.Trim()).ToArray();
            if (parts.Length >= 3)
            {
                _dtMonth.Value = DateTime.ParseExact(parts[0], "yyyy-MM", null);
                _cbCategory.Text = parts[1];
                _txtAmount.Text = parts[2].Replace("$", "").Replace(",", "");
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            var category = _cbCategory.Text.Trim(); if (string.IsNullOrEmpty(category)) { MessageBox.Show("Category required"); return; }
            var month = _dtMonth.Value.ToString("yyyy-MM");
            if (!decimal.TryParse(_txtAmount.Text, out var amt)) { MessageBox.Show("Invalid amount"); return; }
            var b = new Models.Budget { Category = category, Month = month, Amount = amt };
            _service.AddOrUpdate(b);
            LoadCategories(); RefreshList();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_list.SelectedIndex < 0) return;
            var item = _service.GetAll()[_list.SelectedIndex];
            if (MessageBox.Show($"Delete budget {item.Category} {item.Month}?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _service.Delete(item.Id);
                RefreshList();
            }
        }
    }
}
