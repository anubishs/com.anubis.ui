using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(UIRoute))]
public class UIRouteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        UIRoute route = (UIRoute)target;

        GUILayout.Space(10);

        if (GUILayout.Button("➕ Add Point To Scene"))
        {
            AddPoint(route);
        }
    }

    void AddPoint(UIRoute route)
    {
        Canvas canvas = FindObjectOfType<Canvas>();

        if (!canvas)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        GameObject parent = GameObject.Find(route.name + "_Points");

        if (!parent)
        {
            parent = new GameObject(route.name + "_Points", typeof(RectTransform));
            parent.transform.SetParent(canvas.transform, false);
        }

        GameObject point = new GameObject("Point_" + route.pointNames.Count, typeof(RectTransform));
        point.transform.SetParent(parent.transform, false);

        Undo.RegisterCreatedObjectUndo(point, "Create Route Point");

        route.pointNames.Add(point.name);
        EditorUtility.SetDirty(route);

        Selection.activeGameObject = point;
    }
}