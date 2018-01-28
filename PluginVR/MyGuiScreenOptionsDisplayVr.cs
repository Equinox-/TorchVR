using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Sandbox.Engine.Platform.VideoMode;
using Sandbox.Game.Gui;
using Sandbox.Graphics.GUI;
using Torch.Managers.PatchManager;
using Torch.Managers.PatchManager.MSIL;
using Torch.Utils;
using VRage;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using VRageRender;

// ME Edition:
using MyGuiScreenOptionsDisplay=Medieval.GUI.MainMenu.Options.MyDisplayOptionsScreen;

namespace PluginVR
{
    public class MyGuiScreenOptionsDisplayVr : MyGuiScreenOptionsDisplay
    {
        private MyGuiControlCheckbox m_checkboxEnableVR;

        private bool _allowControls = false;
        public MyGuiScreenOptionsDisplayVr()
        {
            Size = new Vector2(850f, 650f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            _allowControls = true;
            RecreateControls(true);
        }

        public override void RecreateControls(bool constructor)
        {
            if (!_allowControls)
                return;
            this.m_checkboxEnableVR = new MyGuiControlCheckbox(null, null, "Enable VR mode");
            base.RecreateControls(constructor);
            if (!constructor)
                return;

            Debug.Assert(m_size.HasValue);

            MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
            Vector2 value = new Vector2(90f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            Vector2 value2 = new Vector2(54f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
            float num = 455f / MyGuiConstants.GUI_OPTIMAL_SIZE.X;
            float num2 = 25f;
            float y = MyGuiConstants.SCREEN_CAPTION_DELTA_Y * 0.5f;
            float num3 = 0.0015f;
            Vector2 value3 = new Vector2(0f, 0.045f);
            float num4 = 0f;
            Vector2 value4 = (this.m_size.Value / 2f - value) * new Vector2(-1f, -1f) + new Vector2(0f, y);
            Vector2 value5 = (this.m_size.Value / 2f - value) * new Vector2(1f, -1f) + new Vector2(0f, y);
            Vector2 value6 = (this.m_size.Value / 2f - value2) * new Vector2(0f, 1f);
            Vector2 value7 = new Vector2(value5.X - (num + num3), value5.Y);
            num4 -= 0.045f;
            num4 += 3;
            num4 += .45f;
            num4 += .66f;
            num4 += 2.0f;
            MyGuiControlLabel lblEnableVr = new MyGuiControlLabel(null, null, "Enable VR", null)
            {
                Position = value4 + num4 * value3,
                OriginAlign = originAlign
            };
            m_checkboxEnableVR.Position = value7 + num4 * value3;
            m_checkboxEnableVR.OriginAlign = originAlign;
            Controls.Add(lblEnableVr);
            Controls.Add(m_checkboxEnableVR);
        }


#pragma warning disable 649
        [ReflectedMethodInfo(typeof(MyGuiScreenOptionsDisplay), "ReadSettingsFromControls")]
        private static MethodInfo _methodReadSettingsFromControls;

        [ReflectedMethodInfo(typeof(MyGuiScreenOptionsDisplay), "WriteSettingsToControls")]
        private static MethodInfo _methodWriteSettingsToControls;
#pragma warning restore 649

        internal static void Patch(PatchContext context)
        {
            context.GetPattern(_methodReadSettingsFromControls).Suffixes
                            .Add(typeof(MyGuiScreenOptionsDisplayVr).GetMethod(nameof(SuffixReadSettingsFromControls),
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
            context.GetPattern(_methodWriteSettingsToControls).Suffixes
                .Add(typeof(MyGuiScreenOptionsDisplayVr).GetMethod(nameof(SuffixWriteSettingsToControls),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
        }

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static void SuffixReadSettingsFromControls(MyGuiScreenOptionsDisplay __instance,
            ref bool __result,
            ref MyRenderDeviceSettings deviceSettings)
        {
            if (__instance is MyGuiScreenOptionsDisplayVr gui && gui.m_checkboxEnableVR != null)
            {
                __result |= gui.m_checkboxEnableVR.IsChecked != deviceSettings.UseStereoRendering;
                deviceSettings.UseStereoRendering = true;//gui.m_checkboxEnableVR.IsChecked;
            }
        }

        private static void SuffixWriteSettingsToControls(MyGuiScreenOptionsDisplay __instance, MyRenderDeviceSettings deviceSettings)
        {
            if (__instance is MyGuiScreenOptionsDisplayVr gui && gui.m_checkboxEnableVR != null)
            {
                gui.m_checkboxEnableVR.IsChecked = true;//deviceSettings.UseStereoRendering;
            }
        }
    }
}
