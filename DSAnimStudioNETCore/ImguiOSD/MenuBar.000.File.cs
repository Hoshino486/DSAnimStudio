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
        public static void BuildMenuBar_000_File(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            if (ImGui.BeginMenu(Loc.T("File")))
            {
                isAnyMenuExpanded = true;

                bool clickedOpen = ClickItem(Loc.T("Open..."));
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                ImGui.Separator();
                bool clickedOpenFromArchives = ClickItem(Loc.T("Open From Packed Game Data Archives..."));
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                ImGui.Separator();
                bool isRecentFilesExpanded = ImGui.BeginMenu(Loc.T("Recent Files"));
                if (isRecentFilesExpanded)
                {
                    if (ClickItem(Loc.T("Clear all recent files...")))
                    {
                        DialogManager.AskYesNo(Loc.T("Clear Recent Files"), Loc.T("Clear all recent files?"), choice =>
                        {
                            if (choice)
                            {
                                lock (Main.Config._lock_ThreadSensitiveStuff)
                                {
                                    Main.Config.RecentFilesList.Clear();
                                }
                                Main.SaveConfig();
                            }
                        });
                    }

                    ImGui.Separator();

                    try
                    {
                        DSAProj.TaeContainerInfo recentContainerSelected = null;
                        //string fileOpened_Model = null;
                        lock (Main.Config._lock_ThreadSensitiveStuff)
                        {
                            foreach (var f in Main.Config.RecentFilesList)
                            {
                                if (recentContainerSelected == null && ClickItem(((DSAProj.TaeContainerInfo)f).GetRecentFileListDispString()))
                                {
                                    recentContainerSelected = f;
                                }
                            }
                        }

                        if (recentContainerSelected != null)
                        {
                            Main.MainThreadLazyDispatch(() =>
                            {
                                zzz_DocumentManager.RequestFileOpenRecent = true;
                                zzz_DocumentManager.RequestFileOpenRecent_SelectedFile = recentContainerSelected;
                            });
                            
                        }
                    }
                    catch
                    {

                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();
                bool clickedReloadGameParam = ClickItem(Loc.T("Reload GameParam and FMGs"), enabled: Tae.IsFileOpen);
                zzz_ParamManagerIns.AutoParamReloadEnabled = Checkbox(Loc.T("Auto Reload Params When File Is Written To"), zzz_ParamManagerIns.AutoParamReloadEnabled);
                ImGui.Separator();
                zzz_GameDataIns.EnableFileCaching = Checkbox(Loc.T("Enable Game File Caching"), zzz_GameDataIns.EnableFileCaching);
                ImGui.Separator();
                bool clickedSave = ClickItem(Loc.T("Save"), enabled: Tae.IsFileOpen && Tae.IsModified, shortcut: "Ctrl+S");
                //bool clickedSaveAs = ClickItem("Save As...", enabled: Tae.IsFileOpen, shortcut: "Ctrl+Shift+S");
                bool extensiveBackupsValue = Checkbox(
                    Loc.T("Save New Backup On Every Save"),
                    Tae.Config.ExtensiveBackupsEnabled);
                //bool clickedExportTAE = ClickItem("Export *.TAE Containing The Currently Selected Animation...", enabled: Tae.SelectedAnimCategory != null);
                //ImGui.Separator();
                //var prevValueSaveAdditionalEventRowInfoToLegacyGames = Main.Config.SaveAdditionalEventRowInfoToLegacyGames;
                //var nextValueSaveAdditionalEventRowInfoToLegacyGames = Checkbox("Save Row Data To Legacy Games",
                //    prevValueSaveAdditionalEventRowInfoToLegacyGames, enabled: true,
                //    shortcut: "DeS/DS1 Only");
                //if (nextValueSaveAdditionalEventRowInfoToLegacyGames != prevValueSaveAdditionalEventRowInfoToLegacyGames)
                //{
                //    if (nextValueSaveAdditionalEventRowInfoToLegacyGames)
                //    {
                //        DialogManager.AskYesNo("Warning", "This option has not been tested in the long run and may cause the game to behave " +
                //            "\nstrangely, or it may not. Are you sure you wish to use this option? " +
                //            "\nNote: effect is reversable if you run into issues.", choice =>
                //            {
                //                if (choice)
                //                {
                //                    Main.Config.SaveAdditionalEventRowInfoToLegacyGames = true;

                //                    if (GameDataManager.GameTypeUsesLegacyEmptyEventGroups)
                //                    {
                //                        Tae.StripExtraEventGroupsInAllLoadedFilesIfNeeded();
                //                    }
                //                }
                //            }, allowCancel: true, enterKeyForYes: false);
                //    }
                //    else
                //    {
                //        DialogManager.AskYesNo("Warning", "Disabling this option will IMMEDIATELY REMOVE ALL of the extra row data from all " +
                //            "\nanimations in anibnd files which utilized this option previously and make them all use the standard " +
                //            "\nautomatic row sorting, which will PERMANENTLY save into the file when resaved. " +
                //            "\nAre you sure you wish to do this?", choice =>
                //            {
                //                if (choice)
                //                {
                //                    Main.Config.SaveAdditionalEventRowInfoToLegacyGames = false;
                //                    Tae.StripExtraEventGroupsInAllLoadedFilesIfNeeded();
                //                }
                //            }, allowCancel: true, enterKeyForYes: false);
                //    }
                //}
                ImGui.Separator();
                var canReload = Tae.IsFileOpen && LiveRefresh.RequestFileReload.CanReloadEntity(Tae?.Graph?.ViewportInteractor?.CurrentModel?.Name);
                bool clickedLiveRefreshNow = ClickItem(Loc.TF("Force Ingame Entity Reload Now"),
                    enabled: canReload, shortcut: "F5");
                bool liveRefreshOnSaveValue = Checkbox(
                    Loc.T("Force Ingame Entity Reload On Save\n    (does nothing if above option is grayed out)"),
                    Tae.Config.LiveRefreshOnSave);
                ImGui.NewLine();
                ImGui.Separator();
                bool clickedSaveConfigManually = ClickItem(Loc.T("Save Config File"));
                Main.DisableConfigFileAutoSave = !Checkbox(Loc.T("Enable Config File Autosaving"), !Main.DisableConfigFileAutoSave);
                bool clickedLoadConfigManually = ClickItem(Loc.T("Reload Config File"));
                bool clickedResetConfigToDefault = ClickItem(Loc.T("Reset Config File To Defaults..."));
                bool clickedToggleRegisterProtocolHandler = ClickItem(Main.IsProtocolHandlerInstalled() ? Loc.T("Unregister Protocol Handler") : Loc.T("Register Protocol Handler"));

                ImGui.Separator();
                bool clickedExit = ClickItem(Loc.T("Exit"));

                // Only do the interaction layer with the main window if the recent files list isn't covering it...
                if (!isRecentFilesExpanded)
                {

                    if (clickedOpen)
                    {
                        Main.MainThreadLazyDispatch(() =>
                        {
                            zzz_DocumentManager.RequestFileOpenBrowse = true;
                        });
                    }
                        

                    if (clickedOpenFromArchives)
                    {
                        Main.MainThreadLazyDispatch(() =>
                        {
                            zzz_DocumentManager.RequestOpenFromPackedGameDataArchives = true;
                        });
                    }
                        

                    if (clickedReloadGameParam)
                    {
                        zzz_DocumentManager.CurrentDocument.LoadingTaskMan.DoLoadingTask("FileReloadGameParam",
                            Loc.T("Reloading GameParam and FMGs..."), prog =>
                            {
                                zzz_DocumentManager.CurrentDocument.GameRoot.ReloadParams();
                                zzz_DocumentManager.CurrentDocument.GameRoot.ReloadFmgs();
                                Tae.Graph?.ViewportInteractor?.CurrentModel?.RescanNpcParams();
                                Tae.Graph?.ViewportInteractor?.NewScrub();
                                Window.Equipment.RequestIndexRefresh = true;
                            }, disableProgressBarByDefault: true);
                    }



                    if (clickedSave)
                    {
                        Main.MainThreadLazyDispatch(() =>
                        {
                            Tae.SaveCurrentFile();
                        });
                    }
                        

                    //if (clickedSaveAs)
                    //{
                    //    Main.MainThreadLazyDispatch(() =>
                    //    {
                    //        Tae.File_SaveAs();
                    //    });
                        
                    //}

                    // if (clickedExportTAE)
                    // {
                    //     Main.MainThreadLazyDispatch(() =>
                    //     {
                    //         Tae.Tools_ExportCurrentTAE();
                    //     });
                    //     
                    // }

                    if (clickedLiveRefreshNow)
                        Tae.LiveRefresh();

                    Tae.Config.LiveRefreshOnSave = liveRefreshOnSaveValue;
                    Tae.Config.ExtensiveBackupsEnabled = extensiveBackupsValue;

                    if (clickedSaveConfigManually)
                    {
                        Main.SaveConfig(isManual: true);
                    }

                    if (clickedLoadConfigManually)
                    {
                        Main.LoadConfig(isManual: true);
                    }

                    if (clickedResetConfigToDefault)
                    {
                        DialogManager.AskYesNo(Loc.T("Reset Config"), Loc.T("Reset entire configuration file to defaults, losing all custom settings?"),
                            choice =>
                            {
                                if (choice)
                                {
                                    Main.MainThreadLazyDispatch(() =>
                                    {
                                        Main.ResetConfigToDefault(preserveRecentFilesList: true);
                                    });
                                    
                                }
                            }, inputFlags: Dialog.InputFlag.EscapeKeyToCancel | Dialog.InputFlag.TitleBarXToCancel);
                    }

                    if (clickedToggleRegisterProtocolHandler)
                    {
                        if (Main.IsProtocolHandlerInstalled())
                        {
                            Main.UnregisterProtocolHandler();
                        } else
                        {
                            Main.RegisterProtocolHandler();
                        }
                    }

                    if (clickedExit)
                    {
                        Main.REQUEST_EXIT_NEXT_IS_AUTOMATIC = false;
                        Main.REQUEST_EXIT = true;
                    }
                }

                ImGui.EndMenu();
            }

        }
    }
}
