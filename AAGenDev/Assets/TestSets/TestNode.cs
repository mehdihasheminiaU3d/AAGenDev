using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create TestNode", fileName = "TestNode")]
public class TestNode : ScriptableObject
{
    public List<TestNode> Neighbors;
    
#if UNITY_EDITOR
    [ContextMenu("Validate Node")]
    void Validate()
    {
        // Check for self-reference
        if (Neighbors.Contains(this))
            Debug.LogError($"{name} has a self-reference.");
    }
#endif
}
