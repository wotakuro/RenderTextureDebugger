using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace UTJ
{
    public class RenderTextureSave 
    {
#if UNITY_2020_2_OR_NEWER
        public static void SaveRenderTextureDepth(RenderTexture src)
        {
            var depthMaterial = new Material(Shader.Find("Hidden/RendeerTextureDebug/RenderDepth"));

            depthMaterial.SetFloat("_MinParam", 0.0f);
            depthMaterial.SetFloat("_MaxParam", 1.0f);

            RenderTexture converted = new RenderTexture(src.width, src.height, 0, GraphicsFormat.R32G32B32A32_SFloat);
            converted.name = src.name + "_depth";

            CommandBuffer cmdBuffer = new CommandBuffer();
            cmdBuffer.SetRenderTarget(converted);
            cmdBuffer.ClearRenderTarget(true, true, Color.black);
            cmdBuffer.Blit(src, converted, depthMaterial);
            Graphics.ExecuteCommandBuffer(cmdBuffer);
            SaveWithDialog(converted);
            converted.Release();
        }

        public static void SaveWithDialog(RenderTexture renderTexture)
        {
            string defaultSaveFilename = renderTexture.name;
            if (string.IsNullOrEmpty(defaultSaveFilename))
            {
                defaultSaveFilename = "RenderTexture";
            }
            string ext = "png";
            if(Prefer16Bit(renderTexture.graphicsFormat) || Prefer32Bit(renderTexture.graphicsFormat))
            {
                ext = "exr";
            }
            string savePath = EditorUtility.SaveFilePanel("Select saveFile", "", defaultSaveFilename, ext);
            if (!string.IsNullOrEmpty(savePath))
            {
                SaveRenderTexture(renderTexture, savePath);
            }
        }
        public static string SaveRenderTexture(RenderTexture rt, string file)
        {
            string saveFile = null;
            byte[] saveData = null;
            RenderTexture capture = null;
            bool isCreateTmpTexture = ShouldCreateTmpTexture(rt.graphicsFormat);
            if ( isCreateTmpTexture)
            {
                capture = CreateTmpTexture(rt);
            }
            else
            {
                capture = rt;
            }
            var req = UnityEngine.Rendering.AsyncGPUReadback.Request(capture, 0,
                capture.graphicsFormat, null);
            req.WaitForCompletion();
            var data = req.GetData<byte>();

            if (Prefer32Bit(capture.graphicsFormat))
            {
                /* can save exr but can't load exr.... */
                var exrBin = ImageConversion.EncodeNativeArrayToEXR(data,
                    capture.graphicsFormat, (uint)capture.width, (uint)capture.height, 0,
                    Texture2D.EXRFlags.OutputAsFloat | Texture2D.EXRFlags.CompressZIP);
                saveData = exrBin.ToArray();
            }
            else if (Prefer16Bit(capture.graphicsFormat))
            {
                /* can save exr but can't load exr.... */
                var exrBin = ImageConversion.EncodeNativeArrayToEXR(data,
                    capture.graphicsFormat, (uint)capture.width, (uint)capture.height, 0,
                    Texture2D.EXRFlags.CompressZIP);
                saveData = exrBin.ToArray();
            }
            else
            {
                var pngBin = ImageConversion.EncodeNativeArrayToPNG(data,
                    capture.graphicsFormat, (uint)capture.width, (uint)capture.height);
                saveData = pngBin.ToArray();
            }

            if (isCreateTmpTexture)
            {
                RenderTexture.active = null;
                capture.Release();
            }
            System.IO.File.WriteAllBytes(file, saveData);
            return saveFile;
        }


        private static RenderTexture CreateTmpTexture(RenderTexture src)
        {

            RenderTexture dest = null;
            if (Prefer32Bit(src.graphicsFormat))
            {
                dest = new RenderTexture(src.width, src.height, 0, GraphicsFormat.R32G32B32A32_SFloat);
            }
            else if (Prefer16Bit(src.graphicsFormat))
            {
                dest = new RenderTexture(src.width, src.height, 0, GraphicsFormat.R16G16B16A16_SFloat);
            }
            else
            {
                dest = new RenderTexture(src.width, src.height, 0, GraphicsFormat.R8G8B8A8_UNorm);
            }
            Graphics.Blit(src, dest);
            return dest;
        }

        private static bool ShouldCreateTmpTexture(GraphicsFormat format)
        {
            bool isSupportReadPixel = SystemInfo.IsFormatSupported(format, FormatUsage.ReadPixels);
            if (isSupportReadPixel) { return true; }

            switch (format) {
                case GraphicsFormat.R16G16B16A16_SFloat:
                case GraphicsFormat.R32G32B32A32_SFloat:
                case GraphicsFormat.R8G8B8A8_SInt:
                case GraphicsFormat.R8G8B8A8_SNorm:
                case GraphicsFormat.R8G8B8A8_SRGB:
                case GraphicsFormat.R8G8B8A8_UInt:
                case GraphicsFormat.R8G8B8A8_UNorm:
                    return false;

            }

            return true;
        }

        private static bool Prefer16Bit(GraphicsFormat format)
        {
            switch (format)
            {
                case GraphicsFormat.A10R10G10B10_XRSRGBPack32:
                case GraphicsFormat.A10R10G10B10_XRUNormPack32:
                case GraphicsFormat.A2B10G10R10_SIntPack32:
                case GraphicsFormat.A2B10G10R10_UIntPack32:
                case GraphicsFormat.A2B10G10R10_UNormPack32:
                case GraphicsFormat.A2R10G10B10_SIntPack32:
                case GraphicsFormat.A2R10G10B10_UIntPack32:
                case GraphicsFormat.A2R10G10B10_UNormPack32:
                case GraphicsFormat.A2R10G10B10_XRSRGBPack32:
                case GraphicsFormat.A2R10G10B10_XRUNormPack32:
                case GraphicsFormat.B10G11R11_UFloatPack32:
                case GraphicsFormat.E5B9G9R9_UFloatPack32:
                case GraphicsFormat.R10G10B10_XRSRGBPack32:
                case GraphicsFormat.R10G10B10_XRUNormPack32:
                case GraphicsFormat.R16G16B16A16_SFloat:
                case GraphicsFormat.R16G16B16A16_SInt:
                case GraphicsFormat.R16G16B16A16_SNorm:
                case GraphicsFormat.R16G16B16A16_UInt:
                case GraphicsFormat.R16G16B16A16_UNorm:
                case GraphicsFormat.R16G16B16_SFloat:
                case GraphicsFormat.R16G16B16_SInt:
                case GraphicsFormat.R16G16B16_SNorm:
                case GraphicsFormat.R16G16B16_UInt:
                case GraphicsFormat.R16G16B16_UNorm:
                case GraphicsFormat.R16G16_SFloat:
                case GraphicsFormat.R16G16_SInt:
                case GraphicsFormat.R16G16_SNorm:
                case GraphicsFormat.R16G16_UInt:
                case GraphicsFormat.R16G16_UNorm:
                case GraphicsFormat.R16_SFloat:
                case GraphicsFormat.R16_SInt:
                case GraphicsFormat.R16_SNorm:
                case GraphicsFormat.R16_UInt:
                case GraphicsFormat.R16_UNorm:
                    return true;
                default:
                    return false;
            }
        }

        private static bool Prefer32Bit(GraphicsFormat format)
        {
            switch (format)
            {
                // 32 bit
                case GraphicsFormat.R32G32B32A32_SFloat:
                case GraphicsFormat.R32G32B32A32_SInt:
                case GraphicsFormat.R32G32B32A32_UInt:
                case GraphicsFormat.R32G32B32_SFloat:
                case GraphicsFormat.R32G32B32_SInt:
                case GraphicsFormat.R32G32B32_UInt:
                case GraphicsFormat.R32G32_SFloat:
                case GraphicsFormat.R32G32_SInt:
                case GraphicsFormat.R32G32_UInt:
                case GraphicsFormat.R32_SFloat:
                case GraphicsFormat.R32_SInt:
                case GraphicsFormat.R32_UInt:
                    return true;
                default:
                    return false;
            }

        }
#else
        public static void SaveRenderTextureDepth(RenderTexture renderTexture)
        {

        }

        public static void SaveWithDialog(RenderTexture renderTexture)
        {
            string defaultSaveFilename = renderTexture.name;
            if (string.IsNullOrEmpty(defaultSaveFilename))
            {
                defaultSaveFilename = "RenderTexture";
            }
            string savePath = EditorUtility.SaveFilePanel("Select saveFile", "", defaultSaveFilename, "png");
            if (!string.IsNullOrEmpty(savePath))
            {
                SaveRenderTextureLegacy(renderTexture, savePath);
            }
        }
        private static void SaveRenderTextureLegacy(RenderTexture renderTexture, string file)
        {
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false, false);
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();

            // Encode texture into PNG
            byte[] bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            //Write to a file in the project folder
            System.IO.File.WriteAllBytes(file, bytes);
        }
#endif

    }
}
