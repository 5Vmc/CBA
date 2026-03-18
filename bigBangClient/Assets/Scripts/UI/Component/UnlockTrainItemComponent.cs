using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class UnlockTrainItemComponent : MonoBehaviour
    {
        public Image TrainItemImage;
        public Image LockImg;
        public Image TitleLight;
        public Image TitleBackground;
        public Image ProjectLight1;
        public Image ProjectLight2;
        public Image ProjectYellowLight;
        public Image ProjectYellowLight2;
        public Button CloseBtn;
        public TMP_Text TitleText;
        public TMP_Text DescText;
        public List<Image> TrainImgs = new List<Image>();
        public List<Image> MoveUpImg = new List<Image>();
        public List<Image> MoveDownImg = new List<Image>();
        public Animator UnlockAnim;

        private void Start()
        {
            //独立字体材质球
            DescText.fontMaterial = Instantiate(DescText.fontMaterial);
            DescText.fontMaterial.EnableKeyword("UNDERLAY_ON");
            DescText.fontMaterial.SetColor("_UnderlayColor", new Color(200 / 255f, 44 / 255f, 0, 128 / 255f));
            DescText.fontMaterial.SetFloat("_UnderlaySoftness", 0.12f);
        }
    }
}
