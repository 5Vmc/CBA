using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class PlayerHeadPad : MonoBehaviour
    {
        [SerializeField] private Button settingUIBtn;//头像设置按钮
        [SerializeField] private ProgressItem progressBar;
        [SerializeField] private TMP_Text txtName;
        [SerializeField] private TMP_Text txtLevel;
        [SerializeField] private ImageFont txtFightPoint;
        [SerializeField] private ClubIconItem clubIcon;//玩家俱乐部图标


        [SerializeField] private BabuButton popButton = null;
        [SerializeField] private RectTransform popPosition = null;

        [SerializeField] private bool isNeedAnim = true;

        protected void OnEnable()
        {
            settingUIBtn.onClick.AddListener(OnSettingUI);
            EventManager.Instance.Register(EventID.OnPlayerHeadChange, SetData);
            popButton.OnClick += OnClickPopButton;
            SetData(true);
        }


        protected void OnDisable()
        {
            settingUIBtn.onClick.RemoveListener(OnSettingUI);
            EventManager.Instance.Unregister(EventID.OnPlayerHeadChange, SetData);
            popButton.OnClick -= OnClickPopButton;
        }

        private void OnClickPopButton(BabuButton button)
        {
            int[] expList = Player.GetExpNum(Player.Level, Player.Exp);
            UIController.Instance.OpenWindow<PoptipsUI>(new PoptipsUIProperties("{0}<color=#fbf17b>/{1}".SafeFormat(expList[0], expList[1]), popPosition, Vector3.zero, TextAlignmentOptions.Midline));
        }

        public void SetData(object[] args)
        {
            SetData(false);
        }
        public void SetData(bool isFirst)
        {
            txtName.text = Player.Name;
            txtLevel.text = Player.Level.ToString();
            clubIcon.SetIcon(Player.Icon);
            txtFightPoint.text = Player.Strength.ToString();
            if (isFirst) progressBar.Anim.Init();
            progressBar.SetData(Player.ExpProgress, !isFirst || isNeedAnim);
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.001f);
            seq.AppendCallback(() => { ForceRebuildLayout(); });
            seq.AddTo(this.gameObject);
        }

        private void OnSettingUI()
        {
            //打开设置界面
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.OpenWindow<SettingsUI>();
        }

        [SerializeField] private HorizontalLayoutGroup nameLayout = null;
        [SerializeField] private HorizontalLayoutGroup expLayout = null;
        private void ForceRebuildLayout()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtLevel.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(txtName.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(expLayout.transform as RectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(nameLayout.transform as RectTransform);
        }
    }
}