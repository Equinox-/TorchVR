using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Torch.Managers.PatchManager;
using Torch.Managers.PatchManager.MSIL;
using Torch.Utils;
using VRage.OpenVRWrapper;
using VRageMath;

namespace PluginVR
{
    public static class CameraMatrixPatch
    {
#pragma warning disable 649
        [ReflectedMethodInfo(null, "SetupCameraMatricesInternal", TypeName = Types.TypeRender11)]
        private static MethodInfo _renderSetupCameraMatricesInternal;

        [ReflectedPropertyInfo(null, "Enable", TypeName = Types.TypeStereoRender)]
        private static PropertyInfo _stereoRenderEnable;
#pragma warning restore 649

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(_renderSetupCameraMatricesInternal).Transpilers
                .Add(Method(nameof(TranspileSetupCameraMatricesInternal)));
        }

        private static MethodInfo Method(string name)
        {
            return typeof(CameraMatrixPatch).GetMethod(name,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private static IEnumerable<MsilInstruction> TranspileSetupCameraMatricesInternal(
            IEnumerable<MsilInstruction> insn, MethodBody __methodBody)
        {
            var hitEyeOverride = false;
            using (var enumerator = insn.GetEnumerator())
                while (enumerator.MoveNext())
                {
                    var i = enumerator.Current;
                    Debug.Assert(i != null);
                    yield return i;

                    // The first stereo thing is broken
                    if (i.Operand is MsilOperandInline<MethodBase> target &&
                        target.Value == _stereoRenderEnable.GetMethod && !hitEyeOverride)
                    {
                        yield return new MsilInstruction(OpCodes.Pop);
                        yield return new MsilInstruction(OpCodes.Ldc_I4_0);
                        hitEyeOverride = true;
                    }
                }
        }
    }
}
