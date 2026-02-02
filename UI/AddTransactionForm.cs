using FinanceManager.WinForms.Models;
using FinanceManager.WinForms.Services;
using FinanceManager.WinForms.Utils;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FinanceManager.WinForms.UI
{
    public class AddTransactionForm : Form
    {
        private readonly TransactionService _service;
        private Transaction? _editing;

        private DateTimePicker dtDate = new DateTimePicker { Width = 200 };
        private TextBox txtAmount = new TextBox { Width = 200 };
        private ComboBox cbType = new ComboBox { Width = 200 };
        private ComboBox cbCategory = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDown };
        private TextBox txtDescription = new TextBox { Width = 200, Multiline = true, Height = 60 };
        private Button btnSave = new Button { Text = "Save", Width = 100 };
        private readonly FinanceManager.WinForms.Services.CategoryService _categoryService;

        public AddTransactionForm(TransactionService service, FinanceManager.WinForms.Services.CategoryService categoryService, Transaction? editing = null)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _editing = editing;
            Text = editing == null ? "Add Transaction" : "Edit Transaction";
            Width = 360; Height = 360; StartPosition = FormStartPosition.CenterParent;

            var y = 20;
            Controls.Add(new Label { Text = "Date", Location = new Point(12, y) }); dtDate.Location = new Point(12, y + 20); Controls.Add(dtDate); y += 60;
            Controls.Add(new Label { Text = "Amount", Location = new Point(12, y) }); txtAmount.Location = new Point(12, y + 20); Controls.Add(txtAmount); y += 60;
            Controls.Add(new Label { Text = "Type", Location = new Point(12, y) }); cbType.Items.AddRange(new[] { "Expense", "Income" }); cbType.SelectedIndex = 0; cbType.Location = new Point(12, y + 20); Controls.Add(cbType); y += 60;
            Controls.Add(new Label { Text = "Category", Location = new Point(12, y) }); cbCategory.Location = new Point(12, y + 20); Controls.Add(cbCategory); y += 60;
            Controls.Add(new Label { Text = "Description", Location = new Point(12, y) }); txtDescription.Location = new Point(12, y + 20); Controls.Add(txtDescription); y += 90;

            btnSave.Location = new Point(12, y); btnSave.Click += BtnSave_Click; Controls.Add(btnSave);

            LoadCategories();
            if (_editing != null) LoadForEdit(_editing);
            Load += (s, e) => ThemeManager.Apply(this);
        }

        private void LoadForEdit(Transaction t)
        {
            dtDate.Value = t.Date;
            txtAmount.Text = t.Amount.ToString();
            cbType.SelectedItem = t.Type;
            cbCategory.Text = t.Category;
            txtDescription.Text = t.Description;
        }

        private void LoadCategories()
        {
            try
            {
                var list = _categoryService.GetAll();
                cbCategory.Items.Clear();
                foreach (var c in list) cbCategory.Items.Add(c.Name);
            }
            catch { }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (!decimal.TryParse(txtAmount.Text, out var amount)) { MessageBox.Show("Invalid amount", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var tx = _editing ?? new Transaction();
            tx.Date = dtDate.Value;
            tx.Amount = amount;
            tx.Type = cbType.SelectedItem?.ToString() ?? "Expense";
            tx.Category = cbCategory.Text.Trim();
            tx.Description = txtDescription.Text.Trim();

            try
            {
                if (_editing == null)
                {
                    var alerts = _service.Add(tx);
                    if (alerts != null && alerts.Count > 0)
                    {
                        MessageBox.Show(string.Join("\n", alerts), "Budget Alerts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    _service.Update(tx);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
