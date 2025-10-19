using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

namespace SelectionOutline
{
    public class SelectionOutlineFeature : ScriptableRendererFeature
    {
        [SerializeField] private Shader m_OutlineShader;
        [SerializeField] private Shader m_MaskShader;
        [SerializeField] private LayerMask m_LayerMask;

        private SelectionOutlinePass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new SelectionOutlinePass(m_OutlineShader, m_MaskShader, m_LayerMask);
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }

        private class SelectionOutlinePass : ScriptableRenderPass
        {
            private Material m_OutlineMaterial;
            private Material m_MaskMaterial;
            private LayerMask m_LayerMask;

            private List<ShaderTagId> m_ShaderTag = new List<ShaderTagId>();

            static readonly int s_OutlineColorID = Shader.PropertyToID("_OutlineColor");
            static readonly int s_OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");

            public SelectionOutlinePass(Shader outlineShader, Shader maskShader, LayerMask layerMask)
            {
                if (outlineShader != null) m_OutlineMaterial = CoreUtils.CreateEngineMaterial(outlineShader);
                if (maskShader != null) m_MaskMaterial = CoreUtils.CreateEngineMaterial(maskShader);

                m_LayerMask = layerMask;
            }

            private class PassData
            {
                public RendererListHandle rendererListHandle;
                public TextureHandle maskTexture;
                public Material outlineMaterial;
            }

            private void InitRendererLists(ContextContainer frameData, ref PassData passData, RenderGraph renderGraph, Material overrideMat)
            {
                UniversalRenderingData universalRenderingData = frameData.Get<UniversalRenderingData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalLightData lightData = frameData.Get<UniversalLightData>();

                SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
                RenderQueueRange renderQueueRange = RenderQueueRange.all;
                FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, m_LayerMask);

                ShaderTagId[] forwardOnlyShaderTagIds = new ShaderTagId[]
                {
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("LightweightForward")
                };

                m_ShaderTag.Clear();

                foreach (ShaderTagId sid in forwardOnlyShaderTagIds)
                    m_ShaderTag.Add(sid);

                DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(m_ShaderTag, universalRenderingData, cameraData, lightData, sortFlags);

                drawSettings.overrideMaterial = overrideMat;
                drawSettings.overrideMaterialPassIndex = 0;

                RendererListParams param = new RendererListParams(universalRenderingData.cullResults, drawSettings, filterSettings);
                passData.rendererListHandle = renderGraph.CreateRendererList(param);
            }

            private void UpdateSettings()
            {
                if (m_OutlineMaterial == null) { Debug.LogError("update settings material null"); return; }
                SelectableOutlineVolume volume = VolumeManager.instance.stack.GetComponent<SelectableOutlineVolume>();

                Color outlineColor = volume.OutlineColor.value;
                float outlineWidth = volume.OutlineWidth.value;

                m_OutlineMaterial.SetColor(s_OutlineColorID, outlineColor);
                m_OutlineMaterial.SetFloat(s_OutlineThicknessID, outlineWidth);
            }

            static void ExecuteMaskPass(PassData data, RasterGraphContext context)
            {
                context.cmd.ClearRenderTarget(false, true, Color.black);
                context.cmd.DrawRendererList(data.rendererListHandle);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData result = frameData.Get<UniversalResourceData>();
                UniversalCameraData camData = frameData.Get<UniversalCameraData>();

                if (!camData.postProcessEnabled || camData.isSceneViewCamera) return;
                if (result.isActiveTargetBackBuffer) return;

                TextureDesc desc = renderGraph.GetTextureDesc(result.activeColorTexture);
                desc.name = "SelectableMaskRT";
                desc.clearBuffer = true;
                desc.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm;
                TextureHandle maskRT = renderGraph.CreateTexture(desc);

                UpdateSettings();

                using (IRasterRenderGraphBuilder pass = renderGraph.AddRasterRenderPass<PassData>("Selectable Mask Render", out PassData passData))
                {
                    passData.maskTexture = maskRT;
                    passData.outlineMaterial = m_OutlineMaterial;

                    InitRendererLists(frameData, ref passData, renderGraph, m_MaskMaterial);

                    pass.UseRendererList(passData.rendererListHandle);
                    pass.SetRenderAttachment(maskRT, 0, AccessFlags.Write);
                    pass.SetRenderAttachmentDepth(result.activeDepthTexture, AccessFlags.Read);

                    pass.SetRenderFunc((PassData data, RasterGraphContext context) => ExecuteMaskPass(data, context));
                }

                RenderGraphUtils.BlitMaterialParameters param = new(maskRT, result.activeColorTexture, m_OutlineMaterial, 0);
                renderGraph.AddBlitPass(param, "Outline From Mask");
            }
        }
    }
}