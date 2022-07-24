using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SSPlanarReflectionFeature : ScriptableRendererFeature
{
    static bool Enable;
    public static float Scale;
    public static float WaterLevel;
    public SSRPSetting setting;
    private SSPlanarReflectionPass _ssrpPass;
    private ReflectData _data;
    private Dictionary<Camera, CameraData> _cameraDataDic;

    [Serializable]
    public class SSRPSetting
    {
        public ComputeShader ssrpCS;
        public float stretchIntensity = 4.5f;
        public float stretchThreshold = 0.2f;

        public RenderPassEvent renderEvent = RenderPassEvent.BeforeRenderingTransparents;
        [HideInInspector] public float waterLevel;
        [HideInInspector] [Range(0.1f, 1f)] public float scale = 1f;
        // public bool blur;
        // public Shader blurShader;

        public void Init()
        {
            // blurShader = Shader.Find("Hidden/Water/GaussianBlur");
#if UNITY_EDITOR
            if (ssrpCS == null)
                ssrpCS = AssetDatabase.LoadAssetAtPath<ComputeShader>(
                    "Packages/water/Runtime/Features/SSPR/SSPlanarReflection.compute");
#endif
        }
    }

    public class ReflectData
    {
        public SSRPSetting setting;
        public RenderTexture reflectTexture;
        public ComputeBuffer computeBuffer;
        public int groupX;

        public int groupY;
        // public Material blurMaterial;
    }

    private class CameraData
    {
        public RenderTexture reflTexture;
        public ComputeBuffer computeBuffer;

        public void Clear()
        {
            if (reflTexture != null)
            {
                reflTexture.Release();
                reflTexture = null;
            }

            if (computeBuffer != null)
            {
                computeBuffer.Dispose();
                computeBuffer = null;
            }
        }
    }

    public override void Create()
    {
        Clear();
        if (setting == null)
        {
            setting = new SSRPSetting();
            setting.Init();
        }

        _cameraDataDic = new Dictionary<Camera, CameraData>(1);
        _data = new ReflectData();
        _ssrpPass = new SSPlanarReflectionPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!Enable) return;
        SetData(ref renderingData);
        _ssrpPass.Setup(_data, renderer);
        renderer.EnqueuePass(_ssrpPass);
    }

    private void SetData(ref RenderingData renderingData)
    {
        setting.scale = Scale;
        setting.waterLevel = WaterLevel;
        ref var desc = ref renderingData.cameraData.cameraTargetDescriptor;
        var camera = renderingData.cameraData.camera;
        if (!_cameraDataDic.TryGetValue(camera, out var cameraData))
        {
            cameraData = new CameraData();
            _cameraDataDic.Add(camera, cameraData);
        }

        var width = Mathf.CeilToInt(desc.width * setting.scale);
        var height = Mathf.CeilToInt(desc.height * setting.scale);
        _data.setting = setting;
        _data.groupX = Mathf.CeilToInt(width / 8f);
        _data.groupY = Mathf.CeilToInt(height / 8f);
        _data.computeBuffer = GetComputeBuffer(cameraData);
        _data.reflectTexture = GetTexture(width, height, cameraData);
        // if (setting.blur && _data.blurMaterial == null)
        //     _data.blurMaterial = new Material(setting.blurShader);
    }

    private void OnDestroy()
    {
        Clear();
    }

    private void OnDisable()
    {
        Clear();
    }

    private void Clear()
    {
        if (_cameraDataDic == null) return;
        foreach (var cameraData in _cameraDataDic.Values)
        {
            cameraData.Clear();
        }

        _cameraDataDic.Clear();
    }

    private ComputeBuffer GetComputeBuffer(CameraData cameraData)
    {
        var length = _data.groupX * _data.groupY * 64;

        if (cameraData.computeBuffer == null)
        {
            cameraData.computeBuffer = new ComputeBuffer(length, sizeof(uint));
        }
        else if (cameraData.computeBuffer.count < length)
        {
            cameraData.computeBuffer.Dispose();
            cameraData.computeBuffer = new ComputeBuffer(length, sizeof(uint));
        }

        return cameraData.computeBuffer;
    }

    private RenderTexture GetTexture(int width, int height, CameraData cameraData)
    {
        if (cameraData.reflTexture == null)
        {
            cameraData.reflTexture = CreateRenderTexture(width, height);
        }
        else
        {
            if (cameraData.reflTexture.width != width || cameraData.reflTexture.height != height)
            {
                cameraData.reflTexture.Release();
                cameraData.reflTexture = CreateRenderTexture(width, height);
            }
        }

        return cameraData.reflTexture;
    }

    private RenderTexture CreateRenderTexture(int width, int height)
    {
        var reflectTex = new RenderTexture(width, height, 0, GraphicsFormat.R8G8B8A8_UNorm);
        reflectTex.name = "_SSPlanarReflectionTexture";
        reflectTex.enableRandomWrite = true;
        // reflectTex.filterMode = FilterMode.Point;
        reflectTex.Create();
        return reflectTex;
    }

    public static void SetSSPREnable(bool val)
    {
        Enable = val;
    }
}

public class SSPlanarReflectionPass : ScriptableRenderPass
{
    static readonly int _Param1 = Shader.PropertyToID("_Param1");
    static readonly int _Param2 = Shader.PropertyToID("_Param2");
    static readonly int _PlanarReflectionTexture = Shader.PropertyToID("_SSPlanarReflectionTexture");

    static readonly int _ReflectBuffer = Shader.PropertyToID("_ReflectBuffer");

    // static readonly int _ReflectBlurTemp = Shader.PropertyToID("_ReflectBlurTemp");
    const string m_ProfilerTag = "SSRP Project";
    const int KernelClear = 0;
    const int KernelProject = 1;
    const int KernelFillHole = 2;
    private ComputeShader _ssrpCS;
    private SSPlanarReflectionFeature.ReflectData _data;
    private ScriptableRenderer _renderer;

    public void Setup(SSPlanarReflectionFeature.ReflectData data, ScriptableRenderer renderer)
    {
        _renderer = renderer;
        _data = data;
        renderPassEvent = data.setting.renderEvent;
        _ssrpCS = data.setting.ssrpCS;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (_ssrpCS == null) return;
        if (!CheckSystemSupported())
        {
            SSPlanarReflectionFeature.SetSSPREnable(false);
            return;
        }

        var camera = renderingData.cameraData.camera;
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        // set params
        cmd.SetComputeVectorParam(_ssrpCS, _Param1,
            new Vector4(_data.groupX * 8, _data.groupY * 8, _data.setting.waterLevel, _data.setting.scale));
        var cameraDirX = camera.transform.eulerAngles.x;
        cameraDirX = cameraDirX > 180 ? cameraDirX - 360 : cameraDirX;
        cameraDirX *= 0.00001f;
        cmd.SetComputeVectorParam(_ssrpCS, _Param2,
            new Vector4(_data.setting.stretchIntensity, _data.setting.stretchThreshold, cameraDirX));

        // clear
        cmd.SetComputeTextureParam(_ssrpCS, KernelClear, _PlanarReflectionTexture, _data.reflectTexture);
        cmd.SetComputeBufferParam(_ssrpCS, KernelClear, _ReflectBuffer, _data.computeBuffer);
        cmd.DispatchCompute(_ssrpCS, KernelClear, _data.groupX, _data.groupY, 1);

        // project
        cmd.SetComputeTextureParam(_ssrpCS, KernelProject, _PlanarReflectionTexture, _data.reflectTexture);
        cmd.SetComputeBufferParam(_ssrpCS, KernelProject, _ReflectBuffer, _data.computeBuffer);
        cmd.DispatchCompute(_ssrpCS, KernelProject, _data.groupX, _data.groupY, 1);

        // fill hole
        cmd.SetComputeTextureParam(_ssrpCS, KernelFillHole, _PlanarReflectionTexture, _data.reflectTexture);
        cmd.SetComputeBufferParam(_ssrpCS, KernelFillHole, _ReflectBuffer, _data.computeBuffer);
        cmd.DispatchCompute(_ssrpCS, KernelFillHole, _data.groupX, _data.groupY, 1);

        // set reflect result
        // if (_data.setting.blur)
        // {
        //     cmd.GetTemporaryRT(_ReflectBlurTemp, _data.reflectTexture.descriptor, FilterMode.Bilinear);
        //     cmd.Blit(_data.reflectTexture, _ReflectBlurTemp, _data.blurMaterial, 0);
        //     cmd.Blit(_ReflectBlurTemp, _data.reflectTexture, _data.blurMaterial, 1);
        // }

        cmd.SetGlobalTexture(_PlanarReflectionTexture, _data.reflectTexture);
        // compute shader之后需要重新设置RenderTarget（？）
        cmd.SetRenderTarget(_renderer.cameraColorTarget, _renderer.cameraDepthTarget);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public static bool CheckSystemSupported()
    {
        // const FormatUsage kFlags = FormatUsage.Linear;
        // var supportFormat = SystemInfo.IsFormatSupported(GraphicsFormat.R32_SFloat, kFlags);
        // if (!supportFormat)
        //     Debug.LogError($"当前设备{SystemInfo.deviceName}不支持GraphicsFormat.R32_UInt格式");

        var supportCs = SystemInfo.supportsComputeShaders;
        if (!supportCs)
            Debug.LogError($"当前设备{SystemInfo.deviceName}不支持ComputeShader");

        return supportCs;
    }
}