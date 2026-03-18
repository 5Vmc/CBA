using Babu;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using Protocol;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class DragonYearRedEnvelopeHistoryItem : MonoBehaviour
    {
        public RedPacketLogInfo redPacketLogInfo = null;
        public void SetData(RedPacketLogInfo redPacketLogInfo)
        {
            this.redPacketLogInfo = redPacketLogInfo;
            RefreshUI();
        }

        [SerializeField] private TMP_Text dateText = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private Image propIcon = null;
        [SerializeField] private TMP_Text countText = null;

        [SerializeField] private BabuButton nameButton = null;
        [SerializeField] private BabuButton propButton = null;
        [SerializeField] private BabuButton countButton = null;

        GameItem gameItem = null;
        private async void RefreshUI()
        {
            gameItem = GameItemUtils.UnPack(redPacketLogInfo.Item);
            dateText.text = TimeUtils.ToDateTime(redPacketLogInfo.Time).ToString("MM月dd日 HH:mm");
            nameText.text = gameItem.GetName();
            propIcon.sprite = await gameItem.GetIcon();
            countText.text = "x{0}".SafeFormat(gameItem.CountString());

            LayoutRebuilder.ForceRebuildLayoutImmediate(nameText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(countText.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameText.transform.parent as RectTransform);
        }

        private void OnEnable()
        {
            nameButton.OnClick += OnClickItem;
            propButton.OnClick += OnClickItem;
            countButton.OnClick += OnClickItem;
        }
        private void OnDisable()
        {
            nameButton.OnClick -= OnClickItem;
            propButton.OnClick -= OnClickItem;
            countButton.OnClick -= OnClickItem;
        }
        private void OnClickItem(BabuButton _)
        {
            UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(gameItem));
        }
    }
}
