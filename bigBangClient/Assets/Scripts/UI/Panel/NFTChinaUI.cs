using Babu;
using Babu.Client.Fsm;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using Protocol;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using YooAsset;
using Vector2 = UnityEngine.Vector2;

namespace BigBang.UI
{
    public class NFTChinaUI : APanelController
    {

        #region 基础

        [SerializeField] private Button closeBtn;

        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            LeftButton.OnClick += OnClickLeftButton;
            RightButton.OnClick += OnClickRightButton;
            DragArea1.DragBeginAction += DragBegin;
            DragArea1.DragMoveAction += DragMove;
            DragArea1.DragEndAction += DragEnd;
            DragArea2.DragBeginAction += DragBegin;
            DragArea2.DragMoveAction += DragMove;
            DragArea2.DragEndAction += DragEnd;
        }
        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            LeftButton.OnClick -= OnClickLeftButton;
            RightButton.OnClick -= OnClickRightButton;
            DragArea1.DragBeginAction -= DragBegin;
            DragArea1.DragMoveAction -= DragMove;
            DragArea1.DragEndAction -= DragEnd;
            DragArea2.DragBeginAction -= DragBegin;
            DragArea2.DragMoveAction -= DragMove;
            DragArea2.DragEndAction -= DragEnd;
        }

        /// <summary>
        /// 显示NFT哈希链
        /// </summary>
        private bool isShowNFT = true;

        protected override void OnPropertiesSet()
        {
            this.QueryNFTList();
            Process3DRes();
            InitOnce();
            Clear();
            SetShowItem(0);
        }

        private void QueryNFTList()
        {
            NetworkManager.Instance.GetNFTs((resp) =>
            {
                RefreshItemData(ProtoUtils.UnPackRepeatedField<NFTGoodsInfo>(resp.Goods));
                RefreshUI();
                PlayShowNameLabelAni();
            });
        }

        private void Clear()
        {
            ClearChangeNameLabelAni();
            ClearItemTrans();
            UnUseCollectionListDic.Clear();
        }
        private bool isInitOnce = false;
        private void InitOnce()
        {
            if (isInitOnce == true) return;
            isInitOnce = true;

            SetItemStartPos();
            SetUIFit();
            InitItem();
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);

            
            FsmManager.Instance.ChangeToState<StateHome>(new StateCommonUserData()
            {
                OpenUIAction = () =>
                {
                    UIController.Instance.HidePanel<NFTChinaUI>();
                    return System.Threading.Tasks.Task.CompletedTask;
                }
            });
        }

        #endregion

        #region UI适配

        [SerializeField] private RectTransform BaseItemTrans;
        [SerializeField] private RectTransform BottomTopTrans;
        [SerializeField] private RectTransform MidItemOwnedTrans;
        [SerializeField] private RectTransform MidItemHashBgTrans;
        private void SetUIFit()
        {
            RectTransform midItemTrans = CollectionNameItemList[1].GetComponent<RectTransform>();

            float baseY = Utility.ConvertLocalPosition(BaseItemTrans, Vector3.zero, midItemTrans.parent).y;
            float bottomY = Utility.ConvertLocalPosition(BottomTopTrans, Vector3.zero, midItemTrans.parent).y;
            float midItemY = (baseY + bottomY) / 2;

            float hwChange = 1385f / 720f;
            float hwScreen = (float)UIFrame.height / (float)UIFrame.width;

            if (hwScreen < hwChange)
            {
                MidItemOwnedTrans.pivot = new Vector2(0.5f, 0f);
                midItemY -= 110;
            }
            else
            {
                MidItemOwnedTrans.pivot = new Vector2(0.5f, 0.5f);
                midItemY -= 80;
            }

            midItemTrans.SetLocalPositionY(midItemY);

            LayoutRebuilder.ForceRebuildLayoutImmediate(MidItemHashBgTrans);
            LayoutRebuilder.ForceRebuildLayoutImmediate(MidItemOwnedTrans);
            LayoutRebuilder.ForceRebuildLayoutImmediate(CollectionNameItemList[1].ItemTrans);
        }

        #endregion

        #region 3D

        [SerializeField] private GameObject collectionAsset;
        private GameObject collectionGameObject;
        private Transform collectionTrans;
        [SerializeField] private RawImage collectionImg;
        private Camera collectionCamera;

        private void Process3DRes()
        {
            collectionGameObject = GameObject.Instantiate(collectionAsset);
            collectionTrans = collectionGameObject.transform;
            collectionCamera = collectionTrans.Find("Main Camera").GetComponent<Camera>();
            CameraManager.Instance.SetTexture(CameraID.Collection, collectionImg);

            UnUseCollectionTrans = (new GameObject("UnUseCollectionTrans")).transform;
            UnUseCollectionTrans.SetParent(collectionTrans);
            UnUseCollectionTrans.gameObject.SetActive(false);

            //float cameraY = Utility.Lerp(56.3f, 71.3f, Utility.GetScreenLerpT());
            //Vector3 camPos = collectionCamera.transform.localPosition;
            //camPos.y = cameraY;
            //collectionCamera.transform.localPosition = camPos;

            isCanHide = true;
        }

        private bool isCanHide = false;
        protected override void WhileHiding()
        {
            if (isCanHide == false) return;
            Clear();
            GameObject.Destroy(collectionGameObject);
            CameraManager.Instance.ReleaseTexture(CameraID.Collection, collectionImg);
            isCanHide = false;
        }



        #endregion

        #region 藏品3D对象池

        private Transform UnUseCollectionTrans;
        private Dictionary<int, Queue<Transform>> UnUseCollectionListDic = new();

        private void Get3DModel(ChinaNFTItem item, Transform parent, Action<Transform> callBack)
        {
            if (UnUseCollectionListDic.ContainsKey(item.nftGoodsConfig.Id) && UnUseCollectionListDic[item.nftGoodsConfig.Id].Count > 0)
            {
                Transform itemTrans = UnUseCollectionListDic[item.nftGoodsConfig.Id].Dequeue();
                itemTrans.SetParent(collectionTrans);
                callBack?.Invoke(itemTrans);
                return;
            }

#if !UNITY_WEBGL
            string prefabPath = ResourcePath.ColletionsPAth + "collection" + item.nftGoodsConfig.Id + ".prefab";
            {
                var h = YooAssets.LoadAssetSync<GameObject>(prefabPath);
                GameObject prefab = h.AssetObject as GameObject;
                h.Release();
                prefab.SetLayerInThisAndAllChild(Layers.Collection);
                Transform itemTrans = GameObject.Instantiate(prefab, collectionTrans).transform;
                callBack?.Invoke(itemTrans);
            }
#else
            {
                string prefabPath = ResourcePath.ColletionsPAth + "collection" + item.nftGoodsConfig.Id + ".prefab";
                var h = YooAssets.LoadAssetAsync<GameObject>(prefabPath);
                h.Completed += _ =>
                {
                    GameObject prefab = h.AssetObject as GameObject;
                    h.Release();

                    prefab.SetLayerInThisAndAllChild(Layers.Collection);
                    Transform itemTrans = GameObject.Instantiate(prefab, collectionTrans).transform;
                    callBack?.Invoke(itemTrans);
                };
            }
#endif
        }
        private void UnUse3DModel(int ntfId, Transform itemTrans)
        {
            itemTrans.parent = UnUseCollectionTrans;
            Queue<Transform> itemTransQueue = null;
            if (UnUseCollectionListDic.ContainsKey(ntfId) == false)
            {
                itemTransQueue = new();
                UnUseCollectionListDic.Add(ntfId, itemTransQueue);
            }
            else
            {
                itemTransQueue = UnUseCollectionListDic[ntfId];
            }
            itemTransQueue.Enqueue(itemTrans);
        }

        #endregion

        #region 获取收藏品在3D中的位置
        [SerializeField] private RectTransform ModelStartPointTransLeft;
        [SerializeField] private RectTransform ModelStartPointTransMid;
        [SerializeField] private RectTransform ModelStartPointTransRight;
        private Vector3 GetItemPosIn3D(Vector3 screenPoint)
        {
            Vector3 hitWorldPoint = Vector3.zero;
            screenPoint = UIFrame.ChangeUIScreenPointTo3DScreenPoint(screenPoint);
            Vector3 viewport = collectionCamera.ScreenToViewportPoint(screenPoint);
            Ray ray = collectionCamera.ViewportPointToRay(viewport);
            //Debug.DrawRay(ray.origin, ray.direction * 200000, Color.red);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.tag == Tags.Collection)
                {
                    return hits[i].point;
                }
            }
            Debug.LogWarning("GetItemPosIn3D ， 找不到位置 , screenPoint = " + screenPoint);
            return Vector3.zero;
        }

        private List<Vector3> itemPosList = new();
        private void SetItemStartPos()
        {
            Vector3 leftScreenPoint = Utility.ConvertLocalPositionToScreenPosition(ModelStartPointTransLeft, Vector3.zero, uiCamera);
            Vector3 leftStartPos = GetItemPosIn3D(leftScreenPoint);
            Vector3 leftLeftStartPos = leftStartPos + (new Vector3(20, 0, 0));

            Vector3 rightScreenPoint = Utility.ConvertLocalPositionToScreenPosition(ModelStartPointTransRight, Vector3.zero, uiCamera);
            Vector3 rightStartPos = GetItemPosIn3D(rightScreenPoint);
            Vector3 rightRightStartPos = rightStartPos + (new Vector3(-20, 0, 0));

            Vector3 midScreenPoint = Utility.ConvertLocalPositionToScreenPosition(ModelStartPointTransMid, Vector3.zero, uiCamera);
            Vector3 midStartPos = GetItemPosIn3D(midScreenPoint);

            itemPosList.Add(leftLeftStartPos);
            itemPosList.Add(leftStartPos);
            itemPosList.Add(midStartPos);
            itemPosList.Add(rightStartPos);
            itemPosList.Add(rightRightStartPos);
        }

        private Camera _uiCamera;
        private Camera uiCamera
        {
            get
            {
                if (_uiCamera == null)
                {
                    _uiCamera = UIController.Instance.GetCamera();
                }
                return _uiCamera;
            }
        }

        #endregion

        #region 藏品数据

        private Dictionary<int, NFTGoodsInfo> infoDic = new();
        private List<ChinaNFTItem> itemList = new();
        public class ChinaNFTItem
        {
            public NftGoodsConfig nftGoodsConfig;
            public NFTGoodsInfo nftGoodsInfo;
            public Transform itemTrans;
            public int index;

            private Sequence rotSeq;
            public void StartRotate()
            {
                StopRotate();
                rotSeq = DOTween.Sequence();
                rotSeq.Append(itemTrans.DORotate(new Vector3(0, 360, 0), 5f, RotateMode.WorldAxisAdd).SetEase(Ease.Linear));
                rotSeq.SetLoops(-1);
            }
            public void StopRotate()
            {
                rotSeq?.Kill();
                rotSeq = null;
            }
        }

        private void RefreshItemData(List<NFTGoodsInfo> nftGoodsInfoList)
        {
            infoDic.Clear();
            foreach (NFTGoodsInfo nftGoodsInfo in nftGoodsInfoList)
            {
                int key = int.Parse(nftGoodsInfo.ItemId.Split('-')[^1]);
                if (infoDic.ContainsKey(key) == false)
                {
                    infoDic.Add(key, nftGoodsInfo);
                }
            }
            foreach (ChinaNFTItem chinaNFTItem in itemList)
            {
                if (chinaNFTItem.nftGoodsConfig != null && infoDic.ContainsKey(chinaNFTItem.nftGoodsConfig.Id))
                {
                    chinaNFTItem.nftGoodsInfo = infoDic[chinaNFTItem.nftGoodsConfig.Id];
                }
            }
        }

        #endregion

        #region 藏品pageview

        [SerializeField] private BabuButton LeftButton;
        [SerializeField] private BabuButton RightButton;
        private void OnClickLeftButton(BabuButton sender)
        {
            ShowLastItem();
        }
        private void OnClickRightButton(BabuButton sender)
        {
            ShowNextItem();
        }

        private void ClearItemTrans()
        {
            foreach (ChinaNFTItem item in itemList)
            {
                item.StopRotate();
                if (item.itemTrans != null)
                {
                    UnUse3DModel(item.nftGoodsConfig.Id, item.itemTrans);
                    item.itemTrans = null;
                }

            }
            moveSeq?.Kill();
            moveSeq = null;
        }

        private float bigScale = 3.0f;
        private int nowItemIndex = 0;
        private int nowCfgIndex = 0;
        private void InitItem()
        {
            nowCfgIndex = 0;
            nowItemIndex = 0;

            itemList.Clear();
            for (int i = 0; i < 4; i++)
            {
                ChinaNFTItem chinaNFTItem = new();
                chinaNFTItem.index = i;
                itemList.Add(chinaNFTItem);
            }
        }

        void SetShowItem(int cfgIndex)
        {
            nowCfgIndex = cfgIndex;
            nowItemIndex = 0;
            foreach (ChinaNFTItem item in itemList)
            {
                if (item.itemTrans != null)
                {
                    UnUse3DModel(item.nftGoodsConfig.Id, item.itemTrans);
                    item.itemTrans = null;
                }
            }

            int leftIndex = (nowItemIndex + itemList.Count - 1) % itemList.Count;
            int rightIndex = (nowItemIndex + 1) % itemList.Count;
            int leftCfgIndex = (nowCfgIndex + Configs.NftGoods.GetConfigList().Count - 1) % Configs.NftGoods.GetConfigList().Count;
            int rightCfgIndex = (nowCfgIndex + 1) % Configs.NftGoods.GetConfigList().Count;
            ChinaNFTItem leftItem = itemList[leftIndex];
            leftItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[leftCfgIndex];
            ChinaNFTItem midItem = itemList[nowItemIndex];
            midItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[nowCfgIndex];
            ChinaNFTItem rightItem = itemList[rightIndex];
            rightItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[rightCfgIndex];

            List<ChinaNFTItem> chinaNFTItemList = new();
            chinaNFTItemList.Add(leftItem);
            chinaNFTItemList.Add(midItem);
            chinaNFTItemList.Add(rightItem);

            for (int i = 0; i < 3; i++)
            {
                int localIndex = i;
                ChinaNFTItem item = chinaNFTItemList[i];
                if (item.itemTrans == null)
                {
                    Get3DModel(item, collectionTrans, (Transform newModelTrans) =>
                    {
                        item.itemTrans = newModelTrans;
                        item.StartRotate();
                        SetItemPos(item, localIndex);
                    });
                }
                else
                {
                    SetItemPos(item, localIndex);
                }
            }

            RefreshUI();
        }

        void SetItemPos(ChinaNFTItem item, int index)
        {
            item.itemTrans.gameObject.SetActive(true);
            item.itemTrans.localRotation = Quaternion.identity;

            if (index == 1)
            {
                item.itemTrans.position = itemPosList[index + 1] + new Vector3(0, item.nftGoodsConfig.ZOffset * bigScale, 0);
                item.itemTrans.localScale = Vector3.one * bigScale * item.nftGoodsConfig.Scale;
            }
            else
            {
                item.itemTrans.position = itemPosList[index + 1] + new Vector3(0, item.nftGoodsConfig.ZOffset, 0);
                item.itemTrans.localScale = Vector3.one * item.nftGoodsConfig.Scale;
            }
        }

        private Sequence moveSeq;
        private bool _isPlaying = false;
        private float moveTime = 0.8f;
        private void ShowNextItem()
        {
            if (_isPlaying) return;
            _isPlaying = true;

            int rightRightIndex = (nowItemIndex + 2) % itemList.Count;
            ChinaNFTItem rightRightItem = itemList[rightRightIndex];
            int rightRightCfgIndex = (nowCfgIndex + 2) % Configs.NftGoods.GetConfigList().Count;
            rightRightItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[rightRightCfgIndex];

            if (rightRightItem.nftGoodsConfig != null && infoDic.ContainsKey(rightRightItem.nftGoodsConfig.Id))
            {
                rightRightItem.nftGoodsInfo = infoDic[rightRightItem.nftGoodsConfig.Id];
            }
            else
            {
                rightRightItem.nftGoodsInfo = null;
            }

            if (rightRightItem.itemTrans == null)
            {
                Get3DModel(rightRightItem, collectionTrans, (Transform newModelTrans) =>
                {
                    rightRightItem.itemTrans = newModelTrans;
                    ShowNextItemInternal();
                });
            }
            else
            {
                ShowNextItemInternal();
            }
        }

        void ShowNextItemInternal()
        {
            int leftIndex = (nowItemIndex + itemList.Count - 1) % itemList.Count;
            int rightIndex = (nowItemIndex + 1) % itemList.Count;
            int rightRightIndex = (nowItemIndex + 2) % itemList.Count;

            int leftCfgIndex = (nowCfgIndex + Configs.NftGoods.GetConfigList().Count - 1) % Configs.NftGoods.GetConfigList().Count;
            int rightCfgIndex = (nowCfgIndex + 1) % Configs.NftGoods.GetConfigList().Count;
            int rightRightCfgIndex = (nowCfgIndex + 2) % Configs.NftGoods.GetConfigList().Count;

            ChinaNFTItem leftItem = itemList[leftIndex];
            leftItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[leftCfgIndex];
            ChinaNFTItem midItem = itemList[nowItemIndex];
            midItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[nowCfgIndex];
            ChinaNFTItem rightItem = itemList[rightIndex];
            rightItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[rightCfgIndex];
            ChinaNFTItem rightRightItem = itemList[rightRightIndex];
            rightRightItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[rightRightCfgIndex];

            List<ChinaNFTItem> chinaNFTItemList = new();
            chinaNFTItemList.Add(leftItem);
            chinaNFTItemList.Add(midItem);
            chinaNFTItemList.Add(rightItem);
            chinaNFTItemList.Add(rightRightItem);

            rightRightItem.itemTrans.position = itemPosList[^1] + new Vector3(0, rightRightItem.nftGoodsConfig.ZOffset);
            rightRightItem.itemTrans.localRotation = Quaternion.identity;
            rightRightItem.itemTrans.localScale = Vector3.one * rightRightItem.nftGoodsConfig.Scale;
            rightRightItem.itemTrans.gameObject.SetActive(true);

            moveSeq = DOTween.Sequence();

            for (int i = 0; i < 4; i++)
            {
                ChinaNFTItem item = chinaNFTItemList[i];
                moveSeq.Join(item.itemTrans.DOMove(i == 2 ? itemPosList[i] + new Vector3(0, item.nftGoodsConfig.ZOffset * bigScale) : itemPosList[i] + new Vector3(0, item.nftGoodsConfig.ZOffset), moveTime));
                moveSeq.Join(item.itemTrans.DOScale(i == 2 ? item.nftGoodsConfig.Scale * bigScale : item.nftGoodsConfig.Scale, moveTime));
                item.StartRotate();
            }
            moveSeq.AppendCallback(() =>
            {
                leftItem.StopRotate();
                UnUse3DModel(leftItem.nftGoodsConfig.Id, leftItem.itemTrans);
                leftItem.itemTrans = null;
                PlayShowNameLabelAni(() =>
                {
                    moveSeq?.Kill();
                    moveSeq = null;
                    _isPlaying = false;
                });
            });
            nowItemIndex++;
            nowItemIndex %= itemList.Count;
            nowCfgIndex++;
            nowCfgIndex %= Configs.NftGoods.GetConfigList().Count;
            PlayHideNameLabelAni(RefreshUI);
        }

        private void ShowLastItem()
        {
            if (_isPlaying) return;
            _isPlaying = true;

            int leftLeftIndex = (nowItemIndex + itemList.Count - 2) % itemList.Count;
            ChinaNFTItem leftLeftItem = itemList[leftLeftIndex];
            int leftLeftCfgIndex = (nowCfgIndex + Configs.NftGoods.GetConfigList().Count - 2) % Configs.NftGoods.GetConfigList().Count;
            leftLeftItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[leftLeftCfgIndex];

            if (leftLeftItem.nftGoodsConfig != null && infoDic.ContainsKey(leftLeftItem.nftGoodsConfig.Id))
            {
                leftLeftItem.nftGoodsInfo = infoDic[leftLeftItem.nftGoodsConfig.Id];
            }
            else
            {
                leftLeftItem.nftGoodsInfo = null;
            }

            if (leftLeftItem.itemTrans == null)
            {
                Get3DModel(leftLeftItem, collectionTrans, (Transform newModelTrans) =>
                {
                    leftLeftItem.itemTrans = newModelTrans;
                    ShowLastItemInternal();
                });
            }
            else
            {
                ShowLastItemInternal();
            }
        }

        void ShowLastItemInternal()
        {
            int leftLeftIndex = (nowItemIndex + itemList.Count - 2) % itemList.Count;
            int leftIndex = (nowItemIndex + itemList.Count - 1) % itemList.Count;
            int rightIndex = (nowItemIndex + 1) % itemList.Count;

            int leftLeftCfgIndex = (nowCfgIndex + Configs.NftGoods.GetConfigList().Count - 2) % Configs.NftGoods.GetConfigList().Count;
            int leftCfgIndex = (nowCfgIndex + Configs.NftGoods.GetConfigList().Count - 1) % Configs.NftGoods.GetConfigList().Count;
            int rightCfgIndex = (nowCfgIndex + 1) % Configs.NftGoods.GetConfigList().Count;

            ChinaNFTItem leftLeftItem = itemList[leftLeftIndex];
            leftLeftItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[leftLeftCfgIndex];
            ChinaNFTItem leftItem = itemList[leftIndex];
            leftItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[leftCfgIndex];
            ChinaNFTItem midItem = itemList[nowItemIndex];
            midItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[nowCfgIndex];
            ChinaNFTItem rightItem = itemList[rightIndex];
            rightItem.nftGoodsConfig = Configs.NftGoods.GetConfigList()[rightCfgIndex];

            List<ChinaNFTItem> chinaNFTItemList = new();
            chinaNFTItemList.Add(leftLeftItem);
            chinaNFTItemList.Add(leftItem);
            chinaNFTItemList.Add(midItem);
            chinaNFTItemList.Add(rightItem);

            leftLeftItem.itemTrans.position = itemPosList[0] + new Vector3(0, leftLeftItem.nftGoodsConfig.ZOffset);
            leftLeftItem.itemTrans.localRotation = Quaternion.identity;
            leftLeftItem.itemTrans.localScale = Vector3.one * leftLeftItem.nftGoodsConfig.Scale;
            leftLeftItem.itemTrans.gameObject.SetActive(true);

            moveSeq = DOTween.Sequence();

            for (int i = 0; i < 4; i++)
            {
                ChinaNFTItem item = chinaNFTItemList[i];
                moveSeq.Join(item.itemTrans.DOMove(i == 1 ? itemPosList[i + 1] + new Vector3(0, item.nftGoodsConfig.ZOffset * bigScale) : itemPosList[i + 1] + new Vector3(0, item.nftGoodsConfig.ZOffset), moveTime));
                moveSeq.Join(item.itemTrans.DOScale(i == 1 ? item.nftGoodsConfig.Scale * bigScale : item.nftGoodsConfig.Scale, moveTime));
                item.StartRotate();
            }
            moveSeq.AppendCallback(() =>
            {
                rightItem.StopRotate();
                UnUse3DModel(rightItem.nftGoodsConfig.Id, rightItem.itemTrans);
                rightItem.itemTrans = null;
                PlayShowNameLabelAni(() =>
                {
                    moveSeq?.Kill();
                    moveSeq = null;
                    _isPlaying = false;
                });
            });
            nowItemIndex = nowItemIndex + itemList.Count - 1;
            nowItemIndex %= itemList.Count;
            nowCfgIndex = nowCfgIndex + Configs.NftGoods.GetConfigList().Count - 1;
            nowCfgIndex %= Configs.NftGoods.GetConfigList().Count;
            PlayHideNameLabelAni(RefreshUI);
        }

        #endregion

        #region 刷新UI

        [SerializeField] private Image progressBgImage = null;
        [SerializeField] private TMP_Text progressText = null;
        [SerializeField] private TMP_Text effectText = null;
        [SerializeField] private RectTransform nameImageParent = null;
        [SerializeField] private Image nameImage = null;
        [SerializeField] private Image hasGetImage = null;
        [SerializeField] private List<CollectionNameItem> CollectionNameItemList;
        [SerializeField] private List<FloatAnim> FloatAnimList;

        private void RefreshUI()
        {
            int leftIndex = (nowItemIndex + itemList.Count - 1) % itemList.Count;
            int rightIndex = (nowItemIndex + 1) % itemList.Count;

            ChinaNFTItem leftItem = itemList[leftIndex];
            ChinaNFTItem midItem = itemList[nowItemIndex];
            ChinaNFTItem rightItem = itemList[rightIndex];

            SetNameItem(leftItem, CollectionNameItemList[0]);
            SetNameItem(midItem, CollectionNameItemList[1], true);
            SetNameItem(rightItem, CollectionNameItemList[2]);

            LayoutRebuilder.ForceRebuildLayoutImmediate(MidItemHashBgTrans);
            LayoutRebuilder.ForceRebuildLayoutImmediate(MidItemOwnedTrans);
            LayoutRebuilder.ForceRebuildLayoutImmediate(CollectionNameItemList[2].ItemTrans);
        }

        [SerializeField] private GameObject midItemHashImageLightGo;
        [SerializeField] private GameObject midItemHashImageGrayGo;
        [SerializeField] private GameObject LineStarParticleGo;
        [SerializeField] private Color GrayColor;
        [SerializeField] private Color WhiteColor;
        [SerializeField] private Color YellowColor;
        private async void SetNameItem(ChinaNFTItem item, CollectionNameItem collectionNameItem, bool isMid = false)
        {
            bool hasGet = item.nftGoodsInfo != null;

            if (isMid)
            {
                collectionNameItem.Owned.SetActive(true);

                nameImage.sprite = await SpriteProxy.GetNFTTitleSprite(item.nftGoodsConfig.Id);
                nameImage.SetNativeSize();
                effectText.text = item.nftGoodsConfig.Buffname;
                progressBgImage.gameObject.SetActive(!hasGet);
                hasGetImage.gameObject.SetActive(hasGet);
                if (!hasGet) progressText.text = "充值达到<color=#ffe259>{0}</color>/{1}获得藏品".SafeFormat(Player.ShopManager.SumCost, item.nftGoodsConfig.Price);

                collectionNameItem.HashPos.transform.parent.gameObject.SetActive(hasGet && isShowNFT);
                midItemHashImageLightGo.SetActive(hasGet && isShowNFT);
                midItemHashImageGrayGo.SetActive(false);//不显示哈希占位符
                LineStarParticleGo.SetActive(hasGet && isShowNFT);

                foreach (FloatAnim floatAnim in FloatAnimList)
                {
                    if (hasGet)
                    {
                        floatAnim.StartPlay();
                    }
                    else
                    {
                        floatAnim.StopPlay();
                    }
                }

                if (isShowNFT)
                {
                    if (hasGet)
                    {
                        collectionNameItem.HashPos.color = YellowColor;
                        if(string.IsNullOrWhiteSpace(item.nftGoodsInfo.HashAddress))
                        {
                            collectionNameItem.HashPos.transform.parent.gameObject.SetActive(false);
                        }
                        else
                        {
                            collectionNameItem.HashPos.text = item.nftGoodsInfo.HashAddress;
                        }
                    }
                    else
                    {
                        collectionNameItem.HashPos.color = GrayColor;
                        collectionNameItem.HashPos.text = "******";
                    }
                }
            }
            else
            {
                collectionNameItem.NotOwned.SetActive(!hasGet);
                collectionNameItem.Owned.SetActive(hasGet);
                collectionNameItem.NotOwnedHash.SetActive(false);//不显示哈希占位符
                collectionNameItem.OwnedHash.SetActive(isShowNFT);

                if (hasGet)
                {
                    if (isShowNFT)
                    {
                        if (string.IsNullOrWhiteSpace(item.nftGoodsInfo.HashAddress))
                        {
                            collectionNameItem.OwnedHash.SetActive(false);
                        }
                        else
                        {
                            if (item.nftGoodsInfo.HashAddress.Length >= 20)
                            {
                                collectionNameItem.HashPos.text = item.nftGoodsInfo.HashAddress.Substring(0, 15) + "......";
                            }
                            else
                            {
                                collectionNameItem.HashPos.text = item.nftGoodsInfo.HashAddress;
                            }
                        }
                    }
                    collectionNameItem.NameText.text = item.nftGoodsConfig.Name;
                }
            }



        }

        #endregion

        #region 手动旋转

        [SerializeField] private DragActionComponent DragArea1;
        [SerializeField] private DragActionComponent DragArea2;

        private float oldX;
        private void DragBegin(PointerEventData eventData)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_SELECT);

            oldX = ConvertScreenToX(eventData.position);
            ChinaNFTItem midItem = itemList[nowItemIndex];
            midItem.StopRotate();
        }
        private void DragMove(PointerEventData eventData)
        {
            float newX = ConvertScreenToX(eventData.position);
            ChinaNFTItem midItem = itemList[nowItemIndex];
            if (midItem.itemTrans != null) midItem.itemTrans.DORotate(new Vector3(0, oldX - newX, 0), 0f, RotateMode.WorldAxisAdd).SetEase(Ease.Linear);
            oldX = newX;
        }
        private void DragEnd(PointerEventData eventData)
        {
            ChinaNFTItem midItem = itemList[nowItemIndex];
            midItem.StartRotate();
        }
        private float ConvertScreenToX(Vector3 screenPoint)
        {
            return Utility.ConvertScreenPositionToLocalPosition(ModelStartPointTransMid, screenPoint, uiCamera).x;
        }

        #endregion

        #region 标签显示与隐藏

        private Sequence changeNameLabelSeq;
        private void ClearChangeNameLabelAni()
        {
            changeNameLabelSeq?.Kill();
            changeNameLabelSeq = null;
            foreach (CollectionNameItem collectionNameItem in CollectionNameItemList)
            {
                collectionNameItem.ItemTrans.gameObject.SetAlpha(0);
            }
            nameImageParent.gameObject.SetAlpha(0);
            hasGetImage.SetAlpha(0);
            hasGetImage.transform.localScale = Vector3.one * 3f;
        }
        private void PlayHideNameLabelAni(Action OnPlayEnd = null)
        {
            changeNameLabelSeq = DOTween.Sequence();
            foreach (CollectionNameItem collectionNameItem in CollectionNameItemList)
            {
                changeNameLabelSeq.Join(collectionNameItem.ItemTrans.gameObject.DOFade(0f, 0.3f));
            }
            changeNameLabelSeq.Join(nameImageParent.gameObject.DOFade(0f, 0.3f));
            changeNameLabelSeq.Join(hasGetImage.DOFade(0f, 0.2f));
            changeNameLabelSeq.Join(hasGetImage.transform.DOScale(3f, 0.2f));
            changeNameLabelSeq.AppendCallback(() =>
            {
                OnPlayEnd?.Invoke();
                changeNameLabelSeq?.Kill();
                changeNameLabelSeq = null;
            });
        }
        private void PlayShowNameLabelAni(Action OnPlayEnd = null)
        {
            changeNameLabelSeq = DOTween.Sequence();
            foreach (CollectionNameItem collectionNameItem in CollectionNameItemList)
            {
                changeNameLabelSeq.Join(collectionNameItem.ItemTrans.gameObject.DOFade(1f, 0.3f));
            }
            changeNameLabelSeq.Join(nameImageParent.gameObject.DOFade(1f, 0.3f));
            changeNameLabelSeq.Join(hasGetImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
            changeNameLabelSeq.Join(hasGetImage.DOFade(1f, 0.3f));
            changeNameLabelSeq.AppendCallback(() =>
            {
                OnPlayEnd?.Invoke();
                changeNameLabelSeq?.Kill();
                changeNameLabelSeq = null;
            });
        }

        #endregion


    }
}
