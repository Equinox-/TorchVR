using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginVR
{
    public static class Types
    {
        public const string LibRenderDx11 = "VRage.Render11";
        public const string TypeRender11 = "VRageRender.MyRender11, " + LibRenderDx11;
        public const string TypeEnvironment = "VRageRender.MyEnvironment, " + LibRenderDx11;
        public const string TypeEnvironmentMatrices = "VRageRender.MyEnvironmentMatrices, " + LibRenderDx11;
        public const string TypeStereoRender = "VRageRender.MyStereoRender, " + Types.LibRenderDx11;
        public const string TypeRenderContext = "VRage.Render11.RenderContext.MyRenderContext, " + Types.LibRenderDx11;
        public const string TypeIBuffer = "VRage.Render11.Resources.IBuffer, " + Types.LibRenderDx11;
        public const string TypeStaticGlassPass = "VRageRender.MyStaticGlassPass, " + Types.LibRenderDx11;
        public const string TypeRenderableProxy = "VRageRender.MyRenderableProxy, " + Types.LibRenderDx11;
        public const string TypeIVertexBuffer = "VRage.Render11.Resources.IVertexBuffer, " + Types.LibRenderDx11;
        public const string TypeRenderableProxy2 = "VRageRender.MyRenderableProxy_2, " + Types.LibRenderDx11;
        public const string TypeHighlightPass = "VRageRender.MyHighlightPass, " + Types.LibRenderDx11;
        public const string TypeForwardPass = "VRageRender.MyForwardPass, " + Types.LibRenderDx11;
        public const string TypeHardwareOcclusionQuery = "VRageRender.MyHardwareOcclusionQuery, " + Types.LibRenderDx11;
        public const string TypeAtmosphereRenderer = "VRageRender.MyAtmosphereRenderer, " + Types.LibRenderDx11;
        public const string TypeSpriteRenderer = "VRageRender.MySpritesRenderer, " + Types.LibRenderDx11;
        public const string TypeScreenPass = "VRageRender.MyScreenPass, " + Types.LibRenderDx11;
        public const string TypeGBuffer = "VRage.Render11.Resources.MyGBuffer, " + Types.LibRenderDx11;
        public const string TypeBackbuffer = "VRage.Render11.Resources.MyBackbuffer, " + Types.LibRenderDx11;
        public const string TypeCopyToRt = "VRageRender.MyCopyToRT, " + Types.LibRenderDx11;
        public const string TypeManagers = "VRage.Render11.Common.MyManagers, " + Types.LibRenderDx11;
        public const string TypeRwTextureManager = "VRage.Render11.Resources.MyBorrowedRwTextureManager, " + Types.LibRenderDx11;

        public const string TypeIBorrowedSrvTexture =
            "VRage.Render11.Resources.IBorrowedSrvTexture, " + Types.LibRenderDx11;

        public const string TypeStage2DepthRenderPass =
            "VRage.Render11.GeometryStage2.RenderPass.MyDepthRenderPass, " + Types.LibRenderDx11;

        public const string TypeStage2GBufferRenderPass =
            "VRage.Render11.GeometryStage2.RenderPass.MyGBufferRenderPass, " + Types.LibRenderDx11;

        public const string TypeStage2GlassForDecalsRenderPass =
            "VRage.Render11.GeometryStage2.RenderPass.MyGlassForDecalsRenderPass, " + Types.LibRenderDx11;

        public const string TypeStage2GlassRenderPass =
            "VRage.Render11.GeometryStage2.RenderPass.MyGlassRenderPass, " + Types.LibRenderDx11;

        public const string TypeGpuProfiler = "VRage.Render11.Profiler.MyGpuProfiler, " + Types.LibRenderDx11;
        public const string TypeFrustumCullQuery = "VRageRender.MyFrustumCullQuery, " + Types.LibRenderDx11;
        public const string TypeFrustumCullingWork = "VRageRender.MyFrustumCullingWork, " + Types.LibRenderDx11;

        public enum StereoRenderRegion
        {
            Fullscreen,
            Left,
            Right
        }

        public const string TypeInt32 = "System.Int32, mscorlib";
    }
}
