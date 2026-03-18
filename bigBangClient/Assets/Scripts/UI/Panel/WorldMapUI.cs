using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;
using static BigBang.ClassicManager;
using static BigBang.WorldMap3DItem;
using Vector2 = UnityEngine.Vector2;

namespace BigBang.UI
{
    [Serializable]
    public class WorldMapUIProperties : PanelProperties
    {
        public bool isOpenByOpenWindow = true;
        public int openLevel;
        public WorldMapUIProperties()
        {

        }
    }

    public class WorldMapUI : APanelController<WorldMapUIProperties>
    {

        [SerializeField] private WorldMapUIItemAdapter adapter;
        [SerializeField] private GameObject worldMap3DItemPrefab;
        [SerializeField] private RawImage worldMapImg;
        [SerializeField] private CanvasGroup bottomUi;
        [SerializeField] private CanvasGroup topBar = null;
        [SerializeField] private Image mask;
        [SerializeField] public WorldMapUIAnim Anim;
        private bool isLoadingTheContinent = true;

        [SerializeField] private GameObject refWorldMap;
        private GameObject worldMapGameObject;

        [SerializeField] WorldMapUIGuide worldMapUIGuide;

        public static bool getNewCountry;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void AddListeners()
        {
            EventManager.Instance.Register(EventID.OnClickWorldUIItem, OnClickWorldUIItem);
            EventManager.Instance.Register(EventID.OnNewCountry, OnNewCountry);
            formationButton.OnClick += OnClickFormationButton;
        }

        protected override void RemoveListeners()
        {
            EventManager.Instance.Unregister(EventID.OnClickWorldUIItem, OnClickWorldUIItem);
            EventManager.Instance.Unregister(EventID.OnNewCountry, OnNewCountry);
            formationButton.OnClick -= OnClickFormationButton;
        }

        [SerializeField] private BabuButton formationButton = null;
        private void OnClickFormationButton(BabuButton button)
        {
            Formation formation = Player.FightManager.FormationController.GetFormation(FormationID.PVE);
            UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(formation, false, FormationUI.FormationShowType.Formation, FormationID.PVE));
        }

        [SerializeField] private RectTransform top;
        [SerializeField] private RectTransform bottom;
        [SerializeField] private RectTransform left;
        [SerializeField] private RectTransform right;
        private bool isDoingWorldToContinentAni = false;
        private void OnWorldToContinent(WorldMap3DItem worldMap3DItem = null)
        {
            WorldMap3DItem targetWorldMap3DItem = null;
            if (worldMap3DItem == null)
                targetWorldMap3DItem = worldMap3DItemNow;
            else
                targetWorldMap3DItem = worldMap3DItem;


            if (!isLoadingTheContinent) return;
            TouchManager.Instance.DisableTouch();
            isDoingWorldToContinentAni = true;
            //isLoadingTheContinent = false;
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            Sequence seq = DOTween.Sequence();

            //UI隐藏动画
            seq.AppendCallback(() =>
            {
                bottomUi.interactable = false;
                topBar.interactable = false;
                worldMapUIGuide.OnHideHole();
            });
            // 镜头拉近动画
            seq.Append(Anim.MoveCameraIn(targetWorldMap3DItem));
            seq.Insert(0, top.DOAnchorPosY(280, 0.25f));
            seq.Insert(0, bottom.DOAnchorPosY(-300, 0.25f));
            seq.Insert(0, left.DOAnchorPosX(-422f, 0.25f));
            seq.Insert(0, right.DOAnchorPosX(458.4f, 0.25f));
            seq.Insert(0.2f, mask.DOFade(0.5f, 0.3f).SetEase(Ease.Linear));

            seq.AppendCallback(() =>
            {
                worldMapUIGuide.OnGuideClickCountryEnd();
                Player.BattleManager.classicTeamData = null;
                ClassicManager.Instance.OpenClassicCountryUI(targetWorldMap3DItem.data.challengeCountryConfig.Id, -1, () => { isDoingWorldToContinentAni = false; });
            });


            //int mapId = Player.ChallengeManager.MapId;
            //if (Player.ChallengeManager.IsNewMap())
            //{
            //    seq.InsertCallback(0.45f, () =>
            //    {
            //        TouchManager.Instance.EnableTouch();
            //        UIController.Instance.OpenWindow<ChallengeAreaEnterUI>(new ChallengeAreaEnterUIProperties(mapId));
            //        isLoadingTheContinent = true;
            //    });
            //}
            //else
            //{
            //    seq.AppendCallback(() =>
            //    {
            //        int mapId = Player.ChallengeManager.MapId;
            //        SceneManagerFor3D.LoadAddressableSceneAdditive(() =>
            //        {
            //            UIController.Instance.ShowPanel<ChallengeUI>(new ChallengeUIProperties(mapId, false));
            //            isLoadingTheContinent = true;
            //        });
            //    });
            //}
        }

        private readonly float newCityTime = 2f;
        //新国家现实
        public void OnNewCountry(object[] args)
        {
            TouchManager.Instance.DisableTouch();
            Sequence seq = DOTween.Sequence();
            string startNode = Player.BattleManager.classicCountryLevelData.challengeCountryConfig.NodeName;
            int startCountId = Player.BattleManager.classicCountryLevelData.challengeCountryConfig.Id;
            string endNode = "";
            foreach (var config in Configs.ChallengeCountry.GetConfigList())
            {
                if (config.Unlock == startCountId)
                {
                    endNode = config.NodeName;
                    break;
                }
            }

            ComPlaneV1.gameObject.SetActive(true);
            GotoUIItem(endNode, true, null, newCityTime);
            seq.Append(chNodeRootScript.PlayPlaneAni(startNode, endNode, newCityTime));
            seq.Insert(0f, chNodeRootScript.TakeOff());
            seq.Insert(newCityTime - 0.7f, chNodeRootScript.Land());
            seq.AppendCallback(() =>
            {
                chNodeRootTrans.gameObject.SetActive(true);
                Player.BattleManager.showCounties = true;
                TouchManager.Instance.EnableTouch();
                ComPlaneV1.gameObject.SetActive(false);
                Refresh3DItem();
            });
            seq.Join(chNodeRootScript.Land());
            seq.AddTo(this.gameObject);
        }

        public void OnClickWorldUIItem(object[] args)
        {
            WorldMapUIItem worldMapUIItem = (WorldMapUIItem)args[0];

            if (worldMapUIItem.data.targetCountryId != 0 && worldMap3DItemDic.ContainsKey(worldMapUIItem.data.targetCountryId) == true)
            {
                GotoUIItem(worldMap3DItemDic[worldMapUIItem.data.targetCountryId].data.challengeCountryConfig.NodeName);
            }
            else
            {
                GotoUIItem(worldMap3DItemDic[ClassicManager.Instance.mapClubFirstDic[worldMapUIItem.data.challengeMapConfig.Id]].data.challengeCountryConfig.NodeName);
            }
        }

        Tween clickWorldUIItemTween = null;
        public Transform GetLastGotoNodeTrans()
        {
            return worldMap3DItemNow.GetMidTrans();
        }
        private void GotoUIItem(string nodeName, bool useAni = true, Action callback = null, float time = 0.8f)
        {

            StopClickWorldUIItemTween();

            Anim.StopIdle();

            Quaternion quaternionStart = WheelTrans.rotation;
            WheelTrans.rotation = GetClickWorldUIItemRot(nodeName);
            WheelTrans.rotation = GetClickWorldUIItemRot(nodeName);
            WheelTrans.rotation = GetClickWorldUIItemRot(nodeName);
            Quaternion quaternionEnd = WheelTrans.rotation;

            if (useAni)
            {

                WheelTrans.rotation = quaternionStart;

                clickWorldUIItemTween = WheelTrans.DORotateQuaternion(quaternionEnd, time).OnUpdate(() =>
                {
                    Update3DItemFace();
                }).OnComplete(() =>
                {
                    Update3DItemFace();
                    Anim.StartIdle();
                    callback?.Invoke();
                });
            }
            else
            {
                Anim.StopIdle();
                WheelTrans.rotation = quaternionEnd;
                callback?.Invoke();
            }
            Update3DItemFace();

            //Debug.Log(worldMapUIItem.data.challengeMapConfig.Des);
        }

        private void StopClickWorldUIItemTween()
        {
            if (clickWorldUIItemTween != null)
            {
                clickWorldUIItemTween.Kill();
                clickWorldUIItemTween = null;
            }
        }

        private Quaternion GetClickWorldUIItemRot(string nodeName)
        {
            Quaternion quaternionStart = WheelTrans.rotation;
            var uiScreenMidPoint = UIFrame.GetFixMidScreenPointInUiCamera();
            var screenPoint = UIFrame.ChangeUIScreenPointTo3DScreenPoint(uiScreenMidPoint);
            var viewport = UIController.Instance.GetCamera().ScreenToViewportPoint(screenPoint);
            var ray = worldMapCamera.ViewportPointToRay(viewport);
            var hits = Physics.RaycastAll(ray);
            Vector3 hitBallTransPos = new Vector3();
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.tag == Tags.WorldMap)
                {
                    hitBallTransPos = hits[i].point;
                    break;
                }
            }

            Vector3 nodeTransPos = chNodeRootTrans.Find(nodeName).position;
            Vector3 nodeDir = nodeTransPos - chNodeRootTrans.position;
            Vector3 camDir = hitBallTransPos - chNodeRootTrans.position;
            Quaternion quaternion = new Quaternion();
            quaternion.SetFromToRotation(camDir, nodeDir);
            WheelTrans.rotation = quaternion * WheelTrans.rotation;
            Vector3 toVec = WheelTrans.rotation.eulerAngles;
            toVec.z = 0;
            if (toVec.x < 180)
            {
                if (toVec.x > 1) toVec = new Vector3(1, toVec.y, 0);
            }
            else
            {
                if (toVec.x < 320) toVec = new Vector3(320, toVec.y, 0);
            }
            Quaternion quaternionEnd = Quaternion.Euler(toVec);
            WheelTrans.rotation = quaternionStart;
            return quaternionEnd;
        }

        private Camera worldMapCamera;
        private Transform WheelTrans;
        private Transform WorldMapTrans;
        private Transform chNodeRootTrans;
        private Transform ComPlaneV1;
        private EarthPath chNodeRootScript;
        Transform FaceToTrans;
        private bool isNew = false;
        [SerializeField] private GameObject levelTabLayoutGo;
        [SerializeField] private GameObject levelTabItem2Go;
        protected override void OnPropertiesSet()
        {
            //isNew = Player.ChallengeManager.IsNewMap();

            worldMap3DItemList.Clear();
            // 加载3D资源
            if (worldMapGameObject == null) worldMapGameObject = GameObject.Instantiate(refWorldMap);

            if (Properties.isOpenByOpenWindow || Player.BattleManager.classicCountryLevelData == null)
            {
                Properties.openLevel = ClassicManager.Instance.classicMapLevelDataLastOpen.challengeMapConfig.Level;
                selectLevel = Properties.openLevel;
                Properties.isOpenByOpenWindow = false;
            }
            else
            {
                selectLevel = Player.BattleManager.classicCountryLevelData.challengeCountryConfig.Level;
                Properties.openLevel = selectLevel;
            }

            //添加3个难度的红点节点
            RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/1");
            RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/2");
            RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/3");
            Process3DRes();
            RefreshMapUIItem();


            if (Player.BattleManager.showCounties)
            {
                Refresh3DItem();
                GotoUIItem(worldMap3DItemNow.data.challengeCountryConfig.NodeName, false);
                WheelTrans.rotation = Quaternion.Euler(WheelTrans.rotation.eulerAngles + new Vector3(0, -10, 0));
                GotoUIItem(worldMap3DItemNow.data.challengeCountryConfig.NodeName, true);
            }
            else
            {
                string startNode = Player.BattleManager.classicCountryLevelData.challengeCountryConfig.NodeName;

                GotoUIItem(startNode, false);
                chNodeRootTrans.gameObject.SetActive(false);
                Player.BattleManager.showCounties = true;
            }

            //设置普通、困难、挑战按钮可见性。添加3个难度的红点节点
            RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/1");

            bool level2Open = ClassicManager.Instance.classicCountryLevelDataListDic[2][0].isOpen;
            levelTabLayoutGo.SetActive(level2Open);
            if (level2Open) RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/2");

            bool level3Open = ClassicManager.Instance.classicCountryLevelDataListDic[3][0].isOpen;
            if (level3Open) RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_ClassicPVE, "/3");

            levelTabItem2Go.gameObject.SetActive(ClassicManager.Instance.classicCountryLevelDataListDic[3][0].isOpen);

            mask.color = new Color(0, 0, 0, 1);
            worldMapImg.color = new Color(1, 1, 1, 1f);
            //bottomUi.alpha = 0;
            bottomUi.interactable = false;
            //topBar.alpha = 0;
            topBar.interactable = false;

            CameraManager.Instance.SetTexture(CameraID.WorldMap, worldMapImg);

            Anim.idleUpdateCallBack = () =>
            {
                Update3DItemFace();
            };



            RefreshLevelTab();

            //if (isNew)
            //{
            //    var cfg = Configs.ChallengeClub.GetConfig(Player.ChallengeManager.ChallengeId);
            //    int oldMapId = cfg.Map - 1;
            //    if (oldMapId < 1)
            //    {
            //        DoEnter();
            //    }
            //    else
            //    {
            //        UIController.Instance.OpenWindow<ChallengeAreaCompleteUI>(new ChallengeAreaCompleteUIProperties(oldMapId, DoEnter));
            //    }
            //}
            //else
            //{
            //DoEnter();
            //}
            SetLevelTabCallBack();

            Anim.useIdleAni = true;
            Anim.PlayEnterAnim(worldMap3DItemNow);
            //AudioManager.Instance.StopMusic();
            //AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);

            worldMapUIGuide.CheckGuide(worldMapCamera, GetFirstNodeTransform, GetLastNodeTransform);

            if (getNewCountry == true) { getNewCountry = false; OnNewCountry(new object[] { }); }
        }

        WorldMap3DItem worldMap3DItemFirst = null;
        public Transform GetFirstNodeTransform()
        {
            return worldMap3DItemFirst.GetMidTrans();
        }

        public Transform GetLastNodeTransform()
        {
            return worldMap3DItemNow.GetMidTrans();
        }

        //private void DoEnter()
        //{
        //    Anim.useIdleAni = true;//Properties.isIdle;
        //    Anim.PlayEnterAnim(worldMap3DItemNow);

        //    if (isNew)
        //    {
        //        TouchManager.Instance.DisableTouch();
        //        UnityTimer.Timer.Register(this.gameObject, 2f, () =>
        //        {
        //            OnWorldToContinent(null);
        //        });
        //    }
        //    else
        //    {
        //        AudioManager.Instance.StopMusic();
        //        AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
        //    }
        //}

        /// <summary>
        /// - [ ] 自动化导入地球资源
        ///    - [ ] 摄像机初始化
        ///    - [ ] 摄像机Cullingmask
        ///    - [ ] 去掉摄像机上的声音接收器
        ///    - [ ] 设置地球及其所有子节点的layer
        ///    - [ ] 海洋上添加碰撞器
        ///    - [ ] 设置地球及其所有子节点的Tag
        ///    - [ ] 添加Faceto节点
        /// </summary>
        private void Process3DRes()
        {
            WorldMapTrans = worldMapGameObject.transform.Find("WorldMap");
            WheelTrans = worldMapGameObject.transform.Find("Wheel");
            Anim.xuanZhuanTrans = WheelTrans;
            chNodeRootTrans = WorldMapTrans.Find("chNode");
            ComPlaneV1 = WorldMapTrans.Find("ComPlaneV1");
            chNodeRootScript = chNodeRootTrans.gameObject.GetComponent<EarthPath>();
            worldMapCamera = WheelTrans.Find("Camera").GetComponent<Camera>();
            FaceToTrans = WorldMapTrans.Find("FaceTo");

            if (worldMapCamera == null)
            {
                Debug.LogError("找不到挑战地球的摄像机");
            }
            if (worldMapGameObject.transform.GetComponent<CameraInitializer>() == null)//挂载摄像机初始化脚本
            {
                CameraInitializer cameraInitializer = worldMapGameObject.AddComponent<CameraInitializer>();
                cameraInitializer.RenderCamera = worldMapCamera;
                cameraInitializer.ID = CameraID.WorldMap;
                CameraManager.Instance.Register(CameraID.WorldMap, worldMapCamera);
            }
            worldMapGameObject.SetCullingMaskInThisAndAllChild(Layers.WorldMap);//设置光照和摄像机的CullingMask
            if (worldMapCamera.transform.GetComponent<AudioListener>() != null)//去掉摄像机上的声音接收器
            {
                worldMapCamera.transform.GetComponent<AudioListener>().enabled = false;
            }
            worldMapGameObject.SetLayerInThisAndAllChild(Layers.WorldMap);//设置地球及其所有子节点的layer
            worldMapGameObject.SetTagInThisAndAllChild(Tags.WorldMap);//设置地球及其所有子节点的Tag
            GameObject ocean = WorldMapTrans.Find("3DXY_geometry_003").gameObject;//海洋上添加碰撞器
            if (ocean.GetComponent<MeshCollider>() == null)
            {
                ocean.AddComponent<MeshCollider>();
            }
            if (FaceToTrans == null)
            {
                FaceToTrans = new GameObject("FaceTo").transform;
                FaceToTrans.SetParent(WorldMapTrans);
                FaceToTrans.localPosition = Vector3.zero;
            }
            isCanHide = true;
        }
        private void RefreshMapUIItem(int selectMapId = 0)
        {
            adapter.SetData(ClassicManager.Instance.classicMapLevelDataListDic[selectLevel], selectMapId);
        }

        private WorldMap3DItem worldMap3DItemNow;
        private List<WorldMap3DItem> worldMap3DItemList = new();//全部
        private Dictionary<int, WorldMap3DItem> worldMap3DItemDic = new();//不全，id，item
        private void Refresh3DItem()
        {
            worldMap3DItemDic.Clear();
            WorldMap3DItem firstItem = null;
            worldMap3DItemNow = null;
            List<ClassicCountryLevelData> classicCountryLevelDataList = ClassicManager.Instance.classicCountryLevelDataListDic[selectLevel];
            //要把第1个被隐藏的城市显示出来作为玩家目标，这个变量做标记
            var lastOpenCountryId = -1;

            for (int i = 0; i < classicCountryLevelDataList.Count; i++)
            {
                if (i >= worldMap3DItemList.Count)
                {
                    GameObject worldMap3DItemGoNew = GameObject.Instantiate(worldMap3DItemPrefab);
                    WorldMap3DItem worldMap3DItemNew = worldMap3DItemGoNew.transform.GetComponent<WorldMap3DItem>();
                    worldMap3DItemList.Add(worldMap3DItemNew);
                }
                WorldMap3DItem worldMap3DItem = worldMap3DItemList[i];
                if (firstItem == null) firstItem = worldMap3DItem;
                Transform worldMap3DItemTrans = worldMap3DItem.transform;
                ClassicCountryLevelData classicCountryLevelData = classicCountryLevelDataList[i];

                if (classicCountryLevelData.isSelect == true) worldMap3DItemNow = worldMap3DItem;
                if (classicCountryLevelData.challengeCountryConfig.Id == 10101) worldMap3DItemFirst = worldMap3DItem;

                worldMap3DItem.SetData(classicCountryLevelData);
                Transform chNodeTrans = chNodeRootTrans.Find(classicCountryLevelData.challengeCountryConfig.NodeName);
                worldMap3DItemTrans.SetParent(chNodeTrans);
                worldMap3DItemTrans.localScale = Vector3.one * 0.03f;
                worldMap3DItemTrans.transform.localPosition = Vector3.zero;
                worldMap3DItemTrans.LookAt(worldMapCamera.transform);
                worldMap3DItemDic.Add(classicCountryLevelData.challengeCountryConfig.Id, worldMap3DItem);
                //还没有开放就隐藏, 
                if (classicCountryLevelData.isOpen)
                {
                    chNodeTrans.gameObject.SetActive(true);
                }
                else
                {
                    //只处理第1个被隐藏的城池（即：下一个城池）如果是被等级卡住，让他显示出来接受点击，并给玩家提示。
                    if (lastOpenCountryId == -1)
                    {
                        lastOpenCountryId = i;
                        if (classicCountryLevelData.challengeCountryConfig.UserLevel > Player.Level)
                        {
                            chNodeTrans.gameObject.SetActive(true);
                        }
                        else chNodeTrans.gameObject.SetActive(false);
                    }
                    else chNodeTrans.gameObject.SetActive(false);
                }
                //chNodeTrans.gameObject.SetActive(classicCountryLevelData.isOpen);
            }

            if (worldMap3DItemList.Count > classicCountryLevelDataList.Count)
            {
                for (int i = classicCountryLevelDataList.Count; i < worldMap3DItemList.Count; i++)
                {
                    worldMap3DItemList[i].gameObject.SetActive(false);
                }
            }
            if (worldMap3DItemNow == null)
            {
                worldMap3DItemNow = firstItem;
            }
        }

        private bool isCanHide = false;
        protected override void WhileHiding()
        {
            if (isCanHide == false) return;
            StopClickWorldUIItemTween();
            Anim.StopIdle();
            worldMap3DItemList.Clear();
            CameraManager.Instance.ReleaseTexture(CameraID.WorldMap, worldMapImg);

            // 卸载3D资源
            GameObject.Destroy(worldMapGameObject);
            worldMapGameObject = null;
            isCanHide = false;
        }

        //private void OnClose()
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
        //    Sequence seq = DOTween.Sequence();
        //    seq.AppendCallback(() =>
        //    {
        //        bottomUi.interactable = false;
        //        topBar.interactable = false;
        //    });
        //    seq.Append(bottomUi.DOFade(0, 0.25f));
        //    seq.Join(topBar.DOFade(0, 0.25f));
        //    seq.Append(mask.DOFade(1, 0.2f).SetEase(Ease.InCubic));
        //    seq.AppendCallback(() =>
        //    {
        //        //if (Properties.lastScreenId == "ChallengeUI")
        //        //{
        //        //    int mapId = Player.ChallengeManager.MapId;
        //        //    SceneManagerFor3D.LoadAddressableSceneAdditive(() =>
        //        //    {
        //        //        UIController.Instance.ShowPanel<ChallengeUI>(new ChallengeUIProperties(mapId, false));
        //        //    });
        //        //}
        //        //else
        //        //{
        //        UIController.Instance.HidePanel<WorldMapUI>();
        //        //}
        //    });
        //}

        /// <summary>
        /// 点击屏幕坐标
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public GameObject GetFirstPickGameObject(Vector2 position)
        {
            EventSystem eventSystem = EventSystem.current;
            PointerEventData pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = position;
            //射线检测ui
            List<RaycastResult> uiRaycastResultCache = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerEventData, uiRaycastResultCache);
            if (uiRaycastResultCache.Count > 0)
                return uiRaycastResultCache[0].gameObject;
            return null;
        }



        private bool isFirst = false;
        private bool isClickUI = false;
        private Vector3 lastDirVec = Vector3.zero;
        public float speed = 10;
        private Vector3 downPos;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                OnMouseDown();
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                OnMouseUp();
            }
            if (Input.GetKey(KeyCode.Mouse0))
            {
                OnMouseMove();
            }
        }

        private void OnMouseDown()
        {
            if (isNew) return;
            if (isDoingWorldToContinentAni) return;

            if (clickWorldUIItemTween != null)
            {
                clickWorldUIItemTween.Kill();
                clickWorldUIItemTween = null;
            }

            isFirst = true;
            isClickUI = false;
            GameObject firstClickObj = GetFirstPickGameObject(Input.mousePosition);
            if (firstClickObj != null)
            {
                if (firstClickObj.name != "PanelLayer" && firstClickObj.name != "ClickMaskImage")
                {
                    isClickUI = true;
                }
            }
            if (isClickUI == false)
            {
                downPos = Input.mousePosition;
                if (worldMapUIGuide.IsGuideClickCountryDoing == false)
                {
                    Anim.StopIdle();
                }
            }
        }
        private void OnMouseUp()
        {
            if (isNew) return;
            if (isDoingWorldToContinentAni) return;

            if (Player.ChallengeManager.IsNewMap()) return;

            if (isClickUI == true && worldMapUIGuide.IsGuideClickCountryDoing == false)
            {
                Anim.StartIdle();
                return;
            }
            if ((Input.mousePosition - downPos).magnitude > 10)
            {
                if (worldMapUIGuide.IsGuideClickCountryDoing == false)
                {
                    Anim.StartIdle();
                }
                return;
            }
            var screenPoint = UIFrame.ChangeUIScreenPointTo3DScreenPoint(Input.mousePosition);
            var ray = worldMapCamera.ScreenPointToRay(screenPoint);
            var hits = Physics.RaycastAll(ray);
            Transform hitItemTrans = null;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.tag == Tags.WorldMapItem)
                {
                    hitItemTrans = hits[i].transform;
                    break;
                }
            }

            bool isgoto = false;
            if (hitItemTrans != null)
            {
                WorldMap3DItem worldMap3DItem = hitItemTrans.parent.parent.parent.GetComponent<WorldMap3DItem>();
                //Tips.PopTips("点击了 {0}".SafeFormat(worldMap3DItem.data.challengeCountryConfig.Name));
                //ClassicManager.Instance.OpenClassicCountryUI(worldMap3DItem.data.challengeCountryConfig.Id);

                if (GuideManager.IsGuideDoing(GuideID.guideGetProgressBox3))
                {
                    if (worldMap3DItem.data.challengeCountryConfig.Id == 10101)
                    {
                        OnWorldToContinent(worldMap3DItem);
                        isgoto = true;
                    }
                }
                else
                {
                    switch (worldMap3DItem.worldMap3DItemState)
                    {
                        case WorldMap3DItemState.Pass:
                        case WorldMap3DItemState.Star:
                        case WorldMap3DItemState.Open:
                            OnWorldToContinent(worldMap3DItem);
                            isgoto = true;
                            return;
                        case WorldMap3DItemState.Lock:
                            Tips.PopTips(string.Format("球队达到Lv.{0}可挑战[{1}]", worldMap3DItem.data.challengeCountryConfig.UserLevel, worldMap3DItem.data.challengeCountryConfig.Name));
                            break;
                        default:
                            break;
                    }
                }
            }
            if (!isgoto && !worldMapUIGuide.IsGuideClickCountryDoing) Anim.StartIdle();
        }
        private void OnMouseMove()
        {
            if (isNew) return;
            if (isDoingWorldToContinentAni) return;

            if (worldMapUIGuide.IsGuideClickCountryDoing) return;

            if (isClickUI == true)
            {
                return;
            }
            if (isFirst)
            {
                isFirst = false;
                return;
            }
            Vector3 nowDirVec = Vector3.zero;
            var screenPoint = UIFrame.ChangeUIScreenPointTo3DScreenPoint(Input.mousePosition);
            //var viewport = UIController.Instance.GetCamera().ScreenToViewportPoint(screenPoint);
            var viewport = worldMapCamera.ScreenToViewportPoint(screenPoint);
            var ray = worldMapCamera.ViewportPointToRay(viewport);
            var hits = Physics.RaycastAll(ray);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.tag == Tags.WorldMap)
                {
                    nowDirVec = hits[i].point;
                    break;
                }
            }

            FaceToTrans.LookAt(nowDirVec);
            Vector3 nowFace = FaceToTrans.rotation.eulerAngles;

            FaceToTrans.LookAt(lastDirVec);
            Vector3 lastFace = FaceToTrans.rotation.eulerAngles;

            Quaternion quaternion = new Quaternion();
            quaternion.SetFromToRotation(nowDirVec, lastDirVec);
            WheelTrans.rotation = quaternion * WheelTrans.rotation;
            Vector3 toVec = WheelTrans.rotation.eulerAngles;
            toVec.z = 0;
            if (toVec.x < 180)
            {
                if (toVec.x > 1) toVec = new Vector3(1, toVec.y, 0);
            }
            else
            {
                if (toVec.x < 310) toVec = new Vector3(310, toVec.y, 0);
            }
            WheelTrans.rotation = Quaternion.Euler(toVec);
            Update3DItemFace();
        }

        private void Update3DItemFace()
        {
            Vector2 size = UIController.Instance.GetComponent<RectTransform>().sizeDelta;
            float halfWidth = UIFrame.width / 2;
            float halfHeight = UIFrame.height / 2;
            foreach (WorldMap3DItem worldMap3DItem in worldMap3DItemList)
            {
                worldMap3DItem.transform.LookAt(worldMapCamera.transform);
                Vector3 WorldMapScreenPoint = worldMapCamera.WorldToScreenPoint(worldMap3DItem.transform.position);
                float x = Mathf.Abs(halfWidth - WorldMapScreenPoint.x) / halfWidth;
                float y = Mathf.Abs(halfHeight - WorldMapScreenPoint.y) / halfHeight;
                Vector3 scale = Vector3.Lerp(Vector3.one * 0.03f, Vector3.one * 0.01f, (x + y) / 2);
                worldMap3DItem.transform.localScale = scale;
            }
        }

        private void LateUpdate()
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (isClickUI == true)
                {
                    return;
                }
                var screenPoint = UIFrame.ChangeUIScreenPointTo3DScreenPoint(Input.mousePosition);
                //var viewport = UIController.Instance.GetCamera().ScreenToViewportPoint(screenPoint);
                var viewport = worldMapCamera.ScreenToViewportPoint(screenPoint);
                var ray = worldMapCamera.ViewportPointToRay(viewport);
                var hits = Physics.RaycastAll(ray);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider.tag == Tags.WorldMap)
                    {
                        lastDirVec = hits[i].point;
                        //Debug.Log(hits[i].point);
                        break;
                    }
                }
            }
        }

        [SerializeField] List<ClassicMapLevelTabItem> LevelTabItemList = new();
        private void SetLevelTabCallBack()
        {
            foreach (var item in LevelTabItemList)
            {
                item.SetCallBack(OnClickLevelTab);
            }
        }
        private void OnClickLevelTab(int level)
        {
            selectLevel = level;
            RefreshLevelTab();

            Properties.openLevel = level;

            int mapId = 0;
            foreach (var item in ClassicManager.Instance.classicMapLevelDataListDic[level])
            {
                if (item.isLastOpen)
                {
                    mapId = item.challengeMapConfig.Id;
                    break;
                }
            }
            if (mapId == 0) mapId = ClassicManager.Instance.classicMapLevelDataListDic[level][0].challengeMapConfig.Id;

            RefreshMapUIItem(mapId);
            Refresh3DItem();

            ClassicMapLevelData data = ClassicManager.Instance.classicMapLevelDataDic[mapId];
            if (data.targetCountryId != 0 && worldMap3DItemDic.ContainsKey(data.targetCountryId) == true)
            {
                GotoUIItem(worldMap3DItemDic[data.targetCountryId].data.challengeCountryConfig.NodeName);
            }
            else
            {
                GotoUIItem(worldMap3DItemDic[ClassicManager.Instance.mapClubFirstDic[data.challengeMapConfig.Id]].data.challengeCountryConfig.NodeName);
            }

        }
        private int selectLevel = 1;
        private void RefreshLevelTab()
        {
            foreach (var item in LevelTabItemList)
            {
                item.SetLight(selectLevel == item.level);
            }
        }

    }
}
