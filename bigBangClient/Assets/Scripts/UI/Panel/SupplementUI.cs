using UnityEngine;
using deVoid.UIFramework;
using UnityEngine.UI;
using TMPro;
using Utils.GameItem;
using GameConfig;
using frame8.Logic.Misc.Other.Extensions;
using System.Linq;
using BigBang.Animation;

namespace BigBang.UI
{
    public class SupplementUIProperties : WindowProperties
    {
        public GameItem Item { get; set; }

        public SupplementUIProperties(GameItem gameItem)
        {
            Item = gameItem;
        }

        public SupplementUIProperties(GameItemType type, int id, int count)
        {
            Item = GameItemUtils.CreateGameItem(type, id, count);
        }
    }
    public class SupplementUI : AWindowController<SupplementUIProperties>
    {
        // 关闭按钮
        [SerializeField] private Button closeBtn;
        // 道具图片
        [SerializeField] private Image propImg;
        // 道具值
        [SerializeField] private TMP_Text propText;
        // 道具描述
        [SerializeField] private TMP_Text propDescText;
        // 道具预制体
        [SerializeField] private GameObject itemPrefab;
        // 列表
        [SerializeField] private Transform content;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
        }

        protected override async void OnPropertiesSet()
        {
            // 设置道具图片
            propImg.sprite = await Properties.Item.GetIcon();
            // 设置道具描述
            propDescText.text = Properties.Item.GetDescription();
            // 设置道具值
            propText.text = string.Format("{0}(<color=#F23C29>{1}</color>/{2})", Properties.Item.GetName(), Properties.Item.GetPlayerCount().ToString(), Properties.Item.Count);
            // 设置列表
            content.GetChildren().ToList().ForEach(item => item.gameObject.SetActive(false));

            var ways = TriggerManager.Instance.GetItemDrop(Properties.Item);

            ways.Sort((a, b) =>
            {
                if (a.openlv != b.openlv)
                {
                    return a.openlv.CompareTo(b.openlv);
                }
                else
                {
                    return -a.weight.CompareTo(b.weight);
                }

            });

            // 设置获得图集
            int index = 0;
            foreach (var way in ways)
            {
                if (content.childCount <= index)
                {
                    Instantiate(itemPrefab, content);
                }
                var cardGetItem = content.GetChild(index).GetComponent<CardGetItem>();
                cardGetItem.gameObject.SetActive(true);
                cardGetItem.SetData(index, way.moduleId, way.txtmoduleName, way.txtDesc, Properties.Item.Id, Properties.Item.Count);
                index++;
            }
            // 播放动画
            GetComponent<SupplementUIAnim>().PlayEnter();
            AudioManager.Instance.PlaySound(AudioNames.BOARD_POP);
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            AudioManager.Instance.PlaySound(AudioNames.BOARD_SHUT);
            UIController.Instance.CloseWindow<SupplementUI>();
        }
    }
}