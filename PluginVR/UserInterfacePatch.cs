using System;
using System.Collections.Generic;
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
using VRageRender;

namespace PluginVR
{
    public class UserInterfacePatch
    {
#pragma warning disable 649
        [ReflectedMethodInfo(null, "DrawScene", TypeName = Types.TypeRender11)]
        private static MethodInfo _drawScene;

        [ReflectedMethodInfo(null, "Draw", TypeName = Types.TypeRender11)]
        private static MethodInfo _draw;

        [ReflectedFieldInfo(null, "m_screenshot", TypeName = Types.TypeRender11)]
        private static FieldInfo _renderScreenshot;

        [ReflectedPropertyInfo(typeof(MyOpenVR), nameof(MyOpenVR.Static))]
        private static PropertyInfo _openvrStatic;

        [ReflectedMethodInfo(typeof(MyOpenVR), nameof(MyOpenVR.DisplayEye))]
        private static MethodInfo _openvrDisplayEye;

        [ReflectedFieldInfo(null, "Main", TypeName = Types.TypeGBuffer)]
        private static FieldInfo _gbufferMain;

        [ReflectedMethodInfo(null, "Clear", TypeName = Types.TypeGBuffer)]
        private static MethodInfo _gbufferClear;

        [ReflectedPropertyInfo(typeof(Color), nameof(Color.Black))]
        private static PropertyInfo _colorBlack;

        [ReflectedPropertyInfo(null, "Backbuffer", TypeName = Types.TypeRender11)]
        private static PropertyInfo _backbuffer;

        [ReflectedPropertyInfo(null, "Resource", TypeName = Types.TypeBackbuffer)]
        private static PropertyInfo _backbufferResource;

        [ReflectedPropertyInfo(typeof(SharpDX.CppObject), nameof(SharpDX.CppObject.NativePointer))]
        private static PropertyInfo _resourcePointer;

        [ReflectedMethodInfo(null, "DrawSprites", TypeName = Types.TypeRender11)]
        private static MethodInfo _drawSprites;

        [ReflectedMethodInfo(null, "DrawSpritesOffscreen", TypeName = Types.TypeRender11)]
        private static MethodInfo _drawSpritesOffscreen;

        [ReflectedMethodInfo(null, "Run", TypeName = Types.TypeCopyToRt)]
        private static MethodInfo _copyToRtv;

        [ReflectedPropertyInfo(null, "Enable", TypeName = Types.TypeStereoRender)]
        private static PropertyInfo _stereoRenderEnable;

        [ReflectedPropertyInfo(null, "ViewportResolution", TypeName = Types.TypeRender11)]
        private static PropertyInfo _viewportResolution;

        [ReflectedFieldInfo(null, "RenderRegion", TypeName = Types.TypeStereoRender)]
        private static FieldInfo _stereoRenderRegion;

        [ReflectedFieldInfo(typeof(Vector2I), nameof(Vector2I.X))]
        private static FieldInfo _vector2IX;

        [ReflectedFieldInfo(typeof(Vector2I), nameof(Vector2I.Y))]
        private static FieldInfo _vector2IY;

        [ReflectedMethodInfo(null, "Release", TypeName = Types.TypeIBorrowedSrvTexture)]
        private static MethodInfo _releaseTexture;

        [ReflectedFieldInfo(null, "RwTexturesPool", TypeName = Types.TypeManagers)]
        private static FieldInfo _rwTexturesPool;

        [ReflectedMethodInfo(null, "BorrowRtv", TypeName = Types.TypeRwTextureManager, Parameters = new[] { typeof(string), typeof(int), typeof(int), typeof(SharpDX.DXGI.Format), typeof(int), typeof(int) })]
        private static MethodInfo _borrowRtv;
#pragma warning restore 649

        public static void Patch(PatchContext ctx)
        {
            ctx.GetPattern(_drawScene).Transpilers
                .Add(Method(nameof(TranspileDrawScene)));
            ctx.GetPattern(_draw).Transpilers
                .Add(Method(nameof(TranspileDraw)));
        }

        private static MethodInfo Method(string name)
        {
            return typeof(UserInterfacePatch).GetMethod(name,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private static readonly Logger _log = LogManager.GetCurrentClassLogger();
        private static IEnumerable<MsilInstruction> TranspileDrawScene(IEnumerable<MsilInstruction> stream)
        {
            var list = stream.ToList();
            var displayEyeBegin = -1;
            var displayEyeEnd = -1;
            for (var i = 0; i < list.Count; i++)
            {
                var ins = list[i];
                if (ins.Operand is MsilOperandInline<MethodBase> target && target.Value == _openvrStatic.GetMethod && i + 1 < list.Count)
                {
                    var branch = list[i + 1];
                    if (branch.Operand is MsilOperandBrTarget brTarget)
                    {
                        var foundDisplayEye = false;
                        var j = i + 1;
                        while (j < list.Count)
                        {
                            foundDisplayEye |= list[j].Operand is MsilOperandInline<MethodBase> targetEye &&
                                               targetEye.Value == _openvrDisplayEye;
                            if (list[j].Labels.Contains(brTarget.Target))
                                break;
                            j++;
                        }
                        if (foundDisplayEye)
                        {
                            displayEyeBegin = i;
                            displayEyeEnd = j;
                        }
                    }
                }
            }

            for (var i = 0; i < list.Count; i++)
                if (i < displayEyeBegin || i >= displayEyeEnd)
                    yield return list[i];
        }

        private static IEnumerable<MsilInstruction> TranspileDraw(IEnumerable<MsilInstruction> stream,
            Func<Type, MsilLocal> __localCreator)
        {
            foreach (var k in TranspileDrawF(stream, __localCreator))
            {
                //                _log.Info(k);
                yield return k;
            }
        }

        private static bool AllowUiHack = false;

        private static IEnumerable<MsilInstruction> TranspileDrawF(IEnumerable<MsilInstruction> stream, Func<Type, MsilLocal> __localCreator)
        {
            var replaced = false;
            foreach (var insn in stream)
            {
                if (AllowUiHack && insn.Operand is MsilOperandInline<MethodBase> mtarget && mtarget.Value == _drawSprites)
                {
                    var stereoMode = new MsilLabel();
                    var endOfSprites = new MsilLabel();
                    yield return new MsilInstruction(OpCodes.Call).InlineValue(_stereoRenderEnable.GetMethod);
                    yield return new MsilInstruction(OpCodes.Brtrue).InlineTarget(stereoMode);
                    // if (!Stereo.Enable)
                    {
                        yield return insn;
                        yield return new MsilInstruction(OpCodes.Br).InlineTarget(endOfSprites);
                    }
                    // else
                    {
                        yield return new MsilInstruction(OpCodes.Nop).LabelWith(stereoMode);
                        // Clear out arguments
                        if (!_drawSprites.IsStatic)
                            yield return new MsilInstruction(OpCodes.Pop);
                        foreach (var k in _drawSprites.GetParameters())
                            yield return new MsilInstruction(OpCodes.Pop);

                        // Create the RTV
                        var uiRtv = __localCreator.Invoke(_drawSpritesOffscreen.ReturnType);
                        yield return new MsilInstruction(OpCodes.Ldstr).InlineValue("VR.Overlay");
                        #region viewport.X / 2
                        yield return new MsilInstruction(OpCodes.Call).InlineValue(_viewportResolution.GetMethod);
                        yield return new MsilInstruction(OpCodes.Ldfld).InlineValue(_vector2IX);
                        yield return new MsilInstruction(OpCodes.Ldc_I4_2);
                        yield return new MsilInstruction(OpCodes.Div);
                        #endregion
                        #region viewport.Y
                        yield return new MsilInstruction(OpCodes.Call).InlineValue(_viewportResolution.GetMethod);
                        yield return new MsilInstruction(OpCodes.Ldfld).InlineValue(_vector2IY);
                        #endregion
                        yield return new MsilInstruction(OpCodes.Ldc_I4).InlineValue((int)SharpDX.DXGI.Format
                            .B8G8R8A8_UNorm);
                        #region (SharpDX.Color?)null
                        var tmpColorNullable = __localCreator.Invoke(typeof(SharpDX.Color?));
                        yield return tmpColorNullable.AsReferenceLoad();
                        yield return new MsilInstruction(OpCodes.Initobj).InlineValue(tmpColorNullable.Type);
                        yield return tmpColorNullable.AsValueLoad();
                        #endregion
                        yield return new MsilInstruction(OpCodes.Call).InlineValue(_drawSpritesOffscreen);
                        yield return uiRtv.AsValueStore();
                        var tmpViewportNullable = __localCreator.Invoke(typeof(MyViewport?));
                        yield return tmpViewportNullable.AsReferenceLoad();
                        yield return new MsilInstruction(OpCodes.Initobj).InlineValue(tmpViewportNullable.Type);

                        foreach (var region in new[]
                            {Types.StereoRenderRegion.Left, Types.StereoRenderRegion.Right})
                        {
                            yield return new MsilInstruction(OpCodes.Call).InlineValue(_backbuffer.GetMethod);
                            yield return uiRtv.AsValueLoad();
                            yield return new MsilInstruction(OpCodes.Ldc_I4_1);
                            yield return tmpViewportNullable.AsValueLoad();

                            // MyStereoRender.RenderRegion = region
                            yield return new MsilInstruction(OpCodes.Ldc_I4).InlineValue((int)region);
                            yield return new MsilInstruction(OpCodes.Stsfld).InlineValue(_stereoRenderRegion);

                            // MyCopyToRtv.Run(MyRender11.Backbuffer, tempBindable, true, null)
                            yield return new MsilInstruction(OpCodes.Call).InlineValue(_copyToRtv);
                        }

                        // MyStereoRender.RenderRegion = StereoRenderRegion.Fullscreen
                        yield return new MsilInstruction(OpCodes.Ldc_I4).InlineValue((int)Types.StereoRenderRegion
                            .Fullscreen);
                        yield return new MsilInstruction(OpCodes.Stsfld).InlineValue(_stereoRenderRegion);
                        yield return uiRtv.AsValueLoad();
                        yield return new MsilInstruction(OpCodes.Callvirt).InlineValue(_releaseTexture);
                    }
                    yield return new MsilInstruction(OpCodes.Nop).LabelWith(endOfSprites);
                }
                else if (!replaced && insn.Operand is MsilOperandInline<FieldInfo> target && target.Value == _renderScreenshot)
                {
                    var endOfDisplay = new MsilLabel();
                    yield return new MsilInstruction(OpCodes.Call).InlineValue(_openvrStatic.GetMethod);
                    yield return new MsilInstruction(OpCodes.Brfalse).InlineTarget(endOfDisplay);
                    // if (OpenVR.Static != null)
                    {
                        yield return new MsilInstruction(OpCodes.Ldsfld).InlineValue(_gbufferMain);
                        yield return new MsilInstruction(OpCodes.Call).InlineValue(_colorBlack.GetMethod);
                        yield return new MsilInstruction(OpCodes.Callvirt).InlineValue(_gbufferClear);
                        // GBuffer.Main.Clear(Color.Black);

                        yield return new MsilInstruction(OpCodes.Call).InlineValue(_openvrStatic.GetMethod);
                        yield return new MsilInstruction(OpCodes.Call).InlineValue(_backbuffer.GetMethod);
                        yield return new MsilInstruction(OpCodes.Callvirt).InlineValue(_backbufferResource.GetMethod);
                        yield return new MsilInstruction(OpCodes.Callvirt).InlineValue(_resourcePointer.GetMethod);
                        yield return new MsilInstruction(OpCodes.Callvirt).InlineValue(_openvrDisplayEye);
                        // OpenVR.Static.DisplayEye(Backbuffer.Resource.NativePointer);
                    }
                    replaced = true;
                    var copy = insn.CopyWith(insn.OpCode);
                    copy.Labels.Add(endOfDisplay);
                    yield return copy;
                }
                else
                    yield return insn;
            }
        }
    }
}
