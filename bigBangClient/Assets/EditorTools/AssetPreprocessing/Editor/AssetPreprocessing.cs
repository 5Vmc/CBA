using UnityEditor;

public class AssetPreprocessing : AssetPostprocessor
{
    // 图片预处理
    private void OnPreprocessTexture()
    {
        TextureImporter textureImporter = assetImporter as TextureImporter;
        if (assetPath.StartsWith("Assets/LocalAsset"))
        {
            if (assetPath.StartsWith("Assets/LocalAsset/Scenes")) return;

            // 关闭读写
            // textureImporter.isReadable = false;
            // 关闭mip贴图
            textureImporter.mipmapEnabled = false;
            // 如果没有Alpha通道,则关闭Alpha通道
            if (!textureImporter.DoesSourceTextureHaveAlpha() && textureImporter.alphaSource == TextureImporterAlphaSource.FromInput)
            {
                textureImporter.alphaSource = TextureImporterAlphaSource.None;
            }

            if (assetPath.StartsWith("Assets/LocalAsset/Sprite"))
            {
                textureImporter.textureType = TextureImporterType.Sprite;
            }
            if (assetPath.StartsWith("Assets/LocalAsset/Texture"))
            {
                // TODO
            }
        }
    }
}
