using Assimp;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;

using DSAnimStudio.Localization;
namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class SceneWindow : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => Loc.Win("Scene");

            private float debug_HkxSkelToHkxAnimWeight_min = 0;
            private float debug_HkxSkelToHkxAnimWeight_max = 1;

            //public readonly zzz_DocumentIns Document;
            //public SceneWindow(zzz_DocumentIns doc)
            //{
            //    Document = doc;
            //}

            protected override void Init()
            {
                
            }

            protected override void BuildContents(ref bool anyFieldFocusedRef)
            {
                bool anyFieldFocused = false;
                //ImGui.SetWindowFocus();

                //Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar;
                Flags = ImGuiWindowFlags.None;
                
                GFX.HighlightedThing = null;

                GFX.NewDebug_ShowTex_Material = null;
                GFX.NewDebug_ShowTex_Sampler = null;
                GFX.NewDebug_ShowTex_SamplerConfig = null;
                int TREE_INDEX = 0;

                lock (DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene._lock_ModelLoad_Draw)
                {
                    int index_Mesh = 0;

                    void DoDebugOfModel(Model model)
                    {
                        ImGui.Checkbox(Loc.TF("Draw DummyPoly Transforms###Model{0}_DebugDispDummyPolyTransforms", model.GUID), ref model.DebugDispDummyPolyTransforms);
                        ImGui.Checkbox(Loc.TF("Draw DummyPoly Texts###Model{0}_DebugDispDummyPolyText", model.GUID), ref model.DebugDispDummyPolyText);

                        ImGui.Checkbox(Loc.TF("Draw Bone Glue Joints###Model{0}_DebugDispBoneGluers", model.GUID), ref model.DebugDispBoneGluers);

                        if (model.SkeletonFlver != null)
                        {
                            ImGui.Checkbox(Loc.TF("Draw FLVER Skeleton Transforms###Model{0}_DebugDispFlverSkeletonTransforms", model.GUID), ref model.SkeletonFlver.ForceDrawBoneTransforms);
                            ImGui.Checkbox(Loc.TF("Draw FLVER Skeleton Lines###Model{0}_DebugDispFlverSkeletonLines", model.GUID), ref model.SkeletonFlver.ForceDrawBoneLines);
                            ImGui.Checkbox(Loc.TF("Draw FLVER Skeleton Boxes###Model{0}_DebugDispFlverSkeletonBoxes", model.GUID), ref model.SkeletonFlver.ForceDrawBoneBoxes);
                            ImGui.Checkbox(Loc.TF("Draw FLVER Skeleton Texts###Model{0}_DebugDispFlverSkeletonText", model.GUID), ref model.SkeletonFlver.ForceDrawBoneText);
                        }

                        if (model.AnimContainer?.Skeleton != null)
                        {
                            ImGui.Checkbox(Loc.TF("Draw HKX Skeleton Transforms###Model{0}_DebugDispHkxSkeletonTransforms", model.GUID), ref model.AnimContainer.Skeleton.ForceDrawBoneTransforms);
                            ImGui.Checkbox(Loc.TF("Draw HKX Skeleton Lines###Model{0}_DebugDispHkxSkeletonLines", model.GUID), ref model.AnimContainer.Skeleton.ForceDrawBoneLines);
                            //ImGui.Checkbox($"Draw HKX Skeleton Boxes###Model{model.GUID}_DebugDispHkxSkeletonBoxes", ref model.AnimContainer.Skeleton.ForceDrawBoneBoxes);
                            ImGui.Checkbox(Loc.TF("Draw HKX Skeleton Texts###Model{0}_DebugDispHkxSkeletonText", model.GUID), ref model.AnimContainer.Skeleton.ForceDrawBoneText);
                        }
                        
                        ImGui.Separator();
                    }

                    void DoMainMeshOfModel(Model model)
                    {
                        if (model == null)
                            return;

                        var modelGUID = model.GUID;

                        var mesh = model.MainMesh;

                        if (mesh == null)
                            return;

                        void inner_Submeshes()
                        {
                            var nodeOpen = ImGui.TreeNode(Loc.TF("Meshes###SceneManager_Mesh[{0}|{1}]", modelGUID, index_Mesh));

                            //if (ImGui.IsItemHovered())
                            //{
                            //    GFX.HighlightedThing = mesh;
                            //}

                            if (nodeOpen)
                            {
                                foreach (var sm in mesh.Submeshes)
                                {
                                    bool isMeshGrayedOut = sm.NewMaterial.ModelMaskIndex >= 0 && sm.NewMaterial.ModelMaskIndex < mesh.DrawMask.Length && !mesh.DrawMask[sm.NewMaterial.ModelMaskIndex];
                                    ImGui.PushStyleColor(ImGuiCol.Text, !isMeshGrayedOut ? new System.Numerics.Vector4(1, 1, 1, 1)
                                        : new System.Numerics.Vector4(0.75f, 0.25f, 0.25f, 1));
                                    ImGui.PushStyleColor(ImGuiCol.TextDisabled, isMeshGrayedOut ? new System.Numerics.Vector4(1, 0, 0, 1)
                                        : new System.Numerics.Vector4(0, 1, 0, 1));
                                    sm.IsVisible = MenuBar.CheckboxDual(sm.FullMaterialName, sm.IsVisible, enabled: true,
                                        shortcut: (sm.NewMaterial.ModelMaskIndex >= 0 ? Loc.TF("[Mask {0}]", sm.NewMaterial.ModelMaskIndex) : null));

                                    if (ImGui.IsItemHovered())
                                    {
                                        GFX.HighlightedThing = sm;
                                    }

                                    ImGui.PopStyleColor(2);
                                }

                                ImGui.TreePop();
                            }
                        }

                        void inner_Materials(ref bool anyFieldFocused_Inner)
                        {
                            if (ImGui.TreeNode(Loc.TF("Materials###Model[{0}]_Materials", modelGUID)))
                            {
                                int index_Material = 0;
                                foreach (var m in mesh.Materials)
                                {
                                    int index_Sampler = 0;
                                    var nodeOpen = ImGui.TreeNode($"[{index_Material}] {(m.Name ?? "")} ---> {(m.ShaderConfigName ?? "Unk Shader")}###SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}");

                                    if (ImGui.IsItemHovered())
                                    {
                                        GFX.HighlightedThing = m;
                                    }

                                    if (nodeOpen)
                                    {
                                        ImGui.Button(Loc.TF("Save global shader config defaults for '{0}'###Name###SceneManager_Mesh[{1}|{2}]_Material{3}_SaveGlobalShaderConfig", m.ShaderConfigName, modelGUID, index_Mesh, index_Material));
                                        if (ImGui.IsItemClicked())
                                        {
                                            ImguiOSD.DialogManager.AskYesNo(Loc.T("Save Global Defaults?"), Loc.T("Save global defaults for this shader type? This will affect every material that uses this shader in any file you open."), choice =>
                                            {
                                                if (choice)
                                                {
                                                    FlverShaderConfig.SaveGlobalDefault(m.ShaderConfig, m.ShaderConfigName);
                                                    FlverShaderConfig.ClearCache();
                                                    DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.TryLoadGlobalShaderConfigs();
                                                }
                                            }, allowCancel: true, inputFlags: Dialog.InputFlag.EscapeKeyToCancel | Dialog.InputFlag.TitleBarXToCancel);
                                        }


                                        ImGui.LabelText(Loc.TF("Name###SceneManager_Mesh[{0}|{1}]_Material{2}_Name", modelGUID, index_Mesh, index_Material), m.Name ?? "<NULL>");
                                        //ImGui.LabelText($"MTD###SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_MTD", m.MtdName ?? "<NULL>");
                                        ImGui.TextWrapped($"MTD : {(m.MtdName ?? "<NULL>")}");

                                        if (ImGui.TreeNode(Loc.TF("MTD Parameters###SceneManager_Mesh[{0}|{1}]_Material{2}_MTDParameters", modelGUID, index_Mesh, index_Material)))
                                        {


                                            var p = m.MtdInfo.ShaderParameters;
                                            foreach (var k in m.MtdInfo.ShaderParameters.Keys)
                                            {
                                                bool isUsed = m.MtdInfo.IsParameterUsedByDSAS(k);
                                                var imguiKey = $"SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_MTDParameters_{k}";
                                                if (!isUsed)
                                                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1));

                                                if (p[k] is bool asBool)
                                                {
                                                    var v = asBool;
                                                    ImGui.Checkbox(Loc.TF("{0}###{1}", k, imguiKey), ref v);
                                                    anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                    p[k] = v;
                                                }
                                                else if (p[k] is int asInt)
                                                {
                                                    var v = asInt;
                                                    ImGui.InputInt(Loc.TF("{0}###{1}", k, imguiKey), ref v);
                                                    anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                    p[k] = v;
                                                }
                                                else if (p[k] is int[] asIntArr)
                                                {
                                                    for (int i = 0; i < asIntArr.Length; i++)
                                                    {
                                                        var v = asIntArr[i];
                                                        ImGui.InputInt($"{(i == 0 ? k : " ")}###{imguiKey}[{i}]", ref v);
                                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                        asIntArr[i] = v;
                                                    }
                                                }
                                                else if (p[k] is float asFloat)
                                                {
                                                    var v = asFloat;
                                                    ImGui.InputFloat(Loc.TF("{0}###{1}", k, imguiKey), ref v);
                                                    anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                    p[k] = v;
                                                }
                                                else if (p[k] is float[] asFloatArr)
                                                {
                                                    if (asFloatArr.Length == 1)
                                                    {
                                                        var v = asFloatArr[0];
                                                        ImGui.InputFloat(Loc.TF("{0}###{1}", k, imguiKey), ref v);
                                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                        asFloatArr[0] = v;
                                                    }
                                                    else if (asFloatArr.Length == 2)
                                                    {
                                                        var v = new System.Numerics.Vector2(asFloatArr[0], asFloatArr[1]);
                                                        ImGui.InputFloat2(Loc.TF("{0}###{1}", k, imguiKey), ref v);
                                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                        asFloatArr[0] = v.X;
                                                        asFloatArr[1] = v.Y;
                                                    }
                                                    else if (asFloatArr.Length == 3)
                                                    {
                                                        var v = new System.Numerics.Vector3(asFloatArr[0], asFloatArr[1], asFloatArr[2]);
                                                        ImGui.InputFloat3(Loc.TF("{0}###{1}", k, imguiKey), ref v);
                                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                        asFloatArr[0] = v.X;
                                                        asFloatArr[1] = v.Y;
                                                        asFloatArr[2] = v.Z;
                                                    }
                                                    else if (asFloatArr.Length == 4)
                                                    {
                                                        var v = new System.Numerics.Vector4(asFloatArr[0], asFloatArr[1], asFloatArr[2], asFloatArr[3]);
                                                        ImGui.InputFloat4(Loc.TF("{0}###{1}", k, imguiKey), ref v);
                                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                        asFloatArr[0] = v.X;
                                                        asFloatArr[1] = v.Y;
                                                        asFloatArr[2] = v.Z;
                                                        asFloatArr[3] = v.W;
                                                    }
                                                    else if (asFloatArr.Length == 5)
                                                    {
                                                        for (int i = 0; i < asFloatArr.Length; i++)
                                                        {
                                                            var v = asFloatArr[i];
                                                            ImGui.InputFloat($"{(i == 0 ? k : " ")}###{imguiKey}[{i}]", ref v);
                                                            anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                            asFloatArr[i] = v;
                                                        }
                                                    }

                                                    
                                                }

                                                if (!isUsed)
                                                    ImGui.PopStyleColor();
                                            }

                                            ImGui.TreePop();
                                        }

                                        ImGui.LabelText(Loc.TF("Shader###SceneManager_Mesh[{0}|{1}]_Material{2}_Shader", modelGUID, index_Mesh, index_Material), m.ShaderConfigName ?? "<NULL>");

                                        ImGui.Separator();

                                        System.Numerics.Vector2 uvTile1 = m.UVTileMult1.ToNumerics();
                                        ImGui.InputFloat2(Loc.TF("UV Group 1 Tile Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_UVTileMult1", modelGUID, index_Mesh, index_Material), ref uvTile1);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.UVTileMult1 = uvTile1.ToXna();

                                        System.Numerics.Vector2 uvTile2 = m.UVTileMult2.ToNumerics();
                                        ImGui.InputFloat2(Loc.TF("UV Group 2 Tile Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_UVTileMult2", modelGUID, index_Mesh, index_Material), ref uvTile2);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.UVTileMult2 = uvTile2.ToXna();

                                        System.Numerics.Vector2 uvTile3 = m.UVTileMult3.ToNumerics();
                                        ImGui.InputFloat2(Loc.TF("UV Group 3 Tile Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_UVTileMult3", modelGUID, index_Mesh, index_Material), ref uvTile3);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.UVTileMult3 = uvTile3.ToXna();

                                        System.Numerics.Vector2 uvTile4 = m.UVTileMult4.ToNumerics();
                                        ImGui.InputFloat2(Loc.TF("UV Group 4 Tile Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_UVTileMult4", modelGUID, index_Mesh, index_Material), ref uvTile4);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.UVTileMult4 = uvTile4.ToXna();

                                        System.Numerics.Vector2 uvTile5 = m.UVTileMult5.ToNumerics();
                                        ImGui.InputFloat2(Loc.TF("UV Group 5 Tile Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_UVTileMult5", modelGUID, index_Mesh, index_Material), ref uvTile5);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.UVTileMult5 = uvTile5.ToXna();

                                        System.Numerics.Vector2 uvTile6 = m.UVTileMult6.ToNumerics();
                                        ImGui.InputFloat2(Loc.TF("UV Group 6 Tile Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_UVTileMult6", modelGUID, index_Mesh, index_Material), ref uvTile6);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.UVTileMult6 = uvTile6.ToXna();

                                        System.Numerics.Vector2 uvTile7 = m.UVTileMult7.ToNumerics();
                                        ImGui.InputFloat2(Loc.TF("UV Group 7 Tile Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_UVTileMult7", modelGUID, index_Mesh, index_Material), ref uvTile7);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.UVTileMult7 = uvTile7.ToXna();

                                        System.Numerics.Vector2 uvTile8 = m.UVTileMult8.ToNumerics();
                                        ImGui.InputFloat2(Loc.TF("UV Group 8 Tile Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_UVTileMult8", modelGUID, index_Mesh, index_Material), ref uvTile8);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.UVTileMult8 = uvTile8.ToXna();

                                        ImGui.Separator();



                                        if (ImGui.TreeNode(Loc.TF("Blend Settings - Diffuse###SceneManager_Mesh[{0}|{1}]_Material{2}_BlendSettings_Diffuse", modelGUID, index_Mesh, index_Material)))
                                        {
                                            FlverShaderEnums.NewBlendOperations_Picker.ShowPickerCombo(Loc.T("Blend Operation"), $"SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_NewBlendOperation", ref m.ShaderConfig.NewBlendOperation_Diffuse, NewBlendOperations.Multiply);
                                            ImGui.Checkbox(Loc.TF("Diffuse - Reverse Blend Dir###SceneManager_Mesh[{0}|{1}]_Material{2}_NewBlendReverseDir_Diffuse", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.NewBlendReverseDir_Diffuse);
                                            ImGui.Checkbox(Loc.TF("Diffuse - Invert Blend Mask Value###SceneManager_Mesh[{0}|{1}]_Material{2}_NewBlendInverseVal_Diffuse", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.NewBlendInverseVal_Diffuse);
                                            ImGui.TreePop();
                                        }

                                        ImGui.Separator();

                                        if (ImGui.TreeNode(Loc.TF("Blend Settings - Specular###SceneManager_Mesh[{0}|{1}]_Material{2}_BlendSettings_Specular", modelGUID, index_Mesh, index_Material)))
                                        {
                                            FlverShaderEnums.NewBlendOperations_Picker.ShowPickerCombo(Loc.T("Specular - Blend Operation"), $"SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_NewBlendOperation_Specular", ref m.ShaderConfig.NewBlendOperation_Specular, NewBlendOperations.Multiply);
                                            ImGui.Checkbox(Loc.TF("Specular - Reverse Blend Dir###SceneManager_Mesh[{0}|{1}]_Material{2}_NewBlendReverseDir_Specular", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.NewBlendReverseDir_Specular);
                                            ImGui.Checkbox(Loc.TF("Specular - Invert Blend Mask Value###SceneManager_Mesh[{0}|{1}]_Material{2}_NewBlendInverseVal_Specular", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.NewBlendInverseVal_Specular);
                                            ImGui.TreePop();
                                        }
                                        ImGui.Separator();

                                        if (ImGui.TreeNode(Loc.TF("Blend Settings - Normal###SceneManager_Mesh[{0}|{1}]_Material{2}_BlendSettings_Normal", modelGUID, index_Mesh, index_Material)))
                                        {
                                            FlverShaderEnums.NewBlendOperations_Picker.ShowPickerCombo(Loc.T("Normal - Blend Operation"), $"SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_NewBlendOperation_Normal", ref m.ShaderConfig.NewBlendOperation_Normal, NewBlendOperations.Multiply);
                                            ImGui.Checkbox(Loc.TF("Normal - Reverse Blend Dir###SceneManager_Mesh[{0}|{1}]_Material{2}_NewBlendReverseDir_Normal", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.NewBlendReverseDir_Normal);
                                            ImGui.Checkbox(Loc.TF("Normal - Invert Blend Mask Value###SceneManager_Mesh[{0}|{1}]_Material{2}_NewBlendInverseVal_Normal", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.NewBlendInverseVal_Normal);
                                            ImGui.TreePop();
                                        }
                                        ImGui.Separator();

                                        if (ImGui.TreeNode(Loc.TF("Blend Settings - Shininess###SceneManager_Mesh[{0}|{1}]_Material{2}_BlendSettings_Shininess", modelGUID, index_Mesh, index_Material)))
                                        {
                                            FlverShaderEnums.NewBlendOperations_Picker.ShowPickerCombo(Loc.T("Shininess - Blend Operation"), $"SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_NewBlendOperation_Shininess", ref m.ShaderConfig.NewBlendOperation_Shininess, NewBlendOperations.Multiply);
                                            ImGui.Checkbox(Loc.TF("Shininess - Reverse Blend Dir###SceneManager_Mesh[{0}|{1}]_Material{2}_NewBlendReverseDir_Shininess", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.NewBlendReverseDir_Shininess);
                                            ImGui.Checkbox(Loc.TF("Shininess - Invert Blend Mask Value###SceneManager_Mesh[{0}|{1}]_Material{2}_NewBlendInverseVal_Shininess", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.NewBlendInverseVal_Shininess);
                                            ImGui.TreePop();
                                        }
                                        ImGui.Separator();

                                        if (ImGui.TreeNode(Loc.TF("Blend Settings - Emissive###SceneManager_Mesh[{0}|{1}]_Material{2}_BlendSettings_Emissive", modelGUID, index_Mesh, index_Material)))
                                        {
                                            FlverShaderEnums.NewBlendOperations_Picker.ShowPickerCombo(Loc.T("Emissive - Blend Operation"), $"SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_NewBlendOperation_Emissive", ref m.ShaderConfig.NewBlendOperation_Emissive, NewBlendOperations.Multiply);
                                            ImGui.Checkbox(Loc.TF("Emissive - Reverse Blend Dir###SceneManager_Mesh[{0}|{1}]_Material{2}_NewBlendReverseDir_Emissive", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.NewBlendReverseDir_Emissive);
                                            ImGui.Checkbox(Loc.TF("Emissive - Invert Blend Mask Value###SceneManager_Mesh[{0}|{1}]_Material{2}_NewBlendInverseVal_Emissive", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.NewBlendInverseVal_Emissive);
                                            ImGui.TreePop();
                                        }




                                        ImGui.Separator();

                                        ImGui.Checkbox(Loc.TF("Visible###SceneManager_Mesh[{0}|{1}]_Material{2}_IsVisible", modelGUID, index_Mesh, index_Material), ref m.IsVisible);

                                        bool isSoloVisible = false;
                                        if (DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.AnyMaterialSoloVisible())
                                        {
                                            isSoloVisible = DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.GetMaterialSoloVisible(model, mesh, m);
                                        }
                                        bool prevSoloVisible = isSoloVisible;
                                        ImGui.Checkbox(Loc.TF("Is The ONLY One Visible###SceneManager_Mesh[{0}|{1}]_Material{2}_IsSoloVisible", modelGUID, index_Mesh, index_Material), ref isSoloVisible);
                                        if (isSoloVisible && !prevSoloVisible)
                                        {
                                            DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.SetMaterialSoloVisible(model, mesh, m);
                                        }
                                        else if (!isSoloVisible && prevSoloVisible)
                                        {
                                            DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.ClearMaterialSoloVisible();
                                        }

                                        string[] ptdeMtdTypes = new string[]
                                        {
                                            Loc.T("Default"),
                                            Loc.T("Metal"),
                                            Loc.T("Wet"),
                                            Loc.T("Dull"),
                                        };
                                        int ptdeMtdTypeIndex = (int)m.PtdeMtdType;
                                        ImGui.Combo(Loc.TF("DS1 PTDE MTD Type###SceneManager_Mesh[{0}|{1}]_Material{2}_PtdeMtdType", modelGUID, index_Mesh, index_Material),
                                            ref ptdeMtdTypeIndex, ptdeMtdTypes,
                                            ptdeMtdTypes.Length);
                                        m.PtdeMtdType = (PtdeMtdTypes)ptdeMtdTypeIndex;

                                        ImGui.Checkbox(Loc.TF("IsDS2EmissiveFlow###SceneManager_Mesh[{0}|{1}]_Material{2}_IsDS2EmissiveFlow", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.IsDS2EmissiveFlow);
                                        ImGui.Checkbox(Loc.TF("IsDS3Veil###SceneManager_Mesh[{0}|{1}]_Material{2}_IsDS3Veil", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.IsDS3Veil);
                                        ImGui.Checkbox(Loc.TF("IsShaderDoubleFaceCloth###SceneManager_Mesh[{0}|{1}]_Material{2}_IsShaderDoubleFaceCloth", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.IsShaderDoubleFaceCloth);
                                        ImGui.Checkbox(Loc.TF("Invert Blend Mask###SceneManager_Mesh[{0}|{1}]_Material{2}_InvertBlendMaskMap", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.InvertBlendMaskMap);

                                        ImGui.Separator();
                                        ImGui.Checkbox(Loc.TF("Is Metallic###SceneManager_Mesh[{0}|{1}]_Material{2}_IsMetallic", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.IsMetallic);

                                        ImGui.SliderFloat(Loc.TF("Metallic Diffuse Decrease Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_MetallicDiffuseDecreaseMult", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.MetallicDiffuseDecreaseMult, 0, 1);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        ImGui.SliderFloat(Loc.TF("Metallic Specular Increase Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_MetallicSpecularIncreaseMult", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.MetallicSpecularIncreaseMult, 0, 2);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        ImGui.SliderFloat(Loc.TF("Metallic Specular Increase Power###SceneManager_Mesh[{0}|{1}]_Material{2}_MetallicSpecularIncreasePower", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.MetallicSpecularIncreasePower, 0, 2);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();

                                        ImGui.Checkbox(Loc.TF("Invert Metallic###SceneManager_Mesh[{0}|{1}]_Material{2}_InvertMetallic", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.InvertMetallic);
                                        ImGui.Checkbox(Loc.TF("Is Undefined Metallic###SceneManager_Mesh[{0}|{1}]_Material{2}_IsUndefinedMetallic", modelGUID, index_Mesh, index_Material), ref m.IsUndefinedMetallic);
                                        ImGui.SliderFloat(Loc.TF("Undefined Metallic Value###SceneManager_Mesh[{0}|{1}]_Material{2}_UndefinedMetallicValue", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.UndefinedMetallicValue, 0, 1);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        var nonmetallicColor = m.ShaderConfig.NonMetallicSpecColor.ToNumerics();
                                        ImGui.ColorEdit3(Loc.TF("NonMetallicSpecColor###SceneManager_Mesh[{0}|{1}]_Material{2}_NonMetallicSpecColor", modelGUID, index_Mesh, index_Material), ref nonmetallicColor, ImGuiColorEditFlags.DisplayHSV | ImGuiColorEditFlags.InputRGB);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.ShaderConfig.NonMetallicSpecColor = nonmetallicColor.ToXna();

                                        ImGui.Separator();
                                        ImGui.Checkbox(Loc.TF("Enable Blend Mask###SceneManager_Mesh[{0}|{1}]_Material{2}_EnableBlendMask", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.EnableBlendMask);
                                        ImGui.Checkbox(Loc.TF("Is Undefined Blend Mask###SceneManager_Mesh[{0}|{1}]_Material{2}_IsUndefinedBlendMask", modelGUID, index_Mesh, index_Material), ref m.IsUndefinedBlendMask);
                                        ImGui.SliderFloat(Loc.TF("Undefined Blend Mask Value###SceneManager_Mesh[{0}|{1}]_Material{2}_UndefinedBlendMaskValue", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.UndefinedBlendMaskValue, 0, 1);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        ImGui.Checkbox(Loc.TF("Get Blend Mask From Normal Map 1 Alpha###SceneManager_Mesh[{0}|{1}]_Material{2}_BlendMaskFromNormalMap1Alpha", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.BlendMaskFromNormalMap1Alpha);
                                        ImGui.Checkbox(Loc.TF("Get Blend Mask From Normal Map 1 Alpha - Is Inverted###SceneManager_Mesh[{0}|{1}]_Material{2}_BlendMaskFromNormalMap1Alpha_IsReverse", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.BlendMaskFromNormalMap1Alpha_IsReverse);
                                        ImGui.Checkbox(Loc.TF("Mult Blend Mask By Albedo Map 2 Alpha###SceneManager_Mesh[{0}|{1}]_Material{2}_BlendMaskMultByAlbedoMap2Alpha", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.BlendMaskMultByAlbedoMap2Alpha);
                                        ImGui.Checkbox(Loc.TF("Mult Blend Mask By Albedo Map 2 Alpha - Is Inverted###SceneManager_Mesh[{0}|{1}]_Material{2}_BlendMaskMultByAlbedoMap2Alpha_IsReverse", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.BlendMaskMultByAlbedoMap2Alpha_IsReverse);
                                        ImGui.Separator();

                                        ImGui.Checkbox(Loc.TF("Enable Texture Alphas###SceneManager_Mesh[{0}|{1}]_Material{2}_EnableAlphas", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.EnableAlphas);
                                        ImGui.Checkbox(Loc.TF("Use Fancy Texture Alphas###SceneManager_Mesh[{0}|{1}]_Material{2}_UseFancyAlphas", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.UseFancyAlphas);
                                        ImGui.SliderFloat(Loc.TF("Fancy Texture Alpha Cutoff###SceneManager_Mesh[{0}|{1}]_Material{2}_FancyAlphaCutoff", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.FancyAlphaCutoff, 0, 1, (m.ShaderConfig.FancyAlphaCutoff < 1) ? "%.2f" : Loc.T("(Use Default)"));
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        //ImGui.Checkbox("Enable Texture Blending", ref GFX.FlverEnableTextureBlending);

                                        ImGui.Checkbox(Loc.TF("Albedo Alpha Mult In Normal Map Alpha###SceneManager_Mesh[{0}|{1}]_Material{2}_IsAlbedoAlphaMultInNormalAlpha", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.IsAlbedoAlphaMultInNormalAlpha);

                                        ImGui.Separator();

                                        ImGui.SliderFloat(Loc.TF("Direct Light Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_DirectLightMult", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.DirectLightingMult, 0, 3);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();

                                        OSD.TooltipManager_Scene.DoTooltip(Loc.T("Direct Light Multiplier"), Loc.T("Multiplies the brightness of light reflected directly off" +
                                            "\nthe surface of the model."));


                                        ImGui.SliderFloat(Loc.TF("Indirect Light Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_IndirectLightMult", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.IndirectLightingMult, 0, 3);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        OSD.TooltipManager_Scene.DoTooltip(Loc.T("Indirect Light Multiplier"), Loc.T("Multiplies the brightness of environment map lighting reflected."));

                                        ImGui.SliderFloat(Loc.T("Direct Diffuse Mult"), ref m.ShaderConfig.DirectDiffuseMult, 0, 3);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        ImGui.SliderFloat(Loc.T("Direct Specular Mult"), ref m.ShaderConfig.DirectSpecularMult, 0, 3);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        ImGui.SliderFloat(Loc.T("Indirect Diffuse Mult"), ref m.ShaderConfig.IndirectDiffuseMult, 0, 3);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        ImGui.SliderFloat(Loc.T("Indirect Specular Mult"), ref m.ShaderConfig.IndirectSpecularMult, 0, 3);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();

                                        //ImGui.SliderFloat("Ambient Light Mult", ref Environment.AmbientLightMult, 0, 3);
                                        ImGui.SliderFloat(Loc.TF("Specular Power Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_SpecularPowerMult", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.SpecularPowerMult, 1, 8);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        OSD.TooltipManager_Scene.DoTooltip(Loc.T("Specular Power Multiplier"), Loc.T("Multiplies the specular power of the lighting. " +
                                            "\nHigher makes thing's very glossy. " +
                                            "\nMight make some Bloodborne kin of the cosmos look more accurate."));
                                        ImGui.SliderFloat(Loc.TF("LdotN Power Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_LdotNPowerMult", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.LdotNPowerMult, 1, 8);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        OSD.TooltipManager_Scene.DoTooltip(Loc.T("L dot N Vector Power Multiplier"), Loc.T("Advanced setting. If you know you know."));

                                        ImGui.SliderFloat(Loc.TF("Emissive Light Mult###SceneManager_Mesh[{0}|{1}]_Material{2}_EmissiveLightMult", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.EmissiveMult, 0, 3);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();

                                        OSD.TooltipManager_Scene.DoTooltip(Loc.T("Emissive Light Mult"), Loc.T("Multiplies the brightness of light emitted by the model's " +
                                            "\nemissive texture map, if applicable."));


                                        ImGui.Checkbox(Loc.TF("Get Emissive Color From Albedo###SceneManager_Mesh[{0}|{1}]_Material{2}_EmissiveColorFromAlbedo", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.EmissiveColorFromAlbedo);


                                        ImGui.Separator();

                                        ImGui.Checkbox(Loc.TF("Reflectance Mult In Normal Map Alpha###SceneManager_Mesh[{0}|{1}]_Material{2}_IsReflectMultInNormalAlpha", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.IsReflectMultInNormalAlpha);
                                        ImGui.Checkbox(Loc.TF("Metallic Mult In Normal Map Alpha###SceneManager_Mesh[{0}|{1}]_Material{2}_IsMetallicMultInNormalAlpha", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.IsMetallicInNormalAlpha);
                                        ImGui.Checkbox(Loc.TF("Swap X & Y Channels In Normal Map###SceneManager_Mesh[{0}|{1}]_Material{2}_SwapNormalXY", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.SwapNormalXY);

                                        ImGui.Separator();

                                        int curChrCustomizeType = m.ShaderConfig.chrCustomizeType_Values.IndexOf(m.ShaderConfig.ChrCustomizeType);
                                        ImGui.ListBox(Loc.TF("ChrCustomize Type###SceneManager_Mesh[{0}|{1}]_Material{2}_ChrCustomizeType", modelGUID, index_Mesh, index_Material), ref curChrCustomizeType, m.ShaderConfig.chrCustomizeType_Names, m.ShaderConfig.chrCustomizeType_Names.Length);
                                        if (curChrCustomizeType == -1)
                                            curChrCustomizeType = 0;
                                        m.ShaderConfig.ChrCustomizeType = m.ShaderConfig.chrCustomizeType_Values[curChrCustomizeType];

                                        ImGui.Checkbox(Loc.TF("ChrCustomize Uses Normal Map Alpha###SceneManager_Mesh[{0}|{1}]_Material{2}_ChrCustomizeUseNormalMapAlpha", modelGUID, index_Mesh, index_Material),
                                            ref m.ShaderConfig.ChrCustomizeUseNormalMapAlpha);
                                        
                                        ImGui.Checkbox(Loc.TF("Enable SSS###SceneManager_Mesh[{0}|{1}]_Material{2}_UseSSS", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.EnableSSS);
                                        var sssColor = m.ShaderConfig.SSSColor.ToCS();
                                        ImGui.DragFloat3(Loc.TF("SSS Color###SceneManager_Mesh[{0}|{1}]_Material{2}_SSSColor", modelGUID, index_Mesh, index_Material), ref sssColor);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.ShaderConfig.SSSColor = sssColor.ToXna();
                                        ImGui.DragFloat(Loc.TF("SSS Intensity###SceneManager_Mesh[{0}|{1}]_Material{2}_SSSIntensity", modelGUID, index_Mesh, index_Material), ref m.ShaderConfig.SSSIntensity, 0.01f);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();

                                        ImGui.Separator();

                                        FlverShaderEnums.NewDebugTypes_Picker.ShowPickerCombo(Loc.T("Shader Debug Type"), $"SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_ShaderDebugType", ref m.NewDebugType, NewDebugTypes.None);

                                        //int drawStepIndex = GFX.DRAW_STEP_LIST.ToList().IndexOf(m.DrawStep);
                                        //var drawStepNames = GFX.DRAW_STEP_LIST.Select(x => x.ToString()).ToArray();
                                        //ImGui.ListBox($"DRAW_STEP###SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_DRAW_STEP",
                                        //    ref drawStepIndex, drawStepNames,
                                        //    GFX.DRAW_STEP_LIST.Length);
                                        //m.DrawStep = GFX.DRAW_STEP_LIST[drawStepIndex];

                                        //int shadingModeIndex = GFX.FlverShadingModeList_NoDefault.IndexOf(m.ShadingMode);
                                        //ImGui.ListBox($"Shading Mode###SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_ShadingMode",
                                        //    ref shadingModeIndex, GFX.FlverShadingModeNamesList_NoDefault,
                                        //    GFX.FlverShadingModeNamesList_NoDefault.Length);
                                        //m.ShadingMode = GFX.FlverShadingModeList_NoDefault[shadingModeIndex];

                                        ImGui.Separator();
                                        
                                        float uvOffsetX = m.DebugUVOffset.X, uvOffsetY = m.DebugUVOffset.Y;
                                        ImGui.DragFloat(Loc.TF("[For Testing]UV Offset X###SceneManager_Mesh[{0}|{1}]_Material{2}_DebugUVOffsetX", modelGUID, index_Mesh, index_Material), ref uvOffsetX, 0.001f);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        ImGui.DragFloat(Loc.TF("[For Testing]UV Offset Y###SceneManager_Mesh[{0}|{1}]_Material{2}_DebugUVOffsetY", modelGUID, index_Mesh, index_Material), ref uvOffsetY, 0.001f);
                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                        m.DebugUVOffset = new Microsoft.Xna.Framework.Vector2(uvOffsetX, uvOffsetY);
                                        
                                        ImGui.Separator();
                                        
                                        if (ImGui.TreeNode(Loc.TF("Samplers###SceneManager_Mesh[{0}|{1}]_Material{2}_Samplers", modelGUID, index_Mesh, index_Material)))
                                        {
                                            bool anySamplerDebugView = m.Samplers.Any(smp => smp.IsDebugView);

                                            foreach (var s in m.Samplers)
                                            {
                                                bool samplerNodeOpen = ImGui.TreeNode(Loc.TF("{0} [{1}]###SceneManager_Mesh[{2}|{3}]_Material{4}_Sampler{5}", s.Name, s.TexPath, modelGUID, index_Mesh, index_Material, index_Sampler));
                                                var matchingSamplerConfig = m.GetSamplerConfig(s.Name);
                                                if (ImGui.IsItemHovered() && !anySamplerDebugView)
                                                {
                                                    GFX.NewDebug_ShowTex_Material = m;
                                                    GFX.NewDebug_ShowTex_Sampler = s;
                                                    GFX.NewDebug_ShowTex_SamplerConfig = matchingSamplerConfig;
                                                }

                                                if (samplerNodeOpen)
                                                {
                                                    
                                                    if (matchingSamplerConfig != null)
                                                    {
                                                        int texTypeIndex = FlverMaterial.AllTextureTypes.IndexOf(matchingSamplerConfig.TexType);
                                                        ImGui.Combo(Loc.TF("Texture Type###SceneManager_Mesh[{0}|{1}]_Material{2}_Sampler{3}_TextureType", modelGUID, index_Mesh, index_Material, index_Sampler), ref texTypeIndex, FlverMaterial.AllTextureTypeNames, FlverMaterial.AllTextureTypeNames.Length);
                                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                        if (texTypeIndex < 0)
                                                            texTypeIndex = 0;
                                                        if (texTypeIndex >= FlverMaterial.AllTextureTypes.Count)
                                                            texTypeIndex = FlverMaterial.AllTextureTypes.Count - 1;
                                                        matchingSamplerConfig.TexType = FlverMaterial.AllTextureTypes[texTypeIndex];

                                                        int uvIndexPlusOne = matchingSamplerConfig.UVIndex + 1;
                                                        ImGui.InputInt(Loc.TF("UV Map###SceneManager_Mesh[{0}|{1}]_Material{2}_Sampler{3}_UVMapIndex", modelGUID, index_Mesh, index_Material, index_Sampler), ref uvIndexPlusOne);
                                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                        if (uvIndexPlusOne < 1)
                                                            uvIndexPlusOne = 1;
                                                        if (uvIndexPlusOne > 8)
                                                            uvIndexPlusOne = 8;
                                                        matchingSamplerConfig.UVIndex = uvIndexPlusOne - 1;


                                                        int uvGroupPlusOne = matchingSamplerConfig.UVGroup + 1;
                                                        ImGui.InputInt(Loc.TF("UV Group###SceneManager_Mesh[{0}|{1}]_Material{2}_Sampler{3}_UVMapGroup", modelGUID, index_Mesh, index_Material, index_Sampler), ref uvGroupPlusOne);
                                                        anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                        if (uvGroupPlusOne < 1)
                                                            uvGroupPlusOne = 1;
                                                        if (uvGroupPlusOne > 8)
                                                            uvGroupPlusOne = 8;
                                                        matchingSamplerConfig.UVGroup = uvGroupPlusOne - 1;


                                                        ImGui.LabelText(Loc.TF("Default Texture###SceneManager_Mesh[{0}|{1}]_Material{2}_Sampler{3}_DefaultTexName", modelGUID, index_Mesh, index_Material, index_Sampler), matchingSamplerConfig.DefaultTexPath ?? "<NULL>");
                                                    }
                                                    else
                                                    {
                                                        ImGui.LabelText(Loc.T(" "), Loc.T("Error: SamplerConfig not found."));
                                                    }

                                                    
                                                    float uvSizeX = s.UVScale.X, uvSizeY = s.UVScale.Y;
                                                    ImGui.InputFloat(Loc.TF("UV Scale X###SceneManager_Mesh[{0}|{1}]_Material{2}_Sampler{3}_UVScaleX", modelGUID, index_Mesh, index_Material, index_Sampler), ref uvSizeX);
                                                    anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                    ImGui.InputFloat(Loc.TF("UV Scale Y###SceneManager_Mesh[{0}|{1}]_Material{2}_Sampler{3}_UVScaleY", modelGUID, index_Mesh, index_Material, index_Sampler), ref uvSizeY);
                                                    anyFieldFocused_Inner |= ImGui.IsItemActive();
                                                    s.UVScale = new Microsoft.Xna.Framework.Vector2(uvSizeX, uvSizeY);
                                                    
                                                    
                                                    
                                                   
                                                    
                                                    

                                                    ImGui.Separator();

                                                    bool prevDebugView = s.IsDebugView;

                                                    ImGui.Checkbox(Loc.TF("Lock Debug Render On###SceneManager_Mesh[{0}|{1}]_Material{2}_Sampler{3}_IsDebugView", modelGUID, index_Mesh, index_Material, index_Sampler), ref s.IsDebugView);
                                                    FlverShaderEnums.NewDebug_ShowTex_ChannelConfigs_Picker.ShowPickerCombo(Loc.T("Debug Render Channels"), 
                                                        $"SceneManager_Mesh[{modelGUID}|{index_Mesh}]_Material{index_Material}_Sampler{index_Sampler}_ChannelConfig_ForDebug", 
                                                        ref s.ChannelConfig_ForDebug, NewDebug_ShowTex_ChannelConfigs.RGBA);

                                                    if (s.IsDebugView)
                                                    {
                                                        GFX.NewDebug_ShowTex_Material = m;
                                                        GFX.NewDebug_ShowTex_Sampler = s;
                                                        GFX.NewDebug_ShowTex_SamplerConfig = matchingSamplerConfig;

                                                        // On initial enable, shutoff all others
                                                        if (!prevDebugView)
                                                        {
                                                            var memes = m.Samplers.Where(smp => smp != s).ToList();
                                                            foreach (var meme in memes)
                                                                meme.IsDebugView = false;
                                                        }
                                                    }

                                                    ImGui.TreePop();
                                                }
                                                else
                                                {
                                                    s.IsDebugView = false;
                                                }

                                                index_Sampler++;

                                            }

                                            ImGui.TreePop();
                                        }


                                        ImGui.Separator();
                                        ImGui.LabelText(Loc.T(" "), " ");
                                        ImGui.Separator();

                                        ImGui.TreePop();
                                    }




                                    index_Material++;
                                }

                                ImGui.TreePop();
                            }
                           
                        }

                        inner_Submeshes();
                        inner_Materials(ref anyFieldFocused);

                        //if (meshKindName != null)
                        //{
                        //    var meshName = (mesh?.Name ?? model?.Name);
                        //    var dispMeshName = meshName != null ? Utils.GetShortIngameFileName(meshName) : "None";
                        //    if (dispMeshName.Contains("_"))
                        //        dispMeshName = dispMeshName.ToUpperInvariant();
                        //    if (dispMeshName.Contains("facegen"))
                        //        dispMeshName = dispMeshName.Replace("facegen", "FaceGen");
                        //    if (ImGui.TreeNode($"{meshKindName} <{dispMeshName}>"))
                        //    {
                        //        inner_Submeshes();
                        //        inner_Materials();
                        //        ImGui.TreePop();
                        //    }
                        //}
                        //else
                        //{
                        //    inner_Submeshes();
                        //    inner_Materials();
                        //}

                        index_Mesh++;
                    }

                    void DoDummyPolyOfModel(Model model)
                    {
                        bool thisFrameHover_DummyPoly = false;
                        if (ImGui.TreeNode(Loc.T("[DummyPoly]")))
                        {
                            if (model.DummyPolyMan != null)
                            {


                                void DoDummyPolyManager(NewDummyPolyManager dmyPolyMan, string dmyPolyGroupName)
                                {
                                    if (dmyPolyMan == null)
                                        return;

                                    if (OSD.RequestExpandAllTreeNodes)
                                        ImGui.SetNextItemOpen(true);

                                    ImGui.Button(Loc.TF("Show All"));
                                    if (ImGui.IsItemClicked())
                                    {
                                        foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                        {
                                            dmyPolyMan.SetDummyPolyVisibility(kvp.Key, true);
                                        }
                                    }

                                    ImGui.Button(Loc.TF("Hide All"));
                                    if (ImGui.IsItemClicked())
                                    {
                                        foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                        {
                                            dmyPolyMan.SetDummyPolyVisibility(kvp.Key, false);
                                        }
                                    }

                                    ImGui.Button(Loc.TF("Invert All"));
                                    if (ImGui.IsItemClicked())
                                    {
                                        foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                        {
                                            dmyPolyMan.SetDummyPolyVisibility(kvp.Key, !dmyPolyMan.DummyPolyVisibleByRefID[kvp.Key]);
                                        }
                                    }

                                    foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                    {
                                        if (dmyPolyMan.DummyPolyVisibleByRefID.ContainsKey(kvp.Key))
                                        {
                                            bool dmyVis = dmyPolyMan.DummyPolyVisibleByRefID[kvp.Key];

                                            bool highlightColor = model.DummyPolyMan.GlobalForceDummyPolyIDVisible == (dmyPolyMan.GlobalDummyPolyIDOffset + kvp.Key);

                                            if (highlightColor)
                                            {
                                                ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0, 1, 1, 1));
                                            }


                                            //ImGui.Checkbox($"{kvp.Key} ({kvp.Value.Count}x)                                       ", ref dmyVis);

                                            ImGui.Selectable(Loc.TF("{0} ({1}x)                                       ", kvp.Key, kvp.Value.Count), ref dmyVis);

                                            dmyPolyMan.SetDummyPolyVisibility(kvp.Key, dmyVis);
                                            if (!thisFrameHover_DummyPoly && ImGui.IsItemHovered())
                                            {
                                                thisFrameHover_DummyPoly = true;
                                                model.DummyPolyMan.GlobalForceDummyPolyIDVisible = (dmyPolyMan.GlobalDummyPolyIDOffset + kvp.Key);
                                            }

                                            if (highlightColor)
                                                ImGui.PopStyleColor();
                                        }
                                    }


                                    ImGui.Separator();
                                }



                                if (!thisFrameHover_DummyPoly)
                                {
                                    model.DummyPolyMan.GlobalForceDummyPolyIDVisible = -1;
                                }

                                //ImGui.Separator();

                                void DummyOperationShowAll(NewDummyPolyManager dmyPolyMan)
                                {
                                    if (dmyPolyMan == null)
                                        return;
                                    foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                        dmyPolyMan.SetDummyPolyVisibility(kvp.Key, true);
                                }

                                void DummyOperationHideAll(NewDummyPolyManager dmyPolyMan)
                                {
                                    if (dmyPolyMan == null)
                                        return;
                                    foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                        dmyPolyMan.SetDummyPolyVisibility(kvp.Key, false);
                                }

                                void DummyOperationInvertAll(NewDummyPolyManager dmyPolyMan)
                                {
                                    if (dmyPolyMan == null)
                                        return;
                                    foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                        dmyPolyMan.SetDummyPolyVisibility(kvp.Key, !dmyPolyMan.DummyPolyVisibleByRefID[kvp.Key]);
                                }

                                //ImGui.Button("Show All");
                                //if (ImGui.IsItemClicked())
                                //{
                                //    DummyOperationShowAll(model.DummyPolyMan);

                                //    //if (Scene.MainModel.ChrAsm != null)
                                //    //{
                                //    //    foreach (var slot in Scene.MainModel.ChrAsm.WeaponSlots)
                                //    //    {
                                //    //        DummyOperationShowAll(slot.WeaponModel0?.DummyPolyMan);
                                //    //        DummyOperationShowAll(slot.WeaponModel1?.DummyPolyMan);
                                //    //        DummyOperationShowAll(slot.WeaponModel2?.DummyPolyMan);
                                //    //        DummyOperationShowAll(slot.WeaponModel3?.DummyPolyMan);
                                //    //    }
                                //    //}
                                //}

                                //ImGui.Button("Hide All");
                                //if (ImGui.IsItemClicked())
                                //{
                                //    DummyOperationHideAll(model.DummyPolyMan);

                                //    //if (Scene.MainModel.ChrAsm != null)
                                //    //{
                                //    //    foreach (var slot in Scene.MainModel.ChrAsm.WeaponSlots)
                                //    //    {
                                //    //        DummyOperationHideAll(slot.WeaponModel0?.DummyPolyMan);
                                //    //        DummyOperationHideAll(slot.WeaponModel1?.DummyPolyMan);
                                //    //        DummyOperationHideAll(slot.WeaponModel2?.DummyPolyMan);
                                //    //        DummyOperationHideAll(slot.WeaponModel3?.DummyPolyMan);
                                //    //    }
                                //    //}
                                //}

                                //ImGui.Button("Invert All");
                                //if (ImGui.IsItemClicked())
                                //{
                                //    DummyOperationInvertAll(model.DummyPolyMan);

                                //    //if (Scene.MainModel.ChrAsm != null)
                                //    //{
                                //    //    foreach (var slot in Scene.MainModel.ChrAsm.WeaponSlots)
                                //    //    {
                                //    //        DummyOperationInvertAll(slot.WeaponModel0?.DummyPolyMan);
                                //    //        DummyOperationInvertAll(slot.WeaponModel1?.DummyPolyMan);
                                //    //        DummyOperationInvertAll(slot.WeaponModel2?.DummyPolyMan);
                                //    //        DummyOperationInvertAll(slot.WeaponModel3?.DummyPolyMan);
                                //    //    }
                                //    //}
                                //}

                                DoDummyPolyManager(model.DummyPolyMan, Loc.T("Body"));

                                //if (model.ChrAsm != null)
                                //{
                                //    foreach (var slot in Scene.MainModel.ChrAsm.WeaponSlots)
                                //    {
                                //        DoDummyPolyManager(slot.WeaponModel0?.DummyPolyMan, $"{slot.SlotDisplayName}[0]");
                                //        DoDummyPolyManager(slot.WeaponModel1?.DummyPolyMan, $"{slot.SlotDisplayName}[1]");
                                //        DoDummyPolyManager(slot.WeaponModel2?.DummyPolyMan, $"{slot.SlotDisplayName}[2]");
                                //        DoDummyPolyManager(slot.WeaponModel3?.DummyPolyMan, $"{slot.SlotDisplayName}[3]");
                                //    }

                                //}
                            }

                            if (thisFrameHover_DummyPoly)
                            {
                                if (model.SkeletonFlver != null)
                                    model.SkeletonFlver.DebugDrawTransformOfFlverBoneIndex = -2;
                                if (model.AnimContainer?.Skeleton != null)
                                    model.AnimContainer.Skeleton.DebugDrawTransformOfFlverBoneIndex = -2;
                            }

                            ImGui.TreePop();
                        }

                    }

                    void DoSkeletonOfModel(Model mdl)
                    {
                        bool thisFrameHover_Bone = false;
                        
                        bool thisFrameHover_Bone_Flver = false;
                        bool thisFrameHover_Bone_Hkx = false;
                        
                        if (mdl.SkeletonFlver != null)
                        {
                            if (ImGui.TreeNode(Loc.TF("[FLVER Skeleton]##{0}_FlverSkeleton", mdl.GUID)))
                            {
                                ImGui.PushID($"{mdl.GUID}_FlverSkeleton_InsideTree");


                                void DoBone(NewAnimSkeleton_FLVER skeleton, NewBone bone)
                                {
                                    int boneIndex = skeleton.Bones.IndexOf(bone);

                                    bool boneDrawEnabled = bone.EnablePrimDraw;

                                    if (OSD.RequestExpandAllTreeNodes)
                                        ImGui.SetNextItemOpen(true);

                                    bool thisBoneHighlighted = mdl.SkeletonFlver.DebugDrawTransformOfFlverBoneIndex ==
                                                               boneIndex;

                                    if (thisBoneHighlighted)
                                        ImGui.PushStyleColor(ImGuiCol.WindowBg,
                                            new System.Numerics.Vector4(0, 1, 1, 1));



                                    //bool boneNodeOpen = (ImGui.TreeNode(bone.Name));
                                    ImGui.BeginDisabled();
                                    ImGui.TreeNodeEx(Loc.TF("{0}##Skeleton{1}_Bone{2}_Index", bone.Index, skeleton.GUID, bone.Index), ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                                    ImGui.EndDisabled();
                                    ImGui.SameLine();
                                    ImGui.Selectable(bone.Name, ref boneDrawEnabled);

                                    bone.EnablePrimDraw = boneDrawEnabled;

                                    if (thisBoneHighlighted)
                                        ImGui.PopStyleColor();

                                    if (ImGui.IsItemHovered())
                                    {
                                        mdl.SkeletonFlver.DebugDrawTransformOfFlverBoneIndex = boneIndex;
                                        thisFrameHover_Bone = true;
                                        thisFrameHover_Bone_Flver = true;
                                    }

                                    foreach (var c in bone.ChildBones)
                                    {
                                        ImGui.Indent();
                                        DoBone(skeleton, c);
                                        ImGui.Unindent();
                                    }

                                    //if (boneNodeOpen) 
                                    //{
                                    //    foreach (var c in bone.ChildBones)
                                    //    {
                                    //        ImGui.Indent();
                                    //        DoBone(skeleton, c);
                                    //        ImGui.Unindent();
                                    //    }


                                    //    ImGui.TreePop();
                                    //}






                                }

                                ImGui.Button(Loc.T("Show All"));
                                if (ImGui.IsItemClicked())
                                    foreach (var b in mdl.SkeletonFlver.Bones)
                                        b.EnablePrimDraw = true;

                                ImGui.Button(Loc.T("Hide All"));
                                if (ImGui.IsItemClicked())
                                    foreach (var b in mdl.SkeletonFlver.Bones)
                                        b.EnablePrimDraw = false;

                                ImGui.Button(Loc.T("Invert All"));
                                if (ImGui.IsItemClicked())
                                    foreach (var b in mdl.SkeletonFlver.Bones)
                                        b.EnablePrimDraw = !b.EnablePrimDraw;

                                foreach (var rootIndex in mdl.SkeletonFlver.TopLevelBoneIndices)
                                    DoBone(mdl.SkeletonFlver, mdl.SkeletonFlver.Bones[rootIndex]);


                                if (!thisFrameHover_Bone)
                                    mdl.SkeletonFlver.DebugDrawTransformOfFlverBoneIndex = -1;



                                ImGui.PopID();
                                ImGui.TreePop();
                            }

                            if (ImGui.TreeNode(Loc.TF("[Bone Weight Debug]##{0}_FlverBoneWeightDebug", mdl.GUID)))
                            {
                                ImGui.PushID($"{mdl.GUID}_FlverBoneWeightDebug");
                                lock (mdl.SkeletonFlver._lock_DebugViewWeightOfBone)
                                {
                                    if (ImGui.TreeNode(Loc.TF("[Weight Shading Settings]##{0}_FlverBoneWeightDebug_WeightShadingSettings", mdl.GUID)))
                                    {
                                        ImGui.PushID($"{mdl.GUID}_FlverBoneWeightDebug_WeightShadingSettings");




                                        ImGui.DragFloat3(Loc.T("Unweighted Color (RGB)"), ref GFX.FlverShader_DebugViewWeightOfBone_BaseColor, 0.001f, 0, 2);
                                        ImGui.DragFloat3(Loc.T("Weighted Color (RGB)"), ref GFX.FlverShader_DebugViewWeightOfBone_WeightColor, 0.001f, 0, 2);
                                        ImGui.Separator();
                                        ImGui.Checkbox(Loc.T("Lighting Enabled"), ref GFX.FlverShader_DebugViewWeightOfBone_EnableLighting);
                                        ImGui.DragFloat(Loc.T("Lighting Mult"), ref GFX.FlverShader_DebugViewWeightOfBone_LightingMult, 0.001f, -1, 1);
                                        ImGui.DragFloat(Loc.T("Lighting Gain"), ref GFX.FlverShader_DebugViewWeightOfBone_LightingGain, 0.001f, -1, 1);
                                        ImGui.DragFloat(Loc.T("Lighting Power"), ref GFX.FlverShader_DebugViewWeightOfBone_LightingPower, 0.01f, 1, 16);
                                        ImGui.DragFloat(Loc.T("Albedo Mult"), ref GFX.FlverShader_DebugViewWeightOfBone_Lighting_AlbedoMult, 0.01f, 0, 1);
                                        ImGui.DragFloat(Loc.T("Reflectance Mult"), ref GFX.FlverShader_DebugViewWeightOfBone_Lighting_ReflectanceMult, 0.01f, 0, 4);
                                        ImGui.DragFloat(Loc.T("Gloss"), ref GFX.FlverShader_DebugViewWeightOfBone_Lighting_Gloss, 0.01f, 0, 1);
                                        ImGui.Separator();
                                        ImGui.Checkbox(Loc.T("Use Texture Alphas"), ref GFX.FlverShader_DebugViewWeightOfBone_EnableTextureAlphas);
                                        ImGui.Checkbox(Loc.T("Wireframe Overlay Enabled"), ref GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_Enabled);
                                        ImGui.Checkbox(Loc.T("Wireframe Overlay Obeys Tex Alphas"), ref GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_ObeysTextureAlphas);
                                        ImGui.PushItemWidth(OSD.DefaultItemWidth * 2);
                                        ImGui.DragFloat4(Loc.T("Wireframe Overlay Color Unweighted (RGB)"), ref GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_Color, 0.01f, 0, 1);
                                        ImGui.DragFloat4(Loc.T("Wireframe Overlay Color Weighted (RGB)"), ref GFX.FlverShader_DebugViewWeightOfBone_WireframeWeightColor, 0.01f, 0, 1);
                                        ImGui.PopItemWidth();
                                        ImGui.Checkbox(Loc.T("Hide Unweighted Geometry"), ref GFX.FlverShader_DebugViewWeightOfBone_ClipUnweightedGeometry);
                                        ImGui.Separator();
                                        if (Tools.SimpleClickButton(Loc.T("Reset to Defaults")))
                                        {
                                            GFX.FlverShader_DebugViewWeightOfBone_BaseColor = new Vector3(0, 0.25f, 0.5f);
                                            GFX.FlverShader_DebugViewWeightOfBone_WeightColor = new Vector3(1, 0, 0);
                                           

                                            GFX.FlverShader_DebugViewWeightOfBone_EnableLighting = true;
                                            GFX.FlverShader_DebugViewWeightOfBone_LightingMult = 1;
                                            GFX.FlverShader_DebugViewWeightOfBone_LightingGain = 0;
                                            GFX.FlverShader_DebugViewWeightOfBone_LightingPower = 4;

                                            GFX.FlverShader_DebugViewWeightOfBone_Lighting_AlbedoMult = 0.5f;
                                            GFX.FlverShader_DebugViewWeightOfBone_Lighting_ReflectanceMult = 2;
                                            GFX.FlverShader_DebugViewWeightOfBone_Lighting_Gloss = 0.15f;

                                            GFX.FlverShader_DebugViewWeightOfBone_EnableTextureAlphas = false;
                                            GFX.FlverShader_DebugViewWeightOfBone_ClipUnweightedGeometry = false;

                                            GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_Enabled = true;
                                            GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_ObeysTextureAlphas = false;
                                            GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_Color = new Vector4(0,0,0.25f,1);
                                            GFX.FlverShader_DebugViewWeightOfBone_WireframeWeightColor = new Vector4(2, 0, 0, 1);
                                        }

                                        

                                        
                                        ImGui.PopID();
                                        ImGui.TreePop();
                                    }

                                    ImGui.PushItemWidth(OSD.DefaultItemWidth * 3);
                                    var skel = mdl.SkeletonFlver;
                                    if (skel.DebugViewWeightOfBone_BoneNames == null)
                                    {
                                        skel.DebugViewWeightOfBone_GenerateBoneNames();
                                    }
                                    int curIndex = skel.DebugViewWeightOfBone_ImguiIndex + 1;
                                    int prevIndex = curIndex;
                                    ImGui.ListBox(Loc.T("Bone"), ref curIndex,
                                        skel.DebugViewWeightOfBone_BoneNames,
                                        skel.DebugViewWeightOfBone_BoneNames.Length);
                                    ImGui.PopItemWidth();

                                    if (curIndex != prevIndex)
                                    {
                                        skel.SetDebugWeightViewBoneIndex(curIndex - 1);
                                    }
                                }
                                ImGui.PopID();
                                ImGui.TreePop();
                            }
                        }

                        if (mdl.AnimContainer?.Skeleton != null)
                        {
                            if (ImGui.TreeNode(Loc.TF("[HKX Skeleton]##{0}_HkxSkeleton", mdl.GUID)))
                            {
                                ImGui.PushID($"{mdl.GUID}_HkxSkeleton_InsideTree");

                                void DoBone(NewAnimSkeleton_HKX skeleton, NewBone bone)
                                {
                                    int boneIndex = skeleton.Bones.IndexOf(bone);

                                    if (OSD.RequestExpandAllTreeNodes)
                                        ImGui.SetNextItemOpen(true);

                                    bool thisBoneHighlighted =
                                        mdl.AnimContainer.Skeleton.DebugDrawTransformOfFlverBoneIndex == boneIndex;

                                    if (thisBoneHighlighted)
                                        ImGui.PushStyleColor(ImGuiCol.WindowBg,
                                            new System.Numerics.Vector4(0, 1, 1, 1));



                                    //bool boneNodeOpen = (ImGui.TreeNode(bone.Name));
                                    bool boneDrawEnabled = false;
                                    ImGui.BeginDisabled();
                                    ImGui.TreeNodeEx(Loc.TF("{0}##Skeleton{1}_Bone{2}_Index", bone.Index, skeleton.GUID, bone.Index), ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                                    ImGui.EndDisabled();
                                    ImGui.SameLine();
                                    ImGui.Selectable(bone.Name, ref boneDrawEnabled);

                                    if (thisBoneHighlighted)
                                        ImGui.PopStyleColor();

                                    if (ImGui.IsItemHovered())
                                    {
                                        mdl.AnimContainer.Skeleton.DebugDrawTransformOfFlverBoneIndex = boneIndex;
                                        thisFrameHover_Bone = true;
                                        thisFrameHover_Bone_Hkx = true;
                                    }

                                    foreach (var c in bone.ChildIndices.Select(ci => skeleton.Bones[ci]))
                                    {
                                        ImGui.Indent();
                                        DoBone(skeleton, c);
                                        ImGui.Unindent();
                                    }
                                }

                                foreach (var rootIndex in mdl.AnimContainer.Skeleton.TopLevelBoneIndices)
                                    DoBone(mdl.AnimContainer.Skeleton, mdl.AnimContainer.Skeleton.Bones[rootIndex]);


                                if (!thisFrameHover_Bone)
                                    mdl.AnimContainer.Skeleton.DebugDrawTransformOfFlverBoneIndex = -1;



                                ImGui.PopID();
                                ImGui.TreePop();
                            }
                        }

                        if (ImGui.TreeNode(Loc.T("[Skeleton Debug]")))
                        {
                            if (mdl.SkeletonFlver != null)
                            {
                                ImGui.InputFloat(Loc.TF("Min###Mdl_{0}_HkxSkelToHkxAnimWeight_Min", mdl.GUID),
                                    ref debug_HkxSkelToHkxAnimWeight_min);
                                
                                ImGui.InputFloat(Loc.TF("Max###Mdl_{0}_HkxSkelToHkxAnimWeight_Max", mdl.GUID),
                                    ref debug_HkxSkelToHkxAnimWeight_max);
                                
                                float animWeight = mdl.AnimContainer.DebugAnimWeight;
                                ImGui.SliderFloat(Loc.TF("HKX Skel -> HKX Anim Weight###Mdl_{0}_HkxSkelToHkxAnimWeight", mdl.GUID), ref animWeight, debug_HkxSkelToHkxAnimWeight_min, debug_HkxSkelToHkxAnimWeight_max);
                                anyFieldFocused |= ImGui.IsItemActive();
                                if (animWeight != mdl.AnimContainer.DebugAnimWeight)
                                {
                                    mdl.AnimContainer.DebugAnimWeight = animWeight;
                                    mdl.RequestSyncUpdate();
                                }

                                float animWeight2 = mdl.DebugAnimWeight_Deprecated;
                                ImGui.SliderFloat(Loc.TF("FLVER Skel -> HKX Skel Weight###Mdl_{0}_FlverSkelToHkxSkelWeight", mdl.GUID), ref animWeight2, debug_HkxSkelToHkxAnimWeight_min, debug_HkxSkelToHkxAnimWeight_max);
                                anyFieldFocused |= ImGui.IsItemActive();
                                mdl.DebugAnimWeight_Deprecated = animWeight2;

                                bool bind = mdl.EnableSkinning;
                                ImGui.Checkbox(Loc.TF("Enable FLVER Skel -> HKX Skel###Mdl_{0}_EnableFlverSkelToHkxSkel", mdl.GUID), ref bind);
                                anyFieldFocused |= ImGui.IsItemActive();
                                mdl.EnableSkinning = bind;
                                
                                
                                ImGui.Separator();

                                ImGui.Checkbox(Loc.TF("Enable Bone Glue Joints###Mdl_{0}_EnableBoneGluers", mdl.GUID), ref mdl.EnableBoneGluers);
                                anyFieldFocused |= ImGui.IsItemActive();

                                ImGui.Checkbox(Loc.TF("Enable Skeleton Mappers###Mdl_{0}_EnableSkeletonMappers", mdl.GUID), ref mdl.EnableSkeletonMappers);
                                anyFieldFocused |= ImGui.IsItemActive();
                                
                                
                                

                                ImGui.Separator();

                                void doSkelDebugWeights(NewAnimSkeleton skel, string skelKey, out bool anyBoneHovered)
                                {
                                    anyBoneHovered = false;
                                    if (skel != null)
                                    {
                                        if (ImGui.TreeNode(
                                                Loc.TF("[{0} Bone Weights]###Mdl_{1}_BoneWeightNode_{2}", skelKey, mdl.GUID, skelKey)))
                                        {
                                            ImGui.PushItemWidth(256);
                                            float namesWidth = skel.ImGuiDebugWeightListNameWidth;
                                            float barWidth = skel.ImGuiDebugWeightListBarWidth;
                                            ImGui.SliderFloat(Loc.TF("Bone Name Width###Mdl_{0}_boneNameWidth_{1}", mdl.GUID, skelKey), ref namesWidth, 0, 1000);
                                            if (namesWidth < 0)
                                                namesWidth = 0;
                                            ImGui.SliderFloat(Loc.TF("Weight Bar Width###Mdl_{0}_weightBarWidth_{1}", mdl.GUID, skelKey), ref barWidth, 0, 1000);
                                            if (barWidth < 0)
                                                barWidth = 0;
                                            skel.ImGuiDebugWeightListNameWidth = namesWidth;
                                            skel.ImGuiDebugWeightListBarWidth = barWidth;

                                            ImGui.Separator();

                                            bool setAllWeightsButton = Tools.SimpleClickButton(Loc.TF("Click To Set All Weights To:##Mdl_{0}_SetAllWeightsTo_Button", mdl.GUID));
                                            //ImGui.SameLine();
                                            ImGui.SliderFloat(Loc.TF("##Mdl_{0}_SetAllWeightsTo_Slider", mdl.GUID), ref skel.Debug_SetAllWeightsTo_Slider, 0, 1);
                                            if (setAllWeightsButton)
                                            {
                                                for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                                {
                                                    skel.Bones[boneIndex].SetWeight = skel.Debug_SetAllWeightsTo_Slider;
                                                }
                                            }

                                            bool enableAllTogglesButton = Tools.SimpleClickButton(Loc.TF("Click To Enable All Bone Toggles##Mdl_{0}_EnableAllToggles_Button", mdl.GUID));
                                            if (enableAllTogglesButton)
                                            {
                                                for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                                {
                                                    skel.Bones[boneIndex].DisableWeight = false;
                                                    skel.Bones[boneIndex].DisableWeight_Rotation = false;
                                                    skel.Bones[boneIndex].DisableWeight_Scale = false;
                                                    skel.Bones[boneIndex].DisableWeight_Translation = false;
                                                }
                                            }

                                            bool disableAllTogglesButton = Tools.SimpleClickButton(Loc.TF("Click To Disable All Bone Toggles##Mdl_{0}_DisableAllToggles_Button", mdl.GUID));
                                            if (disableAllTogglesButton)
                                            {
                                                for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                                {
                                                    skel.Bones[boneIndex].DisableWeight = true;
                                                    skel.Bones[boneIndex].DisableWeight_Rotation = true;
                                                    skel.Bones[boneIndex].DisableWeight_Scale = true;
                                                    skel.Bones[boneIndex].DisableWeight_Translation = true;
                                                }
                                            }

                                            ImGui.Separator();


                                            bool setAllWeightsButton_Translation = Tools.SimpleClickButton(Loc.TF("Click To Set All Weights To (Translation):##Mdl_{0}_SetAllWeightsTo_Button__Translation", mdl.GUID));
                                            //ImGui.SameLine();
                                            ImGui.SliderFloat(Loc.TF("##Mdl_{0}_SetAllWeightsTo_Slider__Translation", mdl.GUID), ref skel.Debug_SetAllWeightsTo_Translation_Slider, 0, 1);
                                            if (setAllWeightsButton_Translation)
                                            {
                                                for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                                {
                                                    skel.Bones[boneIndex].SetWeight_Translation = skel.Debug_SetAllWeightsTo_Translation_Slider;
                                                }
                                            }

                                            ImGui.Separator();


                                            bool setAllWeightsButton_Rotation = Tools.SimpleClickButton(Loc.TF("Click To Set All Weights To (Rotation):##Mdl_{0}_SetAllWeightsTo_Button__Rotation", mdl.GUID));
                                            //ImGui.SameLine();
                                            ImGui.SliderFloat(Loc.TF("##Mdl_{0}_SetAllWeightsTo_Slider__Rotation", mdl.GUID), ref skel.Debug_SetAllWeightsTo_Rotation_Slider, 0, 1);
                                            if (setAllWeightsButton_Rotation)
                                            {
                                                for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                                {
                                                    skel.Bones[boneIndex].SetWeight_Rotation = skel.Debug_SetAllWeightsTo_Rotation_Slider;
                                                }
                                            }
                                            ImGui.Separator();

                                            bool setAllWeightsButton_Scale = Tools.SimpleClickButton(Loc.TF("Click To Set All Weights To (Scale):##Mdl_{0}_SetAllWeightsTo_Button__Scale", mdl.GUID));
                                            //ImGui.SameLine();
                                            ImGui.SliderFloat(Loc.TF("##Mdl_{0}_SetAllWeightsTo_Slider__Scale", mdl.GUID), ref skel.Debug_SetAllWeightsTo_Scale_Slider, 0, 1);
                                            if (setAllWeightsButton_Scale)
                                            {
                                                for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                                {
                                                    skel.Bones[boneIndex].SetWeight_Scale = skel.Debug_SetAllWeightsTo_Scale_Slider;
                                                }
                                            }

                                            ImGui.Separator();

                                            bool setDebugMultsButton = Tools.SimpleClickButton(Loc.TF("Click To Set All Debug Mults:##Mdl_{0}_SetDebugMults_Button", mdl.GUID));
                                            //ImGui.SameLine();
                                            ImGui.SliderFloat(Loc.TF("##Mdl_{0}_SetDebugMults_Slider", mdl.GUID), ref skel.Debug_SetDebugMults_Slider, 0, 1);
                                            if (setDebugMultsButton)
                                            {
                                                for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                                {
                                                    skel.Bones[boneIndex].DebugMult = skel.Debug_SetDebugMults_Slider;
                                                }
                                            }

                                            ImGui.Separator();

                                            bool setDebugMultsButton_Translation = Tools.SimpleClickButton(Loc.TF("Click To Set All Debug Mults (Translation):##Mdl_{0}_SetDebugMults_Button__Translation", mdl.GUID));
                                            //ImGui.SameLine();
                                            ImGui.SliderFloat(Loc.TF("##Mdl_{0}_SetDebugMults_Slider__Translation", mdl.GUID), ref skel.Debug_SetDebugMults_Translation_Slider, 0, 1);
                                            if (setDebugMultsButton_Translation)
                                            {
                                                for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                                {
                                                    skel.Bones[boneIndex].DebugMultTranslation = skel.Debug_SetDebugMults_Translation_Slider;
                                                }
                                            }

                                            ImGui.Separator();

                                            bool setDebugMultsButton_Rotation = Tools.SimpleClickButton(Loc.TF("Click To Set All Debug Mults (Rotation):##Mdl_{0}_SetDebugMults_Button__Rotation", mdl.GUID));
                                            //ImGui.SameLine();
                                            ImGui.SliderFloat(Loc.TF("##Mdl_{0}_SetDebugMults_Slider__Rotation", mdl.GUID), ref skel.Debug_SetDebugMults_Rotation_Slider, 0, 1);
                                            if (setDebugMultsButton_Rotation)
                                            {
                                                for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                                {
                                                    skel.Bones[boneIndex].DebugMultRotation = skel.Debug_SetDebugMults_Rotation_Slider;
                                                }
                                            }

                                            // 

                                            ImGui.Separator();

                                            bool setDebugMultsButton_Scale = Tools.SimpleClickButton(Loc.TF("Click To Set All Debug Mults (Scale):##Mdl_{0}_SetDebugMults_Button__Scale", mdl.GUID));
                                            //ImGui.SameLine();
                                            ImGui.SliderFloat(Loc.TF("##Mdl_{0}_SetDebugMults_Slider__Scale", mdl.GUID), ref skel.Debug_SetDebugMults_Scale_Slider, 0.05f, 1);
                                            if (setDebugMultsButton_Scale)
                                            {
                                                //if (skel.Debug_SetDebugMults_Scale_Slider < 0.05f)
                                                //    skel.Debug_SetDebugMults_Scale_Slider = 0.05f;
                                                for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                                {
                                                    skel.Bones[boneIndex].DebugMultScale = skel.Debug_SetDebugMults_Scale_Slider;
                                                }
                                            }


                                            ImGui.PopItemWidth();
                                            ImGui.Separator();
                                            
                                            
                                            for (int boneIndex = 0; boneIndex < skel.Bones.Count; boneIndex++)
                                            {
                                                var bone = skel.Bones[boneIndex];
                                                for (int d = 0; d < bone.NestDeepness; d++)
                                                {
                                                    ImGui.Text(Loc.T(" "));
                                                    ImGui.SameLine();
                                                }

                                                bool thisBoneHighlighted = skel.DebugDrawTransformOfFlverBoneIndex == boneIndex;
                                                if (thisBoneHighlighted)
                                                    ImGui.PushStyleColor(ImGuiCol.WindowBg,
                                                        new System.Numerics.Vector4(0, 1, 1, 1));
                                                
                                                bool weightEnabled = !bone.DisableWeight;
                                                //ImGui.PushItemWidth(namesWidth);

                                                ImGui.BeginDisabled();
                                                ImGui.TreeNodeEx(Loc.TF("{0}##Skeleton{1}_Bone{2}_Index", bone.Index, skelKey, bone.Index), ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                                                ImGui.EndDisabled();
                                                ImGui.SameLine();

                                                ImGui.Selectable(Loc.TF("{0}###Mdl_{1}_boneWeightCheckbox_{2}_{3}", bone.Name, mdl.GUID, skelKey, bone.Name), ref weightEnabled, ImGuiSelectableFlags.None, new Vector2(namesWidth, ImGui.GetTextLineHeight()));
                                                if (ImGui.IsItemHovered())
                                                {
                                                    skel.DebugDrawTransformOfFlverBoneIndex = boneIndex;
                                                    thisFrameHover_Bone = true;
                                                    anyBoneHovered = true;
                                                }
                                                anyFieldFocused |= ImGui.IsItemActive();
                                                //ImGui.PopItemWidth();
                                                if (bone.DisableWeight != !weightEnabled)
                                                    mdl.RequestSyncUpdate();
                                                bone.DisableWeight = !weightEnabled;
                                                
                                                


                                                if (thisBoneHighlighted)
                                                    ImGui.PopStyleColor();

                                                
                                                
                                                
                                                
                                                
                                                
                                                ImGui.SameLine();
                                                float boneWeight = bone.SetWeight;
                                                ImGui.PushItemWidth(barWidth);
                                                ImGui.SliderFloat(
                                                    Loc.TF("###Mdl_{0}_boneWeight_{1}_{2}", mdl.GUID, skelKey, bone.Name),
                                                    ref boneWeight, 0, 1, "%.2f");
                                                anyFieldFocused |= ImGui.IsItemActive();
                                                ImGui.PopItemWidth();
                                                if (bone.SetWeight != boneWeight)
                                                    mdl.RequestSyncUpdate();
                                                bone.SetWeight = boneWeight;
                                                
                                             
                                            }

                                            ImGui.TreePop();
                                        }
                                        
                                    }
                                }

                                doSkelDebugWeights(mdl.SkeletonFlver, "FLVER", out bool anyFlverBoneHovered);
                                doSkelDebugWeights(mdl.AnimContainer.Skeleton, "HKX", out bool anyHkxBoneHovered);
                                
                                if (anyFlverBoneHovered)
                                    thisFrameHover_Bone_Flver = true;
                                
                                if (anyHkxBoneHovered)
                                    thisFrameHover_Bone_Hkx = true;
                                

                            

                            }

                            ImGui.TreePop();
                        }

                        if (mdl.SkeletonFlver != null && !thisFrameHover_Bone_Flver)
                            mdl.SkeletonFlver.DebugDrawTransformOfFlverBoneIndex = -2;
                        if (mdl.AnimContainer?.Skeleton != null && !thisFrameHover_Bone_Hkx)
                            mdl.AnimContainer.Skeleton.DebugDrawTransformOfFlverBoneIndex = -2;
                            
                        
                        if (thisFrameHover_Bone)
                            mdl.DummyPolyMan.GlobalForceDummyPolyIDVisible = -2;
                    }

                    if (ImGui.TreeNode(Loc.T("Models")))
                    {
                        GFX.HideFLVERs = !MenuBar.CheckboxDual(Loc.T("Render Models"), !GFX.HideFLVERs);

                        ImGui.Separator();

                        bool clearMaterialSoloEnabled = DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.AnyMaterialSoloVisible();
                        if (!clearMaterialSoloEnabled)
                            ImGuiDebugDrawer.PushDisabled();
                        bool clearMaterialSoloClicked = Tools.SimpleClickButton(Loc.T("Clear current \"Is The ONLY One Visible\" option set.\n(In case you forgot which material you set it on)"));
                        if (!clearMaterialSoloEnabled)
                            ImGuiDebugDrawer.PopDisabled();
                        if (clearMaterialSoloClicked)
                        {
                            DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.ClearMaterialSoloVisible();
                        }

                        ImGui.Separator();

                        void DoModel(Model mdl, string modelKindName, int nestLevel, bool isMainModelOrDescendant)
                        {
                            if (mdl == null)
                                return;

                            var mdlGUID = mdl.GUID;
                            bool mdlTreeNode = false;

                            ImGui.Checkbox(Loc.TF("###Model[{0}]_IsVisibleByUserCheckboxForNode", mdlGUID), ref mdl.IsVisibleByUser);

                            

                            var mdlTextColor = mdl.IsVisibleByUser ? new System.Numerics.Vector4(0, 1, 0, 1) : new System.Numerics.Vector4(1, 0, 0, 1);

                            if (!mdl.EffectiveVisibility)
                            {
                                mdlTextColor *= new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1);
                            }

                            ImGui.PushStyleColor(ImGuiCol.Text, mdlTextColor);
                            try
                            {
                                ImGui.SameLine();

                                string nodeName = $"{mdl.Name}";

                                if (modelKindName != null)
                                    nodeName = $"{modelKindName}<{nodeName}>";

                                if (nestLevel == 0 && !isMainModelOrDescendant)
                                    nodeName = Loc.T("#INACTIVE# ") + nodeName;

                                mdlTreeNode = ImGui.TreeNode(Loc.TF("{0}###Model[{1}]_Tree", nodeName, mdlGUID));

                                if (ImGui.IsItemHovered())
                                {
                                    GFX.HighlightedThing = mdl;
                                }
                            }
                            finally
                            {
                                ImGui.PopStyleColor();
                            }
                            

                            if (mdlTreeNode)
                            {
                                if (mdl.MainMesh != null)
                                {

                                    
                                    
                                    if (mdl.IsHiddenByAbsorpPos)
                                    {
                                        ImGui.Text(Loc.T("Note: Model is hidden by WepAbsorpPosParam."));
                                    }

                                    if (mdl.IsHiddenByTae)
                                    {
                                        ImGui.Text(Loc.T("Note: Model is hidden by weapon hide TAE events in graph."));
                                    }

                                    ImGui.Checkbox(Loc.TF("Debug - Force Model Visible No Matter What###Model[{0}]_IsDebug_ForceShowNoMatterWhat", mdlGUID), ref mdl.Debug_ForceShowNoMatterWhat);

                                    ImGui.PushItemWidth(256);
                                    mdl.DebugOpacity =
                                        MenuBar.FloatSlider(Loc.TF("Opacity###Model[{0}]_DebugOpacity", mdlGUID), mdl.DebugOpacity * 100, 0, 100, "%.2f%%", 0, 100) / 100f;
                                    ImGui.PopItemWidth();
                                    ImGui.Separator();

                                    DoDebugOfModel(mdl);

                                    if (nestLevel == 0)
                                    {
                                        bool isMainModelOrDescendant_NewVal = isMainModelOrDescendant;
                                        ImGui.Checkbox(Loc.TF("IS ACTIVE ROOT MODEL###Model[{0}]_IsMainModel", mdlGUID), ref isMainModelOrDescendant_NewVal);
                                        //var isMainModel_NewVal = MenuBar.CheckboxDual($"Active:###Model{mdlIndex}_IsMainModel", isMainModel);
                                        if (isMainModelOrDescendant_NewVal && !isMainModelOrDescendant)
                                        {
                                            DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.SetMainModel(mdl, hideNonMainModels: true);
                                            //var curModels = Scene.Models.ToList();
                                            //foreach (var model in curModels)
                                            //{
                                            //    model.IsVisible = model == mdl;
                                            //}
                                            isMainModelOrDescendant = true;
                                        }
                                    }



                                    if (!isMainModelOrDescendant)
                                    {
                                        //ImGui.PushStyleColor(ImGuiCol.MenuBarBg, new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 0.5f));
                                        ImGuiDebugDrawer.PushDisabled(true);
                                        //ImGui.PushStyleVar(ImGuiStyleVar.DisabledAlpha)
                                    }

                                    try
                                    {





                                        //ImGui.Separator();




                                        //mdl.IsVisible = MenuBar.CheckboxDual($"IS VISIBLE###Model{mdlIndex}_IsVisible", mdl.IsVisible);

                                        //ImGui.Separator();

                                        var maskDict = mdl.GetMaterialNamesPerMask();


                                        foreach (var kvp in maskDict)
                                        {
                                            if (kvp.Key >= 0)
                                                mdl.DefaultDrawMask[kvp.Key] = MenuBar.CheckboxDual(Loc.TF("Mask {0}###Model[{1}]_Mask{2}", kvp.Key, mdlGUID, kvp.Key), mdl.DefaultDrawMask[kvp.Key]);
                                        }

                                        //ImGui.Separator();

                                        mdl.DummyPolyMan.GlobalForceDummyPolyIDVisible = -1;

                                        DoMainMeshOfModel(mdl);

                                        ImGui.Separator();

                                        DoSkeletonOfModel(mdl);

                                        ImGui.Separator();

                                        DoDummyPolyOfModel(mdl);

                                        ImGui.Separator();

                                    
                                        if (mdl.AnimContainer != null)
                                        {
                                            if (ImGui.TreeNode(Loc.T("[Animations(HKX)]")))
                                            {
                                                var animNameMap = mdl.AnimContainer.GetAnimationNameMap();
                                                string[] animNames = animNameMap.Values.ToArray();
                                                List<SplitAnimID> animIDs = animNameMap.Keys.ToList();
                                                int curItem = mdl.AnimContainer.CurrentAnimationID == null ? -1 : animIDs.IndexOf(mdl.AnimContainer.CurrentAnimationID);
                                                int prevSelItem = curItem;
                                                ImGui.ListBox(Loc.T("Animation(HKX)"), ref curItem, animNames, animNames.Length);
                                                if (curItem != prevSelItem)
                                                {
                                                    mdl.AnimContainer.RequestAnim(NewAnimSlot.SlotTypes.Base, curItem >= 0 ? animIDs[curItem] : SplitAnimID.Invalid, true, 1, 0, 0);
                                                }
                                                ImGui.TreePop();
                                            }
                                        }




                                       

                                    
                                        if (mdl.AnimContainer != null && mdl.TaeManager_ForParts != null)
                                        {
                                            if (ImGui.TreeNode(Loc.T("[Animations(TAE)]")))
                                            {
                                                if (mdl.TaeManager_ForParts.TaeCategory != null)
                                                {
                                                    var animListCopy = mdl.TaeManager_ForParts.TaeCategory.SAFE_GetAnimations();
                                                    string[] animNames = animListCopy
                                                        .Select(a => a.SplitID.ToString()).ToArray();
                                                    int curItem =
                                                        animListCopy.IndexOf(
                                                            mdl.TaeManager_ForParts.Anim);
                                                    int prevSelItem = curItem;
                                                    ImGui.ListBox(Loc.T("Animation(TAE)"), ref curItem, animNames,
                                                        animNames.Length);
                                                    if (curItem >= 0 && curItem != prevSelItem)
                                                    {
                                                        var newAnim =
                                                            animListCopy[curItem];
                                                        mdl.TaeManager_ForParts.SelectTaeAnimation(MainDoc, newAnim.SplitID, forceNew: true);
                                                    }
                                                }
                                                else
                                                {
                                                    ImGui.Text(Loc.T("TAE Animation Container Null"));
                                                }

                                                ImGui.TreePop();
                                            }
                                        }




                                       

                                        
                                        if (mdl.ChrAsm != null)
                                        {

                                            ImGui.Separator();

                                            mdl.ChrAsm?.ForAllArmorSlots(slot =>
                                            {
                                                slot.AccessAllModels(model =>
                                                {
                                                    DoModel(model, slot.SlotDisplayName, ++nestLevel, isMainModelOrDescendant);
                                                });

                                            });



                                            foreach (var slot in mdl.ChrAsm.WeaponSlots)
                                            {
                                                slot.AccessModel(0, model =>
                                                {
                                                    ImGui.Separator();
                                                    DoModel(model, $"{slot.SlotDisplayName}[0]", ++nestLevel, isMainModelOrDescendant);
                                                });

                                                slot.AccessModel(1, model =>
                                                {
                                                    ImGui.Separator();
                                                    DoModel(model, $"{slot.SlotDisplayName}[1]", ++nestLevel, isMainModelOrDescendant);
                                                });

                                                slot.AccessModel(2, model =>
                                                {
                                                    ImGui.Separator();
                                                    DoModel(model, $"{slot.SlotDisplayName}[2]", ++nestLevel, isMainModelOrDescendant);
                                                });

                                                slot.AccessModel(3, model =>
                                                {
                                                    ImGui.Separator();
                                                    DoModel(model, $"{slot.SlotDisplayName}[3]", ++nestLevel, isMainModelOrDescendant);
                                                });

                                            }

                                            


                                        }
                                        
                                        if (mdl.AC6NpcParts != null && zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.AC6)
                                        {
                                            mdl.AC6NpcParts?.AccessModelsOfAllParts(
                                                (partIndex, part, model) =>
                                                {
                                                    ImGui.Separator();
                                                    DoModel(model, $"NPC Part [{partIndex + 1}]", ++nestLevel, isMainModelOrDescendant);
                                                });
                                        }





                                    }
                                    catch (Exception handled_ex) when (Main.EnableErrorHandler.SceneWindowModelNode)
                                    {
                                        Main.HandleError(nameof(Main.EnableErrorHandler.SceneWindowModelNode), handled_ex);
                                    }
                                    finally
                                    {
                                        if (!isMainModelOrDescendant)
                                        {
                                            //ImGui.PopStyleColor();
                                            ImGuiDebugDrawer.PopDisabled();
                                        }
                                    }

                                }
                                else
                                {
                                    ImGui.Text(Loc.T("Model file is empty."));
                                }

                                ImGui.TreePop();
                            }


                        }

                        foreach (var mdl in DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.Models)
                        {
                            DoModel(mdl, null, 0, DSAnimStudio.zzz_DocumentManager.CurrentDocument.Scene.MainModel == mdl);
                        }
                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode(Loc.T("Helpers")))
                    {
                        lock (DBG._lock_DebugDrawEnablers)
                        {
                            bool helperFieldFocused = false;
                            Main.HelperDraw.ShowImGui(ref helperFieldFocused);
                            anyFieldFocused |= helperFieldFocused;

                        }

                        
                        ImGui.TreePop();
                    }
                }

                ImGui.Separator();

                bool anyPanelFieldFocused = false;
                zzz_DocumentManager.CurrentDocument.WorldViewManager.DrawImguiPanel(ref anyPanelFieldFocused);
                anyFieldFocused |= anyPanelFieldFocused;

                anyFieldFocusedRef |= anyFieldFocused;
            }
        }
    }
}
