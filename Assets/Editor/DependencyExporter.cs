using System.Linq;
using UnityEditor;
using UnityEngine;

public static class DependencyExporter
{
    [MenuItem("Tools/Export Dependency")]
    private static void Export()
    {
        var list = AssetDatabase.GetAllAssetPaths()
            .Where(path => path.StartsWith("Assets/"))
            .Where(path => !path.StartsWith("Assets/Plugins/"))
            .Where(path => !path.StartsWith("Assets/Editor/"))
            .Where(path => !path.EndsWith(".png"))
            .Where(path => !path.EndsWith(".ogg"))
            .Where(path => !AssetDatabase.IsValidFolder(path))
            .Where(path => AssetDatabase.GetDependencies(path, false).FirstOrDefault() == null)
            .OrderBy(path => path)
            .ToArray();

        var result = string.Join("\n", list);

        Debug.Log(result);

        EditorGUIUtility.systemCopyBuffer = result;
    }
}
