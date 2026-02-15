using FinanceManager.WinForms.Models;
using FinanceManager.WinForms.Services;
using FinanceManager.WinForms.Utils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ClosedXML.Excel;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

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
        private Button _btnExportExcel = new Button { Text = "Export Excel", Location = new System.Drawing.Point(968, 14) };
        private Button _btnExportPdf = new Button { Text = "Export PDF", Location = new System.Drawing.Point(1068, 14) };

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
            _top.Controls.Add(_btnExportExcel);
            _top.Controls.Add(_btnExportPdf);

            _cbTypeFilter.Items.AddRange(new[] { "All", "Expense", "Income" });
            _cbTypeFilter.SelectedIndex = 0;
            _btnFilter.Click += (s, e) => RefreshList();
            _btnClear.Click += (s, e) => { _txtSearch.Clear(); _cbCategoryFilter.SelectedIndex = 0; _cbTypeFilter.SelectedIndex = 0; RefreshList(); };
            _btnExportExcel.Click += ExportExcel_Click;
            _btnExportPdf.Click += ExportPdf_Click;

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

        private void ExportExcel_Click(object? sender, EventArgs e)
        {
            if (_bindingList == null || _bindingList.Count == 0)
            {
                MessageBox.Show("No transactions to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog();
            sfd.Filter = "Excel Workbook|*.xlsx|CSV (Comma delimited)|*.csv";
            sfd.FileName = "transactions";
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var path = sfd.FileName;
                if (Path.GetExtension(path).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                {
                    using var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Transactions");
                    // headers
                    ws.Cell(1, 1).Value = "Date";
                    ws.Cell(1, 2).Value = "Amount";
                    ws.Cell(1, 3).Value = "Description";
                    ws.Cell(1, 4).Value = "Category";
                    ws.Cell(1, 5).Value = "Type";
                    var row = 2;
                    foreach (var t in _bindingList)
                    {
                        ws.Cell(row, 1).Value = t.Date;
                        ws.Cell(row, 2).Value = (double)t.Amount;
                        ws.Cell(row, 3).Value = t.Description;
                        ws.Cell(row, 4).Value = t.Category;
                        ws.Cell(row, 5).Value = t.Type;
                        row++;
                    }
                    ws.Columns().AdjustToContents();
                    wb.SaveAs(path);
                }
                else
                {
                    // CSV
                    var sb = new StringBuilder();
                    sb.AppendLine("Date,Amount,Description,Category,Type");
                    foreach (var t in _bindingList)
                    {
                        var desc = (t.Description ?? string.Empty).Replace("\"", "\"\"");
                        var cat = (t.Category ?? string.Empty).Replace("\"", "\"\"");
                        sb.AppendLine($"\"{t.Date:o}\",{t.Amount},\"{desc}\",\"{cat}\",\"{t.Type}\"");
                    }
                    File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true)); // BOM
                }

                MessageBox.Show("Export completed.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportPdf_Click(object? sender, EventArgs e)
        {
            if (_bindingList == null || _bindingList.Count == 0)
            {
                MessageBox.Show("No transactions to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog();
            sfd.Filter = "PDF File|*.pdf";
            sfd.FileName = "transactions.pdf";
            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                var doc = new PdfDocument();
                var page = doc.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Arial", 10, XFontStyle.Regular);
                double margin = 40;
                double y = margin;
                double lineHeight = font.GetHeight();

                // headers
                gfx.DrawString("Date", font, XBrushes.Black, new XPoint(margin, y));
                gfx.DrawString("Amount", font, XBrushes.Black, new XPoint(margin + 120, y));
                gfx.DrawString("Description", font, XBrushes.Black, new XPoint(margin + 220, y));
                gfx.DrawString("Category", font, XBrushes.Black, new XPoint(margin + 420, y));
                gfx.DrawString("Type", font, XBrushes.Black, new XPoint(margin + 520, y));
                y += lineHeight + 6;

                foreach (var t in _bindingList)
                {
                    if (y + lineHeight > page.Height - margin)
                    {
                        page = doc.AddPage();
                        page.Size = PdfSharpCore.PageSize.A4;
                        gfx = XGraphics.FromPdfPage(page);
                        y = margin;
                    }

                    gfx.DrawString(t.Date.ToString("yyyy-MM-dd"), font, XBrushes.Black, new XPoint(margin, y));
                    gfx.DrawString(t.Amount.ToString("C"), font, XBrushes.Black, new XPoint(margin + 120, y));
                    var desc = t.Description ?? string.Empty;
                    gfx.DrawString(desc, font, XBrushes.Black, new XRect(margin + 220, y, 190, lineHeight), XStringFormats.TopLeft);
                    gfx.DrawString(t.Category ?? string.Empty, font, XBrushes.Black, new XPoint(margin + 420, y));
                    gfx.DrawString(t.Type ?? string.Empty, font, XBrushes.Black, new XPoint(margin + 520, y));
                    y += lineHeight + 6;
                }

                using var ms = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write);
                doc.Save(ms);
                MessageBox.Show("PDF export completed.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("PDF export failed: " + ex.Message, "Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
