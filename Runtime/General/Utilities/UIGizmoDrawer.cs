using UnityEngine;

[ExecuteAlways]
public class UIGizmoDrawer : MonoBehaviour
{
    [Header("Colors")]
    public Color rectColor = Color.green;
    public Color pivotColor = Color.red;
    public Color anchorColor;

    [Header("Options")]
    public bool showLabel = false;
    public float pointSize = 25f;

    private void OnDrawGizmos()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;

        // --- Draw Rect Outline ---
        Gizmos.color = rectColor;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        for (int i = 0; i < 4; i++)
        {
            Vector3 current = corners[i];
            Vector3 next = corners[(i + 1) % 4];
            Gizmos.DrawLine(current, next);
        }

        // --- Draw Pivot ---
        Gizmos.color = pivotColor;
        Gizmos.DrawSphere(rt.position, pointSize);

        // --- Draw Anchors ---
        if (rt.parent is RectTransform parentRT)
        {
            Vector3 anchorMinWorld = WorldFromAnchor(rt, parentRT, rt.anchorMin);
            Vector3 anchorMaxWorld = WorldFromAnchor(rt, parentRT, rt.anchorMax);

            Gizmos.color = anchorColor;
            Gizmos.DrawSphere(anchorMinWorld, pointSize);
            Gizmos.DrawSphere(anchorMaxWorld, pointSize);
            Gizmos.DrawLine(anchorMinWorld, anchorMaxWorld);
        }

        // --- Label ---
#if UNITY_EDITOR
        if (showLabel)
        {
            UnityEditor.Handles.Label(rt.position + Vector3.up * 0.1f, gameObject.name);
        }
#endif
    }

    private Vector3 WorldFromAnchor(RectTransform rt, RectTransform parent, Vector2 anchor)
    {
        Vector3 local = new Vector3(
            parent.rect.xMin + parent.rect.width * anchor.x,
            parent.rect.yMin + parent.rect.height * anchor.y,
            0f
        );
        return parent.TransformPoint(local);
    }
}
