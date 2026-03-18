using BigBang;
using System;
using UnityEngine;
using YooAsset;

public class PlayerJersey : MonoBehaviour
{
    [SerializeField] private new SkinnedMeshRenderer renderer;

    private Texture pattern;

    private static Color[] colors = new Color[11]
    {
            new Color(223/255f, 32/255f, 36/255f,1),
            new Color(174/255f, 26/255f, 162/255f,1),
            new Color(109/255f, 20/255f, 235/255f,1),
            new Color(12/255f, 64/255f, 186/255f,1),
            new Color(0, 147/255f, 211/255f,1),
            new Color(0, 165/255f, 89/255f,1),
            new Color(117/255f, 193/255f, 24/255f,1),
            new Color(255/255f, 221/255f, 0,1),
            new Color(249/255f, 167/255f, 26/255f,1),
            Color.white,
            Color.black
    };

    private void Awake()
    {
        // 独立材质球
        renderer.materials[1] = Instantiate(renderer.materials[1]);
    }

    public void SetJersey()
    {
        // 随机队服
        //var assetPath = ResourcePath.PlayerJerseyPath + Random.Range(0, 5) + ".png";
        //var pattern = Addressables.LoadAssetAsync<Texture>(assetPath).WaitForCompletion();
        //var c1 = colors[Random.Range(0, 11)];
        //var c2 = colors[Random.Range(0, 11)];

        var c1 = colors[StateValue.GetValue(1, Player.HomeJersey)];
        var c2 = colors[StateValue.GetValue(2, Player.HomeJersey)];
        var assetPath = ResourcePath.PlayerJerseyPath + StateValue.GetValue(0, Player.HomeJersey) + ".png";
        if (pattern != null) GameObject.Destroy(pattern);

#if !UNITY_WEBGL
        LoadTextureSync(assetPath, tex =>
        {
            pattern = tex;
            // 设置图案
            renderer.materials[1].SetTexture("_Pattern", pattern);
            // 设置衣服颜色
            renderer.materials[1].SetColor("_ClothesColor", c1);
            // 设置图案颜色
            renderer.materials[1].SetColor("_PatternColor", c2);
            // 设置裤子颜色
            renderer.materials[2].SetColor("_Color", c1);
        });
#else
        LoadTextureAsync(assetPath, tex =>
        {
            pattern = tex;
            // 设置图案
            renderer.materials[1].SetTexture("_Pattern", pattern);
            // 设置衣服颜色
            renderer.materials[1].SetColor("_ClothesColor", c1);
            // 设置图案颜色
            renderer.materials[1].SetColor("_PatternColor", c2);
            // 设置裤子颜色
            renderer.materials[2].SetColor("_Color", c1);
        });
#endif
    }

    void LoadTextureSync(string path, Action<Texture> callback)
    {
        var h = YooAssets.LoadAssetSync<Texture>(path);
        var tex = h.AssetObject as Texture;
        h.Release();
        callback(tex);
    }

    void LoadTextureAsync(string path, Action<Texture> callback)
    {
        var h = YooAssets.LoadAssetAsync<Texture>(path);
        h.Completed += _ =>
        {
            var tex = h.AssetObject as Texture;
            h.Release();
            callback(tex);
        };
    }

    public static void SetAllJersey()
    {
        foreach (var item in FindObjectsOfType<PlayerJersey>())
        {
            item.SetJersey();
        }
    }
}
