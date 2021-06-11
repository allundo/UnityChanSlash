#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class DitherCreator
{
    [MenuItem("Assets/Create Dither Texture")]
    private static void CreateDitherTexture()
    {
        // RenderTextureにUnityのDitherテクスチャをレンダリングする
        var rt = RenderTexture.GetTemporary(4, 4 * 16);
        Graphics.Blit(rt, rt, new Material(Shader.Find("UnityDither")));

        // RenderTextureからテクスチャを作る
        var currentRT = RenderTexture.active;
        RenderTexture.active = rt;
        var texture = new Texture2D(rt.width, rt.height);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        texture.Apply();

        // テクスチャを保存する
        var bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes("Assets/Textures/dither_tex.png", bytes);
        AssetDatabase.Refresh();

        // 元に戻す
        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(rt);
    }
}
#endif