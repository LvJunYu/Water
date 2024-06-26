﻿using System;
using System.Collections.Generic;
using UnityEngine;
using LYU.WaterSystem.Data;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LYU.WaterSystem
{
    [ExecuteAlways]
    public class Water : MonoBehaviour
    {
        [SerializeField] public WaterRangeData waterRangeData;
        [SerializeField] public Material waterMaterial;
        [SerializeField] public WaterSettingsData settingsData;
        public const int WaterLayer = 4;
        static HashSet<Water> waters = new HashSet<Water>();

        void OnValidate()
        {
            Refresh();
        }

        void OnEnable()
        {
            // if (!computeOverride)
            //     useComputeBuffer = SystemInfo.supportsComputeShaders &&
            //                        Application.platform != RuntimePlatform.WebGLPlayer &&
            //                        Application.platform != RuntimePlatform.Android;
            // else
            //     useComputeBuffer = false;
            SetMaterial();
            Refresh();
            waters.Add(this);
            if (settingsData == null)
                Debug.LogError($"Water Debug: water {gameObject.name} dont have setting data.");
            else
                Debug.Log($"Water Debug: water {gameObject.name} setting data : {settingsData.name}");
        }

        void OnDisable()
        {
            Cleanup();
        }

        void Update()
        {
            if (settingsData != null)
                settingsData.UpdateSetting(waterMaterial);
        }

        void LateUpdate()
        {
            if (Application.isPlaying && settingsData != null && settingsData.waveSetting.waveEnable)
                GerstnerWavesJobs.UpdateHeights();
#if UNITY_EDITOR
            // 解决Editor下材质属性丢失的问题
            SetMaterialProperty();
#endif
        }

        private void Cleanup()
        {
            waters.Remove(this);
            //多个Water脚本使用相同Setting文件的情况
            bool noSettingUsed = true;
            bool noSSPR = true;
            bool noWave = true;
            foreach (var water in waters)
            {
                if (water.settingsData == null) continue;
                if (water.settingsData == settingsData) noSettingUsed = false;
                if (water.settingsData.reflectionSetting.refType == ReflectionType.SSPR) noSSPR = false;
                if (water.settingsData.waveSetting.waveEnable) noWave = false;
            }

            if (Application.isPlaying && noWave)
                GerstnerWavesJobs.Cleanup();
            if (noSSPR)
                SSPlanarReflectionFeature.SetSSPREnable(false);
            if (noSettingUsed)
            {
                if (settingsData != null)
                    settingsData.Cleanup();
            }
        }

        private void SetMaterialProperty()
        {
            if (waterMaterial == null) return;
            if (settingsData == null) return;
            settingsData.SetMaterial(waterMaterial);
            var pos = transform.position;
            waterMaterial.SetVector(WaterParam1, new Vector4(pos.x,pos.y,pos.z, waterRangeData.maxWaterVerticalDepth));
            waterMaterial.SetVector(WaterParam2, new Vector4(settingsData.surfaceSetting.waterMaxVisibility,
                settingsData.waveSetting.maxWaveHeight, waterRangeData.deltaHeight, waterRangeData.orthographicSize));
        }

        public void Refresh()
        {
            if (settingsData != null)
                settingsData.Refresh(this);
            SetMaterialProperty();
        }

        public void SetMaterial()
        {
            if (settingsData == null) return;
            if (waterMaterial == null)
            {
                Debug.LogError($"Water {gameObject.name} need a material.");
                // if (settingsData.waterShader == null)
                //     settingsData.waterShader = Shader.Find("LYU/Water/Water");
                // waterMaterial = new Material(settingsData.waterShader) {doubleSidedGI = true, name = gameObject.name};
            }

            // else
            {
                var renders = gameObject.GetComponentsInChildren<MeshRenderer>(true);
                if (renders != null)
                {
                    foreach (var meshRenderer in renders)
                    {
                        meshRenderer.sharedMaterial = waterMaterial;
                        meshRenderer.gameObject.layer = WaterLayer;
                    }
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Bake Height Map of Sea Bed")]
        public void CaptureDepthMap()
        {
            var samePath = GetSamePathWithObj(waterMaterial, "_SeaBedHeightMap.png");
            settingsData.foamSetting.bakedDepthTex = DepthFetch.GetDepth(transform.position, waterRangeData.deltaHeight,
                waterRangeData.orthographicSize, waterRangeData.maxWaterVerticalDepth, samePath);
            Refresh();
        }
        
        string GetSamePathWithObj(Object mat, string name)
        {
            var path = AssetDatabase.GetAssetPath(mat);
            var extendName = System.IO.Path.GetExtension(path);
            return path.Replace(extendName, name);
        }
#endif

        private static readonly int WaterParam1 = Shader.PropertyToID("_WaterParam1");
        private static readonly int WaterParam2 = Shader.PropertyToID("_WaterParam2");
    }

    [Serializable]
    public class WaterRangeData
    {
        public float maxWaterVerticalDepth = 100f;
        public float deltaHeight = 4f;
        public float orthographicSize = 250;
    }
}