using FinanceManager.WinForms.Services;
using FinanceManager.WinForms.Utils;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FinanceManager.WinForms.UI
{
    public class CategoriesForm : Form
    {
        private readonly CategoryService _service;
        private ListBox _list = new ListBox { Dock = DockStyle.Left, Width = 300 };
        private TextBox _txtNew = new TextBox { Width = 200, Location = new Point(320, 20) };
        private Button _btnAdd = new Button { Text = "Add", Location = new Point(320, 56) };
        private Button _btnDelete = new Button { Text = "Delete", Location = new Point(320, 96) };

        public CategoriesForm(CategoryService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Text = "Categories"; Width = 520; Height = 400; StartPosition = FormStartPosition.CenterParent;
            Controls.Add(_list); Controls.Add(_txtNew); Controls.Add(_btnAdd); Controls.Add(_btnDelete);
            _btnAdd.Click += BtnAdd_Click; _btnDelete.Click += BtnDelete_Click;
            Load += (s, e) => { RefreshList(); ThemeManager.Apply(this); };
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var name = _txtNew.Text.Trim(); if (string.IsNullOrEmpty(name)) return;
            _service.Add(name);
            _txtNew.Clear(); RefreshList();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_list.SelectedItem is string name)
            {
                var cat = _service.GetAll().FirstOrDefault(c => c.Name == name);
                if (cat != null)
                {
                    if (MessageBox.Show($"Delete category '{name}'?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _service.Delete(cat.Id);
                        RefreshList();
                    }
                }
            }
        }

        private void RefreshList()
        {
            _list.Items.Clear();
            foreach (var c in _service.GetAll()) _list.Items.Add(c.Name);
        }
    }
}
