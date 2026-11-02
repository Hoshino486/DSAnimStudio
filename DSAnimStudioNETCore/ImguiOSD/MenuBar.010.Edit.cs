using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.ImguiOSD.Window;

using DSAnimStudio.Localization;
namespace DSAnimStudio.ImguiOSD
{
    public static partial class MenuBar
    {
        public static void BuildMenuBar_010_Edit(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            if (ImGui.BeginMenu(Loc.T("Edit")))
            {
                isAnyMenuExpanded = true;

                var nextLocalUndo = Tae.CurrentAnimUndoMan?.GetNextUndoName();
                nextLocalUndo = (nextLocalUndo != null && Tae.CurrentAnimUndoMan?.CanUndo == true) ? $" ({nextLocalUndo})" : "";
                
                var nextLocalRedo = Tae.CurrentAnimUndoMan?.GetNextRedoName();
                nextLocalRedo = (nextLocalRedo != null && Tae.CurrentAnimUndoMan?.CanRedo == true) ? $" ({nextLocalRedo})" : "";
                
                if (ClickItem(Loc.TF("Undo{0}", nextLocalUndo), Tae.CurrentAnimUndoMan?.CanUndo ?? false, "Ctrl+Z"))
                    Tae.CurrentAnimUndoMan?.Undo();
                if (ClickItem(Loc.TF("Redo{0}", nextLocalRedo), Tae.CurrentAnimUndoMan?.CanRedo ?? false, "Ctrl+Y"))
                    Tae.CurrentAnimUndoMan?.Redo();
                
                ImGui.Separator();

                var nextGlobalUndo = Tae.GlobalUndoMan?.GetNextUndoName();
                nextGlobalUndo = (nextGlobalUndo != null && Tae.GlobalUndoMan?.CanUndo == true) ? $" ({nextGlobalUndo})" : "";
                
                var nextGlobalRedo = Tae.GlobalUndoMan?.GetNextRedoName();
                nextGlobalRedo = (nextGlobalRedo != null && Tae.GlobalUndoMan?.CanRedo == true) ? $" ({nextGlobalRedo})" : "";

                if (ClickItem(Loc.TF("Undo [Global] Action{0}", nextGlobalUndo), Tae.GlobalUndoMan?.CanUndo ?? false, "Ctrl+Shift+Z"))
                {
                    DialogManager.AskYesNo(Loc.T("WARNING"),
                        Loc.T("All changes on all animations since the [Global] operation was " +
                        "\nperformed will be lost if you undo the global operation. " +
                        "\nAre you sure you wish to do this?"),
                        choice =>
                        {
                            if (choice)
                            {
                                Tae.GlobalUndoMan?.Undo();
                            }
                        }, allowCancel: true, inputFlags: Dialog.InputFlag.EscapeKeyToCancel | Dialog.InputFlag.TitleBarXToCancel);
                }
                if (ClickItem(Loc.TF("Redo [Global] Action{0}", nextGlobalRedo), Tae.GlobalUndoMan?.CanRedo ?? false, "Ctrl+Shift+Y"))
                {
                    DialogManager.AskYesNo(Loc.T("WARNING"),
                        Loc.T("All changes on all animations since the [Global] operation was " +
                        "\nundone will be lost if you redo the global operation. " +
                        "\nAre you sure you wish to do this?"),
                        choice =>
                        {
                            if (choice)
                            {
                                Tae.GlobalUndoMan?.Redo();
                            }
                        }, allowCancel: true, inputFlags: Dialog.InputFlag.EscapeKeyToCancel | Dialog.InputFlag.TitleBarXToCancel);
                    
                }

                ImGui.Separator();

                if (ClickItem(Loc.T("Collapse All TAEs"), Tae.IsFileOpen))
                    Tae.SetAllTAESectionsCollapsed(true);
                if (ClickItem(Loc.T("Expand All TAEs"), Tae.IsFileOpen))
                    Tae.SetAllTAESectionsCollapsed(false);

                ImGui.Separator();

                if (ClickItem(Loc.T("Find Value..."), Tae.IsFileOpen, "Ctrl+F"))
                    Tae.ShowDialogFind();

                if (ClickItem(Loc.T("Go To Animation ID..."), Tae.IsFileOpen, "Ctrl+G"))
                    Tae.ShowDialogGotoAnimID();

                if (ClickItem(Loc.T("Go To TAE ID..."), Tae.IsFileOpen, "Ctrl+H"))
                    Tae.ShowDialogGotoAnimSectionID();

                if (ClickItem(Loc.T("Import From Animation ID..."), Tae.IsFileOpen && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Ctrl+I"))
                    Tae.ShowDialogImportFromAnimID();

                ImGui.Separator();

                if (ClickItem(Loc.T("Change Type of Selected Action"), Tae.IsFileOpen && Tae.SingleEventBoxSelected && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "F1"))
                    Tae.ChangeTypeOfEvent(Tae.InspectorAction);

                if (ClickItem(Loc.T("Edit Current Animation Name..."), Tae.IsFileOpen && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "F2"))
                    Tae.ShowDialogChangeAnimName(Tae.Proj, Tae.SelectedAnimCategory, Tae.SelectedAnim);

                if (ClickItem(Loc.T("Edit Current Animation Category Name..."), Tae.IsFileOpen, "Shift+F2"))
                    Tae.ShowDialogChangeAnimCategoryName(Tae.SelectedAnimCategory);

                if (ClickItem(Loc.T("Edit Current Animation Properties..."), Tae.IsFileOpen && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "F3"))
                    Tae.ShowDialogEditCurrentAnimInfo();

                if (ClickItem(Loc.T("Edit Current Animation Category Properties..."), Tae.IsFileOpen, "Shift+F3"))
                    Tae.ShowDialogEditAnimCategoryProperties(Tae.SelectedAnimCategory);

                if (ClickItem(Loc.T("Edit Project Root TAE Properties..."), Tae.IsFileOpen, "Ctrl+Shift+F3"))
                    Tae.ShowDialogEditRootTaeProperties();

                if (ClickItem(Loc.T("Go to Referenced Action Source Animation"), Tae.IsFileOpen && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "F4"))
                    Tae.GoToEventSource();

                if (ClickItem(Loc.T("Duplicate Animation"), Tae.IsFileOpen && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Insert"))
                    Tae.ShowDialogDuplicateCurrentAnimation();
                
                if (ClickItem(Loc.T("Duplicate Animation Category"), Tae.IsFileOpen, "Shift+Insert"))
                    Tae.ShowDialogDuplicateToNewTaeSection(Tae.Proj, Tae.SelectedAnimCategory);

                ImGui.Separator();

                if (ClickItem(Loc.T("Sort Tracks"), Tae.IsFileOpen, "F6"))
                    Tae.ResortTracks_Anim();

                if (ClickItem(Loc.T("Regen Track Names"), Tae.IsFileOpen, "F7"))
                    Tae.RegenTrackNames_Anim();

                if (ClickItem(Loc.T("[Global] Sort Tracks (All Animations)"), Tae.IsFileOpen, "Shift+F6"))
                    Tae.ResortTracks_Proj();

                if (ClickItem(Loc.T("[Global] Regen Track Names (All Animations)"), Tae.IsFileOpen, "Shift+F7"))
                    Tae.RegenTrackNames_Proj();

                ImGui.Separator();
                
                if (ClickItem(Loc.T("Mute Selected Action(s)"), Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0 
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "1"))
                    Tae.Graph.InputMan.ToggleMuteOnSelectedActions(false);
                if (ClickItem(Loc.T("Mute Selected Action(s) Uniformly"), Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Shift+1"))
                    Tae.Graph.InputMan.ToggleMuteOnSelectedActions(true);

                if (ClickItem(Loc.T("Solo Selected Action(s)"), Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "2"))
                    Tae.Graph.InputMan.ToggleSoloOnSelectedActions(false);
                if (ClickItem(Loc.T("Solo Selected Action(s) Uniformly"), Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Shift+2"))
                    Tae.Graph.InputMan.ToggleSoloOnSelectedActions(true);
                
                if (ClickItem(Loc.T("Reset Mute/Solo On All Actions"), Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Shift+3"))
                    Tae.Graph.InputMan.ClearAllSoloAndMute();
                
                if (ClickItem(Loc.T("Toggle StateInfo Of Selected Action(s)"), Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "4"))
                    Tae.Graph.InputMan.ToggleStateInfoOnSelectedActions(false);
                if (ClickItem(Loc.T("Toggle StateInfo Of Selected Action(s) Uniformly"), Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Shift+4"))
                    Tae.Graph.InputMan.ToggleStateInfoOnSelectedActions(true);

                ImGui.Separator();

                if (ClickItem(Loc.T("Set Custom Color Of Selected Action(s)"), Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "C"))
                    Tae.SetColorOfSelectedActions(isClearing: false);

                if (ClickItem(Loc.T("Clear Custom Color Of Selected Action(s)"), Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Shift+C"))
                    Tae.SetColorOfSelectedActions(isClearing: true);

                ImGui.EndMenu();
            }

        }
    }
}
