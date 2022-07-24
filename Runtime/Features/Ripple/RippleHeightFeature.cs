using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RippleHeightFeature : ScriptableRendererFeature
{
    static bool Enable;
    public Setting setting;
    private RippleHeightPass _pass;
    private Data _data;

    [Serializable]
    public class Setting
    {
        public RenderPassEvent renderEvent = RenderPassEvent.BeforeRendering;
        public float viscosity;
        public float velocity;
        public Shader waveShader;
    }

    public class Data
    {
        public Setting setting;
        public RenderTexture heightRt;
        public Vector4[] _triggers;

        public void Clear()
        {
            if (heightRt != null)
            {
                heightRt.Release();
                heightRt = null;
            }
        }
    }

    public override void Create()
    {
        if (setting == null)
            setting = new Setting();
        _data = new Data();
        _pass = new RippleHeightPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!Enable) return;
        SetData(ref renderingData);
        _pass.Setup(_data);
        renderer.EnqueuePass(_pass);
    }

    private void SetData(ref RenderingData renderingData)
    {
        ref var desc = ref renderingData.cameraData.cameraTargetDescriptor;
        _data.setting = setting;
        _data.heightRt = GetTexture(desc.width, desc.height, _data.heightRt);
    }

    private void OnDestroy()
    {
        _data.Clear();
    }

    private void OnDisable()
    {
        _data.Clear();
    }

    private RenderTexture GetTexture(int width, int height, RenderTexture rt)
    {
        if (rt == null)
        {
            rt = CreateRenderTexture(width, height);
        }
        else
        {
            if (rt.width != width || rt.height != height)
            {
                rt.Release();
                rt = CreateRenderTexture(width, height);
            }
        }

        return rt;
    }

    private RenderTexture CreateRenderTexture(int width, int height)
    {
        var reflectTex = new RenderTexture(width, height, 0, GraphicsFormat.R8G8B8A8_UNorm);
        reflectTex.enableRandomWrite = true;
        // reflectTex.name = "RippleHeightTexture";
        // reflectTex.filterMode = FilterMode.Point;
        reflectTex.Create();
        return reflectTex;
    }

    public static void SetFeatureEnable(bool val)
    {
        Enable = val;
    }
}

public class RippleHeightPass : ScriptableRenderPass
{
    const string m_ProfilerTag = "RippleHeight";
    private RippleHeightFeature.Data _data;
    private Material _mat;

    public void Setup(RippleHeightFeature.Data data)
    {
        _data = data;
        renderPassEvent = data.setting.renderEvent;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_data.setting.waveShader == null)
        {
            Debug.LogError($"wave shader is null, can not execute rippleHeight pass.");
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        if (_mat == null)
            _mat = new Material(_data.setting.waveShader);
        _mat.SetVectorArray(RippleTriggers, _data._triggers);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private static readonly int RippleTriggers = Shader.PropertyToID("_RippleTriggers");
}