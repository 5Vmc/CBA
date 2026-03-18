using System.Collections.Generic;
using Coffee.UIEffects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class RegularTrainItemComponent : MonoBehaviour
    {
        //项目名
        public TMP_Text ProjectNameText;
        public TMP_Text ProjectValueText;
        public TMP_Text ValueText;
        //项目等级
        public TMP_Text ProjectLevelText;
        public TMP_Text UpgradeBtnText;
        public TMP_Text UpgradeText;
        public TMP_Text UnlockText;
        public TMP_Text IncomeText;
        public TMP_Text FillIncomeText;
        //升级次数图片
        public Image CountImg;
        public Image CountImgCopy;
        //训练图片
        public Image LockTrainImg;
        public RawImage TrainBackground;
        public RawImage TrainImg;
        //进度条
        public Image Progress;
        //突破进度条
        public Image BreakProgress;
        //升级按钮
        public Button UpgradeBtn;
        public Button UnlockBtn;
        public GameObject Pattern;
        public List<GameObject> ActivityGroup = new List<GameObject>();
        public List<GameObject> ActivityLeftGroup = new List<GameObject>();
        public Image LockImg;
        public Image Background;
        public Image FlashImg;
        public UIEffect Effect;
        public UIEffect OutlineEffect;
        public Image BlackBackground;
        public Image DiamondImg;
        public TMP_Text CostText;

        public Image YellowProgress;
        public Image Boxing;
        public Image BoxingYellow;
        public Image FlashBackground;
        public Image Star;
        public Button BreakThroughBtn;
        public Image UnlockBG;

        private void Start()
        {
            //独立字体材质球
            ProjectLevelText.fontMaterial = Instantiate(ProjectLevelText.fontMaterial);
            //ProjectLevelText.fontMaterial.EnableKeyword("GLOW_ON");
            ProjectLevelText.fontMaterial.SetColor("_GlowColor", Color.white);
            FillIncomeText.fontMaterial = Instantiate(FillIncomeText.fontMaterial);
            //FillIncomeText.fontMaterial.EnableKeyword("GLOW_ON");
            FillIncomeText.fontMaterial.SetColor("_GlowColor", Color.white);
        }
    }

}
