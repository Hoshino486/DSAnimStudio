// 递归本地化 WinForms 控件树：
// 在窗体 Load 之后遍历所有子控件，把 .Text 用 Loc.T 包一层。
// 只处理含英文字母且尚未被处理的文本，避免影响纯数字/占位符。
using System;
using System.Windows.Forms;
using DSAnimStudio.Localization;

namespace DSAnimStudio
{
    public static class WinformsLoc
    {
        private const string DoneTag = "DSAS_Localized";

        public static void Apply(Form form)
        {
            if (form == null) return;
            try
            {
                if (!string.IsNullOrEmpty(form.Text) && !form.Text.StartsWith(DoneTag))
                    form.Text = Loc.T(form.Text);
                Walk(form.Controls);
            }
            catch { }
        }

        private static void Walk(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                if (c == null) continue;
                try
                {
                    string old = c.Text;
                    if (!string.IsNullOrEmpty(old) && !HasChinese(old) && HasAsciiLetter(old) && !old.StartsWith(DoneTag))
                    {
                        c.Text = Loc.T(old);
                    }
                }
                catch { }

                try
                {
                    if (c is ToolTip) continue;
                }
                catch { }

                if (c is MenuStrip ms)
                {
                    foreach (ToolStripItem it in ms.Items) WalkItem(it);
                }
                if (c is StatusStrip ss)
                {
                    foreach (ToolStripItem it in ss.Items) WalkItem(it);
                }
                if (c is ToolStrip ts)
                {
                    foreach (ToolStripItem it in ts.Items) WalkItem(it);
                }
                if (c is ListBox lb)
                {
                    for (int i = 0; i < lb.Items.Count; i++)
                    {
                        if (lb.Items[i] is string s && !HasChinese(s) && HasAsciiLetter(s))
                            lb.Items[i] = Loc.T(s);
                    }
                }
                if (c is ComboBox cb)
                {
                    for (int i = 0; i < cb.Items.Count; i++)
                    {
                        if (cb.Items[i] is string s && !HasChinese(s) && HasAsciiLetter(s))
                            cb.Items[i] = Loc.T(s);
                    }
                }
                if (c is DataGridView dgv)
                {
                    foreach (DataGridViewColumn col in dgv.Columns)
                    {
                        if (!string.IsNullOrEmpty(col.HeaderText) && !HasChinese(col.HeaderText) && HasAsciiLetter(col.HeaderText))
                            col.HeaderText = Loc.T(col.HeaderText);
                    }
                }
                if (c is TreeView tv)
                {
                    WalkNodes(tv.Nodes);
                }

                if (c.HasChildren)
                    Walk(c.Controls);
            }
        }

        private static void WalkItem(ToolStripItem it)
        {
            if (it == null) return;
            try
            {
                if (!string.IsNullOrEmpty(it.Text) && !HasChinese(it.Text) && HasAsciiLetter(it.Text))
                    it.Text = Loc.T(it.Text);
            }
            catch { }
            if (it is ToolStripDropDownItem dd)
            {
                foreach (ToolStripItem sub in dd.DropDownItems) WalkItem(sub);
            }
        }

        private static void WalkNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode n in nodes)
            {
                if (!string.IsNullOrEmpty(n.Text) && !HasChinese(n.Text) && HasAsciiLetter(n.Text))
                    n.Text = Loc.T(n.Text);
                if (n.Nodes.Count > 0) WalkNodes(n.Nodes);
            }
        }

        private static bool HasChinese(string s)
        {
            foreach (char ch in s)
                if (ch >= 0x4E00 && ch <= 0x9FFF) return true;
            return false;
        }

        private static bool HasAsciiLetter(string s)
        {
            foreach (char ch in s)
                if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z')) return true;
            return false;
        }
    }
}
