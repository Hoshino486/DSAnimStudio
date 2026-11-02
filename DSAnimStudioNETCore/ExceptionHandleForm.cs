using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DSAnimStudio.Localization;

namespace DSAnimStudio
{
    public partial class ExceptionHandleForm : Form
    {
        public ExceptionHandleForm()
        {
            InitializeComponent();
            WinformsLoc.Apply(this);
            this.Shown += (_, __) => WinformsLoc.Apply(this);
        }

        public bool CreateBackup;

        public void InitializeForException(Exception ex, string messageBeforeColon, string currentFileNameForBackup)
        {
            labelHeader.Text = DSAnimStudio.Localization.Loc.T("[Logged to DSAnimStudio_ErrorLog.txt]\n") + $"{messageBeforeColon}:";
            textBoxError.Text = ex.ToString();
            checkBoxBackup.Text = DSAnimStudio.Localization.Loc.T("Create emergency backup file of last saved version before continuing: \r\n") + System.IO.Path.GetFullPath(currentFileNameForBackup);
            checkBoxBackup.Checked = false;
        }

        private void ExceptionHandleForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonInfiniteLoop_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
            Close();
        }

        private void ExceptionHandleForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            CreateBackup = checkBoxBackup.Checked;
        }
    }
}
