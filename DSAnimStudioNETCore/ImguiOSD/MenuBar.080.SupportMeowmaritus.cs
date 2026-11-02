using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSAnimStudio.Localization;
namespace DSAnimStudio.ImguiOSD
{
    public static partial class MenuBar
    {
        public static void BuildMenuBar_080_SupportMeowmaritus(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Color.Lime.ToNVector4());
            bool supportMenu = ImGui.BeginMenu(Loc.T("Support Meowmaritus"));
            ImGui.PopStyleColor();

            

            if (supportMenu)
            {
                isAnyMenuExpanded = true;

                if (ClickItem(Loc.T("On Patreon..."), textColor: Color.Lime,
                        shortcut: "(https://www.patreon.com/Meowmaritus)", shortcutColor: Color.Lime))
                    OpenSite("https://www.patreon.com/Meowmaritus");

                if (ClickItem(Loc.T("On Paypal..."), textColor: Color.Lime,
                        shortcut: "(https://paypal.me/Meowmaritus)", shortcutColor: Color.Lime))
                    OpenSite("https://paypal.me/Meowmaritus");

                if (ClickItem(Loc.T("On Ko-fi..."), textColor: Color.Lime,
                        shortcut: "(https://ko-fi.com/meowmaritus)", shortcutColor: Color.Lime))
                    OpenSite("https://ko-fi.com/meowmaritus");

                ImGui.EndMenu();
            }

        }
    }
}
