using UnityEngine;
using deVoid.UIFramework;
using UnityEngine.UI;
using TMPro;
using GameConfig;
using BigBang.Animation;
using Utils.GameItem;
using Utils;
using GameConfig.Config;
using System.Linq;
using Babu;

namespace BigBang.UI
{
    public class BasetipsUIProperties : WindowProperties
    {
        public string Name;
        public string Desc;
        public int Quality;
        public bool Actived;
        public Sprite IconSprite;
        public bool Fire;
        public int FireSection;
        public string LvTxt;

        public BasetipsUIProperties(string name, string desc, int quality, bool actived, Sprite iconSprite, bool _fire = false, int _fireSection = 0, string lvTxt = "")
        {
            Name = name;
            Desc = desc;
            Quality = quality;
            Actived = actived;
            Fire = _fire;
            FireSection = _fireSection;
            IconSprite = iconSprite;
            LvTxt = lvTxt;
        }
    }

    public class BasetipsUI : AWindowController<BasetipsUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private InventoryBaseItem iconItem;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text activedText;

        public ItemtipsUIAnim Anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            nameText.text = Properties.Name;
            descText.text = Properties.Desc;
            nameText.color = CBAColorUtil.Instance.GetColor(Properties.Quality);
            activedText.gameObject.SetActive(!Properties.Actived);

            iconItem.SetData("", "", Properties.IconSprite, Properties.Quality, Properties.Actived, false, false, Properties.Fire, Properties.FireSection);

            iconItem.SetFire(Properties.Fire);
            iconItem.SetText(Properties.LvTxt);
            Anim.PlayEnter();
        }

        private void OnClose()
        {
            // 面板关闭音效
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<BasetipsUI>();
        }
    }
}