using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline;
using Vector3 = System.Numerics.Vector3;
using static DSAnimStudio.ParamData;

using DSAnimStudio.Localization;
namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Entity : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => Loc.Win("Entity");
            
            protected override void Init()
            {
                
            }

            protected override void PreUpdate()
            {
                
            }

            bool manuallySelectNpcParamID_IsInit = true;
            int manuallySelectNpcParamID = 0;

            public TaeViewportInteractor.TaeEntityType EntityType;
            public string EntityName => DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.MainModel?.Name ?? Loc.T("?EntityName?");

            private object _lock = new object();

            private Dictionary<int, string> StateInfoSelectConfig_Names = new Dictionary<int, string>();
            private Dictionary<int, bool> StateInfoSelectConfig_Enabled = new Dictionary<int, bool>();

            public bool IsStateInfoEnabled(int stateInfo)
            {
                lock (_lock)
                {
                    return StateInfoSelectConfig_Enabled.ContainsKey(stateInfo) &&
                           StateInfoSelectConfig_Enabled[stateInfo];
                }
            }
            
            public Dictionary<int, string> GetStateInfoSelectConfig_Names()
            {
                var result = new Dictionary<int, string>();
                lock (_lock)
                {
                    result = StateInfoSelectConfig_Names.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result;
            }

            public Dictionary<int, bool> GetStateInfoSelectConfig_Enabled()
            {
                var result = new Dictionary<int, bool>();
                lock (_lock)
                {
                    result = StateInfoSelectConfig_Enabled.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result;
            }

            public void SetStateInfoSelectConfig_Names(Dictionary<int, string> selectConfig)
            {
                lock (_lock)
                {
                    StateInfoSelectConfig_Names = selectConfig.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }

            public void SetStateInfoSelectConfig_Enabled(Dictionary<int, bool> selectConfig)
            {
                lock (_lock)
                {
                    StateInfoSelectConfig_Enabled = selectConfig.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    foreach (var kvp in StateInfoSelectConfig_Enabled)
                    {
                        if (!StateInfoSelectConfig_Names.ContainsKey(kvp.Key))
                            StateInfoSelectConfig_Names.Add(kvp.Key, "");
                    }
                }
            }

            private static string GetEntityDispText(TaeViewportInteractor.TaeEntityType entityType, string entityName)
            {
                string namePart = (!string.IsNullOrWhiteSpace(entityName) ? $"[" + entityName + "]" : "");
                switch (entityType)
                {
                    case TaeViewportInteractor.TaeEntityType.NONE: return Loc.T("No Entity");
                    case TaeViewportInteractor.TaeEntityType.PC: return Loc.T("Player") + namePart;
                    case TaeViewportInteractor.TaeEntityType.NPC: return Loc.T("NPC Character") + namePart;
                    case TaeViewportInteractor.TaeEntityType.OBJ: return Loc.T("Object") + namePart;
                    case TaeViewportInteractor.TaeEntityType.PARTS: return Loc.T("Animated Player Equipment") + namePart;
                    case TaeViewportInteractor.TaeEntityType.REMO: return Loc.T("DS1 Cutscene") + namePart;
                    default: return Loc.T("Invalid Entity Type");
                }
            }
            
            private static string[] BehaviorHitboxSrcEnum_Names => new string[]
            {
                Loc.T("Body"),
                Loc.T("Right Weapon"),
                Loc.T("Left Weapon"),
            };
            private static List<ParamData.AtkParam.DummyPolySource> BehaviorHitboxSrcEnum_Values = new List<ParamData.AtkParam.DummyPolySource>
            {
                ParamData.AtkParam.DummyPolySource.BaseModel,
                ParamData.AtkParam.DummyPolySource.RightWeapon0,
                ParamData.AtkParam.DummyPolySource.LeftWeapon0,
            };

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                var curModel = MainTaeScreen.Graph?.ViewportInteractor?.CurrentModel;

                if (curModel != null)
                {
                    if (manuallySelectNpcParamID_IsInit && curModel.NpcParam != null)
                    {
                        manuallySelectNpcParamID = curModel.NpcParam.ID;
                        manuallySelectNpcParamID_IsInit = false;
                    }

                    ImGui.PushItemWidth(OSD.DefaultItemWidth * 2);

                    if (MainDoc.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR)
                    {

                        int eldenRingHandAnim_CategoryID = curModel.EldenRingHandPoseAnimID.CategoryID;
                        int old_eldenRingHandAnim_CategoryID = eldenRingHandAnim_CategoryID;
                        int eldenRingHandAnim_SubID = curModel.EldenRingHandPoseAnimID.SubID;
                        int old_eldenRingHandAnim_SubID = eldenRingHandAnim_SubID;

                        ImGui.InputInt(Loc.TF("ER Hand Anim Category ID##ER_Hand_Anim_CategoryID[{0}]", curModel.GUID), ref eldenRingHandAnim_CategoryID);
                        ImGui.InputInt(Loc.TF("ER Hand Anim Sub ID##ER_Hand_Anim_SubID[{0}]", curModel.GUID), ref eldenRingHandAnim_SubID);

                        if (eldenRingHandAnim_CategoryID != old_eldenRingHandAnim_CategoryID ||
                            eldenRingHandAnim_SubID != old_eldenRingHandAnim_SubID)
                        {
                            curModel.EldenRingHandPoseAnimID = new SplitAnimID()
                            {
                                CategoryID = eldenRingHandAnim_CategoryID,
                                SubID = eldenRingHandAnim_SubID
                            };
                            curModel.NewForceSyncUpdate();
                        }

                        ImGui.Separator();
                    }

                    var modelPos = curModel.OriginOffset.ToCS();
                    var modelPos_Prev = modelPos;
                    ImGui.DragFloat3(Loc.TF("Model Position##Entity_ModelPosition_Model[{0}]", curModel.GUID), ref modelPos, 0.01f, -10000, 10000, "%.3f m");
                    if (ImGui.IsItemActive())
                        anyFieldFocused = true;
                    if (modelPos != modelPos_Prev)
                        curModel.OriginOffset = modelPos.ToXna();
                    
                    var modelRotation = new Vector3(MathHelper.ToDegrees(curModel.OriginRotation.X), MathHelper.ToDegrees(curModel.OriginRotation.Y), MathHelper.ToDegrees(curModel.OriginRotation.Z));
                    var modelRotation_Prev = modelRotation;
                    ImGui.DragFloat3(Loc.TF("Model Rotation##Entity_ModelRotation_Model[{0}]", curModel.GUID), ref modelRotation, 1, -180, 180, "%.3f °");
                    if (ImGui.IsItemActive())
                        anyFieldFocused = true;
                    if (modelRotation_Prev != modelRotation)
                        curModel.OriginRotation = new Microsoft.Xna.Framework.Vector3(MathHelper.ToRadians(modelRotation.X), MathHelper.ToRadians(modelRotation.Y), MathHelper.ToRadians(modelRotation.Z));
                    
                    var modelScale = curModel.OriginScale.ToCS() * 100f;
                    var modelScale_Prev = modelScale;
                    ImGui.DragFloat3(Loc.TF("Model Scale##Entity_ModelScale_Model[{0}]", curModel.GUID), ref modelScale, 1, 0.1f, 1000, "%.2f%%");
                    if (ImGui.IsItemActive())
                        anyFieldFocused = true;
                    if (modelScale != modelScale_Prev)
                        curModel.OriginScale = modelScale.ToXna() / 100f;
                    
                    ImGui.PopItemWidth();

                    if (Tools.SimpleClickButton(Loc.T("Reset Transform")))
                    {
                        curModel.OriginOffset = Microsoft.Xna.Framework.Vector3.Zero;
                        curModel.OriginRotation = Microsoft.Xna.Framework.Vector3.Zero;
                        curModel.OriginScale = Microsoft.Xna.Framework.Vector3.One;
                    }
                    
                    ImGui.Separator();
                }
                
                
                if (ImGui.TreeNode(Loc.T("StateInfo Toggle")))
                {
                    int i = 0;
                    lock (_lock)
                    {
                        foreach (var kvp in StateInfoSelectConfig_Names)
                        {
                            bool isChecked = StateInfoSelectConfig_Enabled.ContainsKey(kvp.Key) ? StateInfoSelectConfig_Enabled[kvp.Key] : true;
                            ImGui.Checkbox(!string.IsNullOrWhiteSpace(kvp.Value) ? $"{kvp.Key} - {kvp.Value}" : $"{kvp.Key}"
                                + $"###EntitySettings_StateInfoSelect_{kvp.Key}", ref isChecked);
                            StateInfoSelectConfig_Enabled[kvp.Key] = isChecked;
                            ImGui.SameLine(ImGui.GetWindowWidth() - 50 - 64);
                            ImGui.PushItemWidth(64);
                            ImGui.Button(Loc.TF("Rename###EntitySettings_StateInfoSelect_{0}_RenameButton", kvp.Key));
                            ImGui.PopItemWidth();
                            if (ImGui.IsItemClicked())
                            {
                                int captureKey = kvp.Key;
                                string captureValue = kvp.Value;
                                ImguiOSD.DialogManager.AskForInputString(Loc.T("Rename StateInfo"), Loc.TF("Enter new name for StateInfo {0}:", kvp.Key), Loc.T(""), result =>
                                {
                                    lock (_lock)
                                    {
                                        StateInfoSelectConfig_Names[captureKey] = result;
                                    }
                                    zzz_DocumentManager.CurrentDocument.GameData.SaveProjectJson();
                                }, checkError: null, canBeCancelled: true, startingText: captureValue ?? "");
                            }

                            ImGui.SameLine(ImGui.GetWindowWidth() - 50);
                            ImGui.PushItemWidth(32);
                            ImGui.Button(Loc.TF("×###EntitySettings_StateInfoSelect_{0}_DeleteButton", kvp.Key));
                            ImGui.PopItemWidth();
                            if (ImGui.IsItemClicked())
                            {
                                int captureKey = kvp.Key;
                                string captureValue = kvp.Value;
                                ImguiOSD.DialogManager.AskYesNo(Loc.T("Delete StateInfo?"), Loc.TF("Are you sure you wish to delete StateInfo {0}?", captureKey), result =>
                                {
                                    lock (_lock)
                                    {
                                        if (result)
                                        {
                                            if (StateInfoSelectConfig_Names.ContainsKey(captureKey))
                                                StateInfoSelectConfig_Names.Remove(captureKey);
                                            if (StateInfoSelectConfig_Enabled.ContainsKey(captureKey))
                                                StateInfoSelectConfig_Enabled.Remove(captureKey);
                                        }
                                    }
                                    zzz_DocumentManager.CurrentDocument.GameData.SaveProjectJson(silent: true);
                                }, allowCancel: true);
                            }
                        }

                        ImGui.Button(Loc.T("＋###EntitySettings_StateInfoSelect_AddNew"));
                        if (ImGui.IsItemClicked())
                        {
                            ImguiOSD.DialogManager.AskForInputString(Loc.T("Add New StateInfo"), Loc.TF("Enter ID for the new StateInfo to add:"), Loc.T(""), result =>
                            {
                                var digitString = result.ExtractDigits();
                                if (digitString.Length > 0)
                                {
                                    int stateInfoID = int.Parse(digitString);
                                    bool error_alreadyExists = false;
                                    lock (_lock)
                                    {
                                        if (StateInfoSelectConfig_Enabled.ContainsKey(stateInfoID))
                                        {
                                            error_alreadyExists = true;
                                        }
                                        else
                                        {
                                            StateInfoSelectConfig_Enabled.Add(stateInfoID, true);
                                            StateInfoSelectConfig_Names.Add(stateInfoID, "");
                                        }
                                    }

                                    if (error_alreadyExists)
                                        zzz_NotificationManagerIns.PushNotificationWarn(Loc.TF("Cannot add StateInfo - ID {0} already exists.", stateInfoID));
                                    else
                                        zzz_DocumentManager.CurrentDocument.GameData.SaveProjectJson(silent: true);
                                }
                                else
                                {
                                    zzz_NotificationManagerIns.PushNotificationWarn(Loc.TF("Cannot add StateInfo - Unable to parse an integer ID from given string '{0}'.", result));
                                }
                                

                                
                            }, checkError: input =>
                            {
                                
                                var digitString = input.ExtractDigits();
                                if (digitString.Length > 0)
                                {
                                    int stateInfoID = int.Parse(digitString);
                                    bool error_alreadyExists = false;
                                    lock (_lock)
                                    {
                                        if (StateInfoSelectConfig_Enabled.ContainsKey(stateInfoID))
                                        {
                                            error_alreadyExists = true;
                                        }
                                    }

                                    if (error_alreadyExists)
                                        return Loc.TF("StateInfo ID {0} already exists.", stateInfoID);
                                }
                                else
                                {
                                    return
                                        Loc.TF("Unable to parse an integer ID from given string '{0}'.", input);
                                }

                                return null;
                            }, canBeCancelled: true);
                        }
                    }
                    ImGui.TreePop();
                }
                

                ImGui.Separator();

                var entityType = MainTaeScreen.Graph?.ViewportInteractor?.EntityType;
                if (entityType == TaeViewportInteractor.TaeEntityType.NPC)
                {
                    ImGui.Button(Loc.T("Load Additional Texture File(s)..."));
                    if (ImGui.IsItemClicked())
                        MainTaeScreen.BrowseForMoreTextures();

                    if (curModel != null)
                    {
                        ImGui.Text(Loc.TF("NPC Param: "));
                        //ImGui.SameLine();
                        int npcParamIndex = curModel.PossibleNpcParams.IndexOf(curModel.NpcParam);
                        ImGui.Button(Loc.T("-"));
                        if (ImGui.IsItemClicked())
                        {
                            npcParamIndex--;
                            if (npcParamIndex < 0)
                                npcParamIndex = curModel.PossibleNpcParams.Count - 1;
                        }
                        ImGui.SameLine();
                        ImGui.Button(Loc.T("+"));
                        if (ImGui.IsItemClicked())
                        {
                            npcParamIndex++;
                            if (npcParamIndex >= curModel.PossibleNpcParams.Count)
                                npcParamIndex = 0;
                        }
                        ImGui.SameLine();
                        if (curModel.NpcParam == null)
                            ImGui.Text(Loc.TF("<No NpcParam Selected>"));
                        else
                            ImGui.Text($"{curModel.NpcParam.ID}: {(curModel.NpcParam.Name ?? "")}");

                        if (npcParamIndex >= 0 && npcParamIndex < curModel.PossibleNpcParams.Count)
                        {
                            var newNpcParam = curModel.PossibleNpcParams[npcParamIndex];
                            if (newNpcParam != curModel.NpcParam)
                            {
                                curModel.NpcParam = newNpcParam;
                                manuallySelectNpcParamID = newNpcParam.ID;
                                newNpcParam.ApplyToNpcModel(MainDoc, curModel);
                            }
                        }

                        if (ImGui.TreeNode(Loc.T("NPC Param Selection")))
                        {



                            lock (curModel._lock_NpcParams)
                            {
                                foreach (var npc in curModel.PossibleNpcParams)
                                {



                                    bool oldSelected = npc == curModel.NpcParam;

                                    var selected = MenuBar.CheckboxBig(npc.GetDisplayName(), oldSelected);

                                    ImGui.Indent();
                                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0, 1, 1, 1));
                                    {

                                        ImGui.Text(Loc.TF("BehaviorVariationID: {0}", npc.BehaviorVariationID));
                                        if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.AC6)
                                            ImGui.Text(Loc.TF("NpcEquipPartsParam ID: {0}", npc.AC6NpcEquipPartsParamID));

                                        if (curModel.NpcMaterialNamesPerMask.Any(kvp => kvp.Key >= 0))
                                        {

                                            ImGui.Text(Loc.TF("Meshes Visible:"));

                                            ImGui.Indent();
                                            {
                                                foreach (var kvp in curModel.NpcMaterialNamesPerMask)
                                                {
                                                    if (kvp.Key < 0)
                                                        continue;

                                                    if (curModel.NpcMasksEnabledOnAllNpcParams.Contains(kvp.Key))
                                                        continue;

                                                    if (npc.DrawMask[kvp.Key])
                                                    {
                                                        foreach (var v in kvp.Value)
                                                            ImGui.BulletText(v);

                                                    }
                                                }
                                            }
                                            ImGui.Unindent();
                                        }
                                    }
                                    ImGui.PopStyleColor();
                                    ImGui.Unindent();

                                    if (selected != oldSelected)
                                    {
                                        curModel.NpcParam = npc;
                                        npc.ApplyToNpcModel(MainDoc, curModel);
                                        manuallySelectNpcParamID = npc.ID;
                                    }
                                }
                            }
                            ImGui.TreePop();
                        }

                        ImGui.Text(Loc.T("Manually Select NPC Param:"));
                        ImGui.InputInt(Loc.T("NPC Param ID"), ref manuallySelectNpcParamID);
                        ImGui.Button(Loc.T("Select"));
                        if (ImGui.IsItemClicked())
                        {
                            if (curModel.Document.ParamManager.NpcParam.ContainsKey(manuallySelectNpcParamID))
                            {
                                var npcParam = curModel.Document.ParamManager.NpcParam[manuallySelectNpcParamID];
                                curModel.NpcParam = npcParam;
                                npcParam.ApplyToNpcModel(MainDoc, curModel);
                            }
                        }
                    }

                    ImGui.Separator();

                    bool dpf = false;
                    MainTaeScreen?.Graph?.ViewportInteractor?.CurrentModel?.AC6NpcParts?.DoImguiDummyPolySourceField(ref dpf);
                    if (dpf)
                        anyFieldFocused = true;

                    if (Main.IsDebugBuild)
                    {
                        ImGui.Separator();

                        ImGui.Button(Loc.T("[Debug] Open NPC Model Importer"));
                        if (ImGui.IsItemClicked())
                            MainTaeScreen.BringUpImporter_FLVER2();
                    }
                }
                else if (entityType == TaeViewportInteractor.TaeEntityType.OBJ)
                {
                    ImGui.Button(Loc.T("Load Additional Texture File(s)..."));
                    if (ImGui.IsItemClicked())
                        MainTaeScreen.BrowseForMoreTextures();
                    
                }
                else if (entityType == TaeViewportInteractor.TaeEntityType.PC)
                {
                    // OSD.WindowEditPlayerEquip.IsOpen = MenuBar.CheckboxBig("Show Player Equipment Editor Window", OSD.WindowEditPlayerEquip.IsOpen);
                    //
                    // ImGui.Separator();

                    //ImGui.Button("Force Refresh Equipment");
                    //if (ImGui.IsItemClicked())
                    //{
                    //    MainTaeScreen.Graph?.ViewportInteractor?.CurrentModel?.ChrAsm?.UpdateModels(isAsync: true, onCompleteAction: () =>
                    //    {
                    //        FlverMaterialDefInfo.FlushBinderCache();
                    //    }, forceReloadUnchanged: true, disableCache: true);
                    //}

                    if (MainTaeScreen?.Graph?.ViewportInteractor?.ActionSim != null)
                    {
                        var currentHitViewSource = MainTaeScreen.Config.HitViewDummyPolySource;

                        int currentHitViewSourceIndex = BehaviorHitboxSrcEnum_Values.IndexOf(currentHitViewSource);
                        ImGui.ListBox(Loc.T("Behavior / Hitbox Source"), ref currentHitViewSourceIndex, BehaviorHitboxSrcEnum_Names, BehaviorHitboxSrcEnum_Names.Length);
                        var newHitViewSource = currentHitViewSourceIndex >= 0 ? BehaviorHitboxSrcEnum_Values[currentHitViewSourceIndex] : ParamData.AtkParam.DummyPolySource.BaseModel;

                        if (currentHitViewSource != newHitViewSource)
                        {
                            lock (MainTaeScreen.Graph._lock_ActionBoxManagement)
                            {
                                MainTaeScreen.Graph.ViewportInteractor.ActionSim.OnNewAnimSelected(MainTaeScreen.Graph.GetActionListCopy_UsesLock());
                                MainTaeScreen.Config.HitViewDummyPolySource = newHitViewSource;
                                MainTaeScreen.Graph.ViewportInteractor.ActionSim.OnNewAnimSelected(MainTaeScreen.Graph.GetActionListCopy_UsesLock());
                                MainTaeScreen.Graph.ViewportInteractor.NewScrub();
                            }
                        }
                    }
                
                    if (MainTaeScreen?.Graph != null)
                    {
                        
                    }
                
                }
                else if (entityType == TaeViewportInteractor.TaeEntityType.REMO)
                {
                    RemoManager.EnableRemoCameraInViewport = MenuBar.CheckboxBig(Loc.T("Show Cutscene Camera View"), RemoManager.EnableRemoCameraInViewport);
                    RemoManager.EnableDummyPrims = MenuBar.CheckboxBig(Loc.T("Enable Dummy Node Helpers"), RemoManager.EnableDummyPrims);
                    //Main.Config.LockAspectRatioDuringRemo = MenuBar.CheckboxBig("Lock Aspect Ratio to 16:9", Main.Config.LockAspectRatioDuringRemo);

                    ImGui.Button(Loc.T("Preview Full Cutscene With Streamed Audio"));
                    if (ImGui.IsItemClicked())
                    {
                        RemoManager.StartFullPreview();
                    }
                }
                else
                {
                    ImGui.Text(Loc.T("No entity loaded."));
                }


                ImGui.Separator();

                string[] viewportStatusTypes = Enum.GetNames(typeof(TaeConfigFile.ViewportStatusTypes));
                var viewportStatusTypeValues = Enum.GetValues<TaeConfigFile.ViewportStatusTypes>().ToList();
                int curIndex = viewportStatusTypeValues.IndexOf(Main.Config.ShowStatusInViewport);
                ImguiOSD.Tools.FancyComboBox(Loc.T("Viewport Status Type"), ref curIndex, viewportStatusTypes);
                if (curIndex < 0)
                    curIndex = 1;
                Main.Config.ShowStatusInViewport = viewportStatusTypeValues[curIndex];

                if (ImGui.TreeNode(Loc.T("STATUS###WorkspaceConfig_Status")))
                {
                    MainTaeScreen?.Graph?.ViewportInteractor?.BuildImGuiStatus();

                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 2000);
                    ImGui.Text(Loc.T(" "));

                    ImGui.TreePop();
                }


                
            }
        }
    }
}
