using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Torch.Managers.PatchManager;
using Torch.Managers.PatchManager.MSIL;
using Torch.Managers.PatchManager.Transpile;
using Torch.Utils;

namespace PluginVR
{
    public static class BasicRenderPatch
    {
#pragma warning disable 649
        [ReflectedMethodInfo(null, "Draw", TypeName = Types.TypeRenderContext, Parameters = new[] { typeof(int), typeof(int) })]
        private static MethodInfo _normalDrawGBufferPass;
        [ReflectedMethodInfo(null, "DrawGBufferPass", TypeName = Types.TypeStereoRender)]
        private static MethodInfo _stereoDrawGBufferPass;

        [ReflectedMethodInfo(null, "DrawIndexed", TypeName = Types.TypeRenderContext, Parameters = new[] { typeof(int), typeof(int), typeof(int) })]
        private static MethodInfo _normalDrawIndexedGBufferPass;
        [ReflectedMethodInfo(null, "DrawIndexedGBufferPass", TypeName = Types.TypeStereoRender)]
        private static MethodInfo _stereoDrawIndexedGBufferPass;

        [ReflectedMethodInfo(null, "DrawInstanced", TypeName = Types.TypeRenderContext, Parameters = new[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        private static MethodInfo _normalDrawInstancedGBufferPass;
        [ReflectedMethodInfo(null, "DrawInstancedGBufferPass", TypeName = Types.TypeStereoRender)]
        private static MethodInfo _stereoDrawInstancedGBufferPass;

        [ReflectedMethodInfo(null, "DrawIndexedInstanced", TypeName = Types.TypeRenderContext, Parameters = new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) })]
        private static MethodInfo _normalDrawIndexedInstancedGBufferPass;
        [ReflectedMethodInfo(null, "DrawIndexedInstancedGBufferPass", TypeName = Types.TypeStereoRender)]
        private static MethodInfo _stereoDrawIndexedInstancedGBufferPass;

        [ReflectedMethodInfo(null, "DrawIndexedInstancedIndirect", TypeName = Types.TypeRenderContext, ParameterNames = new[] { Types.TypeIBuffer, Types.TypeInt32 })]
        private static MethodInfo _normalDrawIndexedInstancedIndirectGBufferPass;
        [ReflectedMethodInfo(null, "DrawIndexedInstancedIndirectGBufferPass", TypeName = Types.TypeStereoRender)]
        private static MethodInfo _stereoDrawIndexedInstancedIndirectGBufferPass;

        private static readonly Dictionary<MethodBase, MethodBase> _stereoCallReplace = new Dictionary<MethodBase, MethodBase>();

        [ReflectedPropertyInfo(null, "Enable", TypeName = Types.TypeStereoRender)]
        private static PropertyInfo _stereoRenderEnable;

        [ReflectedMethodInfo(null, "RecordCommandsInternal", TypeName = Types.TypeStaticGlassPass, ParameterNames = new[] { Types.TypeRenderableProxy })]
        private static MethodInfo _glassRecordCommandsInternal;

        [ReflectedMethodInfo(null, "DrawHighlightedPart", TypeName = "VRage.Render11.GeometryStage2.SpecialPass.MyHighlightSpecialPass, " + Types.LibRenderDx11)]
        private static MethodInfo _highlightDrawPart;

        [ReflectedMethodInfo(null, "RecordCommands", TypeName = Types.TypeHighlightPass, ParameterNames = new[] { Types.TypeRenderableProxy, Types.TypeInt32, Types.TypeInt32 })]
        private static MethodInfo _highlightRecordCommands;

        [ReflectedMethodInfo(null, "DrawSubmesh", TypeName = Types.TypeHighlightPass)]
        private static MethodInfo _highlightDrawSubmesh;

        [ReflectedMethodInfo(null, "RecordCommandsInternal", TypeName = Types.TypeForwardPass, ParameterNames = new[] { Types.TypeRenderableProxy })]
        private static MethodInfo _forwardRecordCommandsInternal;

        private static MethodInfo _forwardRecordCommandsInternal2;

        [ReflectedMethodInfo(null, "RecordCommands", TypeName = "VRageRender.MyFoliageGeneratingPass, " + Types.LibRenderDx11, ParameterNames = new[]
        {
            Types.TypeRenderableProxy, "VRageRender.MyFoliageStream, " + Types.LibRenderDx11, Types.TypeInt32, "SharpDX.Direct3D11.VertexShader, SharpDX.Direct3D11",
            "SharpDX.Direct3D11.InputLayout, SharpDX.Direct3D11", Types.TypeInt32, Types.TypeInt32, Types.TypeInt32, Types.TypeInt32 })]
        private static MethodInfo _foliageGenRecordCommands;

        [ReflectedMethodInfo(null, "Render", TypeName = "VRageRender.MyCloudRenderer, " + Types.LibRenderDx11)]
        private static MethodInfo _cloudRender;

        [ReflectedMethodInfo(null, "DrawInstanceLodGroup", TypeName = Types.TypeStage2DepthRenderPass)]
        private static MethodInfo _depthDrawInstanceLodGroup;
        [ReflectedMethodInfo(null, "DrawInstanceLodGroup", TypeName = Types.TypeStage2GBufferRenderPass)]
        private static MethodInfo _gbufferDrawInstanceLodGroup;
        [ReflectedMethodInfo(null, "DrawInstanceLodGroup", TypeName = Types.TypeStage2GlassForDecalsRenderPass)]
        private static MethodInfo _glassDecalsDrawInstanceLodGroup;
        [ReflectedMethodInfo(null, "DrawInstanceLodGroup", TypeName = Types.TypeStage2GlassRenderPass)]
        private static MethodInfo _glassDrawInstanceLodGroup;

        [ReflectedMethodInfo(null, "RenderOne", TypeName = Types.TypeAtmosphereRenderer)]
        private static MethodInfo _atmosphereRenderOne;

        [ReflectedMethodInfo(null, "IssueQuery", TypeName = Types.TypeHardwareOcclusionQuery)]
        private static MethodInfo _hardwareOcclusionIssueQuery;
        [ReflectedMethodInfo(null, "Draw", TypeName = Types.TypeSpriteRenderer)]
        private static MethodInfo _spritesDraw;
        [ReflectedMethodInfo(null, "DrawFullscreenQuad", TypeName = Types.TypeScreenPass)]
        private static MethodInfo _screenDrawQuad;
#pragma warning restore 649

        public static void Patch(PatchContext ctx)
        {
            _stereoCallReplace.Clear();
            _stereoCallReplace.Add(_normalDrawGBufferPass, _stereoDrawGBufferPass);
            _stereoCallReplace.Add(_normalDrawIndexedGBufferPass, _stereoDrawIndexedGBufferPass);
            _stereoCallReplace.Add(_normalDrawInstancedGBufferPass, _stereoDrawInstancedGBufferPass);
            _stereoCallReplace.Add(_normalDrawIndexedInstancedGBufferPass, _stereoDrawIndexedInstancedGBufferPass);
            _stereoCallReplace.Add(_normalDrawIndexedInstancedIndirectGBufferPass, _stereoDrawIndexedInstancedIndirectGBufferPass);

            var fixer = Method(nameof(FixDrawCalls));
            ctx.GetPattern(_glassRecordCommandsInternal).Transpilers.Add(fixer);
            ctx.GetPattern(_highlightDrawPart).Transpilers.Add(fixer);
            ctx.GetPattern(_highlightRecordCommands).Transpilers.Add(fixer);
            ctx.GetPattern(_highlightDrawSubmesh).Transpilers.Add(fixer);
            ctx.GetPattern(_forwardRecordCommandsInternal).Transpilers.Add(fixer);
            _forwardRecordCommandsInternal2 = Type.GetType(Types.TypeForwardPass)
                .GetMethod("RecordCommandsInternal",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null,
                    new[] { Type.GetType(Types.TypeRenderableProxy2).MakeByRefType(), typeof(int), typeof(int) }, null);
            Debug.Assert(_forwardRecordCommandsInternal2 != null, $"Couldn't find RecordCommandsInternal(ref, int, int) in {Types.TypeForwardPass}");
            ctx.GetPattern(_forwardRecordCommandsInternal2).Transpilers.Add(fixer);
            ctx.GetPattern(_foliageGenRecordCommands).Transpilers.Add(fixer);
            ctx.GetPattern(_cloudRender).Transpilers.Add(fixer);
            //            ctx.GetPattern(_depthDrawInstanceLodGroup).Transpilers.Add(fixer);
            ctx.GetPattern(_gbufferDrawInstanceLodGroup).Transpilers.Add(fixer);
            ctx.GetPattern(_glassDecalsDrawInstanceLodGroup).Transpilers.Add(fixer);
            ctx.GetPattern(_glassDrawInstanceLodGroup).Transpilers.Add(fixer);

            ctx.GetPattern(_hardwareOcclusionIssueQuery).Transpilers.Add(fixer);
            ctx.GetPattern(_atmosphereRenderOne).Transpilers.Add(fixer);
            // TODO 
            //            ctx.GetPattern(_spritesDraw).Transpilers.Add(fixer);
            //            ctx.GetPattern(_screenDrawQuad).Transpilers.Add(fixer);
        }

        private static MethodInfo Method(string name)
        {
            return typeof(BasicRenderPatch).GetMethod(name,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private static IEnumerable<MsilInstruction> FixDrawCalls(IEnumerable<MsilInstruction> src)
        {
            EnsureStereoMethods();
            foreach (var call in src)
            {
                if ((call.OpCode == OpCodes.Callvirt || call.OpCode == OpCodes.Call) && call.Operand is MsilOperandInline<MethodBase> op
                    && _stereoCallReplace.TryGetValue(op.Value, out MethodBase replaceTarget))
                {
                    var end = new MsilLabel();
                    var noStereo = new MsilLabel();
                    yield return new MsilInstruction(OpCodes.Call).InlineValue(_stereoRenderEnable.GetMethod);
                    yield return new MsilInstruction(OpCodes.Brfalse).InlineTarget(noStereo);
                    yield return new MsilInstruction(OpCodes.Call).InlineValue(replaceTarget);
                    yield return new MsilInstruction(OpCodes.Br).InlineTarget(end);
                    call.Labels.Add(noStereo);
                    yield return call;
                    var t = new MsilInstruction(OpCodes.Nop);
                    t.Labels.Add(end);
                    yield return t;
                }
                else
                    yield return call;
            }
        }

        private static void EnsureStereoMethods()
        {

        }

        #region Emit Stereo
#pragma warning disable 649
        private const string TypeMyCommon = "VRageRender.MyCommon, " + Types.LibRenderDx11;
        private const string TypeIConstantBuffer = "VRage.Render11.Resources.IConstantBuffer, " + Types.LibRenderDx11;
        private const string TypeCommonStage = "VRage.Render11.RenderContext.MyCommonStage, " + Types.LibRenderDx11;

        [ReflectedPropertyInfo(null, "FrameConstantsStereoLeftEye", TypeName = TypeMyCommon)]
        private static PropertyInfo _commonFrameConstantsStereoLeftEye;

        [ReflectedPropertyInfo(null, "FrameConstants", TypeName = TypeMyCommon)]
        private static PropertyInfo _commonFrameConstants;

        [ReflectedPropertyInfo(null, "FrameConstantsStereoRightEye", TypeName = TypeMyCommon)]
        private static PropertyInfo _commonFrameConstantsStereoRightEye;

        [ReflectedMethodInfo(null, "SetConstantBuffer", TypeName = TypeCommonStage, ParameterNames = new[] { Types.TypeInt32, TypeIConstantBuffer })]
        private static MethodInfo _stageSetConstantBuffer;

        [ReflectedPropertyInfo(null, "AllShaderStages", TypeName = Types.TypeRenderContext)]
        private static PropertyInfo _rcAllShaderStages;

        [ReflectedMethodInfo(null, "SetViewport", TypeName = Types.TypeStereoRender, ParameterNames = new[] { Types.TypeRenderContext })]
        private static MethodInfo _stereoRenderSetViewport;

        private enum StereoRenderRegion
        {
            Fullscreen,
            Left,
            Right
        }
#pragma warning restore 649
        private static MethodInfo EmitStereoMethod(MethodInfo rcMethod)
        {
            var dyn = new DynamicMethod("Stereo" + rcMethod.Name, typeof(void), new[] { rcMethod.DeclaringType }.Concat(rcMethod.GetParameters().Select(x => x.ParameterType)).ToArray());
            var gen = new LoggingIlGenerator(dyn.GetILGenerator());
            EmitSetConstantBuffer(_commonFrameConstantsStereoLeftEye);
            EmitSetViewport(StereoRenderRegion.Left);
            for (var i = 0; i < dyn.GetParameters().Length; i++)
                new MsilArgument(i).AsValueLoad().Emit(gen);
            gen.Emit(OpCodes.Callvirt, rcMethod);

            EmitSetConstantBuffer(_commonFrameConstantsStereoRightEye);
            EmitSetViewport(StereoRenderRegion.Right);
            for (var i = 0; i < dyn.GetParameters().Length; i++)
                new MsilArgument(i).AsValueLoad().Emit(gen);
            gen.Emit(OpCodes.Callvirt, rcMethod);

            EmitSetConstantBuffer(_commonFrameConstants);
            EmitSetViewport(StereoRenderRegion.Fullscreen);

            return dyn;

            void EmitSetConstantBuffer(PropertyInfo frameConstants)
            {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Callvirt, _rcAllShaderStages.GetMethod);
                gen.Emit(OpCodes.Ldc_I4_0);
                gen.Emit(OpCodes.Call, frameConstants.GetMethod);
                gen.Emit(OpCodes.Callvirt, _stageSetConstantBuffer);
            }

            void EmitSetViewport(StereoRenderRegion region)
            {
                gen.Emit(OpCodes.Ldarg_0);
                switch (region)
                {
                    case StereoRenderRegion.Fullscreen:
                        gen.Emit(OpCodes.Ldc_I4_0);
                        break;
                    case StereoRenderRegion.Left:
                        gen.Emit(OpCodes.Ldc_I4_1);
                        break;
                    case StereoRenderRegion.Right:
                        gen.Emit(OpCodes.Ldc_I4_2);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(region), region, null);
                }
                gen.Emit(OpCodes.Call, _stereoRenderSetViewport);
            }
        }
        #endregion
    }
}
