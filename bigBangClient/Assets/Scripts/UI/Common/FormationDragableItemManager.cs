using GameConfig;
using PathologicalGames;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using Vectrosity;
using DG.Tweening;
using TMPro;
using System.Linq;
using BigBang.Animation;
using GameConfig.Config;
using System;

namespace BigBang.UI
{
    public class FormationDragableItemManager : DragableItemManager
    {
        public Action MainBoardChangeAction;

        public Action MainBoardOrBenchBoardChangeAction;

        [SerializeField] GameObject debugPrefab;

        [SerializeField] public RectTransform mainContainer;
        [SerializeField] public RectTransform benchContainer;
        [SerializeField] public RectTransform backupContainer;
        [SerializeField] RectTransform soccerField;
        [SerializeField] RectTransform highLightFrame;
        [SerializeField] RectTransform benchHighLightFrame;
        [SerializeField] RectTransform backupHighLightFrame;
        [SerializeField] RectTransform separatedSoccerAreas;
        [SerializeField] RectTransform posHolders;
        private SpawnPool mainPool;
        private SpawnPool benchPool;
        private SpawnPool reservePool;
        public Dictionary<int, Vector2> MainGrids { get; private set; } = new Dictionary<int, Vector2>();
        public Dictionary<int, Vector2> BenchGrids { get; private set; } = new Dictionary<int, Vector2>();
        public Dictionary<int, Vector2> ReserveGrids { get; private set; } = new Dictionary<int, Vector2>();
        public Dictionary<int, Vector2> ReserveLeftGrids { get; private set; } = new Dictionary<int, Vector2>();
        public Dictionary<int, Vector2> ReserveRightGrids { get; private set; } = new Dictionary<int, Vector2>();
        public Dictionary<int, FormationDragableItem> MainPlayerCards { get; private set; } = new Dictionary<int, FormationDragableItem>();
        public Dictionary<int, FormationDragableItem> BenchPlayerCards { get; private set; } = new Dictionary<int, FormationDragableItem>();
        public List<PlayerCard> reserveCardList = new List<PlayerCard>(); //玩家所有后援席卡牌列表
        public Dictionary<int, FormationDragableItem> ReservePlayerCards { get; private set; } = new Dictionary<int, FormationDragableItem>(); //后援席当前卡牌
        public Dictionary<int, FormationDragableItem> ReserveLeftTempCards { get; private set; } = new Dictionary<int, FormationDragableItem>();//后援席左页临时卡牌
        public Dictionary<int, FormationDragableItem> ReserveRightTempCards { get; private set; } = new Dictionary<int, FormationDragableItem>();//后援席右页临时卡牌

        private float baseMainItemWidth = 91f;
        private float baseMainItemHeight = 97f;

        private float _benchGridWidth = 94;
        private float _benchGridHeight = 110; //97
        private float _benchGridOffsetX = 20;
        private float _benchGridOffsetY = 0;
        private int maxBenchRow = 2;
        private int maxBenchColumn = 6;
        private FormationBase _formation;
        //位置区域高亮
        private string curHighLightName = "";

        private FormationDragableItem _curBreathingMainItem;

        [Header("SelectLine")]
        [SerializeField] Texture lineTexture;
        [SerializeField] Image highLightImg;
        [SerializeField] TMP_Text highLightText;
        private VectorLine selectionLine;
        private bool isLineShow = false;
        private int highLightFieldShown = 0;
        private Tween lineTween;
        //private Dictionary<int, Rect> rectDic = new Dictionary<int, Rect>();
        private Dictionary<int, List<int>> separatePosDicList = new Dictionary<int, List<int>>();

        //各个位置的区域线框顶点，用于虚线特效，key为小位置的id
        private Dictionary<int, List<Vector2>> separatedAeraVerts = new Dictionary<int, List<Vector2>>();
        List<GameObject> MainHighLightSeparatedArea = new();

        //后援席
        private const int RESERVE_ROW_COUNT = 3;
        private const int RESERVE_COLUMN_COUNT = 6;
        private const float RESERVE_GRID_WIDTH = 110;
        private const float RESERVE_GRID_HEIGHT = 110;
        private const float RESERVE_GRID_OFFSET_X = -30;
        private const float RESERVE_GRID_OFFSET_Y = 0;
        private bool isBackupWindowOpening = false;
        private int curBackupPage = 1;

#if UNITY_EDITOR
        private List<GameObject> debugPosPrefabs = new List<GameObject>();
#endif
        #region Initialize & Clear
        public void Init()
        {
            mainPool = mainContainer.GetComponent<SpawnPool>();
            benchPool = benchContainer.GetComponent<SpawnPool>();
            reservePool = backupContainer.GetComponent<SpawnPool>();
            //float realHeight = Screen.height * 720 / Screen.width;
            //float realBottomHeight = (199 + 146) * 720 / Screen.width;
            //float realTopHeight = 72 * 720 / Screen.width;
            //float realMainHeight = realHeight - realBottomHeight - realTopHeight;
            //Debug.Log($"RealMainHeight: {realMainHeight}");

            InitMainGrids();
            InitBenchGrids();
            InitReserveGrid();
            InitHighLight();
            InitBenchHighLight();
            InitBackupHighLight();
            InitSelectionLine();
            HideHighLightField();
        }

        private void InitMainGrids()
        {
            //if ((float)Screen.width / (float)Screen.height < 0.5f)
            //{
            //    var prePos = mainContainer.parent.GetComponent<RectTransform>().anchoredPosition;
            //    mainContainer.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2(prePos.x, prePos.y + 150);
            //}

            foreach (var cfg in Configs.FormationBoard.GetConfigList())
            {
                if (!separatePosDicList.ContainsKey(cfg.SeparatedPosition))
                {
                    separatePosDicList.Add(cfg.SeparatedPosition, new List<int>() { cfg.Id });
                }
                else
                {
                    separatePosDicList[cfg.SeparatedPosition].Add(cfg.Id);
                }
                var holder = posHolders.Find(cfg.Id.ToString()).GetComponent<RectTransform>();
                MainGrids.Add(cfg.Id, holder.anchoredPosition);

                //#if UNITY_EDITOR
                //                GameObject go = Instantiate(debugPrefab, mainContainer);
                //                go.GetComponent<Text>().text = cfg.Id.ToString();
                //                go.GetComponent<RectTransform>().anchoredPosition = MainGrids[cfg.Id];
                //                debugPosPrefabs.Add(go);
                //#endif
            }
            GenMainGridEdges();
            InitSeparatedQuadVert();
        }

        private List<List<Vector2>> MainEdgesListList = new();
        //生成MainGrids的边界，用于鼠标位置判断
        private void GenMainGridEdges()
        {
            foreach (var cfg in Configs.FormationBoard.GetConfigList())
            {
                List<Vector2> MainEdgesList = new();
                for (int i = 1; i <= 4; i++)
                {
                    string separatedPositionName = ((PositionSeparatedType)cfg.SeparatedPosition).ToString() + i.ToString();
                    var holder = posHolders.Find(separatedPositionName).GetComponent<RectTransform>();
                    MainEdgesList.Add(holder.localPosition.ToVec2());
                }
                MainEdgesListList.Add(MainEdgesList);
            }
        }

        private void InitSeparatedQuadVert()
        {
            float halfMainConWidth = mainContainer.rect.width / 2;
            foreach (var cfg in Configs.FormationBoard.GetConfigList())
            {
                var separatedAreaGo = separatedSoccerAreas.Find(((PositionSeparatedType)cfg.SeparatedPosition).ToString()).gameObject;
                MainHighLightSeparatedArea.Add(separatedAreaGo);

                var list = new List<Vector2>();
                for (int i = 1; i <= 4; i++)
                {
                    string separatedPositionName = ((PositionSeparatedType)cfg.SeparatedPosition).ToString() + i.ToString();
                    var vec = posHolders.Find(separatedPositionName).GetComponent<RectTransform>().localPosition;
                    list.Add(new Vector2(vec.x + halfMainConWidth, vec.y));
                }
                var vec1 = posHolders.Find(((PositionSeparatedType)cfg.SeparatedPosition).ToString() + 1.ToString()).GetComponent<RectTransform>().localPosition;
                list.Add(new Vector2(vec1.x + halfMainConWidth, vec1.y));
                separatedAeraVerts.Add(cfg.Id, list);
            }
        }

        private void InitBenchGrids()
        {
            //if ((float)Screen.width / (float)Screen.height < 0.5f)
            //{
            //    _benchGridOffsetY += 60;
            //    var y = benchContainer.anchoredPosition.y;
            //    benchContainer.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, y, 470);
            //}
            for (int row = 0; row <= maxBenchRow; row++)
            {
                for (int column = 1; column <= maxBenchColumn; column++)
                {
                    if (row > 1 || (row == 1 && column > 1)) break;
                    int index = column + row * maxBenchColumn;
                    BenchGrids.Add(index, new Vector2((column - 3.5f) * (_benchGridWidth + _benchGridOffsetX),
                        -row * _benchGridHeight + _benchGridHeight / 2 + _benchGridOffsetY));
                }
            }
        }

        private void InitReserveCardList()
        {
            reserveCardList.Clear();
            foreach (var card in Player.CardManager.CardList)
            {
                if (_formation.StarterBoardCardDic.Values.Contains(card.Config.Id)) continue;
                if (_formation.SubstituteBoardCardDic.Values.Contains(card.Config.Id)) continue;
                reserveCardList.Add(card);
            }
            SortReserveCardList();
            curBackupPage = 1;
        }

        private void SortReserveCardList()
        {
            if (isBounty)
            {
                reserveCardList = reserveCardList
                .OrderBy(card => card.IsUsingInBounty)
                .ThenBy(card => card.Quality)
                .ThenBy(card => card.Star)
                .ThenBy(card => card.FightPoint)
                .ThenBy(card => card.CardId)
                .ToList();
                return;
            }
            reserveCardList = reserveCardList.OrderByDescending(card => card.Config.Quality)
                .ThenByDescending(card => card.FightPoint)
                .ThenBy(card => card.CardId).ToList();
            //球员ID变动可能会导致这里报错
        }

        public int GetBackupCardCount()
        {
            return reserveCardList.Count;
        }

        public int GetReserveCardPage()
        {
            return Mathf.CeilToInt((float)reserveCardList.Count / (float)RESERVE_ROW_COUNT / (float)RESERVE_COLUMN_COUNT);
        }
        public void RefreshNextBtnShow(Button nextBtn, Button prevBtn)
        {
            nextBtn.gameObject.SetActive(curBackupPage != GetReserveCardPage());
            prevBtn.gameObject.SetActive(curBackupPage != 1);
        }
        public void GetNextPage()
        {
            if (UIController.Instance.IsTouchMaskShow == true) return;

            if (curBackupPage == GetReserveCardPage())
            {
                Tips.PopError(ErrorID.NoMoreBackupCard);
                return;
            }

            AddReserveCard(RESERVE_ROW_COUNT * RESERVE_COLUMN_COUNT * curBackupPage + 1, ReserveRightGrids);
            //向左滑动动画
            foreach (var item in ReservePlayerCards)
            {
                item.Value.GetComponent<RectTransform>().DoRelativeAnchorPosX(-Screen.width, 0.3f).AddTo(this.gameObject);
            }
            foreach (var item in ReserveRightTempCards)
            {
                item.Value.GetComponent<RectTransform>().DoRelativeAnchorPosX(-Screen.width, 0.3f).AddTo(this.gameObject);
            }

            TouchManager.Instance.DisableTouch();
            UnityTimer.Timer.Register(this.gameObject, 0.31f, () =>
            {
                TouchManager.Instance.EnableTouch();
            });

            BackupCardClear();

            foreach (var item in ReserveRightTempCards)
            {
                ReservePlayerCards.Add(item.Key, item.Value);
            }
            ReserveRightTempCards.Clear();

            curBackupPage++;

            RefreshState();
        }

        public void GetPreviousPage()
        {
            if (UIController.Instance.IsTouchMaskShow == true) return;

            if (curBackupPage == 1)
            {
                Tips.PopError(ErrorID.NoMoreBackupCard);
                return;
            }
            curBackupPage--;
            AddReserveCard(RESERVE_ROW_COUNT * RESERVE_COLUMN_COUNT * (curBackupPage - 1) + 1, ReserveLeftGrids);
            foreach (var item in ReservePlayerCards)
            {
                item.Value.GetComponent<RectTransform>().DoRelativeAnchorPosX(Screen.width, 0.3f).AddTo(this.gameObject);
            }
            foreach (var item in ReserveLeftTempCards)
            {
                item.Value.GetComponent<RectTransform>().DoRelativeAnchorPosX(Screen.width, 0.3f).AddTo(this.gameObject);
            }

            TouchManager.Instance.DisableTouch();
            UnityTimer.Timer.Register(this.gameObject, 0.31f, () =>
            {
                TouchManager.Instance.EnableTouch();
            });

            BackupCardClear();

            foreach (var item in ReserveLeftTempCards)
            {
                ReservePlayerCards.Add(item.Key, item.Value);
            }
            ReserveLeftTempCards.Clear();

            RefreshState();
        }

        private void InitReserveGrid()
        {
            var width = Screen.width;
            for (int row = 1; row <= RESERVE_ROW_COUNT; row++)
            {
                for (int col = 1; col <= RESERVE_COLUMN_COUNT; col++)
                {
                    int index = (row - 1) * RESERVE_COLUMN_COUNT + col;
                    var pos = new Vector2(col * RESERVE_GRID_WIDTH + RESERVE_GRID_OFFSET_X, -row * RESERVE_GRID_HEIGHT + RESERVE_GRID_OFFSET_Y);
                    ReserveGrids.Add(index + 10000, pos); //BackupBoard的index从10001开始
                    ReserveLeftGrids.Add(index + 10000, new Vector2(pos.x - width, pos.y));
                    ReserveRightGrids.Add(index + 10000, new Vector2(pos.x + width, pos.y));
                }
            }
        }

        private void InitHighLight()
        {
            highLightFrame.anchoredPosition = new Vector2(-Screen.width, -Screen.height);
        }

        private void InitBenchHighLight()
        {
            benchHighLightFrame.anchoredPosition = new Vector2(-Screen.width, -Screen.height);
        }

        private void InitBackupHighLight()
        {
            backupHighLightFrame.anchoredPosition = new Vector2(-Screen.width, -Screen.height);
        }

        private void InitSelectionLine()
        {
            selectionLine = new VectorLine("Selection", new List<Vector2>(5), 10f, LineType.Continuous) //注意是连续线，通过WrapMode = Repeated的texture来形成断点
            {
                texture = lineTexture,
                textureScale = 1f,
                color = new Color32(94, 241, 83, 255)
            };
            selectionLine.rectTransform.SetParent(mainContainer);
            selectionLine.rectTransform.localScale = Vector3.one;
            selectionLine.rectTransform.anchoredPosition = Vector2.zero;
        }

        public void OnBackupWindowOpened()
        {
            isBackupWindowOpening = true;
            SetReserveData(RESERVE_ROW_COUNT * RESERVE_COLUMN_COUNT * (curBackupPage - 1) + 1);
        }

        public void OnBackupWindowClosed(bool needSaveToServer = false)
        {
            isBackupWindowOpening = false;
            _formation.UpdateCardFormationInfo();
            InitReserveCardList();
            if (needSaveToServer)
                _formation.SaveToServer();
        }
        private bool isBenchWindowOpening = true;
        public void SetBenchPanelIsShow(bool isShow)
        {
            isBenchWindowOpening = isShow;
        }

        public bool IsBackupWindowOpening()
        {
            return isBackupWindowOpening;
        }
        public bool IsBenchWindowOpening()
        {
            return isBenchWindowOpening;
        }

        public void Clear()
        {
            mainPool.DespawnAll();
            foreach (var item in MainPlayerCards.Values)
            {
                item.Clear();
            }
            MainPlayerCards.Clear();

            benchPool.DespawnAll();
            foreach (var item in BenchPlayerCards.Values)
            {
                item.Clear();
            }
            BenchPlayerCards.Clear();

            BackupCardClear();
        }
        #endregion

        bool isBounty = false;
        public void SetData(FormationBase formation, int[] limitIntArr = null, bool isBounty = false)
        {
            _formation = formation;
            this.isBounty = isBounty;
            ProcessLimit(limitIntArr);
            SetMainData(formation.StarterBoardCardDic);
            SetBenchData(formation.SubstituteBoardCardDic);
            InitReserveCardList();
        }

        HashSet<PlayerCard> canNotUsePlayCardSet = new();
        private void ProcessLimit(int[] limitIntArr = null)
        {
            canNotUsePlayCardSet.Clear();
            if (limitIntArr == null || limitIntArr.Length <= 0) return;
            List<ChallengeRuleConfig> challengeRuleConfigList = new();

            foreach (int limitInt in limitIntArr)
            {
                ChallengeRuleConfig challengeRuleConfig = Configs.ChallengeRule.GetConfig(limitInt);
                if (challengeRuleConfig == null)
                {
                    Debug.LogWarningFormat("FormationPad , CheckLimit , challengeRuleConfig == null , limitInt = {0}", limitInt);
                    continue;
                }
                challengeRuleConfigList.Add(challengeRuleConfig);
            }
            foreach (ChallengeRuleConfig challengeRuleConfig in challengeRuleConfigList)
            {
                switch (challengeRuleConfig.Key)
                {
                    case "count"://3人出场
                        {
                            //限制出场人数会导致很大的改动，此条件暂时不会用到
                        }
                        break;
                    case "pos"://控球后卫至少3人
                        {

                        }
                        break;
                    case "quality"://紫色球员至少3人
                        {

                        }
                        break;
                    case "player"://易建联必须上场
                        {
                            if (challengeRuleConfig.Judge == "=" && challengeRuleConfig.Value == 1 ||
                                challengeRuleConfig.Judge == ">=" && challengeRuleConfig.Value == 1 ||
                                challengeRuleConfig.Judge == ">" && challengeRuleConfig.Value == 0)
                            {

                            }
                            else
                            {
                                PlayerCard playerCard = Player.CardManager.GetCard(challengeRuleConfig.KeyValue);
                                if (playerCard != null && canNotUsePlayCardSet.Contains(playerCard) == false)
                                {
                                    canNotUsePlayCardSet.Add(playerCard);
                                }
                            }

                        }
                        break;
                    default: break;
                }
            }
        }

        public void PackData()
        {
            if (_formation == null) return;
            var mainDic = new Dictionary<int, int>();
            foreach (var item in MainPlayerCards)
            {
                mainDic.Add(item.Key, item.Value.GetData().CardId);
            }
            _formation.StarterBoardCardDic = mainDic;
            foreach (var item in BenchPlayerCards)
            {
                _formation.SubstituteBoardCardDic[item.Key] = item.Value.GetData().CardId;
            }
            _formation.SetChangeFlag(true);
        }

        public List<int> GetBoardList()
        {
            List<int> boardList = new List<int>();
            string str = "Exist Formation: ";
            foreach (int boardId in MainPlayerCards.Keys)
            {
                boardList.Add(boardId);
                str += boardId + ", ";
            }
            Debug.Log(str);
            return boardList;
        }

        private FormationDragableItem AddCard(int boardId, int cardId, Dictionary<int, Vector2> grid, SpawnPool pool, string prefabName, RectTransform parent)
        {
            if (grid.ContainsKey(boardId))
            {
                RectTransform rt = pool.Spawn(prefabName) as RectTransform;
                rt.anchoredPosition = grid[boardId];
                FormationDragableItem dragableItem = rt.GetComponent<FormationDragableItem>();
                dragableItem.Init();
                PlayerCard playerCard = Player.CardManager.GetCard(cardId);
                dragableItem.InitData(playerCard, _formation.FormationId, stateSave);
                dragableItem.SetBoardId(boardId);
                dragableItem.SetParent(parent);
                dragableItem.SetManager(this);
                RefreshDragItemState(dragableItem, playerCard);

                if (parent.name == benchContainer.name)
                {
                    var num = benchContainer.childCount;
                    dragableItem.root.SetSiblingIndex(boardId - 1);
                }
                return dragableItem;
            }
            else
            {
                Debug.Log("Invalid BoardId: " + boardId);
                return null;
            }
        }
        private void RefreshDragItemState(FormationDragableItem dragableItem, PlayerCard playerCard)
        {
            FormationDragableItem.StateSign stateSign = FormationDragableItem.StateSign.Normal;
            if (canNotUsePlayCardSet.Contains(playerCard))
            {
                stateSign = FormationDragableItem.StateSign.CanNotUse;
            }
            else
            {
                if (isBounty && BountyTaskManager.Instance.IsPlayerCardUsing(playerCard.CardId))
                {
                    stateSign = FormationDragableItem.StateSign.Dispatched;
                }
            }
            dragableItem.SetStateSign(stateSign);
        }

        private void SetMainData(Dictionary<int, int> starterBoardCards)
        {
            foreach (var item in starterBoardCards)
            {
                int boardId = item.Key;
                int playerCardId = item.Value;
                FormationDragableItem dragableItem = AddCard(boardId, playerCardId, MainGrids, mainPool, "FormationMainDragableItem", mainContainer);

                int row = Mathf.FloorToInt(boardId / 100);
                if (row > 2)
                {
                    float rate = 1.0f - (row - 2) * 0.02f;
                    (dragableItem as FormationMainDragableItem).SetScale(rate);
                }
                else
                {
                    (dragableItem as FormationMainDragableItem).SetScale(1);
                }

                (dragableItem as FormationMainDragableItem).GrowUp(0.3f, 0.25f);//首发席卡牌长出
                if (dragableItem != null && !MainPlayerCards.ContainsKey(boardId))
                {
                    MainPlayerCards.Add(boardId, dragableItem);
                }
                dragableItem.RefreshCombatEffectivenessInMain();
            }
        }
#if UNITY_EDITOR
        public void ShowDebugPoses()
        {
            foreach (var go in debugPosPrefabs)
            {
                go.SetActive(true);
            }
        }

        public void HideDebugPoses()
        {
            foreach (var go in debugPosPrefabs)
            {
                go.SetActive(false);
            }
        }
#endif

        private void SetBenchData(Dictionary<int, int> substituteBoardCards)
        {
            foreach (var item in substituteBoardCards)
            {
                int boardId = item.Key;
                int playerCardId = item.Value;
                if (playerCardId == 0) continue;
                FormationDragableItem dragableItem = AddCard(boardId, playerCardId, BenchGrids, benchPool, "FormationBenchDragableItem", benchContainer);
                if (dragableItem != null && !BenchPlayerCards.ContainsKey(boardId))
                {
                    BenchPlayerCards.Add(boardId, dragableItem);
                }
                (dragableItem as FormationBenchDragableItem).PlayFadeIn(0.2f, (boardId - 1) * 0.03f + 0.2f); //id * 间隔时间 + 起始时间
            }
        }

        public void SetReserveData(int startReserveIndex = 1)
        {
            BackupCardClear();
            reservePool.DespawnAll();
            int leftCardNum = reserveCardList.Count - startReserveIndex + 1;
            for (int index = 1; index <= RESERVE_ROW_COUNT * RESERVE_COLUMN_COUNT && index <= leftCardNum; index++)
            {
                //目前后援席的boardId为序号+10000
                FormationDragableItem dragableItem = AddCard(10000 + index, reserveCardList[startReserveIndex + index - 2].CardId,
                    ReserveGrids, reservePool, "FormationReserveDragableItem", backupContainer);
                ReservePlayerCards.Add(10000 + index, dragableItem);
            }
            RefreshState();
        }

        public int GetCurStartIndex()
        {
            return (curBackupPage - 1) * RESERVE_COLUMN_COUNT * RESERVE_ROW_COUNT + 1;
        }

        //目前用于在翻页时，在left或right的grid上加card，仅AddCard，判断逻辑在外面
        private void AddReserveCard(int startIndex, Dictionary<int, Vector2> grids)
        {
            int index = 1;
            while (index <= RESERVE_COLUMN_COUNT * RESERVE_ROW_COUNT && reserveCardList.Count >= index + startIndex - 1)
            {
                var dragableItem = AddCard(10000 + index, reserveCardList[startIndex + index - 2].CardId,
                    grids, reservePool, "FormationReserveDragableItem", backupContainer);
                if (grids == ReserveLeftGrids)
                    ReserveLeftTempCards.Add(10000 + index, dragableItem);
                else if (grids == ReserveRightGrids)
                    ReserveRightTempCards.Add(10000 + index, dragableItem);
                else
                {
                    Debug.LogError("Temp Card Added Incorrect");
                }
                index++;
            }
        }

        private void BackupCardClear()
        {
            foreach (var item in ReservePlayerCards)
            {
                item.Value.Clear();
                reservePool.Despawn(item.Value.transform);
            }
            ReservePlayerCards.Clear();
        }

        private int GetBenchColumnWithPositionX(float xPosition)
        {
            var index = (xPosition) / (_benchGridWidth + _benchGridOffsetX) + 4;
            //Debug.Log("GetBenchColumnWithPositionX" + index);
            return index < 0 ? -1 : Mathf.FloorToInt(index);
        }

        private int GetBenchRowWithPositionY(float yPosition)
        {
            var index = -(yPosition - _benchGridOffsetY) / _benchGridHeight + 1;
            //Debug.Log("GetBenchRowWithPositionY" + index);
            return index < 0 ? -1 : Mathf.FloorToInt(index);
        }

        private int GetReserveColumnWithPositionX(float xPosition)
        {
            return Mathf.FloorToInt((xPosition - RESERVE_GRID_OFFSET_X) / RESERVE_GRID_WIDTH + 0.5f);
        }

        private int GetReserveRowWithPositionY(float yPosition)
        {
            return Mathf.FloorToInt((-yPosition - RESERVE_GRID_OFFSET_Y) / RESERVE_GRID_HEIGHT + 0.5f);
        }

        public Vector2 GetPosByBoardId(int boardId)
        {
            if (MainGrids.ContainsKey(boardId))
                return MainGrids[boardId];
            if (BenchGrids.ContainsKey(boardId))
                return BenchGrids[boardId];
            if (ReserveGrids.ContainsKey(boardId))
                return ReserveGrids[boardId];
            return Vector2.zero;
        }

        public int GetMainBoardId(PointerEventData eventData)
        {
            var camera = UIController.Instance.GetCamera();
            var mainPos = CoordinateUtil.World2UI(eventData.pointerCurrentRaycast.worldPosition, posHolders, camera, camera);
            for (int i = 0; i < MainEdgesListList.Count; i++)
            {
                if (mainPos.InRegion(MainEdgesListList[i]))
                {
                    return Configs.FormationBoard.GetConfigList()[i].Id;
                }
            }
            return 0;
        }

        public int GetBenchBoardId(PointerEventData eventData)
        {
            var camera = UIController.Instance.GetCamera();
            var benchPos = CoordinateUtil.World2UI(eventData.pointerCurrentRaycast.worldPosition, benchContainer, camera, camera);
            int benchColumn = GetBenchColumnWithPositionX(benchPos.x);
            int benchRow = GetBenchRowWithPositionY(benchPos.y);
            return benchRow * maxBenchColumn + benchColumn;
        }

        public int GetReserveBoardId(PointerEventData eventData)
        {
            var camera = UIController.Instance.GetCamera();
            var backupPos = CoordinateUtil.World2UI(eventData.pointerCurrentRaycast.worldPosition, backupContainer, camera, camera);
            //下面参数中修正值，是因为ScreenPointToLocalPointInRectangle是按照中点为锚点算的。感觉这样修正不是太好，探索更好的方法。
            int col = GetReserveColumnWithPositionX(backupPos.x + 0.5f * backupContainer.rect.width);
            int row = GetReserveRowWithPositionY(backupPos.y - 0.5f * backupContainer.rect.height);
            return (row - 1) * RESERVE_COLUMN_COUNT + col + 10000;
        }

        public override void PickItem(PointerEventData eventData, DragableItem item)
        {
            base.PickItem(eventData, item);
            //后援席打开时，关闭首发和替补之间的逻辑
            if (!isBackupWindowOpening && isBenchWindowOpening || isBackupWindowOpening && !isBenchWindowOpening)
            {
                int adaptPosition = (item as FormationDragableItem).GetData().Config.AdaptPosition[0];

                FormationBoardConfig formationBoardConfigSelect = null;
                for (int i = 0; i < Configs.FormationBoard.GetConfigList().Count; i++)
                {
                    var formationBoardConfig = Configs.FormationBoard.GetConfigList()[i];
                    bool isSelect = formationBoardConfig.SeparatedPosition == adaptPosition;
                    if (isSelect)
                    {
                        formationBoardConfigSelect = formationBoardConfig;
                    }
                    MainHighLightSeparatedArea[i].SetActive(isSelect);
                }

                ShowLine(formationBoardConfigSelect.Id);
            }
        }

        public override void DropItem(PointerEventData eventData, DragableItem dragableItem)
        {
            base.DropItem(eventData, dragableItem);

            foreach (var item in MainHighLightSeparatedArea)
            {
                item.SetActive(false);
            }

            if (isBackupWindowOpening)
            {
                if (isBenchWindowOpening)
                {
                    dropItemBetweenBenchAndBackup();
                }
                else
                {
                    dropItemBetweenMainAndBackup();
                }
            }
            else
            {
                dropItemBetweenMainAndBench();
            }
        }

        private void dropItemBetweenMainAndBackup()
        {
            InitHighLight();
            InitBackupHighLight();
            HideLine();
            HideSeparatedAera();
        }

        private void dropItemBetweenMainAndBench()
        {
            InitHighLight();
            InitBenchHighLight();
            HideLine();
            HideSeparatedAera();
        }

        private void dropItemBetweenBenchAndBackup()
        {
            InitBenchHighLight();
            InitBackupHighLight();
        }

        public override void DragItem(PointerEventData eventData, DragableItem dragableItem)
        {
            base.DragItem(eventData, dragableItem);
            if (!isBackupWindowOpening)
            {
                dragBetweenMainAndBench(eventData, dragableItem);
            }
            else
            {
                if (isBenchWindowOpening)
                {
                    dragBetweenBenchAndBackup(eventData, dragableItem);
                }
                else
                {
                    dragBetweenMainAndBackup(eventData, dragableItem);
                }
            }
        }

        private void dragBetweenMainAndBench(PointerEventData eventData, DragableItem dragableItem)
        {
            int mainBoardId = GetMainBoardId(eventData);
            int benchBoardId = GetBenchBoardId(eventData);
            if (MainPlayerCards.ContainsKey(mainBoardId) && (dragableItem as FormationDragableItem).BoardId != mainBoardId)
            {
                InitBenchHighLight();
                highLightFrame.sizeDelta = GetHighLightWHVector2(mainBoardId);
                highLightFrame.anchoredPosition = MainGrids[mainBoardId];

                var mainBoardItem = MainPlayerCards[mainBoardId] as FormationMainDragableItem;
                if (_curBreathingMainItem != null)
                {
                    if (_curBreathingMainItem.BoardId != mainBoardItem.BoardId)
                    {
                        _curBreathingMainItem.StopBreath();
                        mainBoardItem.StartBreath();
                        _curBreathingMainItem = mainBoardItem;
                    }
                }
                else
                {
                    mainBoardItem.StartBreath();
                    _curBreathingMainItem = mainBoardItem;
                }
            }
            else if (BenchPlayerCards.ContainsKey(benchBoardId) && (dragableItem as FormationDragableItem).BoardId != benchBoardId)
            {
                InitHighLight();
                benchHighLightFrame.anchoredPosition = BenchGrids[benchBoardId];

                var benchBoardItem = BenchPlayerCards[benchBoardId];
                if (_curBreathingMainItem != null)
                {
                    if (_curBreathingMainItem.BoardId != benchBoardItem.BoardId)
                    {
                        _curBreathingMainItem.StopBreath();
                        benchBoardItem.StartBreath();
                        _curBreathingMainItem = benchBoardItem;
                    }
                }
                else
                {
                    benchBoardItem.StartBreath();
                    _curBreathingMainItem = benchBoardItem;
                }
            }
            else
            {
                InitHighLight();
                InitBenchHighLight();
                if (_curBreathingMainItem != null)
                {
                    _curBreathingMainItem.StopBreath();
                    _curBreathingMainItem = null;
                }
            }
        }

        private void dragBetweenBenchAndBackup(PointerEventData eventData, DragableItem dragableItem)
        {
            int benchBoardId = GetBenchBoardId(eventData);
            int backupBoardId = GetReserveBoardId(eventData);
            if (BenchPlayerCards.ContainsKey(benchBoardId) && (dragableItem as FormationDragableItem).BoardId != benchBoardId)
            {
                InitBackupHighLight();
                benchHighLightFrame.anchoredPosition = BenchGrids[benchBoardId];
                var benchBoardItem = BenchPlayerCards[benchBoardId];
                if (_curBreathingMainItem != null)
                {
                    if (_curBreathingMainItem.BoardId != benchBoardItem.BoardId)
                    {
                        _curBreathingMainItem.StopBreath();
                        benchBoardItem.StartBreath();
                        _curBreathingMainItem = benchBoardItem;
                    }
                }
                else
                {
                    benchBoardItem.StartBreath();
                    _curBreathingMainItem = benchBoardItem;
                }
            }
            else if (ReservePlayerCards.ContainsKey(backupBoardId) && (dragableItem as FormationDragableItem).BoardId != backupBoardId)
            {
                InitBenchHighLight();
                backupHighLightFrame.anchoredPosition = ReserveGrids[backupBoardId];
                var backupBoardItem = ReservePlayerCards[backupBoardId];
                if (_curBreathingMainItem != null)
                {
                    if (_curBreathingMainItem.BoardId != backupBoardItem.BoardId)
                    {
                        _curBreathingMainItem.StopBreath();
                        backupBoardItem.StartBreath();
                        _curBreathingMainItem = backupBoardItem;
                    }
                }
                else
                {
                    backupBoardItem.StartBreath();
                    _curBreathingMainItem = backupBoardItem;
                }
            }
            else
            {
                InitBenchHighLight();
                InitBackupHighLight();
                if (_curBreathingMainItem != null)
                {
                    _curBreathingMainItem.StopBreath();
                    _curBreathingMainItem = null;
                }
            }
        }

        private void dragBetweenMainAndBackup(PointerEventData eventData, DragableItem dragableItem)
        {
            int mainBoardId = GetMainBoardId(eventData);
            int backupBoardId = GetReserveBoardId(eventData);
            if (MainPlayerCards.ContainsKey(mainBoardId) && (dragableItem as FormationDragableItem).BoardId != mainBoardId)
            {
                InitBackupHighLight();
                var mainBoardItem = MainPlayerCards[mainBoardId];
                if (_curBreathingMainItem != null)
                {
                    if (_curBreathingMainItem.BoardId != mainBoardItem.BoardId)
                    {
                        _curBreathingMainItem.StopBreath();
                        mainBoardItem.StartBreath();
                        _curBreathingMainItem = mainBoardItem;
                    }
                }
                else
                {
                    mainBoardItem.StartBreath();
                    _curBreathingMainItem = mainBoardItem;
                }
            }
            else if (ReservePlayerCards.ContainsKey(backupBoardId) && (dragableItem as FormationDragableItem).BoardId != backupBoardId)
            {
                InitHighLight();
                backupHighLightFrame.anchoredPosition = ReserveGrids[backupBoardId];
                var backupBoardItem = ReservePlayerCards[backupBoardId];
                if (_curBreathingMainItem != null)
                {
                    if (_curBreathingMainItem.BoardId != backupBoardItem.BoardId)
                    {
                        _curBreathingMainItem.StopBreath();
                        backupBoardItem.StartBreath();
                        _curBreathingMainItem = backupBoardItem;
                    }
                }
                else
                {
                    backupBoardItem.StartBreath();
                    _curBreathingMainItem = backupBoardItem;
                }
            }
            else
            {
                InitHighLight();
                InitBackupHighLight();
                if (_curBreathingMainItem != null)
                {
                    _curBreathingMainItem.StopBreath();
                    _curBreathingMainItem = null;
                }
            }
        }

        public void Swap(int swapType, FormationDragableItem origin, FormationDragableItem target)
        {
            AudioManager.Instance.PlaySound(AudioNames.ANI_PICKEXCHG);
            if (swapType == FormationSwapType.BenchToMan || swapType == FormationSwapType.MainToBench)
            {
                if (!_formation.CheckCanExchangeCard())
                {
                    // todo tips
                    Tips.PopError(ErrorID.FiveExchangeOneGameAtMost);
                    origin.Reset();
                    target.Reset();
                    return;
                }
            }
            if (swapType == FormationSwapType.BackupToBench)
            {
                _formation.SwapBenchWithBackup(origin.GetData().CardId, target.BoardId);
            }
            else if (swapType == FormationSwapType.BenchToBackup)
            {
                _formation.SwapBenchWithBackup(target.GetData().CardId, origin.BoardId);
            }
            else if (swapType == FormationSwapType.MainToBackup)
            {
                _formation.SwapMainWithBackup(target.GetData().CardId, origin.BoardId);
            }
            else if (swapType == FormationSwapType.BackupToMain)
            {
                _formation.SwapMainWithBackup(origin.GetData().CardId, target.BoardId);
            }
            else
            {
                _formation.SwapBoard(swapType, origin.BoardId, target.BoardId);
            }

            int originCardId = origin.GetData().CardId;
            int targetCardId = target.GetData().CardId;

            PlayerCard originPlayerCard = Player.CardManager.GetCard(originCardId);
            PlayerCard targetPlayerCard = Player.CardManager.GetCard(targetCardId);

            origin.InitData(targetPlayerCard, _formation.FormationId, stateSave);
            target.InitData(originPlayerCard, _formation.FormationId, stateSave);

            RefreshDragItemState(origin, targetPlayerCard);
            RefreshDragItemState(target, originPlayerCard);

            target.Drop();
            origin.Reset();

            switch (swapType)
            {
                case FormationSwapType.MainToMain:
                    origin.RefreshCombatEffectivenessInMain();
                    target.RefreshCombatEffectivenessInMain();
                    MainBoardChangeAction?.Invoke();
                    break;
                case FormationSwapType.MainToBench:
                    origin.RefreshCombatEffectivenessInMain();
                    target.RefreshCombatEffectivenessNormal();
                    MainBoardChangeAction?.Invoke();
                    break;
                case FormationSwapType.BenchToMan:
                    origin.RefreshCombatEffectivenessNormal();
                    target.RefreshCombatEffectivenessInMain();
                    MainBoardChangeAction?.Invoke();
                    break;
            }
            MainBoardOrBenchBoardChangeAction?.Invoke();
        }
        /// <summary>
        /// 把阵型的爆发数据同步过来
        /// </summary>
        /// <returns></returns>
        public void SyncFireStarState(Dictionary<int, List<SkillGiftItemData>> boardSkillList, Dictionary<int, List<SkillGiftItemData>> sectionGiftSkillList, int fireSection)
        {
            foreach (var _boardId in MainPlayerCards.Keys)
            {
                var _skList = boardSkillList[_boardId];
                var _cardid = MainPlayerCards[_boardId].GetData().CardId;

                foreach (var _sectionId in sectionGiftSkillList.Keys)
                {
                    var _dragItem = (MainPlayerCards[_boardId] as FormationMainDragableItem);
                    //0 没有， 1有，但没有激活， 2有且激活， 3有且激活且是爆发节技能
                    var skstate = 0;
                    foreach (var sk in sectionGiftSkillList[_sectionId])
                    {
                        if (sk.cardId == _cardid && skstate == 0)
                        {
                            skstate = 1;
                            if (sk.isUnLock && skstate == 1)
                            {
                                skstate = 2;
                                if (fireSection == _sectionId && skstate == 2)
                                {
                                    skstate = 3;
                                    break;
                                }
                            }
                        }
                    }
                    _dragItem.fireStarList[_sectionId - 1].transform.localScale = new Vector3(1f, 1f, 1f);
                    _dragItem.fireStarList[_sectionId - 1].gameObject.SetActive(true);
                    switch (skstate)
                    {
                        case 1:
                            SpriteManager.GetSprite(AtlasNames.Formation, "火图标灰", (s) => { _dragItem.fireStarList[_sectionId - 1].sprite = s; });
                            break;
                        case 2:
                            SpriteManager.GetSprite(AtlasNames.Formation, "火图标", (s) => { _dragItem.fireStarList[_sectionId - 1].sprite = s; });
                            break;
                        case 3:
                            SpriteManager.GetSprite(AtlasNames.Formation, "火图标", (s) => { _dragItem.fireStarList[_sectionId - 1].sprite = s; });
                            _dragItem.fireStarList[_sectionId - 1].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                            break;
                        default:
                            _dragItem.fireStarList[_sectionId - 1].gameObject.SetActive(false);
                            break;
                    }
                }
            }
        }

        public bool ChangeMainPlayerCardPos(FormationDragableItem item, int targetBoardId)
        {
            //if (item.BoardId == 105)
            //{
            //    // todo tips
            //    Tips.PopError(ErrorID.GoalKeeperCannotBeNull);
            //    item.Reset();
            //    return false;
            //}
            _formation.MoveStarterBoard(item.BoardId, targetBoardId);
            MainPlayerCards.Remove(item.BoardId);
            item.SetBoardId(targetBoardId);
            item.RefreshCombatEffectivenessInMain();
            MainPlayerCards.Add(targetBoardId, item);
            return true;
        }

        public void ChangeBenchPlayerCardPos(FormationDragableItem item, int targetBoardId)
        {
            _formation.MoveSubstituteBoard(item.BoardId, targetBoardId);
            BenchPlayerCards.Remove(item.BoardId);
            item.SetBoardId(targetBoardId);
            BenchPlayerCards.Add(targetBoardId, item);
        }

        int stateSave = 0;
        public void RefreshState()
        {
            ChangeState(stateSave);
        }

        public void ChangeState(int state = 0)
        {
            stateSave = state;
            foreach (var item in MainPlayerCards.Values)
            {
                item.ChangeState(state);
            }

            foreach (var item in BenchPlayerCards.Values)
            {
                item.ChangeState(state);
            }

            foreach (var item in ReservePlayerCards.Values)
            {
                item.ChangeState(state);
            }
            foreach (var item in ReserveLeftTempCards.Values)
            {
                item.ChangeState(state);
            }
            foreach (var item in ReserveRightTempCards.Values)
            {
                item.ChangeState(state);
            }

        }

        private async void HideSeparatedAera()
        {
            if (curHighLightName == "")
                return;
            var areaSprite = await SpriteProxy.GetSoccerFieldAreaSprite(curHighLightName, false);
            separatedSoccerAreas.Find(curHighLightName).GetComponent<Image>().sprite = areaSprite;

            int childNum = separatedSoccerAreas.childCount;
            for (int i = 0; i < childNum - 1; i++)
            {
                var img = separatedSoccerAreas.GetChild(i).GetComponent<Image>();
                img.DOFade(0, 0.2f).AddTo(this.gameObject);
            }
            separatedSoccerAreas.GetChild(childNum - 1).GetComponent<Image>().DOFade(0, 0.2f).OnComplete(() =>
            {
                separatedSoccerAreas.gameObject.SetActive(false);
            }).AddTo(this.gameObject);
            curHighLightName = "";
        }

        public void HideHighLightField()
        {
            if (highLightFieldShown == 0) return;
            highLightFieldShown = 0;
            highLightImg.gameObject.SetActive(false);
            HideLine();
        }

        private void ShowLine(int separateAreaId)
        {
            isLineShow = true;
            selectionLine.rectTransform.gameObject.SetActive(true);
            selectionLine.lineWidth = 12;
            selectionLine.texture = lineTexture;
            selectionLine.textureScale = 2f;
            selectionLine.color = new Color32(127, 247, 255, 255);

            selectionLine.points2 = separatedAeraVerts[separateAreaId];//注意，需要5个点，而不是四个
            selectionLine.Draw();
            lineTween = DOTween.To(value => selectionLine.textureOffset = value, 0, 2, 0.5f).SetLoops(-1).SetEase(Ease.Linear).AddTo(this.gameObject);

        }

        public void HideLine()
        {
            if (!isLineShow) return;
            isLineShow = false;
            lineTween.Kill();
            selectionLine.rectTransform.gameObject.SetActive(false);
        }

        private Vector2 GetHighLightWHVector2(int mainBoardId)
        {
            int row = Mathf.FloorToInt(mainBoardId / 100);
            if (row > 2)
            {
                float rate = 1.0f - (row - 2) * 0.02f;
                return new Vector2(baseMainItemWidth * rate + 4, baseMainItemHeight * rate + 4);
            }
            else
            {
                return new Vector2(baseMainItemWidth, baseMainItemHeight);
            }
        }

        public void CheckAndStopHighLightItem()
        {
            if (_curBreathingMainItem != null)
            {
                _curBreathingMainItem.StopBreath();
                _curBreathingMainItem = null;
            }
        }
    }
}
