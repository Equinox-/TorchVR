using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Sandbox.Engine.Platform.VideoMode;
using Sandbox.Graphics.GUI;
using Torch;
using Torch.API;
using Torch.API.Session;
using Torch.Client;
using Torch.Managers;
using Torch.Managers.PatchManager;
using Torch.Managers.PatchManager.MSIL;
using Torch.Utils;
using VRage;
using VRage.OpenVRWrapper;
using VRageMath;
using VRageRender;

namespace PluginVR
{
    public class VRManager : Manager
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

#pragma warning disable 649
        [Dependency]
        private PatchManager _patcher;
#pragma warning restore 649

        private PatchContext _patchContext;

        public VRManager(ITorchBase torch) : base(torch)
        {
        }

#pragma warning disable 649
        [ReflectedMethodInfo(null, "RecreateControls", TypeName = "SpaceEngineers.Game.GUI.MyGuiScreenOptionsSpace, SpaceEngineers.Game", Parameters = new[] { typeof(bool) })]
        private static MethodInfo _createOptionsControls;

        [ReflectedMethodInfo(null, "ApplySettings", TypeName = Types.TypeRender11)]
        private static MethodInfo _renderApplySettings;

        [ReflectedStaticMethod(Name = "InitUsingOpenVR", TypeName = "VRageRender.MyStereoStencilMask, " + Types.LibRenderDx11)]
        private static Action _stereoStencilMaskInitUsingOVR;

        [ReflectedSetter(Name = "ButtonClicked")]
        private static Action<MyGuiControlButton, Action<MyGuiControlButton>> _onClickBackingField;
#pragma warning restore 649

        public override void Attach()
        {
            _log.Info("Injecting VR manager");

            _patchContext = _patcher.AcquireContext();

            _patchContext.GetPattern(_createOptionsControls).Suffixes.Add(MyMeth(nameof(FixOptionsControls)));
            _patchContext.GetPattern(_renderApplySettings).Prefixes.Add(MyMeth(nameof(InitStereoMode)));
            CameraMatrixPatch.Patch(_patchContext);
            BasicRenderPatch.Patch(_patchContext);
            AmbientOcclusionPatch.Patch(_patchContext);
            MyGuiScreenOptionsDisplayVr.Patch(_patchContext);
            UserInterfacePatch.Patch(_patchContext);
        }

        public override void Detach()
        {
            if (_patchContext != null)
                _patcher.FreeContext(_patchContext);
            _patcher.Commit();
        }


        private static MethodInfo MyMeth(string name)
        {
            return typeof(VRManager).GetMethod(name,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private static void InitStereoMode(MyRenderDeviceSettings settings)
        {
            if (settings.UseStereoRendering)
            {
                if (MyOpenVR.Static == null)
                    new MyOpenVR();
                _stereoStencilMaskInitUsingOVR.Invoke();
            }
        }

        private static void FixOptionsControls(MyGuiScreenBase __instance)
        {
            foreach (var control in __instance.Controls)
                if (control is MyGuiControlButton btn && btn.Text != null &&
                    btn.Text.Equals(MyTexts.Get(MyCommonTexts.ScreenOptionsButtonDisplay).ToString()))
                    _onClickBackingField.Invoke(btn, (x) =>
                    {
                        MyGuiSandbox.AddScreen(new MyGuiScreenOptionsDisplayVr());
                    });
        }
    }
}
