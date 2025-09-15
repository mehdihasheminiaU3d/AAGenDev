using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Test Sets/" + nameof(AssetReferenceSoUser))]
public class AssetReferenceSoUser : ScriptableObject
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
