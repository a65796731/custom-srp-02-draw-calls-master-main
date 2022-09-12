using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

struct ShadowedDirectionalLight
{
    public int visibleLightIndex;
}

public class Shadow 
{
    int ShadowedDirectionalLightCount = 0;
    const int maxShadowedDirectionalLightCount = 4;
    const string bufferName = "Shadow";
    CommandBuffer  buffer=new CommandBuffer { name = bufferName };
    ShadowedDirectionalLight[] shadowedDirectionals = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];
    ScriptableRenderContext context;
    CullingResults cullingResults;
    ShadowSettings shadowSettings;

    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
                dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
    static Matrix4x4 []dirshadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount];
    public void SetUp(ScriptableRenderContext context, CullingResults cullingResults,
         ShadowSettings shadowSettings)
    {
        ShadowedDirectionalLightCount = 0;
        this.context = context;
        this.cullingResults = cullingResults;
        this.shadowSettings= shadowSettings;
    }
    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
   public void ReserveDirectionalShadow(Light light,int visibleLightIndex)
    {
        if(visibleLightIndex<maxShadowedDirectionalLightCount&&
            light.shadows!=LightShadows.None&&light.shadowStrength>0f&&
            cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            shadowedDirectionals[ShadowedDirectionalLightCount++] = new ShadowedDirectionalLight
            {
                visibleLightIndex = visibleLightIndex
            };
        }
    }
    public void Render()
    {
        if(ShadowedDirectionalLightCount>0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }
    private void RenderDirectionalShadows()
    {
        int atlasSize = (int)shadowSettings.directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId,atlasSize, atlasSize,32,FilterMode.Bilinear,RenderTextureFormat.Shadowmap);
        buffer.SetRenderTarget(dirShadowAtlasId,RenderBufferLoadAction.DontCare,RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true,false,Color.clear);
        buffer.BeginSample(bufferName);
        ExecuteBuffer();
        int split = ShadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tilSize = atlasSize / split;
        for(int i=0;i<ShadowedDirectionalLightCount;i++)
        {
            RenderDirectionalShadows(i, split,tilSize);
        }
        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirshadowMatrices);
        buffer.EndSample(bufferName);
        ExecuteBuffer();

    }
    private void RenderDirectionalShadows(int index,int split,int tileSize)
    {
         ShadowedDirectionalLight shadowedDirectionalLight = shadowedDirectionals[index];
        ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(cullingResults, shadowedDirectionalLight.visibleLightIndex);
      
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            shadowedDirectionalLight.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
            out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix,
            out ShadowSplitData splitData
        );
        shadowSettings.splitData = splitData;
     
        dirshadowMatrices[index] = ConvertToAtlasMatrix( projectionMatrix * viewMatrix, SetTileViewport(index,split,tileSize),split);
        buffer.SetViewProjectionMatrices(viewMatrix,projectionMatrix);
        ExecuteBuffer();
        context.DrawShadows(ref shadowSettings);
    }
    Vector2 SetTileViewport(int index,int split,float tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect(offset.x*tileSize, offset.y * tileSize, tileSize, tileSize));
        return offset;
    }
    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m,Vector2 offset,int split)
    {
        if (SystemInfo.usesReversedZBuffer)
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        return m;
    }
    public void Cleanup()
    {
        buffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();

    }
}
