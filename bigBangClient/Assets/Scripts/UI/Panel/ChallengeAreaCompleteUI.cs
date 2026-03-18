using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using Babu.Client.Fsm;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;
using static BigBang.ClassicManager;
using GameItem = Utils.GameItem.GameItem;

namespace BigBang.UI
{
    public class ChallengeAreaCompleteUIProperties : WindowProperties
    {
        public int challengeId;
        public ClassicTeamData teamData;
        public int nextMapId;
        public Action AfterCloseCallBack;

        public ChallengeAreaCompleteUIProperties(ClassicTeamData _teamData, Action AfterCloseCallBack)
        {
            this.teamData = _teamData;
            this.AfterCloseCallBack = AfterCloseCallBack;
        }
    }

    public class ChallengeAreaCompleteUI : AWindowController<ChallengeAreaCompleteUIProperties>
    {
        [SerializeField] private Button goBtn;
        [SerializeField] private TMP_Text areaDesText;
        [SerializeField] private TMP_Text areaDes1Text;
        [SerializeField] private RectTransform areaMiddle;
        [SerializeField] private RectTransform areaLeft;
        [SerializeField] private RectTransform areaRight;
        [SerializeField] private TMP_Text areaStageClearText;
        [SerializeField] private TMP_Text goBtnTxt;

        [SerializeField] private GameObject area;
        [SerializeField] private GameObject global;

        [SerializeField] public ChallengeAreaCompleteUIAnim Anim;

        private string sourceGoBtnTxt;
        private string sourceDes2Txt;
        private string sourceDes1Txt;

        protected override void Awake()
        {
            base.Awake();
            sourceGoBtnTxt = goBtnTxt.text;
            sourceDes2Txt = areaDesText.text;
            sourceDes1Txt = areaDes1Text.text;
        }

        protected override void AddListeners()
        {
            goBtn.onClick.AddListener(OnGo);
        }

        protected override void RemoveListeners()
        {
            goBtn.onClick.RemoveListener(OnGo);
        }

        protected override void OnPropertiesSet()
        {
            int countryId = Properties.teamData.challengeClubConfig.Country;
            ClassicCountryLevelData countryData = ClassicManager.Instance.classicCountryLevelDataDic[countryId];
            int mapid = countryData.challengeCountryConfig.Map;

            var currentMapCfg = Configs.ChallengeMap.GetConfig(mapid);
            areaDesText.text = sourceDes2Txt.Replace("{value1}", countryData.challengeCountryConfig.Name);
            areaDes1Text.text = sourceDes1Txt.Replace("{value1}", currentMapCfg.Name);
            if (currentMapCfg.Next > 0)
            {
                //var nextCfg = Configs.ChallengeMap.GetConfig(currentMapCfg.Next);
                //goBtnTxt.text = sourceGoBtnTxt.Replace("{value}", nextCfg.Name);
                goBtnTxt.text = "继续挑战";
                WorldMapUI.getNewCountry = true;
            }
            else
            {
                goBtnTxt.text = "关闭";
                WorldMapUI.getNewCountry = false;
            }

            ChapterInfo chapterInfo = ClassicManager.Instance.chapterInfoDic[countryId];
            ChallengeCountryConfig challengeCountryConfig = Configs.ChallengeCountry.GetConfig(chapterInfo.Id);
            List<string> rewardStrList = new();
            if (chapterInfo.Rewards[0] == 0 && chapterInfo.Star >= challengeCountryConfig.Star1) rewardStrList.Add(challengeCountryConfig.Reward1);
            if (chapterInfo.Rewards[1] == 0 && chapterInfo.Star >= challengeCountryConfig.Star2) rewardStrList.Add(challengeCountryConfig.Reward2);
            if (chapterInfo.Rewards[2] == 0 && chapterInfo.Star >= challengeCountryConfig.Star3) rewardStrList.Add(challengeCountryConfig.Reward3);
            bool hasRewards = rewardStrList.Count > 0;
            if (hasRewards)
            {
                SetRewards(rewardStrList, starRewardLayout);
                NetworkManager.Instance.CollectChapterBoxReward(countryId, -1, response =>
                {
                    if (chapterInfo.Rewards[0] == 0 && chapterInfo.Star >= challengeCountryConfig.Star1) chapterInfo.Rewards[0] = 1;
                    if (chapterInfo.Rewards[1] == 0 && chapterInfo.Star >= challengeCountryConfig.Star2) chapterInfo.Rewards[1] = 1;
                    if (chapterInfo.Rewards[2] == 0 && chapterInfo.Star >= challengeCountryConfig.Star3) chapterInfo.Rewards[2] = 1;
                    string redDotPath = PanelNodePath.Home_ClassicPVE + "/" + challengeCountryConfig.Level.ToString() + "/" + challengeCountryConfig.Map.ToString() + "/" + countryId.ToString() + "/box";
                    RedDotNode node = RedDotManager.Instance.ConfirmNode(redDotPath, "");
                    //通关领了以后当时是一定没有小红点了
                    node.AddValue(-1);
                    EventManager.Instance.Dispatch(EventID.RefreshUIRedDot);
                });
            }
            starBox.gameObject.SetActive(hasRewards);

            Anim.PlayEnter(hasRewards);
        }

        protected void OnGo()
        {
            Properties.AfterCloseCallBack?.Invoke();
            UIController.Instance.CloseWindow<ChallengeAreaCompleteUI>();
            if (WorldMapUI.getNewCountry == true)
            {
                Player.BattleManager.showCounties = false;
                UIController.Instance.HidePanel<ClassicCountryUI>();
                Babu.EventManager.Instance.Dispatch(EventID.OnNewCountry);
            }
        }

        [SerializeField] private Image starBox = null;
        [SerializeField] private HorizontalAdapter starRewardLayout;
        [SerializeField] private GameObject itemPrefab;
        private void SetRewards(List<string> rewardStrList, HorizontalAdapter layout)
        {
            Transform layoutTrans = layout.transform;
            List<GameItem> gameItemList = new();
            foreach (var rewardStr in rewardStrList)
            {
                gameItemList.AddRange(GameItemUtils.CreateGameItems(rewardStr).ToList());
            }
            while (layoutTrans.childCount < gameItemList.Count) Instantiate(itemPrefab, layoutTrans);
            for (int i = 0; i < layoutTrans.childCount; i++)
            {
                if (i < gameItemList.Count)
                {
                    var reward = gameItemList[i];
                    var child = layoutTrans.GetChild(i);
                    child.gameObject.SetActive(true);
                    child.GetComponent<InventoryItem>().SetData(reward);
                }
                else
                {
                    layoutTrans.GetChild(i).gameObject.SetActive(false);
                }
            }
            layout.Calculate();
        }
    }
}
