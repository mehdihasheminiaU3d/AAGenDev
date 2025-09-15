using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

[System.Serializable]
public class AssetReferenceOnlyScene : AssetReference
{
    public AssetReferenceOnlyScene(string guid) : base(guid) { }

#if UNITY_EDITOR
    // Called when user picks an asset via the object picker
    public override bool ValidateAsset(Object obj)
    {
        return obj is SceneAsset;
    }

    // Called when assigning by path (e.g., via drag & drop / addressables picker)
    public override bool ValidateAsset(string path)
    {
        // Fast check by extension, then confirm main asset type
        if (!path.EndsWith(".unity", System.StringComparison.OrdinalIgnoreCase))
            return false;

        var t = AssetDatabase.GetMainAssetTypeAtPath(path);
        return t == typeof(SceneAsset);
    }
#endif
}

[System.Serializable]
public class AssetReferenceWrapper
{
    public AssetReference m_AssetReference;
}

public class AssetReferenceMonoUser : MonoBehaviour
{
    [SerializeField]
    AssetReference m_AssetReference;

    [SerializeField]
    AssetReferenceT<Sprite> m_SpriteAssetReference;
    
    [SerializeField]
    AssetReferenceTexture2D m_TextureAssetReference;

    [SerializeField]
    AssetReferenceOnlyScene m_ReferenceOnlyScene;

    [SerializeField]
    AssetReferenceWrapper m_AssetReferenceWrapper;
}
