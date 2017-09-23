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

namespace PluginVR
{
    public class AmbientOcclusionPatch
    {
#pragma warning disable 649
        [ReflectedMethodInfo(null, "DrawGameScene", TypeName = Types.TypeRender11)]
        private static MethodInfo _drawGameScene;

        [ReflectedMethodInfo(null, "IC_EndBlockAlways", TypeName = Types.TypeGpuProfiler)]
        private static MethodInfo _gpuProfilerEndBlock;

        [ReflectedFieldInfo(null, "RenderRegion", TypeName = Types.TypeStereoRender)]
        private static FieldInfo _stereoRenderRegion;
#pragma warning restore 649

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(_drawGameScene).Transpilers.Add(Method(nameof(MoveAmbientOcclusionOutOfLoop)));
        }

        private static MethodInfo Method(string name)
        {
            return typeof(AmbientOcclusionPatch).GetMethod(name,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private static IEnumerable<MsilInstruction> MoveAmbientOcclusionOutOfLoop(
            IEnumerable<MsilInstruction> instructionStream)
        {
            var labelPosition = new Dictionary<MsilLabel, int>();
            var body = instructionStream.ToList();
            var positionBeginAo = -1;
            var positionEndAo = -1;
            var positionAsFullscreen = -1;
            for (var i = 0; i < body.Count; i++)
            {
                var ins = body[i];
                foreach (var k in ins.Labels)
                    labelPosition.Add(k, i);
                if (ins.OpCode == OpCodes.Stsfld && ins.Operand is MsilOperandInline<FieldInfo> fieldInfo && fieldInfo.Value == _stereoRenderRegion &&
                    i > 0 && body[i - 1].IsConstIntLoad() && body[i - 1].GetConstInt() == (int)Types.StereoRenderRegion.Fullscreen)
                    positionAsFullscreen = i;
                if (ins.OpCode == OpCodes.Ldstr && ins.Operand is MsilOperandInline<string> inStr &&
                    inStr.Value.Equals("SSAO"))
                {
                    // Advance back up until we find the end of the previous block.
                    var j = i;
                    while (j-- > 0)
                    {
                        if (body[j].Operand is MsilOperandInline<MethodBase> operand &&
                            operand.Value == _gpuProfilerEndBlock)
                        {
                            positionBeginAo = j + 1;
                            break;
                        }
                    }
                    j = i;
                    MsilLabel endLabel = null;
                    while (++j < body.Count)
                    {
                        if (body[j].OpCode == OpCodes.Br || body[j].OpCode == OpCodes.Br_S)
                        {
                            endLabel = ((MsilOperandBrTarget)body[j].Operand).Target;
                            break;
                        }
                    }
                    while (++j < body.Count)
                        if (body[j].Labels.Contains(endLabel))
                            positionEndAo = j;
                }
            }
            if (positionEndAo == -1 || positionBeginAo == -1 || positionAsFullscreen == -1)
            {
                foreach (var k in body)
                    yield return k;
                yield break;
            }

            for (var i = 0; i < body.Count; i++)
            {
                if (i < positionBeginAo)
                    yield return body[i];
                else if (i >= positionEndAo)
                    yield return body[i];
                if (i == positionAsFullscreen)
                {
                    var endOfAo = new MsilLabel();
                    for (var j = positionBeginAo; j < positionEndAo; j++)
                    {
                        if (body[j].Operand is MsilOperandBrTarget target)
                        {
                            var tiv = labelPosition[target.Target];
                            if (tiv == positionEndAo)
                            {
                                yield return body[j].CopyWith(body[j].OpCode).InlineTarget(endOfAo);
                                continue;
                            }
                            else if (tiv > positionEndAo || tiv < positionBeginAo)
                                _log.Warn($"Unable to figure out how to deal with label {target.Target} at {tiv}.  AO data is from {positionBeginAo}-{positionEndAo}");
                        }
                        yield return body[j];
                    }
                    var temp = new MsilInstruction(OpCodes.Nop);
                    temp.Labels.Add(endOfAo);
                    yield return temp;
                }
            }
        }
    }
}