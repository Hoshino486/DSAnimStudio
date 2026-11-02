using System;
using System.Windows.Forms;
using DSAnimStudio.Localization;

namespace DSAnimStudio
{

    /// <summary>
    /// 主类。
    /// </summary>
    public static class Program
    {
        public static string[] ARGS;
        public static Main MainInstance;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            //SoulsFormatsNEXT 待办
            //SoulsFormats.DCX.LoadOodleAction = () =>
            //{
            //    MessageBox.Show("To load Sekiro / Elden Ring / Armored Core 6 / Nightreign files, you need to give DS Anim Studio access to the 'oo2core_6_win64.dll', 'oo2core_8_win64.dll', or 'oo2core_9_win64.dll' file bundled next to the EXE of either game. Click OK to browse to this file now.");

            //    var browseDlg = new OpenFileDialog()
            //    {
            //        FileName = "",
            //        CheckFileExists = false,
            //        CheckPathExists = true,
            //        Title = "Select Oodle DLL",
            //        Filter = "DLLs (*.dll)|*.dll"
            //    };
            //    browseDlg.FileName = "oo2core_6_win64.dll";
            //    if (browseDlg.ShowDialog() == DialogResult.OK)
            //    {
            //        System.IO.File.Copy(browseDlg.FileName, DSAnimStudio.Main.Directory + "\\oo2core_6_win64.dll", true);
            //    }
            //};

            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            

            ARGS = args;
            //ARGS = new string[] { @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr\c4100_bak-chrbnd\chr\c4100\c4100.flver" };

#if !DEBUG
            try
            {
#endif
                MainInstance = new Main();

#if !DEBUG
        }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Loc.TF("Error occurred before DS Anim Studio had a chance to initialize (please report):\n\n{0}", ex.ToString()),
                    Loc.T("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif

            try
            {
                MainInstance.Run(Microsoft.Xna.Framework.GameRunBehavior.Synchronous);
            }
            finally
            {
                LiveRefresh.Memory.CloseHandle();
                MainInstance?.Dispose();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }

}
