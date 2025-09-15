// FolderToJsonExporter.cs
// Simple Unity Editor tool: pick a folder, gather all assets in it, and export a JSON array of asset paths.
// Place this script anywhere inside an Editor/ folder in your project.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class FolderToJsonExporterWindow : EditorWindow
{
    [SerializeField] private DefaultAsset inputFolder; // Drag a folder from Project here
    [SerializeField] private bool includeSubfolders = true;

    private string _lastStatus = string.Empty;

    [MenuItem("Tools/Folder → JSON Asset List")] 
    public static void ShowWindow()
    {
        var window = GetWindow<FolderToJsonExporterWindow>(true, "Folder → JSON Asset List");
        window.minSize = new Vector2(460, 180);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Export assets in a folder to JSON", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            inputFolder = (DefaultAsset)EditorGUILayout.ObjectField(new GUIContent("Input Folder", "Project folder to scan"), inputFolder, typeof(DefaultAsset), false);
            includeSubfolders = EditorGUILayout.Toggle(new GUIContent("Include Subfolders"), includeSubfolders);

            string folderPath = inputFolder != null ? AssetDatabase.GetAssetPath(inputFolder) : string.Empty;
            bool folderValid = !string.IsNullOrEmpty(folderPath) && AssetDatabase.IsValidFolder(folderPath);

            EditorGUILayout.HelpBox(folderValid ? $"Folder: {folderPath}" : "Drag a Project folder here.", folderValid ? MessageType.Info : MessageType.Warning);

            EditorGUI.BeginDisabledGroup(!folderValid);
            if (GUILayout.Button("Generate JSON"))
            {
                GenerateJson(folderPath, includeSubfolders);
            }
            EditorGUI.EndDisabledGroup();
        }

        if (!string.IsNullOrEmpty(_lastStatus))
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(_lastStatus, MessageType.None);
        }
    }

    private void GenerateJson(string folderPath, bool recursive)
    {
        try
        {
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                EditorUtility.DisplayDialog("Invalid Folder", "Please select a valid Project folder.", "OK");
                return;
            }

            // Ask for output location
            string defaultName = Path.GetFileName(folderPath).Replace(' ', '_') + "_assets.json";
            string savePath = EditorUtility.SaveFilePanel("Save JSON", Application.dataPath, defaultName, "json");
            if (string.IsNullOrEmpty(savePath))
            {
                _lastStatus = "Export canceled.";
                return;
            }

            // Collect GUIDs for assets within the folder
            // Empty search filter returns all asset types. Path filter restricts to the chosen folder.
            var searchFolders = new[] { folderPath };
            string filter = string.Empty; // all assets
            string[] guids = AssetDatabase.FindAssets(filter, searchFolders);

            var paths = new List<string>(guids.Length);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                    continue;

                // Skip folders themselves
                if (AssetDatabase.IsValidFolder(path))
                    continue;

                // Ensure the asset is inside the selected folder (FindAssets respects the folder, but keep guard)
                if (!path.StartsWith(folderPath, StringComparison.Ordinal))
                    continue;

                // Skip .meta files just in case
                if (path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                    continue;

                // If not including subfolders, enforce direct child check
                if (!recursive)
                {
                    // Expect paths like "Folder/Child.ext" -> only one additional segment
                    var remaining = path.Substring(folderPath.Length).TrimStart('/');
                    if (remaining.Contains('/'))
                        continue;
                }

                // Normalize slashes
                path = path.Replace('\\', '/');
                paths.Add(path);
            }

            // Stable order for diffs
            paths.Sort(StringComparer.Ordinal);

            // Build a JSON array of strings without needing a wrapper type
            // Example:
            // [
            //   "Assets/Some.asset",
            //   "Assets/Other.prefab"
            // ]
            string json = BuildStringArrayJson(paths);

            // Write UTF-8 (no BOM)
            File.WriteAllText(savePath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            AssetDatabase.Refresh();

            _lastStatus = $"Exported {paths.Count} asset path(s) to: {savePath}";
            Debug.Log(_lastStatus);

            if (EditorUtility.DisplayDialog("JSON Exported", _lastStatus + "\n\nOpen location?", "Open", "Close"))
            {
                EditorUtility.RevealInFinder(savePath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("FolderToJsonExporter failed: " + ex);
            EditorUtility.DisplayDialog("Error", "Export failed. Check the Console for details.\n\n" + ex.Message, "OK");
            _lastStatus = "Export failed: " + ex.Message;
        }
    }

    private static string BuildStringArrayJson(IList<string> items)
    {
        // Escape and pretty-print
        var sb = new StringBuilder();
        sb.Append('[').Append('\n');
        for (int i = 0; i < items.Count; i++)
        {
            string escaped = EscapeForJson(items[i]);
            sb.Append("  \"").Append(escaped).Append("\"");
            if (i < items.Count - 1) sb.Append(',');
            sb.Append('\n');
        }
        sb.Append(']');
        return sb.ToString();
    }

    private static string EscapeForJson(string s)
    {
        // Minimal JSON string escape (sufficient for Unity asset paths)
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 32)
                    {
                        sb.Append("\\u").Append(((int)c).ToString("X4"));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }
}
