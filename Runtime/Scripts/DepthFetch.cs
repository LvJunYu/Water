#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthFetch
{
    public static Texture2D GetDepth(Vector3 pos, float deltaHeight, float orthographicSize, float waterMaxVisibility,
        Shader depthCopyShader)
    {
        //Generate the camera
        GameObject go = new GameObject("depthCamera"); //create the cameraObject
        go.hideFlags = HideFlags.HideAndDontSave;
        var depthCam = go.AddComponent<Camera>();
        var cameraData = depthCam.GetComponent<UniversalAdditionalCameraData>();
        if (cameraData == null)
        {
            cameraData = depthCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
        }
        
        cameraData.renderShadows = false;
        cameraData.requiresColorOption = CameraOverrideOption.Off;
        cameraData.requiresDepthOption = CameraOverrideOption.Off;
        cameraData.SetRenderer(1);
        var transform1 = depthCam.transform;

        transform1.position = pos + Vector3.up * deltaHeight; //center the camera on this water plane
        transform1.up = Vector3.forward; //face teh camera down
        depthCam.enabled = true;
        depthCam.orthographic = true;
        depthCam.orthographicSize = orthographicSize; //hardcoded = 1k area - TODO
        //_depthCam.depthTextureMode = DepthTextureMode.Depth;
        depthCam.nearClipPlane = 0.1f;
        depthCam.farClipPlane = waterMaxVisibility + 0;
        depthCam.allowHDR = false;
        depthCam.allowMSAA = false;
        depthCam.cullingMask = LayerMask.GetMask("SeaFloor");

        //Generate RT
        var tempTex = RenderTexture.GetTemporary(1024, 1024, 24, RenderTextureFormat.Depth,
            RenderTextureReadWrite.Linear);
        var tempTex2 =
            RenderTexture.GetTemporary(1024, 1024, 16, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
        {
            tempTex2.filterMode = tempTex.filterMode = FilterMode.Point;
            tempTex2.wrapMode = tempTex.wrapMode = TextureWrapMode.Clamp;
        }

        //do depth capture
        depthCam.targetTexture = tempTex;
        depthCam.Render();

        var copyMat = new Material(depthCopyShader);
        Graphics.Blit(tempTex, tempTex2, copyMat);
        depthCam.enabled = false;
        depthCam.targetTexture = null;
        var bakedDepthTex = new Texture2D(1024, 1024, TextureFormat.R16, false, true);
        RenderTexture.active = tempTex2;
        bakedDepthTex.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0);
        bakedDepthTex.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(tempTex);
        RenderTexture.ReleaseTemporary(tempTex2);
        SafeDestroy(copyMat);
        SafeDestroy(go);
#if UNITY_EDITOR
        // save depth tex to asset
        byte[] image = bakedDepthTex.EncodeToPNG();
        var path = Application.dataPath + "/WaterDepth.png";
        var assetPath = "Assets/WaterDepth.png";
        System.IO.File.WriteAllBytes(path, image);
        AssetDatabase.Refresh();
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        TextureImporterSettings setting = new TextureImporterSettings();
        if (importer != null)
        {
            importer.ReadTextureSettings(setting);
            setting.textureType = TextureImporterType.SingleChannel;
            setting.singleChannelComponent = TextureImporterSingleChannelComponent.Red;
            setting.wrapMode = TextureWrapMode.Clamp;
            importer.SetTextureSettings(setting);
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        bakedDepthTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
#endif
        return bakedDepthTex;
    }

    private static void SafeDestroy(Object o)
    {
        if (Application.isPlaying)
            Object.Destroy(o);
        else
            Object.DestroyImmediate(o);
    }
}