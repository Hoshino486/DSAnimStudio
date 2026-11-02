using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DSAnimStudio.Localization;
namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        public class SetupWorkspace : Dialog
        {
            string GameDirectory = null;
            string ModEngineDirectory = null;
            bool LoadLooseParams = false;
            bool LoadUnpackedGameFiles = false;
            bool DisableInterrootDCX = false;

            
            public SetupWorkspace()
                : base(Loc.T("Setup Workspace"))
            {
                zzz_DocumentManager.CurrentDocument.GameData.LoadProjectJson();
                
                zzz_DocumentManager.CurrentDocument.GameData.ProjectJsonLockAct(proj =>
                {
                    GameDirectory = proj.GameDirectory;
                    ModEngineDirectory = proj.ModEngineDirectory;
                    LoadLooseParams = proj.LoadLooseParams;
                    LoadUnpackedGameFiles = proj.LoadUnpackedGameFiles;
                    DisableInterrootDCX = proj.DisableInterrootDCX;
                });

                CancelHandledByInheritor = true;
                AcceptHandledByInheritor = true;
            }

            private bool RequestBrowseGameDir = false;
            private bool RequestBrowseModEngineDir = false;
            
            protected override void BuildInsideOfWindow()
            {
                

                
                
                
                if (RequestBrowseGameDir)
                {
                    var browseDlg = new SaveFileDialog()
                    {
                        FileName = Loc.T("GO TO DIRECTORY AND CLICK SAVE"),
                        CheckFileExists = false,
                        CheckPathExists = true,
                        OverwritePrompt = false,
                        Title = Loc.T("Select Game Directory"),
                    };
                    if (!string.IsNullOrWhiteSpace(GameDirectory) && System.IO.Directory.Exists(GameDirectory))
                    {
                        browseDlg.InitialDirectory = GameDirectory;
                    }
                    if (browseDlg.ShowDialog() == DialogResult.OK)
                    {
                        GameDirectory = System.IO.Path.GetDirectoryName(browseDlg.FileName);
                    }
                    
                    RequestBrowseGameDir = false;
                }

                if (RequestBrowseModEngineDir)
                {
                    var browseDlg = new SaveFileDialog()
                    {
                        FileName = Loc.T("GO TO DIRECTORY AND CLICK SAVE"),
                        CheckFileExists = false,
                        CheckPathExists = true,
                        OverwritePrompt = false,
                        Title = Loc.T("Select ModEngine Directory"),
                    };
                    if (!string.IsNullOrWhiteSpace(ModEngineDirectory) && System.IO.Directory.Exists(ModEngineDirectory))
                    {
                        browseDlg.InitialDirectory = ModEngineDirectory;
                    }
                    if (browseDlg.ShowDialog() == DialogResult.OK)
                    {
                        ModEngineDirectory = System.IO.Path.GetDirectoryName(browseDlg.FileName);
                    }
                    
                    RequestBrowseModEngineDir = false;
                }
                
                ImGui.InputText(Loc.T("Game Data Directory##Dialog.SetupWorkspace.GameDirectory"), ref GameDirectory, 256);
                bool editingGameDirectory = ImGui.IsItemActive();
                if (Tools.SimpleClickButton(Loc.T("Browse...")))
                {
                    RequestBrowseGameDir = true;
                }
                ImGui.InputText(Loc.T("ModEngine '/mod/' Directory (Optional)##Dialog.SetupWorkspace.ModEngineDirectory"), ref ModEngineDirectory, 256);
                bool editingModEngineDirectory = ImGui.IsItemActive();
                ImGui.SameLine();
                if (Tools.SimpleClickButton(Loc.T("Browse...")))
                {
                    RequestBrowseModEngineDir = true;
                }

                ImGui.Checkbox(Loc.T("Load Loose GameParam Instead of Regulation"), ref LoadLooseParams);
                ImGui.Checkbox(Loc.T("Load UXM Unpacked Game Files"), ref LoadUnpackedGameFiles);
                ImGui.Checkbox(Loc.T("Disable Interroot DCX"), ref DisableInterrootDCX);
                
                bool clickedCancel = Tools.SimpleClickButton(Loc.T("Cancel")) || IsTitleBarXRequested;
                ImGui.SameLine();
                bool clickedAccept = Tools.SimpleClickButton(Loc.T("Accept"));
                bool pressedEscape = IsEscapeKeyRequested;
                bool pressedEnter = IsEnterKeyRequested && !editingGameDirectory && !editingModEngineDirectory;

                if (clickedCancel)
                {
                    ResultType = ResultTypes.Cancel;
                    Dismiss();
                }
                else if (pressedEscape)
                {
                    ResultType = ResultTypes.Cancel;
                    Dismiss();
                }
                else if (clickedAccept)
                {
                    ResultType = ResultTypes.Accept;
                    Dismiss();
                }
                else if (pressedEnter)
                {
                    ResultType = ResultTypes.Accept;
                    Dismiss();
                }
            }
        }
    }
}
