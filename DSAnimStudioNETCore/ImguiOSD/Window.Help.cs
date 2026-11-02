using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.Localization;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Help : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => Loc.Win("Help");
            
            protected override void Init()
            {
                
            }

            private string[] helpText = null;
            private LocLanguage helpTextLanguage = LocLanguage.English;
            private object _lock_helpText = new object();

            protected override void BuildContents(ref bool anyFieldFocused)
            {

                //ImGui.PushTextWrapPos(ImGui.GetFontSize() * 400);

                lock (_lock_helpText)
                {
                    if (helpText == null || helpTextLanguage != Loc.Language)
                    {
                        helpTextLanguage = Loc.Language;
                        string resName = Loc.IsChinese
                            ? "/EmbRes/Help.zh-Hans.txt"
                            : "/EmbRes/Help.txt";
                        string raw = null;
                        try
                        {
                            raw = Main.GetEmbeddedResourceText(resName);
                        }
                        catch
                        {
                            raw = null;
                        }
                        if (string.IsNullOrEmpty(raw))
                            raw = Main.GetEmbeddedResourceText("/EmbRes/Help.txt");
                        helpText = raw.Split('\n');
                    }

                    for (int i = 0; i < helpText.Length; i++)
                    {
                        bool isHeader = helpText[i].StartsWith("[");
                        bool isIndent = helpText[i].StartsWith("    ");
                        
                        if (isHeader)
                            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF00FFFF);
                        else if (!isIndent)
                            ImGui.PushStyleColor(ImGuiCol.Text, 0xFFFFFF00);
                        
                        ImGui.TextWrapped(helpText[i]);
                        
                        if (isHeader)
                            ImGui.PopStyleColor();
                        else if (!isIndent)
                            ImGui.PopStyleColor();
                    }
                    //ImGui.PopTextWrapPos();
                }
                
                
            }
        }
    }
}
