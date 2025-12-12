using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using Cysharp.Threading.Tasks;
using HybridToolkit.LogicPipeline;
using UnityEngine.Experimental.Rendering; // 用于 GraphicsFormat

#if UNITY_URP // 假设你定义了宏，或者是直接在 URP 项目中
using UnityEngine.Rendering.Universal;
#endif

namespace HybridToolkit.ScreenShotTool
{
    /// <summary>
    /// 输入：截图指令
    /// </summary>
    public struct ScreenshotCommand
    {
        public Camera TargetCamera;
        public string SavePath;
        public float Scale;
        public int JpgQuality;

        public ScreenshotCommand(Camera cam, string path, float scale = 1.0f, int quality = 75)
        {
            TargetCamera = cam;
            SavePath = path;
            Scale = scale;
            JpgQuality = quality;
        }
    }

    /// <summary>
    /// 中间产物：编码后的图片数据
    /// </summary>
    public struct ImageBlob
    {
        public string TargetPath;
        public byte[] Data;
        public int Width;
        public int Height;
    }
    
    public sealed class CaptureAndEncodeOp : IAsyncProcessor<ScreenshotCommand, ImageBlob>
    {
        public async UniTask<ImageBlob> ProcessAsync(ScreenshotCommand input, System.Threading.CancellationToken ct)
        {
            // 1. 准备参数
            var cam = input.TargetCamera;
            if (cam == null) throw new ArgumentNullException(nameof(cam));

            int srcWidth = cam.targetTexture != null ? cam.targetTexture.width : Screen.width;
            int srcHeight = cam.targetTexture != null ? cam.targetTexture.height : Screen.height;
            int finalWidth = Mathf.RoundToInt(srcWidth * input.Scale);
            int finalHeight = Mathf.RoundToInt(srcHeight * input.Scale);

            // 2. 创建临时 RT
            var finalRT = RenderTexture.GetTemporary(finalWidth, finalHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);

            try
            {
                // 3. 等待当前帧渲染结束
                await UniTask.WaitForEndOfFrame(cam.GetCancellationTokenOnDestroy());
                if (ct.IsCancellationRequested) return default;

                // 4. 执行 Blit (处理缩放和翻转)
                if (cam.targetTexture != null)
                {
                    Graphics.Blit(cam.targetTexture, finalRT);
                }
                else
                {
                    // 屏幕直接截图
                    Graphics.Blit(null, finalRT);
                }

                // 5. GPU 异步回读
                var request = await AsyncGPUReadback.Request(finalRT, 0, TextureFormat.RGB24);
                if (request.hasError) throw new Exception("GPU Readback error");

                if (ct.IsCancellationRequested) return default;

                // 6. 数据提取与编码 (Zero-Alloc 提取)
                var rawData = request.GetData<byte>();
                
                // 必须在主线程创建 Texture2D
                var tempTex = new Texture2D(request.width, request.height, TextureFormat.RGB24, false);
                byte[] jpgBytes;
                
                try
                {
                    tempTex.LoadRawTextureData(rawData);
                    tempTex.Apply();
                    jpgBytes = tempTex.EncodeToJPG(input.JpgQuality);
                }
                finally
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(tempTex);
                    else UnityEngine.Object.DestroyImmediate(tempTex);
                }

                // 7. 返回中间数据
                return new ImageBlob
                {
                    TargetPath = input.SavePath,
                    Data = jpgBytes,
                    Width = finalWidth,
                    Height = finalHeight
                };
            }
            finally
            {
                RenderTexture.ReleaseTemporary(finalRT);
            }
        }
    }
    
    public sealed class FileWriteOp : IAsyncProcessor<ImageBlob, string>
    {
        public async UniTask<string> ProcessAsync(ImageBlob input, System.Threading.CancellationToken ct)
        {
            var dir = Path.GetDirectoryName(input.TargetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) 
                Directory.CreateDirectory(dir);

            // 切换到线程池执行写入，避免卡顿主线程
            await UniTask.RunOnThreadPool(() => 
            {
                File.WriteAllBytes(input.TargetPath, input.Data);
            }, cancellationToken: ct);

            return input.TargetPath; // 返回最终路径
        }
    }
    
    public static class ScreenshotPipeline
    {
        // 缓存 Processor 实例以复用 (如果它们是无状态的)
        private static readonly CaptureAndEncodeOp _captureOp = new CaptureAndEncodeOp();
        private static readonly FileWriteOp _writeOp = new FileWriteOp();

        /// <summary>
        /// 执行截图管线
        /// </summary>
        public static async UniTask<string> CaptureAsync(Camera cam, string path, float scale = 1.0f)
        {
            var cts = new CancellationTokenSource();
            
            // 构建指令
            var command = new ScreenshotCommand(cam, path, scale);

            // ==========================================
            // 核心逻辑链： Capture -> Write
            // ==========================================
            // 这里利用了你的 LogicPipeline 扩展方法 .ThenAsync
            var pipeline = _captureOp.ThenAsync(_writeOp);

            // 执行
            return await pipeline.ProcessAsync(command, cts.Token);
        }

        // 或者，如果你想动态构建更复杂的流程（例如截图后还要上传服务器）
        public static IAsyncProcessor<ScreenshotCommand, string> GetPipeline()
        {
            return _captureOp.ThenAsync(_writeOp);
        }
    }
}
public static class ScreenshotTool
{
    // 这里保留你的 CameraScope，写得很好，无需改动
    public readonly struct CameraScope : IDisposable
    {
        private readonly Camera _cam;
        private readonly RenderTexture _originalTarget;
        #if UNITY_URP
        private readonly bool _originalPostProcess;
        #endif

        public CameraScope(Camera cam, RenderTexture targetRT)
        {
            _cam = cam;
            _originalTarget = cam.targetTexture;
            
            #if UNITY_URP
            // 处理 URP PostProcessing
            var data = cam.GetComponent<UniversalAdditionalCameraData>();
            if (data != null)
            {
                _originalPostProcess = data.renderPostProcessing;
                // 截图时通常希望保留后处理，如果不希望，这里设为 false
                // data.renderPostProcessing = true; 
            }
            #endif

            cam.targetTexture = targetRT;
        }

        public void Dispose()
        {
            _cam.targetTexture = _originalTarget;
            #if UNITY_URP
            var data = _cam.GetComponent<UniversalAdditionalCameraData>();
            if (data != null) data.renderPostProcessing = _originalPostProcess;
            #endif
        }
    }

    /// <summary>
    /// 核心截图方法 (优化版)
    /// </summary>
    public static async UniTask CaptureToFileAsync(Camera cam, string fullPath, float scale = 1.0f, int jpgQuality = 75)
    {
        // 1. 确保目录存在 (提前做，失败就不用渲染了)
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // 2. 确定源尺寸和源纹理标识
        int srcWidth = cam.targetTexture != null ? cam.targetTexture.width : Screen.width;
        int srcHeight = cam.targetTexture != null ? cam.targetTexture.height : Screen.height;
        
        // 3. 计算目标尺寸
        int finalWidth = Mathf.RoundToInt(srcWidth * scale);
        int finalHeight = Mathf.RoundToInt(srcHeight * scale);

        // 4. 创建临时 RT 用于 GPU Readback
        // 注意：使用 Default 格式通常能自动兼容 sRGB，但为了保险最好指定格式
        RenderTexture finalRT = RenderTexture.GetTemporary(finalWidth, finalHeight, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
        
        try
        {
            // 5. 等待帧结束，确保画面已渲染
            await UniTask.WaitForEndOfFrame(cam.GetCancellationTokenOnDestroy());

            // 6. 执行 Blit (包含缩放 + 翻转修正 + 屏幕抓取逻辑)
            if (cam.targetTexture != null)
            {
                // Case A: 相机渲染到 RT
                Graphics.Blit(cam.targetTexture, finalRT);
            }
            else
            {
                // Case B: 相机渲染到屏幕 (BackBuffer)
                // 注意：在 URP 中，EndOfFrame 时 BackBuffer 内容可用。
                // 这里的 source 传入 null 或 BuiltinRenderTextureType.CameraTarget 均可
                Graphics.Blit(null, finalRT); 
            }

            // 7. GPU 异步回读
            // 显式指定 RGB24，避免 Alpha 通道浪费带宽 (JPG 不需要 Alpha)
            var request = await AsyncGPUReadback.Request(finalRT, 0, TextureFormat.RGB24);
            
            if (request.hasError) 
            {
                Debug.LogError("GPU Readback Error");
                return;
            }

            // 8. 数据提取 & 编码 (优化点：无 GC Alloc)
            // 直接使用 NativeArray，避免 .ToArray() 的巨大内存分配
            var rawData = request.GetData<byte>(); 
            var width = request.width;
            var height = request.height;

            Texture2D tempTex = new Texture2D(width, height, TextureFormat.RGB24, false);
            byte[] jpgBytes;
            
            try
            {
                // 直接加载 NativeArray 数据
                tempTex.LoadRawTextureData(rawData);
                tempTex.Apply(); 
                
                // 编码为 JPG (这是主线程耗时点)
                jpgBytes = tempTex.EncodeToJPG(jpgQuality);
            }
            finally
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(tempTex);
                else UnityEngine.Object.DestroyImmediate(tempTex);
            }

            // 9. 线程池写入文件
            await UniTask.RunOnThreadPool(() => File.WriteAllBytes(fullPath, jpgBytes));
            
            // Debug.Log($"Screenshot saved: {fullPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Screenshot failed: {ex}");
        }
        finally
        {
            RenderTexture.ReleaseTemporary(finalRT);
        }
    }
}