using System;
using Babu;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;

namespace BigBang.UI
{
    public class InventoryBaseItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image imgHightLight;
        [SerializeField] public Image imgDisable;
        [SerializeField] public Image imgQuality;
        [SerializeField] public Image imgFire;
        [SerializeField] private Button selfBtn;
        [SerializeField] private TMP_Text txtAdd;
        [SerializeField] private TMP_Text txtFire;

        public bool OpenTips = false;

        private string Name;
        private string Desc;
        private int Quality;
        private Sprite iconSprite;
        private bool actived;
        private bool fire;
        private int fireSection;

        private void OnEnable()
        {
            selfBtn?.onClick.AddListener(OnOpenTips);
        }

        private void OnDisable()
        {
            selfBtn?.onClick.AddListener(OnOpenTips);
        }

        private void OnOpenTips()
        {
            if (!OpenTips) return;
            UIController.Instance.OpenWindow<BasetipsUI>(new BasetipsUIProperties(Name, Desc, Quality, actived, iconSprite, fire, fireSection, txtAdd.text));
        }

        public async void SetData(string name, string desc, Sprite iconsp, int quality = 4, bool _actived = true, bool highlight = false, bool tips = true, bool _fire = false, int _fireSection = 0)
        {
            Name = name;
            Desc = desc;
            Quality = quality;
            iconSprite = iconsp;
            icon.sprite = iconSprite;
            actived = _actived;
            imgHightLight.gameObject.SetActive(highlight);
            imgQuality.sprite = await SpriteProxy.GetInvetoryQuality(quality);
            OpenTips = tips;
            fire = _fire;
            fireSection = _fireSection;
            SetFire(fire);
            SetActive(actived);
            SetFireTxt(fireSection == 0 ? "" : fireSection.ToString());
        }

        public void SetActive(bool actived)
        {
            imgDisable.gameObject.SetActive(!actived);
            if (actived)
                SpriteManager.GetSprite(AtlasNames.Formation, "火图标", s => imgFire.sprite = s);
            else
                SpriteManager.GetSprite(AtlasNames.Formation, "火图标灰", s => imgFire.sprite = s);
        }

        public void SetFire(bool value)
        {
            imgFire.gameObject.SetActive(value);
        }

        public void SetFireTxt(string value)
        {
            txtFire.text = value;
        }

        /// <summary>
        /// 设置高亮边框
        /// </summary>
        /// <param name="highlight"></param>
        public void SetHightLight(bool highlight)
        {
            imgHightLight.gameObject.SetActive(highlight);
        }

        /// <summary>
        /// 设置启用/禁用状态
        /// </summary>
        /// <param name="enable"></param>
        public void setEnable(bool enable)
        {
            actived = enable;
            imgDisable.gameObject.SetActive(!enable);
        }

        /// <summary>
        /// 设置品质色背景
        /// </summary>
        /// <param name="quality"></param>
        public async void SetQuality(int quality)
        {
            imgQuality.sprite = await SpriteProxy.GetInvetoryQuality(quality);
        }

        public void SetText(string txt)
        {
            txtAdd.text = txt;
        }
    }
}