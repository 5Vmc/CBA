using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    class IntPair
    {
        public int gid;
        public int count;
    }
    public class CardFirePad : AWindowController<WindowProperties>
    {
        [SerializeField] private Button quickSelBtn;
        [SerializeField] private Button clearAllBtn;

        public CardFirePadAnim anim;
        [SerializeField] private Button fireBtn;
        [SerializeField] private TMP_Text textSelected;

        [SerializeField] private CardFireGridAdapter osa;
        [SerializeField] private Button closeBtn;
        [SerializeField] private BabuToggleGroup toggleGroup;

        [SerializeField] private InventoryItem inventoryItem;
        [SerializeField] private GameObject box;


        private Dictionary<int, GameItem> returnItemDict = new Dictionary<int, GameItem>();
        private Dictionary<int, int> selectedCardDict = new Dictionary<int, int>();

        readonly int SEL_MAX = 12;
        private static CardFirePad _inst;

        public static CardFirePad Inst
        {
            get { return _inst; }
            private set { }
        }

        protected override void Awake()
        {
            base.Awake();
            //this.Anim = GetComponent<ArenaPadAnim>();
            _inst = this;
        }

        protected void OnEnable()
        {
            quickSelBtn.onClick.AddListener(OnClickQuickSelBtn);
            fireBtn.onClick.AddListener(OnClickFireBtn);
            clearAllBtn.onClick.AddListener(OnClickClearAllBtn);
            closeBtn.onClick.AddListener(OnCloseBtn);

            toggleGroup.OnValueChanged += ToggleGroup_OnValueChanged;
            EventManager.Instance.Register(EventID.OnClickWillFireMe, OnClickWillFireMe);
        }
        protected void OnDisable()
        {
            quickSelBtn.onClick.RemoveListener(OnClickQuickSelBtn);
            fireBtn.onClick.RemoveListener(OnClickFireBtn);
            clearAllBtn.onClick.RemoveListener(OnClickClearAllBtn);
            closeBtn.onClick.RemoveListener(OnCloseBtn);

            toggleGroup.OnValueChanged -= ToggleGroup_OnValueChanged;
            EventManager.Instance.Unregister(EventID.OnClickWillFireMe, OnClickWillFireMe);
        }

        private void ToggleGroup_OnValueChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            oldToggle?.GetComponent<StatusControl>().SetStatus(false);
            newToggle?.GetComponent<StatusControl>().SetStatus(true);

            PositionType position;
            switch (toggleGroup.EnableIndex)
            {
                case 0:
                    position = PositionType.All; break;
                case 1:
                    position = PositionType.HouWei; break;
                case 2:
                    position = PositionType.QianFeng; break;
                case 3:
                    position = PositionType.ZhongFeng; break;
                default:
                    position = PositionType.All; break;
            }

            AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            var list = Player.CardManager.GetCardList(position);
            osa.SetData(list);
        }

        private void OnCloseBtn()
        {
            ClearUI();
            anim.PlayExit(() => UIController.Instance.CloseWindow<CardFirePad>());
            EventManager.Instance.Dispatch(EventID.RefreshWindow, 0);
        }



        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            toggleGroup.Switch(0);
            RefreshSelectText();
            anim.PlayEnter();
        }

        private void OnClickQuickSelBtn()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);

            selectedCardDict.Clear();
            var list = Player.CardManager.GetCardList();
            if (toggleGroup.EnableIndex != 0)
            {
                list = list.Where(item => item.Config.Position == toggleGroup.EnableIndex).ToList();
            }

            list.Sort((x, y) => x.Quality.CompareTo(y.Quality));

            int firstSelectedIndex = 0;
            int cardCount = list.Count();

            for (int i = 0; i < cardCount; i++)
            {
                PlayerCard card = list[i];
                if (card.IsStarter1() || card.IsStarter() || card.IsStarter2() || card.IsStarter3() || card.IsStarter4())
                    continue;
                if (card.IsUsingInBounty)
                    continue;
                if (card.SkillTrainRoomId != 0)
                    continue;
                if (card.Quality >= QualityType.Orange)
                    continue;
                selectedCardDict.Add(card.CardId, 1);
                if (selectedCardDict.Count >= SEL_MAX)
                {
                    break;
                }
                //展示列表是倒序排，选择要解雇的是顺序选，所以选的最后1个就是展示的第1个
                firstSelectedIndex = selectedCardDict.Count - 1;
            }

            if (selectedCardDict.Count == 0)
            {
                Tips.PopTips("没有合适的目标球员");
                return;
            }

            this.OnClickWillFireMe(null);
            osa.RefreshUI();
            //osa.SmoothScrollTo(selectedCardDict.Count - 1 - firstSelectedIndex, 0.25f);
            osa.SmoothScrollTo(firstSelectedIndex, 0.25f);
        }

        private void OnClickFireBtn()
        {
            var owerList = Player.CardManager.GetCardList();
            int delCount = selectedCardDict.Count();
            if (delCount == 0)
            {
                Tips.PopTips($"请选择要解雇的球员。");
                return;
            }
            if (owerList.Count - delCount <= GameConst.FIRE_MIN_LEFT)
            {
                Tips.PopTips($"俱乐部球员少于{GameConst.FIRE_MIN_LEFT}个。");
                return;
            }

            List<int> wantDelList = selectedCardDict.Keys.ToList();
            foreach (int cardId in wantDelList)
            {
                PlayerCard card = Player.CardManager.GetCard(cardId);
                if (card.IsStarter())
                {
                    Tips.PopTips($"经典赛首发球员不能解雇。");
                    return;
                }
                if (card.IsStarter1())
                {
                    Tips.PopTips($"赛事首发球员不能解雇。");
                    return;
                }
                if (card.IsStarter2())
                {
                    Tips.PopTips($"排位赛首发球员不能解雇。");
                    return;
                }
                if (card.IsStarter3())
                {
                    Tips.PopTips($"篮球殿堂首发球员不能解雇。");
                    return;
                }
                if (card.IsStarter4())
                {
                    Tips.PopTips($"百分大战上场球员不能解雇。");
                    return;
                }
                if (card.IsUsingInBounty)
                {
                    Tips.PopTips($"悬赏任务已派遣球员不能解雇。");
                    return;
                }
                if (card.SkillTrainRoomId != 0)
                {
                    Tips.PopTips($"特级训练中的球员不能解雇。");
                    return;
                }
            }

            NetworkManager.Instance.CardFire(selectedCardDict.Keys.ToList(), (resp) =>
            {

                OnFireResp(resp.CardIdList);
            });
        }

        private void OnFireResp(IList<int> fireList)
        {
            List<PlayerCard> delList = new List<PlayerCard>();
            foreach (int cardId in fireList)
            {
                PlayerCard card = Player.CardManager.RemoveCard(cardId);
                if (card != null)
                {
                    delList.Add(card);
                }
            }

            List<Formation> formationList = Player.FightManager.FormationController.GetAllFormationList();
            foreach (int cardId in fireList)
            {
                foreach (Formation formation in formationList)
                {
                    formation.RemoveCard(cardId);
                }
            }

            var list = returnItemDict.Values.ToList();
            var properties = new InventoryObtainedUIProperties(returnItemDict.Values.ToList());
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            this.ClearUI(delList);

        }

        private void OnClickClearAllBtn()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            this.ClearUI();
        }

        private void ClearUI(IList<PlayerCard> delCardList = null)
        {
            selectedCardDict.Clear();
            osa.RefreshUI(delCardList);
            RefreshSelectText();

            box.GetComponentsInChildren<InventoryItem>().ToList().ForEach(p => p.gameObject.SetActive(false));
            returnItemDict.Clear();
        }

        private void RefreshSelectText()
        {
            textSelected.text = string.Format("{0}/{1}", this.selectedCardDict.Count, SEL_MAX);
        }
        private void OnClickWillFireMe(object[] args)
        {
            RefreshSelectText();

            returnItemDict.Clear();
            GameItem moneyItem = moneyItem = GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.Money, 0);
            returnItemDict.Add(ResourceId.Money, moneyItem);

            foreach (var kv in this.selectedCardDict)
            {
                int cardId = kv.Key;
                PlayerCard card = Player.CardManager.GetCard(cardId);
                GameItem mi = ReturnMoney(card.Config.Quality, card.Quality, card.Star, card.EquipGrade, card.DefaultPosition);
                moneyItem.Count += mi.Count;

                List<GameItem> tempList = ReturnGoods(card.Config.Quality, card.Quality, card.Star, card.EquipGrade, card.EquipLevels, card.DefaultPosition, card.Exp);
                foreach (GameItem temp in tempList)
                {

                    if (returnItemDict.ContainsKey(temp.Id) == false)
                    {
                        returnItemDict.Add(temp.Id, temp);
                    }
                    else
                    {
                        returnItemDict[temp.Id].Count += temp.Count;
                    }
                }

            }

            var children = box.GetComponentsInChildren<InventoryItem>().ToList();
            var childrenCount = children.Count();
            int counter = 0;
            foreach (var item in returnItemDict.Values)
            {
                if (item.Count > 0)
                {
                    InventoryItem _item;
                    if (counter >= childrenCount)
                    {
                        _item = Instantiate<InventoryItem>(inventoryItem, box.transform);
                        childrenCount++;
                    }
                    else
                    {
                        _item = children[counter];
                    }
                    _item.transform.localScale = new Vector3(0.8f, 0.8f);
                    _item.SetData(item);
                    _item.gameObject.SetActive(true);
                    counter++;
                }
            }
            children = box.GetComponentsInChildren<InventoryItem>().ToList();
            if (childrenCount > counter)
            {
                for (var index = counter; index < childrenCount; index++)
                {
                    children[index].gameObject.SetActive(false);
                }
            }
        }

        //返还
        public static GameItem ReturnMoney(int startQulity, int nowQuality, int star, int equipGrade, int position)
        {
            //500*N+（升级到该星级所消耗的欧元）*60%
            int total = 0;
            int start = startQulity * 1000 + star;
            int end = nowQuality * 1000 + star;
            CardFireConfig fireConf = Configs.CardFire.GetConfig(nowQuality);
            total += fireConf.Money;
            for (int i = start; i <= end; i++)
            {
                QualityStarConfig starUpConf = Configs.QualityStar.GetConfig(i);
                if (starUpConf == null)
                    continue;
                fireConf = Configs.CardFire.GetConfig(starUpConf.Quality);

                total += Mathf.FloorToInt(starUpConf.CostMoney * fireConf.ReturnPercent / 100);
            }

            //装备突破消耗
            var equipConfigs = Configs.JerseyBreak.GetConfigList().FindAll(p => p.Position == position && p.Level <= equipGrade);
            foreach (var cfg in equipConfigs)
            {
                GameItemUtils.CreateGameItems(cfg.Cost).ToList().ForEach((p) =>
                {
                    if (p.Type == GameItemType.Resource && p.Id == ResourceId.Money)
                    {
                        total += p.Count;
                    }
                });
            }
            return GameItemUtils.CreateGameItem(GameItemType.Resource, ResourceId.Money, total);
        }

        public static List<GameItem> ReturnGoods(int startQulity, int nowQuality, int star, int equipGrade, List<int> equipLvList, int position, int totalExp)
        {
            List<GameItem> retList = new List<GameItem>();

            //品质带来的物品
            CardFireConfig fireConf = Configs.CardFire.GetConfig(nowQuality);
            foreach (var returnItem in fireConf.ReturnGoods)
            {
                retList.Add(GameItemUtils.CreateGameItem(GameItemType.Goods, returnItem.Key, returnItem.Value));
            }

            //升星消耗的物品
            int begin = startQulity * 1000;
            int current = nowQuality * 1000 + star;
            foreach (var item in Configs.QualityStar.GetConfigList())
            {
                if (begin <= item.Id && current >= item.Id)
                {
                    KeyValuePair<int, int> costGoods = item.CostGoods.FirstOrDefault();
                    GameItem gameItem = GameItemUtils.CreateGameItem(GameItemType.Goods, costGoods.Key, costGoods.Value);
                    gameItem.Count = Mathf.FloorToInt((gameItem.Count + item.CostSelf) * fireConf.ReturnPercent / 100);
                    retList.Add(gameItem);
                }
            }

            //合并这些物品
            retList = GameItemUtils.MergeGameItemList(retList);

            Dictionary<int, GameItem> goods = new();
            #region 装备返还
            for (var index = 100107; index >= 100104; index--)
            {
                var expItemcfg = Configs.Goods.GetConfig(index);
                var count = totalExp / expItemcfg.Param1;
                totalExp = totalExp % expItemcfg.Param1;
                if (count > 0)
                {
                    if (!goods.ContainsKey(expItemcfg.Id))
                    {
                        goods[expItemcfg.Id] = GameItemUtils.CreateGameItem(GameItemType.Goods, expItemcfg.Id, count);
                    }
                    else
                    {
                        goods[expItemcfg.Id].Count += count;
                    }

                }
            }

            var equipConfigs = Configs.JerseyBreak.GetConfigList().FindAll(p => p.Position == position && p.Level <= equipGrade);
            foreach (var cfg in equipConfigs)
            {
                GameItemUtils.CreateGameItems(cfg.Cost).ToList().ForEach((p) =>
                {
                    if (p.Type == GameItemType.Goods)
                    {
                        if (goods.ContainsKey(p.Id))
                        {
                            goods[p.Id].Count += p.Count;
                        }
                        else
                        {
                            goods[p.Id] = GameItemUtils.CreateGameItem(GameItemType.Goods, p.Id, p.Count);
                        }
                    }
                });
            }

            var equipLvConfigs = new List<JerseyUpgradeConfig>();
            Configs.JerseyUpgrade.GetConfigList().ForEach((p) =>
            {
                for (var index = 0; index < 4; index++)
                {
                    if (p.Position == position && p.Part == (index + 1) && p.Level <= equipLvList[index])
                    {
                        GameItemUtils.CreateGameItems(p.Cost).ToList().ForEach((p) =>
                        {
                            if (p.Type == GameItemType.Goods)
                            {
                                if (goods.ContainsKey(p.Id))
                                {
                                    goods[p.Id].Count += p.Count;
                                }
                                else
                                {
                                    goods[p.Id] = GameItemUtils.CreateGameItem(GameItemType.Goods, p.Id, p.Count);
                                }
                            }
                        });
                    }
                }
            });


            #endregion
            foreach (var item in goods.Values)
            {
                retList.Add(item);
            }
            return retList;

        }

        public bool CheckSelectedCount()
        {
            return this.selectedCardDict.Count < SEL_MAX;
        }
        public void SelectCard(int cardId)
        {
            this.selectedCardDict.TryAdd(cardId, 1);
        }
        public void UnselectCard(int cardId)
        {
            if (this.selectedCardDict.ContainsKey(cardId))
                this.selectedCardDict.Remove(cardId);
        }

        public bool IsSelectedCard(int cardId)
        {
            return this.selectedCardDict.ContainsKey(cardId);
        }

    }
}