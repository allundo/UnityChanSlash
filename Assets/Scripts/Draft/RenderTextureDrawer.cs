using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System;

public class RenderTextureDrawer : MonoBehaviour
{
    // XXX: There is no clear basis that the render scale is 4/3(= 1.3333333f).
    // This scale might change in future update or some reason.
    private static readonly float RENDER_SCALE = 2f;

    private static readonly Vector3 RENDER_ORIGIN = new Vector3(-RENDER_SCALE * 0.5f, -RENDER_SCALE * 0.5f);
    private static readonly int MINIMAP_BLOCKS = 16;
    private float RenderUnit => RENDER_SCALE / renderBlocks;
    private int renderBlocks = MINIMAP_BLOCKS;
    private RawImage image = default;
    private RectTransform rectTransform;

    private RenderTexture renderTexture;
    private CommandBuffer commandBuffer;
    private Mesh mesh;
    private Material material;

    private Vector2 defaultSize;
    private Vector3 position = new Vector3(50f, 50f, 1f);

    void Awake()
    {
        image = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        defaultSize = rectTransform.sizeDelta;

        // Depth is set to 0 for current 2D map use
        image.texture = renderTexture = new RenderTexture((int)defaultSize.x, (int)defaultSize.y, 0);

        InitCommandBuffer(defaultSize);
        InitMesh();
    }

    private void InitCommandBuffer(Vector2 viewport)
    {
        commandBuffer = new CommandBuffer();
        commandBuffer.name = "MiniMapRenderer";
        commandBuffer.SetViewport(new Rect(0, 0, viewport.x, viewport.y));
    }

    private void InitMesh()
    {
        // メッシュ（2D矩形）を用意する
        mesh = new Mesh();
        mesh.name = "RedMesh";

        mesh.vertices = new[] {
            new Vector3(0f, 0f), // 左下
            new Vector3(1f, 0f), // 右下
            new Vector3(0f, 1f), // 左上
            new Vector3(1f, 1f)  // 右上
        };
        mesh.triangles = new[] {
            0, 2, 1,
            2, 3, 1
        };

        // マテリアル（塗りつぶしの赤）を用意する
        material = new Material(Shader.Find("Unlit/Color"));
        material.SetColor("_Color", Color.red);
    }

    private void Update()
    {
        var tmpActiveRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        // ClearBuffer(Color.white);
        // DrawTileBuffer(0, 0);
        // DrawTileBuffer(3, 4);
        // DrawTileBuffer(5, 4, Color.blue);
        // DrawTileBuffer(12, 15);

        Graphics.ExecuteCommandBuffer(commandBuffer);
        RenderTexture.active = tmpActiveRT;
    }

    private void DrawTileBuffer(int x, int y, Material material = null)
    {
        if (IsOutOfBlocks(x, y)) throw new ArgumentException("Out of blocks(max: " + renderBlocks + "), (" + x + ", " + y + ")");

        var pos = RENDER_ORIGIN + new Vector3(x, y) * RenderUnit;
        var scale = Vector3.one * RenderUnit;

        commandBuffer.DrawMesh(mesh, Matrix4x4.TRS(pos, Quaternion.identity, scale), material ?? this.material);
    }

    private void DrawTileBuffer(int x, int y, Color color)
    {
        var mat = new Material(material);
        mat.SetColor("_Color", color);
        DrawTileBuffer(x, y, mat);
    }
    private void ClearBuffer(Color color)
    {
        var mat = new Material(material);

        mat.SetColor("_Color", color);
        commandBuffer.DrawMesh(mesh, Matrix4x4.TRS(RENDER_ORIGIN, Quaternion.identity, Vector3.one * RENDER_SCALE), mat);

    }

    private bool IsOutOfBlocks(int x, int y)
        => x < 0 || x >= renderBlocks || y < 0 || y >= renderBlocks;
}