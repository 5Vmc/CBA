using System.Collections.Generic;
using System.Linq;
using Babu.Client.Fsm;
using BigBang.Animation;
using BigBang.Battle;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Spine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.Battle.ShootUI;

namespace BigBang.UI
{
    public class ShootEndRewardUIProperties : WindowProperties
    {
        public ShootEndData shootEndData;
        public ShootEndRewardUIProperties(ShootEndData shootEndData)
        {
            this.shootEndData = shootEndData;
        }
    }
    public class ShootEndRewardUI : AWindowController<ShootEndRewardUIProperties>
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private ImageFont nowLevelNumImageFont = null;
        [SerializeField] private GameObject shootEndItemPrefab = null;
        [SerializeField] private RectTransform content = null;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeButton.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeButton.onClick.RemoveListener(OnClose);
        }

        private int oldLevel = 0;
        private int newLevel = 0;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();

            TouchManager.Instance.EnableTouch();

            SetDataOnce();

            oldLevel = ScoreToLevel(Properties.shootEndData.oldScore);
            newLevel = ScoreToLevel(Properties.shootEndData.newScore);

            nowLevelNumImageFont.text = newLevel.ToString();

            RefreshData();

            PlayEnterAnim();
        }

        private List<ShootEndItem> shootEndItemList = new();
        private bool isSetDataOnce = false;
        private void SetDataOnce()
        {
            if (isSetDataOnce == true) return;
            isSetDataOnce = true;
            for (int i = 0; i < Configs.ShootGameStage.GetConfigList().Count; i++)
            {
                GameObject shootEndItemGameObject = GameObject.Instantiate(shootEndItemPrefab, content);
                ShootEndItem shootEndItem = shootEndItemGameObject.GetComponent<ShootEndItem>();
                shootEndItemList.Add(shootEndItem);
                shootEndItem.SetData(Configs.ShootGameStage.GetConfigList()[i]);
                shootEndItem.gameObject.SetActive(string.IsNullOrWhiteSpace(shootEndItem.shootGameStageConfig.Reward) == false);
            }
        }

        private void RefreshData()
        {
            foreach (ShootEndItem shootEndItem in shootEndItemList)
            {
                shootEndItem.RefreshInfo(oldLevel, newLevel);
            }
        }

        private int ScoreToLevel(int score)
        {
            if (score <= 0) return 0;
            foreach (var item in Configs.ShootGameStage.GetConfigList())
            {
                if (score < item.Point) return item.Id;
            }
            return Configs.ShootGameStage.GetConfigList()[^1].Id;
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);

            List<GameItem> gameItemList = new();
            foreach (ShootGameStageConfig shootGameStageConfig in Configs.ShootGameStage.GetConfigList())
            {
                if (newLevel > oldLevel && shootGameStageConfig.Id > oldLevel && shootGameStageConfig.Id <= newLevel)
                {
                    if (string.IsNullOrEmpty(shootGameStageConfig.Reward) == false)
                    {
                        gameItemList.AddRange(GameItemUtils.CreateGameItems(shootGameStageConfig.Reward));
                    }
                }
            }

            if (gameItemList.Count <= 0)
            {
                UIController.Instance.CloseWindow<ShootEndRewardUI>();
                GoToOut(Properties.shootEndData.ShootUIEnterPos);
            }
            else
            {
                UIController.Instance.CloseWindow<ShootEndRewardUI>();
                UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(gameItemList, () =>
                {
                    GoToOut(Properties.shootEndData.ShootUIEnterPos);
                }));
            }
        }
        public static void GoToOut(ShootUIEnterPos shootUIEnterPos)
        {
            FsmManager.Instance.ChangeToState<StateHome>(new StateCommonUserData()
            {
                OpenUIAction = async () =>
                {
                    if (ActivityController.Instance.IsActivityOpen(ActivityID.NewYearChallenge) && TriggerManager.Instance.CheckModuleOpen(TriggerModuleType.Activity) && shootUIEnterPos == ShootUIEnterPos.Jump)
                    {
                        await UIController.Instance.ShowPanel<HomeUI>();
                        await UIController.Instance.ShowPanel<NewYearHomeUI>();
                    }
                    else
                    {
                        await UIController.Instance.ShowPanel<HomeUI>();
                    }
                }
            });
        }

        [SerializeField] private Image titleImage = null;
        [SerializeField] private RectTransform nowLevelPanel = null;
        [SerializeField] private TMP_Text closeTipText = null;

        private Sequence seq = null;
        private void PlayEnterAnim()
        {
            ClearEnterAnim();
            seq = DOTween.Sequence();
            seq.AddTo(this.gameObject);

            List<GameObject> fadeList = new();
            fadeList.Add(titleImage.gameObject);
            fadeList.Add(nowLevelPanel.gameObject);
            foreach (ShootEndItem shootEndItem in shootEndItemList)
            {
                fadeList.Add(shootEndItem.gameObject);
            }
            fadeList.Add(closeTipText.gameObject);

            foreach (var fadeItem in fadeList)
            {
                if (fadeItem.activeSelf == false) continue;
                fadeItem.SetAlpha(0);
                fadeItem.transform.SetLocalScaleY(0);
            }

            foreach (var fadeItem in fadeList)
            {
                if (fadeItem.activeSelf == false) continue;
                seq.AppendCallback(() => { AudioManager.Instance.PlaySound(AudioNames.ENT_FLOP); });
                seq.Append(fadeItem.DOFade(1, 0.08f));
                seq.Join(fadeItem.transform.DOScaleY(1, 0.08f).SetEase(Ease.OutBack));
            }
        }
        private void ClearEnterAnim()
        {
            seq?.Kill();
            seq = null;
        }

    }
}