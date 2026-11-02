using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DbgMenus
{
    public class DbgMenuItemResolutionChange : DbgMenuItem
    {
        private List<DisplayMode> supportedModes;
        private int modeIndex = 0;

        public DbgMenuItemResolutionChange()
        {
            supportedModes = Main.GetAllResolutions();
            modeIndex = supportedModes.IndexOf(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);
            UpdateText();
        }

        private void UpdateText()
        {
            Text = $"Display Mode: {supportedModes[modeIndex].Width}x{supportedModes[modeIndex].Height} " +
                $"({GetSurfaceFormatFriendlyName(supportedModes[modeIndex].Format)})";
        }

        public override void OnIncrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = modeIndex;
            modeIndex += incrementAmount;

            //如果到上界了
            if (modeIndex >= supportedModes.Count)
            {
                //如果已经到末尾了，还又点了一下按钮
                if (prevIndex == supportedModes.Count - 1 && !isRepeat)
                    modeIndex = 0; //Wrap Around
                else
                    modeIndex = supportedModes.Count - 1; //Stop
            }

            GFX.Display.SetFromDisplayMode(supportedModes[modeIndex]);

            UpdateText();
        }

        public override void OnDecrease(bool isRepeat, int incrementAmount)
        {
            int prevIndex = modeIndex;
            modeIndex -= incrementAmount;

            //如果到上界了
            if (modeIndex < 0)
            {
                //如果已经到末尾了，还又点了一下按钮
                if (prevIndex == 0 && !isRepeat)
                    modeIndex = supportedModes.Count - 1; //Wrap Around
                else
                    modeIndex = 0; //Stop
            }

            GFX.Display.SetFromDisplayMode(supportedModes[modeIndex]);

            UpdateText();
        }

        public override void OnResetDefault()
        {
            base.OnResetDefault();
        }

        private string GetSurfaceFormatFriendlyName(SurfaceFormat f)
        {
            switch(f)
            {
                case SurfaceFormat.Color:
                    return "32-Bit";
                default:
                    return $"<{nameof(SurfaceFormat)}.{f.ToString()}>";
            }
        }

        public override void OnRequestTextRefresh()
        {
            UpdateText();
        }
    }
}
