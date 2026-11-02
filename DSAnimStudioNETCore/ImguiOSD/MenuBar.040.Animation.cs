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
        public static void BuildMenuBar_040_Animation(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            if (ImGui.BeginMenu(Loc.T("Animation")))
            {
                isAnyMenuExpanded = true;

                Tae.Config.LockFramerateToOriginalAnimFramerate = Checkbox(
                    Loc.T("Lock Cursor to Frame Rate Defined in HKX"), Tae.Config.LockFramerateToOriginalAnimFramerate,
                    shortcut: Tae.PlaybackCursor != null
                    ? $"({((int)Math.Round(Tae.PlaybackCursor.CurrentSnapFPS))} FPS)" : null);

                float curPlaybackSpeed = TaeEditor.TaePlaybackCursor.GlobalBasePlaybackSpeed;
                
                curPlaybackSpeed = FloatSlider(Loc.T("Playback Speed"),
                    curPlaybackSpeed * 100f, 0, 100, "%.2f%%") / 100f;
                
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                if (curPlaybackSpeed > 999.99f)
                    curPlaybackSpeed = 999.99f;

                if (curPlaybackSpeed < 0)
                    curPlaybackSpeed = 0;

                TaeEditor.TaePlaybackCursor.GlobalBasePlaybackSpeed = curPlaybackSpeed;

                ImGui.Separator();

                Tae.Config.EnableAnimRootMotion = Checkbox(
                    Loc.T("Enable Root Motion"), Tae.Config.EnableAnimRootMotion);

                Tae.Config.RootMotionTranslationMultiplierXZ = FloatSlider(Loc.T("Root Motion Translation Mult XZ"),
                    Tae.Config.RootMotionTranslationMultiplierXZ, 0, 20, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.RootMotionTranslationPowerXZ = FloatSlider(Loc.T("Root Motion Translation Power XZ"),
                   Tae.Config.RootMotionTranslationPowerXZ, 0, 2, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                Tae.Config.RootMotionTranslationMultiplierY = FloatSlider(Loc.T("Root Motion Translation Mult Y"),
                    Tae.Config.RootMotionTranslationMultiplierY, 0, 1, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.RootMotionTranslationPowerY = FloatSlider(Loc.T("Root Motion Translation Power Y"),
                    Tae.Config.RootMotionTranslationPowerY, 0, 1, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                Tae.Config.RootMotionRotationMultiplier = FloatSlider(Loc.T("Root Motion Rotation Mult"),
                    Tae.Config.RootMotionRotationMultiplier, 0, 1, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;
                Tae.Config.RootMotionRotationPower = FloatSlider(Loc.T("Root Motion Rotation Power"),
                    Tae.Config.RootMotionRotationPower, 0, 1, "%.2f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                ImGui.Separator();

                if (ClickItem(Loc.TF("Follow Type: Root Motion"), shortcut: $"", shortcutColor: Color.Cyan, selected: Tae.Config.CameraFollowType == TaeEditor.TaeConfigFile.CameraFollowTypes.RootMotion))
                {
                    Tae.Config.CameraFollowType = TaeEditor.TaeConfigFile.CameraFollowTypes.RootMotion;
                }

                if (ClickItem(Loc.TF("Follow Type: Body DummyPoly"), shortcut: $"", shortcutColor: Color.Cyan, selected: Tae.Config.CameraFollowType == TaeEditor.TaeConfigFile.CameraFollowTypes.BodyDummyPoly))
                {
                    Tae.Config.CameraFollowType = TaeEditor.TaeConfigFile.CameraFollowTypes.BodyDummyPoly;
                }



                Tae.Config.CameraFollowDummyPolyID = IntItem(Loc.T("DummyPoly Follow ID"), Tae.Config.CameraFollowDummyPolyID);
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                ImGui.Separator();

                Tae.Config.CameraFollowsRootMotionZX = Checkbox(
                    Loc.T("Camera Follow - Translation ZX"), Tae.Config.CameraFollowsRootMotionZX, shortcut: "", shortcutColor: Color.White);

                //Tae.Config.CameraFollowsRootMotionZX_Interpolation = FloatSlider(
                //    "Camera Follow - Translation ZX - Interpolation", Tae.Config.CameraFollowsRootMotionZX_Interpolation, 
                //    0, 1, "%f", power: 2f);

                ImGui.Separator();

                Tae.Config.CameraFollowsRootMotionY = Checkbox(
                    Loc.T("Camera Follow - Translation Y"), Tae.Config.CameraFollowsRootMotionY, shortcut: "", shortcutColor: Color.White);

                //Tae.Config.CameraFollowsRootMotionY_Interpolation = FloatSlider(
                //    "Camera Follow - Translation Y - Interpolation", Tae.Config.CameraFollowsRootMotionY_Interpolation,
                //    0, 1, "%f", power: 2f);

                ImGui.Separator();

                Tae.Config.CameraFollowsRootMotionRotation = Checkbox(
                    Loc.T("Camera Follow - Rotation"), Tae.Config.CameraFollowsRootMotionRotation, shortcut: "", shortcutColor: Color.White);

                Tae.Config.CameraFollowsRootMotionRotation_Interpolation = FloatSlider(
                    Loc.T("Camera Follow - Rotation - Interpolation"), Tae.Config.CameraFollowsRootMotionRotation_Interpolation,
                    0, 1, "%f");
                if (ImGui.IsItemActive())
                    anyItemFocused = true;

                //ImGui.Separator();

                //Tae.Config.WrapRootMotion = Checkbox(
                //    "Prevent Root Motion From Reaching End Of Grid", Tae.Config.WrapRootMotion);

                ImGui.Separator();
                
                Tae.Config.EnableRumbleCamSmoothing = Checkbox(
                    Loc.T("Smooth RumbleCam End Transitions"), Tae.Config.EnableRumbleCamSmoothing);

                ImGui.Separator();

                Tae.Config.EnableBoneScale_NormalAnims = Checkbox(
                    Loc.T("Enable Bone Scale Changes (Normal Animations)"), Tae.Config.EnableBoneScale_NormalAnims);

                Tae.Config.EnableBoneScale_AdditiveAnims = Checkbox(
                    Loc.T("Enable Bone Scale Changes (Additive Animations)"), Tae.Config.EnableBoneScale_AdditiveAnims);

                Tae.Config.EnableBoneTranslation_NormalAnims = Checkbox(
                    Loc.T("Enable Bone Translation Changes (Normal Animations)"), Tae.Config.EnableBoneTranslation_NormalAnims);

                Tae.Config.EnableBoneTranslation_AdditiveAnims = Checkbox(
                    Loc.T("Enable Bone Translation Changes (Additive Animations)"), Tae.Config.EnableBoneTranslation_AdditiveAnims);

                ImGui.Separator();

                Tae.Config.ResetFloorOnAnimStart = Checkbox(
                    Loc.T("Reset Floor On Animation Start"), Tae.Config.ResetFloorOnAnimStart);

                Tae.Config.ResetHeadingOnAnimStart = Checkbox(
                    Loc.T("Reset Heading On Animation Start"), Tae.Config.ResetHeadingOnAnimStart);

                ImGui.EndMenu();
            }

        }
    }
}
