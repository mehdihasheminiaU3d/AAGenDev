using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public enum GraphTopology
{
    Linear,
    Star,
    Tree,
    Cycle,
    Complete
}


public class TestNodeGeneratorWindow : EditorWindow
{
    private GraphTopology topology = GraphTopology.Linear;
    private int nodeCount = 5;
    private string folderPath = "Assets/";

    [MenuItem("Tools/AAGen/Test Node Generator")]
    public static void ShowWindow()
    {
        GetWindow<TestNodeGeneratorWindow>("Test Node Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("TestNode Graph Generator", EditorStyles.boldLabel);
        topology = (GraphTopology)EditorGUILayout.EnumPopup("Topology", topology);
        nodeCount = EditorGUILayout.IntSlider("Node Count", nodeCount, 2, 100);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("Folder", folderPath);
        if (GUILayout.Button("Select Folder", GUILayout.MaxWidth(120)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder to Save TestNodes", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                string relativePath = "Assets" + selectedPath.Replace(Application.dataPath, "").Replace("\\", "/");
                if (relativePath.StartsWith("Assets/") && IsFolderEmpty(relativePath))
                {
                    folderPath = relativePath;
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Folder", "Please select an **empty folder** inside the Assets folder.", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate"))
        {
            GenerateGraph();
        }
    }

    private void GenerateGraph()
    {
        if (!AssetDatabase.IsValidFolder(folderPath) || !IsFolderEmpty(folderPath))
        {
            EditorUtility.DisplayDialog("Invalid Folder", "Please select an empty folder inside the Assets directory.", "OK");
            return;
        }

        List<TestNode> nodes = CreateTestNodes(nodeCount);

        switch (topology)
        {
            case GraphTopology.Linear:
                GenerateLinearTopology(nodes);
                break;
            case GraphTopology.Star:
                GenerateStarTopology(nodes);
                break;
            case GraphTopology.Tree:
                GenerateTreeTopology(nodes);
                break;
            case GraphTopology.Cycle:
                GenerateCycleTopology(nodes);
                break;
            case GraphTopology.Complete:
                GenerateCompleteTopology(nodes);
                break;
        }

        foreach (var node in nodes)
        {
            EditorUtility.SetDirty(node);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", $"Generated {nodeCount} nodes with {topology} topology.", "OK");
    }

    private List<TestNode> CreateTestNodes(int count)
    {
        List<TestNode> nodes = new List<TestNode>();
        for (int i = 0; i < count; i++)
        {
            var node = ScriptableObject.CreateInstance<TestNode>();
            node.name = $"TestNode_{i}";
            node.Neighbors = new List<TestNode>();
            string path = $"{folderPath}/TestNode_{i}.asset";
            AssetDatabase.CreateAsset(node, path);
            nodes.Add(node);
        }
        return nodes;
    }

    private void GenerateLinearTopology(List<TestNode> nodes)
    {
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            nodes[i].Neighbors.Add(nodes[i + 1]);
        }
    }

    private void GenerateStarTopology(List<TestNode> nodes)
    {
        var center = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            center.Neighbors.Add(nodes[i]);
        }
    }

    private void GenerateTreeTopology(List<TestNode> nodes)
    {
        for (int i = 1; i < nodes.Count; i++)
        {
            int parentIndex = (i - 1) / 2;
            nodes[parentIndex].Neighbors.Add(nodes[i]);
        }
    }

    private void GenerateCycleTopology(List<TestNode> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Neighbors.Add(nodes[(i + 1) % nodes.Count]);
        }
    }

    private void GenerateCompleteTopology(List<TestNode> nodes)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i != j)
                    nodes[i].Neighbors.Add(nodes[j]);
            }
        }
    }

    private bool IsFolderEmpty(string folder)
    {
        var assetPaths = AssetDatabase.FindAssets("", new[] { folder });
        return assetPaths == null || assetPaths.Length == 0;
    }
}
