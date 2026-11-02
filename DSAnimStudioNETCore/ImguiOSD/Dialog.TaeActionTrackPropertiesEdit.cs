using ImGuiNET;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSAnimStudio.Localization;
namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        public class TaeActionTrackPropertiesEdit : Dialog
        {
            public DSAProj.Animation AnimRef;
            public int TrackIndex;
            public DSAProj.ActionTrack Track;
            public DSAProj.ActionTrack TrackCopy;

            public bool AnyUnsavedChanges()
            {
                if (TrackCopy.TrackType != Track.TrackType
                    || TrackCopy.TrackData != Track.TrackData
                    || !TrackCopy.Info.IsTheSameAs(Track.Info)
                    || Math.Abs(TrackCopy.NewActionDefaultLength - Track.NewActionDefaultLength) > 0.001f
                    || TrackCopy.NewActionDefaultType != Track.NewActionDefaultType)
                    return true;

                if (TrackCopy.NewActionDefaultParameters == null && Track.NewActionDefaultParameters != null)
                    return true;
                
                if (TrackCopy.NewActionDefaultParameters != null && Track.NewActionDefaultParameters == null)
                    return true;
                
                if (TrackCopy.NewActionDefaultParameters != null && Track.NewActionDefaultParameters != null &&
                    !TrackCopy.NewActionDefaultParameters.SequenceEqual(Track.NewActionDefaultParameters))
                    return true;
                
                return false;
            }

            public TaeActionTrackPropertiesEdit(DSAProj.Animation anim, int trackIndex, DSAProj.ActionTrack track)
                : base(Loc.T("Edit Action Track Properties"))
            {
                AnimRef = anim;
                TrackIndex = trackIndex;
                Track = track;
                TrackCopy = track.GetClone();
                CancelHandledByInheritor = true;
                AcceptHandledByInheritor = true;
                TitleBarXToCancel = true;
                EscapeKeyToCancel = true;
                AllowsResultCancel = true;
            }

            protected override void BuildInsideOfWindow()
            {
                ImGui.PushID($"ImGui_TaeActionTrackPropertiesEdit_{Track.GUID}");
                try
                {
                    ImGui.InputInt(Loc.T("Track Type"), ref TrackCopy.TrackType);
                    Tools.InputTextNullable("Name", ref TrackCopy.Info.DisplayName, 256);

                    if (ImGui.TreeNode(Loc.T("Track Data")))
                    {
                        Tools.EnumPicker(Loc.T("Track Data Type"), ref TrackCopy.TrackData.DataType);

                        if (TrackCopy.TrackData.DataType ==
                            TAE.ActionTrack.ActionTrackDataType.ApplyToSpecificCutsceneEntity)
                        {
                            Tools.EnumPicker(Loc.T("Entity Type"), ref TrackCopy.TrackData.CutsceneEntityType);
                            Tools.GhettoInputShort(Loc.T("Entity ID First Part"), ref TrackCopy.TrackData.CutsceneEntityIDPart1, minVal: -1, maxVal: 9999);
                            Tools.GhettoInputShort(Loc.T("Entity ID Second Part"), ref TrackCopy.TrackData.CutsceneEntityIDPart2, minVal: -1, maxVal: 9999);
                            ImGui.LabelText(TrackCopy.TrackData.GetEntityNameString(), Loc.T("Entity Name Preview"));
                            
                            Tools.GhettoInputSbyte(Loc.T("Entity Area ID"), ref TrackCopy.TrackData.Area, minVal: -1, maxVal: 99);
                            Tools.GhettoInputSbyte(Loc.T("Entity Block ID"), ref TrackCopy.TrackData.Block, minVal: -1, maxVal: 99);
                            ImGui.LabelText(TrackCopy.TrackData.GetAreaNameString(), Loc.T("Area Name Preview"));
                        }
                        
                        
                        ImGui.TreePop();
                    }
                    
                    
                    
                    
                    ImGui.Separator();
                    bool clickedCancel = Tools.SimpleClickButton(Loc.T("Cancel")) || IsTitleBarXRequested;
                    bool pressedEscape = IsEscapeKeyRequested;
    
                    if (clickedCancel || pressedEscape)
                    {
                        bool unsavedChanges = AnyUnsavedChanges();
                        bool forceClose = Main.Input.ShiftHeld;
                        if (!unsavedChanges || forceClose)
                        {
                            ResultType = ResultTypes.Cancel;
                            Dismiss();
                        }
                        else
                        {
                            DialogManager.AskYesNo(Loc.T("Unsaved Changes"), Loc.T("You have unsaved changes, would you like to discard them and close this dialog?"), choice =>
                            {
                                if (choice == true)
                                {
                                    ResultType = ResultTypes.Cancel;
                                    Dismiss();
                                }
                            });
                        }
                    }
    
                    bool invalidState = false;//(ImportFromAnimID_Value == null || TaeAnimID_Value == null);
    
                    if (invalidState)
                        Tools.PushGrayedOut();
                    ImGui.Button(Loc.T("Save & Accept"));
                    if (invalidState)
                    {
                        Tools.PopGrayedOut();
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip(Loc.T("Cannot accept changes until the animation ID formatting errors (shown in red) are fixed."));
                        }
                    }
                    if (ImGui.IsItemClicked())
                    {
                        if (!invalidState)
                        {
                            ResultType = ResultTypes.Accept;
                            Dismiss();
                        }
                       
                    }
                    
                    
                    
                }
                finally
                {
                    ImGui.PopID();
                }
                
                
               

                //throw new NotImplementedException();
            }
        }
    }
}
