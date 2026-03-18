using Babu;
using BigBang;
using BigBang.Animation;
using BigBang.UI;
using GameConfig;
using GameConfig.Config;
using Google.Protobuf.Collections;
using Protocol;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

public class RecruitPad : MonoBehaviour, IActivity
{

    [SerializeField] private TMP_Text floorsInfoText;
    [SerializeField] private Button poolInfoBtn;
    [SerializeField] private RecruitBtn _recruitOnceBtn;
    [SerializeField] private RecruitBtn _recruitTenBtn;
    [SerializeField] private List<CardSelectIcon> appointCardList;
    [SerializeField] private Button appointBtn;
    [SerializeField] private RawImage rawImg;
    [SerializeField] private RecruitUIAnim anim;
    [SerializeField] private GameObject info;
    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private BabuButton btnNew;
    [SerializeField] private BabuButton btnShop;
    [SerializeField] ResourceTitle resTitle;

    private GameObject model;
    private RecruitPool _pool;
    private List<CardModelConfig> SuperCardList = new List<CardModelConfig>();
    private List<CardModelConfig> YellowCardList = new List<CardModelConfig>();

    private int poolId = 0;
    private RepeatedField<GameItem> resultList;
    private int recruitCountType;
    private int costType;
    private bool isCanHide = false;

    [SerializeField] private BabuButton btnGiftShop = null;

    protected void OnEnable()
    {

        // 播放音效
        AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);

        poolInfoBtn.onClick.AddListener(OnClickPoolInfo);
        appointBtn.onClick.AddListener(OnClickAppoint);

        _recruitOnceBtn.SetClickAction(OnRecruit);
        _recruitTenBtn.SetClickAction(OnRecruit);
        btnNew.OnClick += OnBtnNewClick;
        btnShop.OnClick += OnBtnShopClick;

        EventManager.Instance.Register(EventID.RecruitUIShowInfo, ShowInfo);
        EventManager.Instance.Register(EventID.OnRecruitPoolRefresh, RefreshPoolInfo);
        EventManager.Instance.Register(EventID.ShowRecruitResult, ShowRecruitResult);
        EventManager.Instance.Register(EventID.RefreshUIRedDot, refreshRedDot);
        cardHelpButton.OnClick += OnCardHelpButtonClick;
        cardHelpButtonBig.OnClick += OnCardHelpButtonClick;
        btnGiftShop.OnClick += OnBtnGiftShopClick;
        gotoTimeRecruitBtn.OnClick += OnGotoTimeRecruitBtnClick;
        SecondUpdateManager.Instance.RegistAction(RefreshLeftTime);
        northButton.OnClick += ChangeToNorth;
        southButton.OnClick += ChangeToSouth;

        InitRotationChart();
    }

    protected void OnDisable()
    {
        poolInfoBtn.onClick.RemoveListener(OnClickPoolInfo);
        appointBtn.onClick.RemoveListener(OnClickAppoint);
        btnNew.OnClick -= OnBtnNewClick;
        btnShop.OnClick -= OnBtnShopClick;
        EventManager.Instance.Unregister(EventID.RecruitUIShowInfo, ShowInfo);
        EventManager.Instance.Unregister(EventID.OnRecruitPoolRefresh, RefreshPoolInfo);
        EventManager.Instance.Unregister(EventID.ShowRecruitResult, ShowRecruitResult);
        EventManager.Instance.Unregister(EventID.RefreshUIRedDot, refreshRedDot);
        cardHelpButton.OnClick -= OnCardHelpButtonClick;
        cardHelpButtonBig.OnClick -= OnCardHelpButtonClick;
        btnGiftShop.OnClick -= OnBtnGiftShopClick;
        gotoTimeRecruitBtn.OnClick -= OnGotoTimeRecruitBtnClick;
        SecondUpdateManager.Instance.UnRegistAction(RefreshLeftTime);
        northButton.OnClick -= ChangeToNorth;
        southButton.OnClick -= ChangeToSouth;

        if (isCanHide == false) return;
        // 关闭渲染
        model.GetComponent<CameraInitializer>().LogoutCameraOnce();
        model.GetComponent<GameObjectIdentity>().LogoutGameObjectOnce();
        model.transform.Find("空白").Find("文件夹板").Find("纸").Find("Canvas").Find("RecruitlistUI").GetComponent<GameObjectIdentity>().LogoutGameObjectOnce();
        DisableCameraRender();
        // 关闭模型上的动画
        GameObjectManager.Instance.GetComponent<RecruitlistUI>(GameObjectID.RecruitlistUI)?.StopAni();
        // 卸载3D模型
        GameObject.Destroy(model);
        // GameObject.DestroyImmediate(model);
        isCanHide = false;
    }

    private void OnBtnShopClick(BabuButton sender)
    {
        AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Recruit));
    }

    private void OnBtnNewClick(BabuButton sender)
    {
        UIController.Instance.ShowPanel<RecruitRewardsUI>(new RecruitRewardsUIProperties(poolId, activityData != null));
    }
    // 设置按钮样式
    private void UpdateButtonStyle(bool isActivity)
    {
        _recruitOnceBtn.SetButtonStyle(RecruitCountType.Once, isActivity);
        _recruitTenBtn.SetButtonStyle(RecruitCountType.Ten, isActivity);
    }

    // 启用相机渲染
    private void EnableCameraRender()
    {
        // 获得相机
        var c = CameraManager.Instance.GetCamera(CameraID.RecruitModel);
        // 启用相机
        c.gameObject.SetActive(true);
        // 传感器尺寸
        c.sensorSize = new UnityEngine.Vector2(52.5f, 52.5f / ((float)rawImg.rectTransform.rect.width / rawImg.rectTransform.rect.height));
        // 获得临时渲染纹理
        var temporary = RenderTexture.GetTemporary((int)rawImg.rectTransform.rect.width, (int)rawImg.rectTransform.rect.height, 24);
        // ⚠ 根据配置动态调整
        temporary.anisoLevel = 8;
        // 设置训练纹理
        rawImg.texture = temporary;
        // 设置相机目标渲染纹理
        c.targetTexture = temporary;
    }

    // 禁用相机渲染
    private void DisableCameraRender()
    {
        // 获得相机
        var c = CameraManager.Instance.GetCamera(CameraID.RecruitModel);
        if (c == null) return;
        // 设置相机目标渲染纹理为空
        c.targetTexture = null;
        // 禁用相机
        c.gameObject.SetActive(false);
        // 释放临时渲染纹理
        RenderTexture.ReleaseTemporary(rawImg.texture as RenderTexture);
        // 设置训练纹理为空
        rawImg.texture = null;
    }

    private void UpdateAppointInfo()
    {
        GameObjectManager.Instance.GetComponent<RecruitlistUI>(GameObjectID.RecruitlistUI)?.SetData(_pool.AppointCardDic);
    }

    // 刷新卡池信息
    private void RefreshPoolInfo(object[] agrs)
    {
        UpdatePoolInfo();
        UpdateAppointInfo();
    }

    [SerializeField] private Image northRedDot = null;
    [SerializeField] private Image southRedDot = null;
    public void refreshRedDot(object[] args = null)
    {
        {
            RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/" + poolId.ToString() + "/NewRewards");
            node.IsRed(btnNew.transform.Find("DotNodeImg").transform);

            ActivityData activityData = ActivityController.Instance.FindTimeRecruitActivity;
            bool isTimeRecruitNeedShow = activityData != null;
            if (isTimeRecruitNeedShow)
            {
                node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Activity, "/" + activityData.cfg.Id);
                node.IsRed(btnGiftShop.transform.Find("DotNodeImg").transform);
            }
        }

        {
            ActivityData allStarTimeRecruitActivityDataNorth = ActivityController.Instance.FindAllStar2024NorthTimeRecruit;
            if (allStarTimeRecruitActivityDataNorth == null)
            {
                northRedDot.gameObject.SetActive(false);
            }
            else
            {
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/" + allStarTimeRecruitActivityDataNorth.cfg.Param1);
                node.IsRed(northRedDot.transform);
            }
        }

        {
            ActivityData allStarTimeRecruitActivityDataSouth = ActivityController.Instance.FindAllStar2024SouthTimeRecruit;
            if (allStarTimeRecruitActivityDataSouth == null)
            {
                southRedDot.gameObject.SetActive(false);
            }
            else
            {
                RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "/" + allStarTimeRecruitActivityDataSouth.cfg.Param1);
                node.IsRed(southRedDot.transform);
            }
        }
    }

    // 更新卡池信息
    private void UpdatePoolInfo()
    {
        var total = _pool.Config.FloorsCount;
        var _poolGroupList = Configs.CardPoolGroup.GetConfigList().FindAll(P => P.PoolInfo == _pool.Config.Id && P.Operator == "every");
        var nxtCountList = new List<int>();

        int nxtCount = 9999;
        int minIndex = 0;
        for (var index = 0; index < _poolGroupList.Count; index++)
        {
            var _count = _poolGroupList[index].Operand - _pool.TotalCount % _poolGroupList[index].Operand;
            if (_count < nxtCount)
            {
                nxtCount = _count;
                minIndex = index;
            }
        }
        floorsInfoText.text = string.Format(_poolGroupList[minIndex].Nxtinfo, nxtCount);
    }

    private void OnRecruitSuccess(RecruitResponse response)
    {
        TouchManager.Instance.DisableTouch();
        // 播放打开书本动画
        anim.PlayRecruit(() =>
        {
            //RecruitBtn.IsClick = false;
            // 展示超级牌
            foreach (var item in response.ResultList)
            {
                // 如果是超级牌
                if (item.Type == (int)GameItemType.Card)
                {
                    if (Configs.CardModel.GetConfig(item.Id).Quality == QualityType.Red)
                    {
                        SuperCardList.Add(Configs.CardModel.GetConfig(item.Id));
                    }
                }
            }
            poolId = response.PoolInfo.PoolId;
            resultList = response.ResultList;
            recruitCountType = response.RecruitCountType;
            costType = response.CostType;
            // 如果没有超级牌，立即打开结果界面
            if (SuperCardList.Count == 0 && YellowCardList.Count == 0)
            {
                ShowRecruitResult(null);
            }
            else if (SuperCardList.Count != 0 && YellowCardList.Count == 0)
            {
                UIController.Instance.OpenWindow<SuperCardUI>(new SuperCardUIProperties(true, null, SuperCardList));
            }
            Player.CardManager.RecruitController.CheckRedData(poolId);
        });
    }

    // 显示招募结果
    private void ShowRecruitResult(object[] args)
    {
        UIController.Instance.ShowPanel<RecruitResultUI>(new RecruitResultProperties(poolId, resultList, recruitCountType, costType));
    }

    // 显示信息
    private void ShowInfo(object[] args)
    {
        _recruitOnceBtn.gameObject.SetAlpha(1);
        _recruitTenBtn.gameObject.SetAlpha(1);
        info.SetAlpha(1);
    }

    [EditorButton("显示超级牌")]
    public void ShowSuperCard()
    {
        UIController.Instance.OpenWindow<SuperCardUI>(new SuperCardUIProperties(true, null, SuperCardList));
    }

    private void OnRecruit(RecruitCountType recruitCountType, RecruitCostType recruitCostType)
    {
        EventManager.Instance.Dispatch(EventID.OnStartRecruit);
        AudioManager.Instance.PlaySound(AudioNames.ANI_SCOUT);
        bool doRecruit = Player.CardManager.RecruitController.DoRecruit(_pool.PoolId, recruitCountType, recruitCostType, OnRecruitSuccess);
        if (doRecruit)
        {
            info.SetAlpha(0);
            Timer.Register(this.gameObject, 2, () => { info.SetAlpha(1); });
        }
    }

    public void OnClickPoolInfo()
    {
        poolInfoBtn.GetComponent<ButtonAnim>().Play(() =>
        {
            UIController.Instance.OpenWindow<RecruitPoolPreviewUI>(new RecruitPoolPreviewProperties(_pool.PoolId));
        }, playAudio: false, audioCallback: () =>
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);
        });
    }

    // 打开心愿单
    public void OnClickAppoint()
    {
        if (Player.CardManager.RecruitController.TotalRecruitCount < 100)
        {
            //未满一百次  飘动提示
            Tips.PopTips(Lang.Get(LangID.RecruitAppointLimit).Replace("{RecruitTimes}", Player.CardManager.RecruitController.TotalRecruitCount.ToString()));
        }
        else
        {
            //满一百次
            AudioManager.Instance.PlaySound(AudioNames.ANI_TLIST);
            info.DOFade(0, 0.3f);
            // 按钮淡出
            _recruitOnceBtn.gameObject.DOFade(0, 0.3f);
            _recruitTenBtn.gameObject.DOFade(0, 0.3f);
            anim.PlayWish();
            UIController.Instance.OpenWindow<RecruitAppointUI>(new RecruitAppointProperties(_pool.PoolId));
        }
    }

    private ActivityData activityData = null;
    /// <summary>
    /// 常驻抽卡用这个接口
    /// </summary>
    public void LoadActivity(int _poolId)
    {
        activityData = null;
        poolId = _poolId;
        resTitle.FieldRecruitItem = true;
        resTitle.FieldRecruitItem1 = false;
        EventManager.Instance.Dispatch(EventID.OnResourceChange);
        LoadActivity();
    }

    /// <summary>
    /// 活动抽卡用这个
    /// </summary>
    /// <param name="_data"></param>
    public void LoadActivity(ActivityData _data)
    {
        activityData = _data;
        resTitle.FieldRecruitItem1 = true;
        resTitle.FieldRecruitItem = false;
        EventManager.Instance.Dispatch(EventID.OnResourceChange);
        poolId = _data.cfg.Param1;
        LoadActivity();
    }

    private void LoadActivity()
    {
        // 实例化3D模型
        model = GameObject.Instantiate(modelPrefab);
        model.GetComponent<CameraInitializer>().RegistCameraOnce();
        model.GetComponent<GameObjectIdentity>().RegistGameObjectOnce();
        model.transform.Find("空白").Find("文件夹板").Find("纸").Find("Canvas").Find("RecruitlistUI").GetComponent<GameObjectIdentity>().RegistGameObjectOnce();
        // 开启渲染
        EnableCameraRender();
        isCanHide = true;
        // 根据情况显示不同样式的模型
        UpdateModelStyle(poolId != 1);
        // 根据情况显示不同样式的按钮
        UpdateButtonStyle(poolId != 1);

        _pool = Player.CardManager.RecruitController.GetPool(poolId);
        if (_pool == null) return;
        UpdatePoolInfo();
        UpdateAppointInfo();
        GetComponent<RecruitUIAnim>().PlayEnter();
        refreshRedDot();
        CheckCardRetire();
        CheckAllStar();
    }

    [SerializeField] private RectTransform updatePhotoPanel = null;
    [SerializeField] private TMP_Text updatePhotoTipText = null;
    private readonly string photoTipPrefabStr = "2024全明星{0}最新定妆照将于后续版本更新";
    [SerializeField] private RectTransform areaSwitchPanel = null;
    [SerializeField] private BabuButton northButton = null;
    [SerializeField] private BabuButton southButton = null;
    [SerializeField] private Image northButtonImage = null;
    [SerializeField] private Image southButtonImage = null;
    private readonly Color allStarDarkColor = new Color(0.8f, 0.8f, 0.8f);
    private readonly Color allStarLightColor = new Color(1f, 1f, 1f);
    [SerializeField] private Image northLightImage = null;
    [SerializeField] private Image southLightImage = null;
    private void CheckAllStar()
    {
        bool isAllStar = activityData != null && activityData.clientType == ActivityClientType.AllStarTimeRecruit;
        btnGiftShop.gameObject.SetActive(!isAllStar);
        areaSwitchPanel.gameObject.SetActive(isAllStar);
        bool isNeedNewPhoto = false;
        updatePhotoPanel.gameObject.SetActive(isAllStar && isNeedNewPhoto);
        if (isAllStar && isNeedNewPhoto)
        {
            int cardId = CardId.ZhaoRui;
            int.TryParse(activityData.cfg.Param2, out cardId);
            CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(cardId);
            if (cardModelConfig == null)
            {
                Debug.LogWarningFormat("RecruitPad , CheckAllStar , cardModelConfig is null , activityData.cfg.Id = {0} , cardId = {1}", activityData.cfg.Id, cardId);
            }
            else
            {
                updatePhotoTipText.text = photoTipPrefabStr.SafeFormat(cardModelConfig.Name);
            }
        }
        if (isAllStar)
        {
            bool isNorth = activityData == ActivityController.Instance.FindAllStar2024NorthTimeRecruit;
            northButtonImage.transform.SetLocalScale(isNorth ? 1f : 0.9f);
            northButtonImage.color = isNorth ? allStarLightColor : allStarDarkColor;
            southButtonImage.transform.SetLocalScale(!isNorth ? 1f : 0.9f);
            southButtonImage.color = !isNorth ? allStarLightColor : allStarDarkColor;
            northLightImage.gameObject.SetActive(isNorth);
            southLightImage.gameObject.SetActive(!isNorth);
        }
    }
    private void ChangeToNorth(BabuButton _)
    {
        if (activityData == ActivityController.Instance.FindAllStar2024NorthTimeRecruit)
        {
            return;
        }
        else
        {
            this.enabled = false;
            this.enabled = true;
            ActivityData allStarTimeRecruitActivityDataNorth = ActivityController.Instance.FindAllStar2024NorthTimeRecruit;
            LoadActivity(allStarTimeRecruitActivityDataNorth);
            EventManager.Instance.Dispatch(EventID.OnRecruitChangeArea, AllStarManager.Area.North);
        }
    }
    private void ChangeToSouth(BabuButton _)
    {
        if (activityData == ActivityController.Instance.FindAllStar2024SouthTimeRecruit)
        {
            return;
        }
        else
        {
            this.enabled = false;
            this.enabled = true;
            ActivityData allStarTimeRecruitActivityDataSouth = ActivityController.Instance.FindAllStar2024SouthTimeRecruit;
            LoadActivity(allStarTimeRecruitActivityDataSouth);
            EventManager.Instance.Dispatch(EventID.OnRecruitChangeArea, AllStarManager.Area.South);
        }
    }

    /// <summary>
    /// 清理所有退休球员
    /// </summary>
    private void CheckCardRetire()
    {
        //退休球员不要出现在心愿单里
        foreach (var info in _pool.AppointCardDic)
        {
            CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(info.Value.CardId);
            if (cardModelConfig == null || cardModelConfig.IsRetire == 1)
            {
                var index = info.Key;
                var appointCard = info.Value;
                var cardId = appointCard.CardId;
                Player.CardManager.RecruitController.DoCancelAppoint(_pool.PoolId, index, (_) => { UpdateAppointInfo(); });
            }
        }
    }

    [SerializeField] private Button wishClick = null;
    [SerializeField] private BabuButton cardHelpButton = null;
    [SerializeField] private BabuButton cardHelpButtonBig = null;
    [SerializeField] private TMP_Text activityTimeText = null;
    private async void UpdateModelStyle(bool isActivity)
    {
        anim.isActivity = isActivity;
        btnGiftShop.gameObject.SetActive(isActivity);
        wishClick.gameObject.SetActive(!isActivity);
        cardHelpButton.gameObject.SetActive(isActivity);
        cardHelpButtonBig.gameObject.SetActive(isActivity);
        activityTimeText.gameObject.SetActive(isActivity);

        Transform recruitModelTrans = GameObjectManager.Instance.GetGameObject(GameObjectID.RecruitModel).transform;
        Transform kongbaiTrans = recruitModelTrans.Find("空白");
        specialCard = kongbaiTrans.Find("SpecialCard");
        specialCard.gameObject.SetActive(isActivity);
        string[] needHideInActivityName = { "信1", "光", "文件夹板" };
        foreach (var item in needHideInActivityName)
        {
            Transform itemTrans = kongbaiTrans.Find(item);
            itemTrans.gameObject.SetActive(!isActivity);
        }
        if (isActivity)
        {
            gotoTimeRecruitBtn.gameObject.SetActive(false);
            ProcessActivity3DItem();
        }
        else
        {
            bool isTimeRecruitNeedShow = ActivityController.Instance.FindTimeRecruitActivity != null;
            // isTimeRecruitNeedShow = false;//TODO: 调试轮播图用
            gotoTimeRecruitBtn.gameObject.SetActive(isTimeRecruitNeedShow);
            rotationChart.gameObject.SetActive(!isTimeRecruitNeedShow);

            if (isTimeRecruitNeedShow)
            {
                int cardId;
                int.TryParse(ActivityController.Instance.FindTimeRecruitActivity.cfg.Param2, out cardId);
                CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(cardId);
                gotoTimeRecruitBtn.GetComponent<Image>().sprite = await SpriteProxy.GetActivityRecruitSprite(cardModelConfig.Id.ToString() + "_banner");
            }
        }
        RefreshLeftTime();
    }
    private void RefreshLeftTime()
    {
        if (activityData == null)
        {
            return;
        }
        long leftTime = activityData.EndTime - Utils.DataConvUtil.ServerTime;
        activityTimeText.text = "活动结束：{0}".SafeFormat(TimeUtils.FormatLeftTimeWithDayCnOtherEn((int)leftTime));
        if (leftTime <= 0)
        {
            UIController.Instance.HidePanel<RecruitUI>();
        }
    }

    private Transform specialCard = null;
    private SpriteRenderer cardImage = null;
    private SpriteRenderer ballImage = null;
    private TMP_Text cardName = null;
    private TMP_Text cardDetail = null;
    private async void ProcessActivity3DItem()
    {
        int cardId = 104004;
        int.TryParse(activityData.cfg.Param2, out cardId);
        CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(cardId);
        if (cardModelConfig == null)
        {
            Debug.LogWarningFormat("RecruitPad , ProcessActivity3DItem , cardModelConfig is null , activityData.cfg.Id = {0} , cardId = {1}", activityData.cfg.Id, cardId);
            return;
        }

        cardImage = specialCard.Find("CardImage").GetComponent<SpriteRenderer>();
        ballImage = specialCard.Find("BallImage").GetComponent<SpriteRenderer>();
        cardName = specialCard.Find("CardName").GetComponent<TMP_Text>();
        cardDetail = specialCard.Find("CardDetail").GetComponent<TMP_Text>();

        cardImage.sprite = await SpriteProxy.GetActivityRecruitSprite(cardModelConfig.Id.ToString());

        bool isAllStar = activityData != null && activityData.clientType == ActivityClientType.AllStarTimeRecruit;
        ballImage.gameObject.SetActive(!isAllStar);
        cardName.gameObject.SetActive(!isAllStar);
        cardDetail.gameObject.SetActive(!isAllStar);
        if (!isAllStar)
        {
            cardName.text = PlayerCard.GetFullName(cardModelConfig);
            cardDetail.text = cardModelConfig.RecruitWords;
        }

        anim.InitActivityAnim();
    }

    private void OnCardHelpButtonClick(BabuButton _)
    {
        int cardId = 104004;
        int.TryParse(activityData.cfg.Param2, out cardId);
        CardModelConfig cardModelConfig = Configs.CardModel.GetConfig(cardId);
        if (cardModelConfig == null)
        {
            Debug.LogWarningFormat("RecruitPad , OnCardHelpButtonClick , cardModelConfig is null , activityData.cfg.Id = {0} , cardId = {1}", activityData.cfg.Id, cardId);
            return;
        }
        UIController.Instance.OpenWindow<CardDetailUI>(new CardDetailProperties(cardId));
    }

    public void OnBtnGiftShopClick(BabuButton _)
    {
        UIController.Instance.ShowPanel<RecruitGiftUI>();
    }

    [SerializeField] private BabuButton gotoTimeRecruitBtn = null;
    public void OnGotoTimeRecruitBtnClick(BabuButton _)
    {
        ActivityData activityData = ActivityController.Instance.FindTimeRecruitActivity;
        if (activityData == null)
        {
            Debug.LogWarning("RecruitPad , OnGotoTimeRecruitBtnClick , activityData is null");
            return;
        }
        //UIController.Instance.ShowPanel<ActivityMainUI>(new ActivityMainUIProperties(EActivityType.TimeRecruit, new() { EActivityType.TimeRecruit }));
        TriggerManager.Instance.JumpPanel(TriggerModuleType.Recruit_Time);
    }

    [SerializeField] private RotationChart rotationChart = null;
    private void InitRotationChart()
    {
        RecruitPool pool = Player.CardManager.RecruitController.GetPool(1);
        rotationChart.SetData(pool.GetQualityCardIdList(QualityType.Orange));
    }

}
