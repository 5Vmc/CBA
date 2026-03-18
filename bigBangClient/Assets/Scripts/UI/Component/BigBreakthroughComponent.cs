using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class BigBreakthroughComponent : MonoBehaviour
    {
        public TMP_Text NameText;
        public TMP_Text LevelText;
        public Button CloseBtn;
        public RectTransform Boxing;
        public GameObject Title;
        public Image BlackImg;
        public Image Background;
        public List<TMP_Text> Txts;
        public Image Blur;
        public Image YellowBoxing;
        public Image HighlightImage;
        public Image Ghost1;
        public Image Ghost2;
        public Image FlashBoard;
        public ParticleSystem Particle;
        public Image Image;
        public Image Image1;
        public TMP_Text NameText1;
        public List<TMP_Text> Txts1;
        public List<GameObject> TxtsGroup;
        public GameObject Texts1;

        private void Start()
        {
            //独立字体材质球
            foreach (var item in Txts)
            {
                item.fontMaterial = Instantiate(item.fontMaterial);
                float factor = Mathf.Pow(2, 0.5f);
                item.fontMaterial.SetColor("",
                    new Color(191 / 255f * factor, 81 / 255f * factor, 0, 81 / 255f * factor));
                item.fontMaterial.SetVector("_GlowColor", new Vector4(191 / 255f, 81 / 255f, 0, 0.5f));
                item.fontMaterial.SetFloat("_GlowOffset", 0.04f);
                item.fontMaterial.SetFloat("_GlowInner", 1);
                item.fontMaterial.SetFloat("_GlowOuter", 1);
                item.fontMaterial.SetFloat("_GlowPower", 0.5f);
            }
        }
    }
}
