using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class TargetPointerRenderer : MaskableGraphic
{
    private UIVertex[] uiVertices = new UIVertex[3];

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        for (var i = 0; i < 3; i++)
        {
            vh.AddVert(uiVertices[i]);
        }

        vh.AddTriangle(0, 1, 2);
    }

    public void SetVerticesPos(Vector2 pointerVec)
    {
        var orthogonalVec = new Vector2(-pointerVec.y, pointerVec.x).normalized * 50f;

        uiVertices[0].position = pointerVec;
        uiVertices[1].position = -pointerVec + orthogonalVec;
        uiVertices[2].position = -pointerVec - orthogonalVec;

        SetVerticesDirty();
    }

    public void SetVerticesColor(Color color)
    {
        uiVertices[0].color = color;
        uiVertices[1].color = uiVertices[2].color = new Color(color.r, color.g, color.b, color.a * 0.25f);

        SetVerticesDirty();
    }
    public Color GetVerticesColor() => uiVertices[0].color;
}