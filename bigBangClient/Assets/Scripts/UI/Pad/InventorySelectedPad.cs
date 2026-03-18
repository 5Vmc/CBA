using System;
using System.Collections.Generic;
using System.Linq;
using Babu;
using GameConfig;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class InventorySelectedPad : MonoBehaviour
    {
        [SerializeField] private InventoryItem item;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private TMP_Text useBtnText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text mergeText;
        [SerializeField] private Button useButton;
        [SerializeField] private RectTransform timeRect;

        private GoodsData data;
        private ButtonState buttonState = ButtonState.Open;
        public event Action RequestRefresh;

        enum ButtonState
        {
            Open,
            Use,
            Compound
        }

        private void OnEnable()
        {
            useButton.onClick.AddListener(OnUse);
        }

        private void OnDisable()
        {
            useButton.onClick.RemoveListener(OnUse);
        }

        public void SetData(GoodsData goodsData)
        {
            data = goodsData;
            item.SetData(goodsData, false);
            // 设置道具名字
            nameText.text = goodsData.Config.Name;
            // 根据品质设置道具名字颜色
            nameText.color = CBAColorUtil.Instance.GetColor(data.Config.Quality);
            // 设置道具描述
            descText.text = goodsData.Config.Desc;
            // 设置过期文本
            var expirationTime = goodsData.Config.ExpirationTime;
            timeRect.gameObject.SetActive(expirationTime > 0);
            timeText.text = Lang.Get(LangID.ExpirationTimeText).Replace("{time}", TimeUtils.GetUnixTimeString(expirationTime, Lang.Get(LangID.DateString)));
            mergeText.gameObject.SetActive(false);
            useButton.gameObject.SetActive(false);
            switch ((GoodsType)goodsData.Config.Type)
            {
                // 若是普通道具，也不显示此部件
                case GoodsType.Normal:
                    break;
                // 若部件3选中的是宝箱道具，则此部件显示为【打开】
                case GoodsType.Box:
                case GoodsType.RandomExistCardPieces:
                    // 若是过期宝箱道具，则不显示此部件
                    if (expirationTime <= 0 || Utils.DataConvUtil.ServerTime < expirationTime)
                    {
                        useButton.gameObject.SetActive(true);
                    }
                    // 【打开】
                    useBtnText.text = Lang.Get(LangID.OpenText);
                    buttonState = ButtonState.Open;
                    break;
                // 若是材料道具，或者使用后兑换碎片的道具，则此部件显示为【使用】
                case GoodsType.Material:
                case GoodsType.SelectProp:
                case GoodsType.SelectBoxProp:
                    useButton.gameObject.SetActive(true);
                    // 【使用】
                    useBtnText.text = Lang.Get(LangID.UseText);
                    buttonState = ButtonState.Use;
                    break;
                // 若是球员碎片道具，则显示为【合成】
                case GoodsType.Pieces:
                    // 如果存在该球员，不显示X:Y
                    mergeText.gameObject.SetActive(!Player.CardManager.CardList.Exists(item => item.CardId == goodsData.Config.Param2));
                    mergeText.text = $"{goodsData.Count}/{goodsData.Config.Param1}";
                    useButton.gameObject.SetActive(true);
                    // 【合成】
                    useBtnText.text = Lang.Get(LangID.CompoundText);
                    buttonState = ButtonState.Compound;
                    break;
                //若是道具碎片，则显示为【合成】
                case GoodsType.PropSplinter:
                    //应一直显示X:Y
                    mergeText.gameObject.SetActive(true);
                    mergeText.text = $"{goodsData.Count}/{goodsData.Config.Param1}";
                    useButton.gameObject.SetActive(true);
                    //【合成】
                    useBtnText.text = Lang.Get(LangID.CompoundText);
                    buttonState = ButtonState.Compound;
                    break;
            }

            if (goodsData.Config.Id == GoodsId.DragonYearRedEnvelope && ActivityController.Instance.IsTypeOpen(ActivityClientType.DragonYearRedEnvelope))
            {
                useButton.gameObject.SetActive(true);
                // 【使用】
                useBtnText.text = Lang.Get(LangID.UseText);
                buttonState = ButtonState.Use;
            }
        }

        private void OnUse()
        {
            switch (buttonState)
            {
                case ButtonState.Open:

                    AudioManager.Instance.PlaySound(AudioNames.BTN_1);

                    if (item.Data.Config.Uselv > Player.Level)
                    {
                        Tips.PopTips(string.Format("{0}级才能使用[{1}]", item.Data.Config.Uselv, item.Data.Config.Name));
                        return;
                    }

                    if (item.Data.Config.Type == (int)GoodsType.RandomExistCardPieces)
                    {
                        var list = Player.CardManager.GetCardList(PositionType.All, item.Data.Config.Param1);
                        if (list.Count == 0)
                        {
                            Tips.PopTips(string.Format("您没有{0}品质的球员", CBAColorUtil.Instance.GetQualityName(item.Data.Config.Param1)));
                            return;
                        }
                    }


                    UIController.Instance.OpenWindow<InventoryUseUI>(new InventoryUseUIProperties(item.Data));
                    break;
                case ButtonState.Use:
                    AudioManager.Instance.PlaySound(AudioNames.BTN_1);

                    if (data.Config.Id == GoodsId.DragonYearRedEnvelope)
                    {
                        if (ActivityController.Instance.IsTypeOpen(ActivityClientType.DragonYearRedEnvelope))
                        {
                            TriggerManager.Instance.JumpPanel(TriggerModuleType.DragonYearRedEnvelope);
                        }
                        else
                        {
                            Tips.PopTips("不在活动时间内");
                        }
                        return;
                    }
                    //兼容老代码，实在没办法统一，这么瞎几把写,优先用类型来判断打开
                    if (data.Config.Type == (int)GoodsType.SelectBoxProp)
                    {

                        UIController.Instance.OpenWindow<ChooseItemUI>(new ChooseItemUIProperties(item.Data));
                        return;
                    }

                    // 跳转到对应界面
                    int uiid = data.Config.Param1;
                    if (uiid == UIID.SelectProps)
                    {
                        var result = Player.PackageManager.GoodsFilter(item.Data.Config.Quality, 3);
                        if (result == null || result.Count == 0)
                        {
                            Tips.PopTips(string.Format("您没有{0}品质的球员", CBAColorUtil.Instance.GetQualityName(item.Data.Config.Quality)));
                            return;
                        }
                        UIController.Instance.OpenWindow<ChooseItemUI>(new ChooseItemUIProperties(item.Data));
                        return;
                    }

                    if (uiid == UIID.RenameUI)
                    {
                        // 🔴
                        Tips.PopTips("暂无改名界面和功能");
                        return;
                    }
                    if (uiid == UIID.CardUI)
                    {
                        UIController.Instance.ShowPanel<CardUI>(new CardUIProperties(CardUI.SubUIID.Card));
                        return;
                    }
                    if (uiid == UIID.RecruitUI)
                    {
                        UIController.Instance.ShowPanel<RecruitUI>(new RecruitUIProperties(RecruitUI.SubUIID.Auto));
                        return;
                    }
                    if (uiid == UIID.TacticUI)
                    {
                        Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.PVE, formation =>
                        {
                            UIController.Instance.HidePanel<InventoryUI>();
                            UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(formation, true, FormationUI.FormationShowType.Tactics));
                        });
                        return;
                    }
                    if (uiid == UIID.ArenaExShopUI)
                    {
                        NetworkManager.Instance.GetArenaInfo(resp =>
                        {
                            if (resp.Succeed)
                            {
                                UIController.Instance.ShowPanel<ArenaExShopUI>();
                            }
                            else
                            {
                                Tips.PopTips("竞技场数据返回错误");
                            }
                        });
                        return;
                    }
                    if (uiid == UIID.RecruitShopUI)
                    {
                        UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Recruit));
                        return;
                    }
                    Debug.LogWarningFormat("OnUse , 途径错误：GoodsConfig.Id = {0} , GoodsConfig.Param1 = {1}", data.Config.Id, data.Config.Param1);
                    break;
                case ButtonState.Compound:
                    AudioManager.Instance.PlaySound(AudioNames.SWITCH_MAX_3);
                    if (data.Count < data.Config.Param1)
                    {
                        // 碎片不足
                        Tips.PopError(ErrorID.DebirsNotEnough);
                        return;
                    }
                    if (data.Config.Type == (int)GoodsType.Pieces)
                    {
                        if (Player.CardManager.GetCard(data.Config.Param2) != null)
                        {
                            // 若已有该球员，点击【合成】按钮，浮动文字“你已拥有该球员”
                            Tips.PopError(ErrorID.AlreadyHavePlayer);
                            return;
                        }
                        var cfg = Configs.CardModel.GetConfig(data.Config.Param2);
                        Player.CardManager.MergeCard(cfg.PiecesId, OnMergeSucceed);
                        return;
                    }
                    else if (data.Config.Type == (int)GoodsType.PropSplinter)
                    {
                        //合成
                        //var cfg = Configs.
                        //var cfg = Configs.Goods.GetConfig(data.Config.Param2);
                        //Player.PackageManager.MergeSplinter(data.Id);
                        NetworkManager.Instance.MergeSplinter(data.Id, OnMergeSplinterSucceed);
                        return;
                    }
                    break;

            }
        }

        // 合成成功
        private void OnMergeSucceed(MergeCardResponse response)
        {
            UIController.Instance.OpenWindow<MergeCardUI>(new MergeCardUIProperties(Configs.CardModel.GetConfig(response.CardId)));
            RequestRefresh?.Invoke();
        }

        private void OnMergeSplinterSucceed(MergeSplinterResponse response)
        {
            var cfg = Configs.Goods.GetConfig(response.GoodsId);
            var properties = new InventoryObtainedUIProperties(GameItemUtils.CreateGameItem((GameItemType)cfg.Type, cfg.Id, 1));
            UIController.Instance.OpenWindow<InventoryObtainedUI>(properties);
            RequestRefresh?.Invoke();
        }
    }
}