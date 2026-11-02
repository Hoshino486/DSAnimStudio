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
        public static void BuildMenuBar_020_Graph(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            if (ImGui.BeginMenu(Loc.T("Action Graph")))
            {
                isAnyMenuExpanded = true;

                Tae.Config.ActionSnapType = EnumSelectorItem<TaeEditor.TaeConfigFile.ActionSnapTypes>(Loc.T("Snap Actions To Framerate"),
                    Tae.Config.ActionSnapType, new Dictionary<TaeEditor.TaeConfigFile.ActionSnapTypes, string>
                {
                        //{ TaeEditor.TaeConfigFile.EventSnapTypes.None, "None (not recommended)" },
                        { TaeEditor.TaeConfigFile.ActionSnapTypes.FPS30, Loc.T("30 FPS (used by FromSoft)") },
                        { TaeEditor.TaeConfigFile.ActionSnapTypes.FPS60, Loc.T("60 FPS") },
                }, enabled: Tae.IsFileOpen);

                ImGui.Separator();

                //Tae.Config.IsNewGraphVisiMode = Checkbox("Use New Graph Design", Tae.Config.IsNewGraphVisiMode);
                Tae.Config.EnableFancyScrollingStrings = Checkbox(Loc.T("Use Fancy Text Scrolling"), Tae.Config.EnableFancyScrollingStrings);
                Tae.Config.FancyScrollingStringsScrollSpeed = FloatSlider(Loc.T("Fancy Text Scroll Speed"), Tae.Config.FancyScrollingStringsScrollSpeed, 1, 256, "%f pixels/sec");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                ImGui.Separator();
                Tae.Config.AutoCollapseAllTaeSections = Checkbox(Loc.T("Start with all TAEs collapsed"), Tae.Config.AutoCollapseAllTaeSections);
                Tae.Config.GoToFirstAnimInCategoryWhenChangingCategory = Checkbox(Loc.T("Go To First Anim In Category When Changing Category"), Tae.Config.GoToFirstAnimInCategoryWhenChangingCategory);
                Tae.Config.AnimListShowHkxNames = Checkbox(Loc.T("Show Animation File Names"), Tae.Config.AnimListShowHkxNames);
                ImGui.Separator();
                Tae.Config.AutoScrollDuringAnimPlayback = Checkbox(Loc.T("Auto-scroll During Anim Playback"), Tae.Config.AutoScrollDuringAnimPlayback);
                ImGui.Separator();
                Tae.Config.SoloHighlightActionOnHover = Checkbox(Loc.T("Solo Highlight Action on Hover"), Tae.Config.SoloHighlightActionOnHover);
                Tae.Config.SoloHighlightActionOnHover_IgnoresStateInfo = Checkbox(Loc.T("Solo Highlight Action on Hover Ignores StateInfo"), Tae.Config.SoloHighlightActionOnHover_IgnoresStateInfo);
                Tae.Config.ShowActionTooltips = Checkbox(Loc.T("Show Action Info Popup When Hovering Over Action"), Tae.Config.ShowActionTooltips);

                Tae.Config.UseOldActionSelectBehavior = Checkbox(Loc.T("Use Old Action Select Behavior"), Tae.Config.UseOldActionSelectBehavior);

                ImGui.Separator();
                
                Tae.Config.NewRelativeTimelineScrubEnabled = Checkbox(Loc.T("Relative Timeline Scrub - Enabled"), Tae.Config.NewRelativeTimelineScrubEnabled);
                Tae.Config.NewRelativeTimelineScrubSyncMouseToPlaybackCursor = Checkbox(Loc.T("Relative Timeline Scrub - Sync Mouse To Playback Cursor"), Tae.Config.NewRelativeTimelineScrubSyncMouseToPlaybackCursor);
                Tae.Config.NewRelativeTimelineScrubHideMouseCursor = Checkbox(Loc.T("Relative Timeline Scrub - Hide Mouse"), Tae.Config.NewRelativeTimelineScrubHideMouseCursor);
                Tae.Config.NewRelativeTimelineScrubScrollScreen = Checkbox(Loc.T("Relative Timeline Scrub - Scroll Screen"), Tae.Config.NewRelativeTimelineScrubScrollScreen);
                Tae.Config.NewRelativeTimelineScrubScalesWithZoom = Checkbox(Loc.T("Relative Timeline Scrub - Speed Scales With Zoom"), Tae.Config.NewRelativeTimelineScrubScalesWithZoom);
                Tae.Config.NewRelativeTimelineScrubLockToAnimSpeed = Checkbox(Loc.T("Relative Timeline Scrub - Lock To Anim Speed"), Tae.Config.NewRelativeTimelineScrubLockToAnimSpeed);
                var speedLock = Tae.Config.NewRelativeTimelineScrubLockToAnimSpeed;
                
                if (speedLock)
                    ImGui.BeginDisabled();
                
                float curRelativeScrubSpeed = Tae.Config.NewRelativeTimelineScrubSpeed;
                curRelativeScrubSpeed = FloatSlider(Loc.T("Relative Timeline Scrub - Sensitivity"),
                    curRelativeScrubSpeed * 100f, 0, 100, "%.2f%%") / 100f;
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                if (curRelativeScrubSpeed > 9.99f)
                    curRelativeScrubSpeed = 9.99f;
                if (curRelativeScrubSpeed < 0)
                    curRelativeScrubSpeed = 0;
                Tae.Config.NewRelativeTimelineScrubSpeed = curRelativeScrubSpeed;
                
                // float curRelativeScrubSmoothing = Tae.Config.NewRelativeTimelineScrubSmoothing;
                // curRelativeScrubSmoothing = FloatSlider("Relative Timeline Scrub - Smoothing",
                //     curRelativeScrubSmoothing * 100f, 0, 100, "%.2f%%") / 100f;
                // if (curRelativeScrubSmoothing > 1)
                //     curRelativeScrubSmoothing = 1;
                // if (curRelativeScrubSmoothing < 0)
                //     curRelativeScrubSmoothing = 0;
                // Tae.Config.NewRelativeTimelineScrubSmoothing = curRelativeScrubSmoothing;
                
                if (speedLock)
                    ImGui.EndDisabled();
                
                
                ImGui.Separator();
                
                Tae.Config.ScrollWhileDraggingActions_Enabled = Checkbox(Loc.T("Scroll While Dragging Actions - Enabled"), Tae.Config.ScrollWhileDraggingActions_Enabled);
                Tae.Config.ScrollWhileDraggingActions_ThreshMin = FloatSlider(Loc.T("Scroll While Dragging Actions - Min Threshold"), 
                    Tae.Config.ScrollWhileDraggingActions_ThreshMin * 100f, 0, 100, "%.2f%%", clampMin: 0, clampMax: 100) / 100f;
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.ScrollWhileDraggingActions_ThreshMax = FloatSlider(Loc.T("Scroll While Dragging Actions - Max Threshold"), 
                    Tae.Config.ScrollWhileDraggingActions_ThreshMax * 100f, 0, 100, "%.2f%%", clampMin: 0, clampMax: 100) / 100f;
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.ScrollWhileDraggingActions_Speed = FloatSlider(Loc.T("Scroll While Dragging Actions - Speed"), 
                    Tae.Config.ScrollWhileDraggingActions_Speed, 10, 1000, "%.2f px/s", clampMin: 10, clampMax: 10000);
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.ScrollWhileDraggingActions_SpeedPower = FloatSlider(Loc.T("Scroll While Dragging Actions - Speed Power"), 
                    Tae.Config.ScrollWhileDraggingActions_SpeedPower, 0.1f, 4, "%.2f", clampMin: 0.1f, clampMax: 4f);
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.ScrollWhileDraggingActions_DragDistBlendMin = FloatSlider(Loc.T("Scroll While Dragging Actions - Activation Dist Min"), 
                    Tae.Config.ScrollWhileDraggingActions_DragDistBlendMin, 0, 100, "%.2fpx", clampMin: 0, clampMax: 1000);
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.ScrollWhileDraggingActions_DragDistBlendMax = FloatSlider(Loc.T("Scroll While Dragging Actions - Activation Dist Max"), 
                    Tae.Config.ScrollWhileDraggingActions_DragDistBlendMax, 0, 100, "%.2fpx", clampMin: 0, clampMax: 1000);
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.ScrollWhileDraggingActions_LimitSpeed = Checkbox(Loc.T("Scroll While Dragging Actions - Limit Speed"), Tae.Config.ScrollWhileDraggingActions_LimitSpeed);


                ImGui.Separator();
                bool showActionIDsPrev = Tae.Config.ShowActionIDs;
                Tae.Config.ShowActionIDs = Checkbox(Loc.T("Show Action IDs"), Tae.Config.ShowActionIDs);
                if (Tae.Config.ShowActionIDs != showActionIDsPrev)
                {
                    Tae.RefreshTextForAllActions();
                }
                ImGui.Separator();

                Tae.Config.ResetScrollWhenChangingAnimations = Checkbox(Loc.T("Reset Scroll When Changing Animations"), Tae.Config.ResetScrollWhenChangingAnimations);

                //ImGui.Separator();

                //Tae.Config.TemplateEditorAutoRefresh = Checkbox("Template Editor Auto Refresh", Tae.Config.TemplateEditorAutoRefresh);

                ImGui.EndMenu();
            }

        }
    }
}
