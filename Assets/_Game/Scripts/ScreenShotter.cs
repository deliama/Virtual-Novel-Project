using UnityEngine;

public class ScreenShotter : MonoBehaviour
{

    public Texture2D CaptureScreenShot()
    {
        /*获取屏幕尺寸（单位是像素）
         截图的纹理大小需要与屏幕尺寸匹配*/
        int width = Screen.width;
        int height = Screen.height;
        
        /*创建渲染纹理
         RenderTexture是一种特殊的纹理，用于存储摄像机的渲染输出
         可以简单地理解为摄像机的画布*/
        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24);
        
        //获取主摄像机，有主摄像机才可以进行渲染
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is null");
        }
        
        //设置相机渲染目标
        mainCamera.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        mainCamera.Render();
        
        //读取像素数据
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        //VITAL:ReadPixels是在GPU上完成的，写入了在GPU上的副本，要想传到CPU，则还需要Apply函数
        screenshot.Apply();
        
        //重置渲染目标并释放资源
        mainCamera.targetTexture = null;
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        //调整截图大小，节省内存
        Texture2D resizedScreenshot = ResizeTexture(screenshot, width / 6, height / 6);
        
        //销毁原始截图，释放内存
        Destroy(screenshot);

        return resizedScreenshot;
    }


    /// <summary>
    /// 辅助方法，将纹理缩小到指定的分辨率
    /// </summary>
    /// <param name="original">原来的纹理</param>
    /// <param name="newWidth">新宽度</param>
    /// <param name="newHeight">新高度</param>
    /// <returns></returns>
    private Texture2D ResizeTexture(Texture2D original, int newWidth, int newHeight)
    {
        //创建渲染纹理
        //创建一个与目标分辨率相匹配的渲染纹理，并激活
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight,24);
        RenderTexture.active = rt;
        
        //使用GPU缩放
        /*为什么使用GPU?
         GPU操作比手动逐像素缩放效率更高，适合实时操作*/
        Graphics.Blit(original, rt);
        
        //读取缩放后的纹理数据
        Texture2D result = new Texture2D(newWidth, newHeight,TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        //ReadPixels是在GPU上完成的，写入了在GPU上的副本，要想传到CPU，则还需要Apply函数
        result.Apply();
        
        //释放资源
        //清理临时数据，避免内存泄漏
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        
        return result;
    }
}
