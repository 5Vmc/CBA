using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace BigBang.UI
{
    public class JerseyItem : MonoBehaviour
    {

        [HideInInspector] public Image Img;

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

        private static string sleevePath = ResourcePath.TexturePath + "JerseyTemplate/jersey__sleeve.png";
        private static string backgroundPath = ResourcePath.TexturePath + "JerseyTemplate/jersey_background.png";
        private static string maskPath = ResourcePath.TexturePath + "JerseyTemplate/jersey_mask{id}.png";
        private static string jerseyPath = ResourcePath.TexturePath + "JerseyTemplate/jersey{id}.png";
        private static string shaderNames = ResourcePath.ShaderPath + "JerseyItemShader.shader";

        private static Texture backgroundTexture;
        private static Texture sleeveTexture;

        private Shader shader;
        private StateValue jerseyValue;
        private int textureIndex = 0;
        private int backgroundColorIndex = 1;
        private int maskColorIndex = 2;

        private void Awake()
        {
            Img = GetComponent<Image>();
            Img.sprite = null;

            {
                shader = Shader.Find("Custom/JerseyItemShader");
                Img.material = new Material(shader);
            }

#if !UNITY_WEBGL
            LoadTextureSync();
#else
            LoadTextureAsync();
#endif
        }

        void LoadTextureSync()
        {
            {
                var h = YooAssets.LoadAssetSync<Texture>(backgroundPath);
                backgroundTexture = h.AssetObject as Texture;
                h.Release();
            }

            {
                var h = YooAssets.LoadAssetSync<Texture>(sleevePath);
                sleeveTexture = h.AssetObject as Texture;
                h.Release();
            }
        }

        async void LoadTextureAsync()
        {
            var materialForRendering = Img.materialForRendering;
            {
                var h = YooAssets.LoadAssetAsync<Texture>(backgroundPath);
                await h.Task;
                backgroundTexture = h.AssetObject as Texture;
                h.Release();

                materialForRendering.SetTexture("_Background", backgroundTexture);
            }

            {
                var h = YooAssets.LoadAssetAsync<Texture>(sleevePath);
                await h.Task;
                sleeveTexture = h.AssetObject as Texture;
                h.Release();

                materialForRendering.SetTexture("_Sleeve", sleeveTexture);
            }
        }

        private void OnEnable()
        {
            if (jerseyValue != null)
            {
                SetIcon(jerseyValue);
            }
        }

        private void OnDestroy()
        {
        }

        public void SetIcon(StateValue value)
        {
            jerseyValue = value;

#if !UNITY_WEBGL
            SetIconSync(value);
#else
            SetIconAsync(value);
#endif
        }

        void SetIconSync(StateValue value)
        {
            Texture maskTexture;
            Texture patternTexture;

            {
                var h = YooAssets.LoadAssetSync<Texture>(maskPath.Replace("{id}", value[textureIndex].ToString()));
                maskTexture = h.AssetObject as Texture;
                h.Release();
            }

            {
                var h = YooAssets.LoadAssetSync<Texture>(jerseyPath.Replace("{id}", value[textureIndex].ToString()));
                patternTexture = h.AssetObject as Texture;
                h.Release();
            }

            SetIconInternal(value, maskTexture, patternTexture);
        }

        async void SetIconAsync(StateValue value)
        {
            Texture maskTexture;
            Texture patternTexture;

            {
                var h = YooAssets.LoadAssetAsync<Texture>(maskPath.Replace("{id}", value[textureIndex].ToString()));
                await h.Task;
                maskTexture = h.AssetObject as Texture;
                h.Release();
            }

            {
                var h = YooAssets.LoadAssetAsync<Texture>(jerseyPath.Replace("{id}", value[textureIndex].ToString()));
                await h.Task;
                patternTexture = h.AssetObject as Texture;
                h.Release();
            }

            SetIconInternal(value, maskTexture, patternTexture);
        }

        void SetIconInternal(StateValue value, Texture maskTexture, Texture patternTexture)
        {
            var materialForRendering = Img.materialForRendering;

            materialForRendering.SetTexture("_Background", backgroundTexture);
            materialForRendering.SetTexture("_Sleeve", sleeveTexture);

            materialForRendering.SetTexture("_Mask", maskTexture);
            materialForRendering.SetTexture("_Pattern", patternTexture);

            materialForRendering.SetColor("_BackgroundColor", colors[value[backgroundColorIndex]]);
            materialForRendering.SetColor("_MaskColor", colors[value[maskColorIndex]]);
        }
    }
}