using System.Collections.Generic;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class BigBangPadComponent : MonoBehaviour
    {
        //当前原力
        public TMP_Text ForceText;
        //加成信息
        public TMP_Text AdditonText;
        //CD时间
        public TMP_Text CDText;
        public TMP_Text ClearBigBangCDDiamonText;
        //还需要吸收的经验数值
        public TMP_Text NeedTotalExpText;
        //按照当前产出效率还需多少时间
        public TMP_Text NeedTimeText;
        public TMP_Text CanGetForceText;
        public TMP_Text CanGetForceAddText;
        public RectTransform CdPad;
        //加速按钮
        public Button ClearCDBtn;
        //开启按钮
        public Button StartBtn;
        public Image ReadyCircle;
        public Image ReadyLine;
        public RectTransform Pad;
        public GameObject Ready;
        public GameObject NotReady;
        public TMP_Text LineText;
        public Image ClockImg;
        public TMP_Text CDTitle;
        public RectTransform TopDomain;
        public Image BallImg;
        public SkeletonGraphic BackgroundGraphic;
        public List<TMP_Text> PadText = new List<TMP_Text>();
        public Image ReadyFlashImg;
        public Image UnReadyFlashImg0;
        public Image UnReadyFlashImg1;
        public TMP_Text StartBtnText;
        public TMP_Text StartingText;
        public GameObject Progress;
        public Image ProgressValue;
        public Rigidbody2D Inner1;
        public Rigidbody2D Outter1;
        public Rigidbody2D Outter2;
        public Image WhiteBlackGround;
        public Image LightBorder;
        public ParticleSystem Explosion;
        public Image StartBackground;
        public TMP_Text TimeTitle;
        public RawImage PlayerImg;

        private void Start()
        {
            //独立字体材质球
            StartBtnText.fontMaterial = Instantiate(StartBtnText.fontMaterial);
            StartBtnText.fontMaterial.EnableKeyword("GLOW_ON");
            StartBtnText.fontMaterial.SetColor("_GlowColor", Color.white);
        }
    }
}