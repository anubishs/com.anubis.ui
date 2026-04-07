using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

[CustomEditor(typeof(PathManager))]
public class PathManagerEditor : Editor
{
    const string ROUTE_FOLDER = "Assets/ScriptableAssets/Routes";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("➕ Add Route"))
        {
            CreateRoute();
        }
    }

    void CreateRoute()
    {
        PathManager pm = (PathManager)target;

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(ROUTE_FOLDER))
        {
            Directory.CreateDirectory(ROUTE_FOLDER);
            AssetDatabase.Refresh();
        }

        int index = 1;
        string path;

        // Find next free Route #
        do
        {
            path = $"{ROUTE_FOLDER}/Route {index}.asset";
            index++;
        }
        while (File.Exists(path));

        // Create asset
        UIRoute route = ScriptableObject.CreateInstance<UIRoute>();
        AssetDatabase.CreateAsset(route, path);
        AssetDatabase.SaveAssets();

        route.name = Path.GetFileNameWithoutExtension(path);

        // Add to PathManager routes list
        List<UIRoute> list = new List<UIRoute>();

        if (pm.routes != null)
            list.AddRange(pm.routes);

        list.Add(route);
        pm.routes = list.ToArray();

        EditorUtility.SetDirty(pm);
        AssetDatabase.SaveAssets();

        Selection.activeObject = route;
    }
}