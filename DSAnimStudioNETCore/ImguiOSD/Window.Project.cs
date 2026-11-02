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
        public class Project : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => Loc.Win("Project");

            public DSAProj Proj;

            public bool RequestShowTagsSection = false;
            private bool RequestShowTagsSection_ScrollNextFrame = false;
            private bool RequestShowTagsSection_OpenTreeNextFrame = false;

            protected override void Init()
            {
                
            }

            private string[] helpText = null;
            private object _lock_helpText = new object();

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                bool isRequestShowTagsSectionThisFrame = RequestShowTagsSection;
                RequestShowTagsSection = false;

                bool isRequestShowTagsSectionThisFrame_DoScroll = RequestShowTagsSection_ScrollNextFrame;
                RequestShowTagsSection_ScrollNextFrame = false;

                bool isRequestShowTagsSectionThisFrame_OpenTree = RequestShowTagsSection_OpenTreeNextFrame;
                RequestShowTagsSection_OpenTreeNextFrame = false;

                Proj = zzz_DocumentManager.CurrentDocument.EditorScreen.Proj;

                if (Proj == null)
                    return;

                ImGui.Text(Loc.TF("Game Type: {0}", Proj.GameType));
                ImGui.Text(Loc.TF(".DSAPROJ Format Version: {0}", Proj.FILE_VERSION));

                //testing
                //ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 500);
                //ImGui.Text("TEST");

                

                if (isRequestShowTagsSectionThisFrame)
                {
                    RequestShowTagsSection_OpenTreeNextFrame = true;
                    RequestShowTagsSection_ScrollNextFrame = false;
                    
                    
                }
                if (isRequestShowTagsSectionThisFrame_DoScroll)
                {
                    ImGui.SetScrollHereY();
                    RequestShowTagsSection_OpenTreeNextFrame = false;
                    RequestShowTagsSection_ScrollNextFrame = false;
                }
                if (isRequestShowTagsSectionThisFrame_OpenTree)
                {
                    ImGui.SetNextItemOpen(true);
                    RequestShowTagsSection_OpenTreeNextFrame = false;
                    RequestShowTagsSection_ScrollNextFrame = true;
                }

                if (ImGui.TreeNode(Loc.T("Tags##Proj_TagList_TreeNode")))
                {




                    lock (Proj._lock_Tags)
                    {
                        foreach (var t in Proj.Tags)
                        {
                            ImGui.ColorButton(Loc.TF("{0}##TagColorButton_{1}", t.Name, t.GUID),
                                (t.CustomColor ?? Main.Colors.ColorProjectTagDefault).ToNVector4());
                            anyFieldFocused |= ImGui.IsItemActive();
                            ImGui.SameLine();
                            var tagNodeOpen = ImGui.TreeNode(Loc.TF("{0}##Proj_TagList_Tag_{1}", t.Name, t.GUID));
                            //ImGui.SameLine();

                            if (tagNodeOpen)
                            {
                                ImGui.Button(Loc.T("Rename Tag..."));
                                anyFieldFocused |= ImGui.IsItemActive();
                                if (ImGui.IsItemClicked())
                                {
                                    ImguiOSD.DialogManager.AskForInputString(Loc.T("Change Tag Name"), Loc.T("Enter new name for tag.") +
                                        Loc.TF("\nCurrent tag name: '{0}'.", t.Name) +
                                        Loc.T("\n\nThis will change the name of this tag in all locations it is used."), Loc.T(""), result =>
                                    {
                                        lock (Proj._lock_Tags)
                                        {
                                            result = result.Trim();
                                            t.Name = result;
                                        }
                                    }, errCheck =>
                                    {
                                        string result = null;
                                        lock (Proj._lock_Tags)
                                        {
                                            errCheck = errCheck.Trim();
                                            if (Proj.Tags.Any(tag => tag.Name == errCheck && tag.GUID != t.GUID))
                                                result = Loc.TF("Another tag named '{0}' already exists.", errCheck);
                                        }
                                        return result;
                                    }, canBeCancelled: true);
                                }
                                ImguiOSD.Tools.CustomColorPicker(Loc.T("Custom Tag Color"), $"TagCustomColor_{t.GUID}", ref t.CustomColor, Main.Colors.ColorProjectTagDefault);
                                ImGui.Button(Loc.T("Delete Tag..."));
                                anyFieldFocused |= ImGui.IsItemActive();
                                if (ImGui.IsItemClicked())
                                {
                                    var tagToDelete = t;
                                    DialogManager.AskYesNo(Loc.T("Delete Tag"),
                                        Loc.T("Delete this tag from the project? All instances of it being used throughout the project will be lost." +
                                        "\n\nThis is not undoable."), choice =>
                                        {
                                            lock (Proj._lock_Tags)
                                            {
                                                Proj.Tags.Remove(tagToDelete);
                                            }
                                        },
                                        allowCancel: true, inputFlags: Dialog.InputFlag.EscapeKeyToCancel | Dialog.InputFlag.TitleBarXToCancel);
                                }

                                ImGui.TreePop();
                            }

                        }

                        ImGui.Separator();
                        ImGui.Button(Loc.T("Add New Tag..."));
                        anyFieldFocused |= ImGui.IsItemActive();
                        if (ImGui.IsItemClicked())
                        {
                            ImguiOSD.DialogManager.AskForInputString(Loc.T("Add New Tag"), Loc.T("Enter name for new tag."), Loc.T(""), result =>
                            {
                                lock (Proj._lock_Tags)
                                {
                                    result = result.Trim();
                                    var resultTag = Proj.Tags.FirstOrDefault(t => t.Name == result);
                                    if (resultTag == null)
                                    {
                                        resultTag = new DSAProj.Tag(result);
                                        Proj.Tags.Add(resultTag);
                                    }
                                }
                            }, errCheck =>
                            {
                                string result = null;
                                lock (Proj._lock_Tags)
                                {
                                    errCheck = errCheck.Trim();
                                    if (Proj.Tags.Any(tag => tag.Name == errCheck))                                    
                                        result = Loc.TF("Tag named '{0}' already exists.", errCheck);
                                }
                                return result;
                            }, canBeCancelled: true);
                        }
                        ImGui.TreePop();
                    }




                }

            }
        }
    }
}
