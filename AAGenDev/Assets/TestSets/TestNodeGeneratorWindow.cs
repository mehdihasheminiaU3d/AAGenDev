using System;
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
    Complete,
    SharedDependency1,
    Looped
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

        var nodes = topology switch
        {
            GraphTopology.Linear => GenerateLinearTopology(),
            GraphTopology.Star => GenerateStarTopology(),
            GraphTopology.Tree => GenerateTreeTopology(),
            GraphTopology.Cycle => GenerateCycleTopology(),
            GraphTopology.Complete => GenerateCompleteTopology(),
            GraphTopology.SharedDependency1 => GenerateSharedDependencyTopology1(),
            GraphTopology.Looped => GenerateLoopedTopology1(),
            _ => throw new Exception($"Unexpected enum1")
        };

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

    List<TestNode> GenerateLinearTopology()
    {
        var nodes = CreateTestNodes(nodeCount);
        
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            nodes[i].Neighbors.Add(nodes[i + 1]);
        }

        return nodes;
    }

    List<TestNode> GenerateStarTopology()
    {
        var nodes = CreateTestNodes(nodeCount);
        
        var center = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            center.Neighbors.Add(nodes[i]);
        }
        return nodes;
    }

    List<TestNode> GenerateTreeTopology()
    {
        var nodes = CreateTestNodes(nodeCount);
        
        for (int i = 1; i < nodes.Count; i++)
        {
            int parentIndex = (i - 1) / 2;
            nodes[parentIndex].Neighbors.Add(nodes[i]);
        }
        return nodes;
    }

    List<TestNode> GenerateCycleTopology()
    {
        var nodes = CreateTestNodes(nodeCount);
        
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Neighbors.Add(nodes[(i + 1) % nodes.Count]);
        }
        return nodes;
    }

    List<TestNode> GenerateCompleteTopology()
    {
        var nodes = CreateTestNodes(nodeCount);
        
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = 0; j < nodes.Count; j++)
            {
                if (i != j)
                    nodes[i].Neighbors.Add(nodes[j]);
            }
        }
        return nodes;
    }
    
    List<TestNode> GenerateSharedDependencyTopology1()
    {
        var nodes = CreateTestNodes(10);
        
        //first common sources
        nodes[0].ConnectTo(nodes[1]);
        nodes[7].ConnectTo(nodes[1]);
        
        //first shared dependency
        nodes[1].ConnectTo(nodes[2]);
        nodes[1].ConnectTo(nodes[3]);
        nodes[2].ConnectTo(nodes[4]);
        nodes[3].ConnectTo(nodes[4]);
        
        //second shared dependency
        nodes[4].ConnectTo(nodes[5]);
        nodes[5].ConnectTo(nodes[6]);
        
        //second common sources
        nodes[8].ConnectTo(nodes[9]);
        nodes[9].ConnectTo(nodes[5]);
        
        return nodes;
    }
    
    List<TestNode> GenerateLoopedTopology1()
    {
        var nodes = CreateTestNodes(11);
      
        //source of the first branch
        nodes[0].ConnectTo(nodes[1]);
        
        //first loop
        nodes[1].ConnectTo(nodes[2]);
        nodes[2].ConnectTo(nodes[3]);
        nodes[3].ConnectTo(nodes[4]);
        nodes[4].ConnectTo(nodes[1]);
        
        //source of second branch
        nodes[5].ConnectTo(nodes[6]);
        nodes[6].ConnectTo(nodes[7]);
        
        //second loop
        nodes[7].ConnectTo(nodes[10]);
        nodes[10].ConnectTo(nodes[9]);
        nodes[9].ConnectTo(nodes[8]);
        nodes[8].ConnectTo(nodes[7]);
        
        //connect two branches
        nodes[10].ConnectTo(nodes[4]);
        
        return nodes;
    }

    private bool IsFolderEmpty(string folder)
    {
        var assetPaths = AssetDatabase.FindAssets("", new[] { folder });
        return assetPaths == null || assetPaths.Length == 0;
    }
}
