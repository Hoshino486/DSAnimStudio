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
        /// <summary>
        /// 顶部「语言」菜单：简体中文 / English 二选一，切换即时生效并写入配置文件。
        /// </summary>
        public static void BuildMenuBar_075_Language(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, Color.Yellow.ToNVector4());
            bool langMenu = ImGui.BeginMenu(Loc.T("Language"));
            ImGui.PopStyleColor();
            if (langMenu)
            {
                isAnyMenuExpanded = true;

                bool isChinese = Loc.Language == LocLanguage.ChineseSimplified;
                if (ImGui.MenuItem(Loc.T("Simplified Chinese"), null, isChinese))
                {
                    if (!isChinese)
                        SetLanguage(LocLanguage.ChineseSimplified);
                }

                bool isEnglish = Loc.Language == LocLanguage.English;
                if (ImGui.MenuItem(Loc.T("English"), null, isEnglish))
                {
                    if (!isEnglish)
                        SetLanguage(LocLanguage.English);
                }

                ImGui.EndMenu();
            }
        }

        /// <summary>
        /// 切换语言：词条缓存清空，字体图集里两种语言的字形都已在启动时烤好，
        /// 停靠布局用的是 "###英文原名" 这种与语言无关的窗口 ID，因此不需要重建字体重载布局。
        /// </summary>
        public static void SetLanguage(LocLanguage language)
        {
            Loc.Language = language;

            if (Main.Config != null)
            {
                Main.Config.UILanguage = language;
                Main.SaveConfig();
            }
        }
    }
}
