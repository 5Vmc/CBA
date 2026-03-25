using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace BigBang.UI
{
    public class ClubIconItem : MonoBehaviour
    {
        private static string framePath = ResourcePath.TexturePath + "ClubIconTemplate/Frame/frame{id}.png";
        private static string patternPath = ResourcePath.TexturePath + "ClubIconTemplate/Pattern/pattern{id}.png";
        private static string flagPath = ResourcePath.TexturePath + "ClubIconTemplate/Flag/flag{id}.png";
        private static string maskPath = ResourcePath.TexturePath + "ClubIconTemplate/Mask/mask{id}.png";
        private static string shaderNames = ResourcePath.ShaderPath + "ClubIconShader.shader";

        public static int FrameTextureIndex = 0;
        public static int PatternTextureIndex = 1;
        public static int FlagTextureIndex = 2;
        public static int Color1Index = 3;
        public static int Color2Index = 4;
        public static int Color3Index = 5;
        public static int FlagIndex = 7;
        // 标记位(从配置表读取)
        public static int FromConfig = 0;
        // 标记位(从自定义读取)
        public static int FromCustom = 1;

        private Shader shader;
        private Material material;
        private Material defaultMaterial;

        private StateValue iconValue;

        private Image img;
        public Image Img
        {
            get
            {
                if (img != null) return img;
                img = GetComponent<Image>();
                //var h = YooAssets.LoadAssetSync<Shader>(shaderNames);
                shader = Shader.Find("Custom/ClubIconShader"); //h.AssetObject as Shader;
                //h.Release();
                defaultMaterial = img.material;
                material = new Material(shader);
                return img;
            }
        }

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

        private void OnEnable()
        {
            if (iconValue != null)
            {
                SetIcon(iconValue);
            }
        }

        public void SetIcon(StateValue value)
        {
            if (value[FlagIndex] == FromConfig)
            {
                // 从配置表读取
                SetIcon(value.Value.ToString());
                return;
            }

#if !UNITY_WEBGL
            SetIconSync(value);
#else
            SetIconAsync(value);
#endif
        }

        void SetIconSync(StateValue value)
        {
            Texture frameTexture = null;
            Texture maskTexture = null;
            Texture patternTexture = null;
            Texture flagTexture = null;
            {
                var h = YooAssets.LoadAssetSync<Texture>(framePath.Replace("{id}", value[FrameTextureIndex].ToString()));
                frameTexture = h.AssetObject as Texture;
                h.Release();
            }

            {
                var h = YooAssets.LoadAssetSync<Texture>(maskPath.Replace("{id}", value[FrameTextureIndex].ToString()));
                maskTexture = h.AssetObject as Texture;
                h.Release();
            }

            {
                var h = YooAssets.LoadAssetSync<Texture>(patternPath.Replace("{id}", value[PatternTextureIndex].ToString()));
                patternTexture = h.AssetObject as Texture;
                h.Release();
            }

            {
                var h = YooAssets.LoadAssetSync<Texture>(flagPath.Replace("{id}", value[FlagTextureIndex].ToString()));
                flagTexture = h.AssetObject as Texture;
                h.Release();
            }

            SetIconInternal(value, frameTexture, maskTexture, patternTexture, flagTexture);
        }

        async void SetIconAsync(StateValue value)
        {
            try
            {
                Task[] tasks = new Task[4];

                AssetOperationHandle frameTextureH = YooAssets.LoadAssetAsync<Texture>(framePath.Replace("{id}", value[FrameTextureIndex].ToString()));
                AssetOperationHandle maskTextureH = YooAssets.LoadAssetAsync<Texture>(maskPath.Replace("{id}", value[FrameTextureIndex].ToString()));
                AssetOperationHandle patternTextureH = YooAssets.LoadAssetAsync<Texture>(patternPath.Replace("{id}", value[PatternTextureIndex].ToString()));
                AssetOperationHandle flagTextureH = YooAssets.LoadAssetAsync<Texture>(flagPath.Replace("{id}", value[FlagTextureIndex].ToString()));

                tasks[0] = frameTextureH.Task;
                tasks[1] = maskTextureH.Task;
                tasks[2] = patternTextureH.Task;
                tasks[3] = flagTextureH.Task;
                await Task.WhenAll(tasks);

                Texture frameTexture = frameTextureH.AssetObject as Texture;
                Texture maskTexture = maskTextureH.AssetObject as Texture;
                Texture patternTexture = patternTextureH.AssetObject as Texture;
                Texture flagTexture = flagTextureH.AssetObject as Texture;

                frameTextureH.Release();
                maskTextureH.Release();
                patternTextureH.Release();
                flagTextureH.Release();

                SetIconInternal(value, frameTexture, maskTexture, patternTexture, flagTexture);
            }
            catch(Exception)
            {
            }


        }

        void SetIconInternal(StateValue value, Texture frameTexture, Texture maskTexture, Texture patternTexture, Texture flagTexture)
        {
            if (frameTexture == null || maskTexture == null || patternTexture == null || flagTexture == null)
            {
                SetUnknown();
                return;
            }
            Img.sprite = null;
            Img.material = material;

            var materialForRendering = Img.materialForRendering;
            materialForRendering.SetTexture("_Frame", frameTexture);
            materialForRendering.SetColor("_FrameColor", colors[value[Color1Index]]);

            materialForRendering.SetTexture("_Mask", maskTexture);
            materialForRendering.SetColor("_MaskColor", colors[value[Color2Index]]);

            materialForRendering.SetTexture("_Pattern", patternTexture);
            materialForRendering.SetColor("_PatternColor", colors[value[Color3Index]]);

            materialForRendering.SetTexture("_Flag", flagTexture);
            materialForRendering.SetColor("_FlagColor", colors[value[Color1Index]]);
        }

        public void SetIcon(int value)
        {
            iconValue = new StateValue(value);
            SetIcon(iconValue);
        }

        // 旧版队徽显示方式
        private async void SetIcon(string id)
        {
            Img.sprite = await SpriteProxy.GetClubIcon(id);
            Img.material = defaultMaterial;
        }

        // 未知队徽
        public async void SetUnknown()
        {
            Img.sprite = await SpriteProxy.UnknownClubIcon;
            Img.material = defaultMaterial;
        }

        // 全透明图片
        public async void SetNone()
        {
            Img.sprite = await SpriteProxy.None;
            Img.material = defaultMaterial;
        }

        private void OnDestroy()
        {
        }
    }
}
