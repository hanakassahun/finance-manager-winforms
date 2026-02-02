using System.Drawing;
using System.Windows.Forms;

namespace FinanceManager.WinForms.Utils
{
    public enum Theme { Light, Dark }
    public static class ThemeManager
    {
        public static Theme Current { get; private set; } = Theme.Light;
        public static void Apply(Form f)
        {
            if (Current == Theme.Dark) ApplyDark(f); else ApplyLight(f);
        }
        public static void Toggle(Form f) { Current = Current == Theme.Dark ? Theme.Light : Theme.Dark; Apply(f); }
        private static void ApplyDark(Form f)
        {
            f.BackColor = Color.FromArgb(30, 30, 30);
            f.ForeColor = Color.White;
            foreach (Control c in f.Controls) ApplyControlDark(c);
        }
        private static void ApplyLight(Form f)
        {
            f.BackColor = SystemColors.Control;
            f.ForeColor = Color.Black;
            foreach (Control c in f.Controls) ApplyControlLight(c);
        }
        private static void ApplyControlDark(Control c)
        {
            c.BackColor = Color.FromArgb(45, 45, 48);
            c.ForeColor = Color.White;
            if (c is Button b) { b.FlatStyle = FlatStyle.Flat; }
            if (c is DataGridView dgv) { dgv.BackgroundColor = Color.FromArgb(30, 30, 30); dgv.ForeColor = Color.White; dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48); dgv.EnableHeadersVisualStyles = false; }
            foreach (Control child in c.Controls) ApplyControlDark(child);
        }
        private static void ApplyControlLight(Control c)
        {
            c.BackColor = SystemColors.Control;
            c.ForeColor = Color.Black;
            if (c is Button b) { b.FlatStyle = FlatStyle.System; }
            if (c is DataGridView dgv) { dgv.BackgroundColor = SystemColors.Window; dgv.ForeColor = Color.Black; dgv.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control; dgv.EnableHeadersVisualStyles = true; }
            foreach (Control child in c.Controls) ApplyControlLight(child);
        }
    }
}
