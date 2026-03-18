using System.Collections.Generic;
using BigBang.Animation;
using deVoid.UIFramework;
using GameConfig;
using GameConfig.Config;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    [System.Serializable]
    public class OtherPlayerTeamProperties : WindowProperties
    {
        public RankTeamInfo rankTeamInfo { get; set; }
        public bool useSelfCard { get; set; } = false;

        public OtherPlayerTeamProperties(RankTeamInfo rankTeamInfo, bool useSelfCard = false)
        {
            this.rankTeamInfo = rankTeamInfo;
            this.useSelfCard = useSelfCard;
        }
    }
    public class OtherPlayerTeamUI : AWindowController<OtherPlayerTeamProperties>
    {

        #region 初始化

        [SerializeField] private CardDetailUIAnim anim;
        [SerializeField] private Button closeBtn;

        [SerializeField] private ClubIconItem clubIcon = null;
        [SerializeField] private TMP_Text nameText = null;
        [SerializeField] private TMP_Text levelNumText = null;
        [SerializeField] private TMP_Text combatNumText = null;
        [SerializeField] private GameObject cardLineLayoutPrefab = null;
        [SerializeField] private Image cardPanel = null;
        [SerializeField] private List<OtherPlayerTeamTrainItem> trainItemList = new();
        [SerializeField] private List<TacticCardItem> DEFCardList = new();
        [SerializeField] private List<TacticCardItem> ATKCardList = new();
        [SerializeField] private ScrollRect scrollView = null;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
        }
        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
        }
        private void OnClose()
        {
            closeBtn.GetComponent<ButtonAnim>().PlayBack(() => UIController.Instance.CloseWindow<OtherPlayerTeamUI>(), playAudio: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            });
        }
        protected override void OnPropertiesSet()
        {
            RefreshTeamInfo();
            RefreshCard();
            RefreshTrain();
            SetStaticConfig();
            RefreshTactic();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollView.content as RectTransform);
            scrollView.ScroolToTop(0);
            anim.PlayEnter();
        }

        #endregion

        #region 队伍信息

        private void RefreshTeamInfo()
        {
            clubIcon.SetIcon(Properties.rankTeamInfo.TeamIcon);
            nameText.text = Properties.rankTeamInfo.TeamName;
            levelNumText.text = Properties.rankTeamInfo.Level.ToString();
            combatNumText.text = Properties.rankTeamInfo.Strength.ToString();
        }

        #endregion

        #region 卡牌信息

        private class CardLine
        {
            public GameObject LineGo = null;
            public List<CardItem> cardItemList = new();
        }
        private List<CardLine> cardLineList = new();
        private void RefreshCard()
        {
            List<PlayerCard> playerCardList = new();
            if (Properties.useSelfCard == false)
            {
                foreach (PlayerCardInfo playerCardInfo in Properties.rankTeamInfo.CourtCard)
                {
                    PlayerCard playerCard = new(playerCardInfo.CardId);
                    playerCard.UnPack(playerCardInfo);
                    playerCardList.Add(playerCard);
                }
            }
            else
            {
                Formation formation = Player.FightManager.FormationController.GetFormation(FightType.PVE);
                foreach (var item in formation.StarterBoardCardDic)
                {
                    PlayerCard playerCard = Player.CardManager.GetCard(item.Value);
                    playerCardList.Add(playerCard);
                }
            }

            int needLineCount = playerCardList.Count / 5;
            int nowLineCount = cardLineList.Count;
            for (int i = 0; i < Mathf.Max(needLineCount, nowLineCount); i++)
            {
                CardLine cardLine = null;
                if (i < nowLineCount)
                {
                    cardLine = cardLineList[i];
                }
                else
                {
                    cardLine = new();
                    cardLine.LineGo = Instantiate(cardLineLayoutPrefab, cardPanel.transform);
                    foreach (Transform transform in cardLine.LineGo.transform.GetChildren())
                    {
                        CardItem cardItem = transform.GetComponent<CardItem>();
                        cardItem.isSelf = false;
                        cardLine.cardItemList.Add(cardItem);
                    }
                    cardLineList.Add(cardLine);
                }
                if (i < needLineCount)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        int cardIndex = i * 5 + j;
                        CardItem cardItem = cardLine.cardItemList[j];
                        if (cardIndex < playerCardList.Count)
                        {
                            cardItem.SetData(playerCardList[cardIndex]);
                            cardItem.gameObject.SetActive(true);
                        }
                        else
                        {
                            cardItem.gameObject.SetActive(false);
                        }
                    }
                    cardLine.LineGo.SetActive(true);
                }
                else
                {
                    cardLine.LineGo.SetActive(false);
                }
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(cardPanel.transform as RectTransform);
        }

        #endregion

        #region 训练信息

        private async void RefreshTrain()
        {
            List<TrainConfig> trainConfigList = Configs.Train.GetConfigList();
            for (int i = 0; i < 10; i++)
            {
                TrainConfig trainConfig = trainConfigList[i];
                OtherPlayerTeamTrainItem trainItem = trainItemList[i];
                trainItem.trainTitleText.text = "{0}训练".SafeFormat(trainConfig.Name);
                if (Properties.rankTeamInfo.TrainLevels.ContainsKey(trainConfig.Id))
                {
                    trainItem.trainNumText.text = "{0}级".SafeFormat(Properties.rankTeamInfo.TrainLevels[trainConfig.Id]);
                }
                else
                {
                    trainItem.trainNumText.text = "0级";
                }
                trainItem.iconImage.sprite = await SpriteProxy.GetActivityImage("train" + trainConfig.Id);
            }
        }

        #endregion

        #region 阵型信息

        private bool isConfigInited = false;
        private void SetStaticConfig()
        {
            if (isConfigInited == true) return;
            isConfigInited = true;
            for (int i = 0; i < 5; i++)
            {
                TacticCardItem defTacticCardItem = DEFCardList[i];
                int defId = 200 + i + 1;
                TacticsConfig defTacticCfg = Configs.Tactics.GetDataDictionary()[defId];
                defTacticCardItem.SetConfig(defTacticCfg);
                defTacticCardItem.isClickToOptnTip = true;

                TacticCardItem atkTacticCardItem = ATKCardList[i];
                int atkId = 100 + i + 1;
                TacticsConfig atkTacticCfg = Configs.Tactics.GetDataDictionary()[atkId];
                atkTacticCardItem.SetConfig(atkTacticCfg);
                atkTacticCardItem.isClickToOptnTip = true;
            }
        }

        private void RefreshTactic()
        {
            for (int i = 0; i < 5; i++)
            {
                DEFCardList[i].SetData(Properties.rankTeamInfo.TacticsIdList, Properties.rankTeamInfo.TacticsLevels);
                ATKCardList[i].SetData(Properties.rankTeamInfo.TacticsIdList, Properties.rankTeamInfo.TacticsLevels);
            }
        }

        #endregion
    }
}