using System.Collections.Generic;
using Babu;
using BigBang.Animation;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using static BigBang.UI.NavigationButton;

namespace BigBang.UI
{

    public class NavigationPad : MonoBehaviour
    {

        //[SerializeField] private List<NavigationButton> itemList;

        [SerializeField] public NavigationButtonType showPanel = NavigationButtonType.Home;

        //private void OnEnable()
        //{
        //    //UpdateSelectImage();
        //    //foreach (NavigationButton navigationButton in itemList)
        //    //{
        //    //    navigationButton.SetClickCallBack(OnClickTabBtn);
        //    //}
        //    //EventManager.Instance.Register(EventID.OnRefreshNavigationUIRedDot, RefreshRedDotStatus);

        //    //RefreshRedDotStatus();
        //}

        //private void OnDisable()
        //{
        //    EventManager.Instance.Unregister(EventID.OnRefreshNavigationUIRedDot, RefreshRedDotStatus);
        //}

        //[SerializeField] public List<Transform> RedDotList;
        //public void RefreshRedDotStatus(object[] args = null)
        //{
        //    RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Card, "");
        //    node.IsRed(RedDotList[0]);
        //    node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Formation, "");
        //    node.IsRed(RedDotList[1]);
        //    node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home, "");
        //    node.IsRed(RedDotList[2]);
        //    node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Recruit, "");
        //    node.IsRed(RedDotList[3]);
        //    node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Shop, "");
        //    node.IsRed(RedDotList[4]);
        //}


        //private void UpdateSelectImage()
        //{
        //    foreach (NavigationButton navigationButton in itemList)
        //    {
        //        navigationButton.SetLight(_showPanel == navigationButton.navigationButtonType);
        //    }
        //}

        //private void OnClickTabBtn(NavigationButtonType newType)
        //{
        //    if (_showPanel == newType) return;
        //    //_showPanel = newType;
        //    //UpdateSelectImage();
        //    switch (newType)
        //    {
        //        case NavigationButtonType.Player: OnCard(); break;
        //        case NavigationButtonType.Formation: OnFormation(); break;
        //        case NavigationButtonType.Home: OnHome(); break;
        //        case NavigationButtonType.Recruit: OnRecruit(); break;
        //        case NavigationButtonType.Shop: OnShop(); break;
        //    }
        //}

        //private void OnCard()
        //{
        //    //cardBtn.GetComponent<ButtonAnim>().Play(null);
        //    //homeAnim.PlayExit(() =>
        //    //{
        //    Player.FightManager.LoginSuccess();
        //    UIController.Instance.ShowPanel<CardUI>(new CardUIProperties(CardUI.SubUIID.Card));
        //    //});
        //}

        //private void OnFormation()
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.SWITCH_HOME);
        //    Player.FightManager.FormationController.GetAndCheckDefaultFormation(FormationID.PVE, formation =>
        //    {
        //        UIController.Instance.ShowPanel<FormationUI>(new FormationProperties(formation, true, FormationUI.FormationShowType.Formation));
        //    });
        //}

        //private void OnHome()
        //{
        //    //var animOut = UIController.Instance.CurrentPanel.GetComponent<AnimOut>();
        //    //AudioManager.Instance.PlaySound(AudioNames.SWITCH_HOME);
        //    ////如果有退出动画，播放退出动画
        //    //if (animOut != null)
        //    //{
        //    //    TouchManager.Instance.DisableTouch();
        //    //    animOut.Play(() =>
        //    //    {
        //    //        TouchManager.Instance.EnableTouch();
        //    //        UIController.Instance.ShowPanel<HomeUI>();
        //    //    });
        //    //}
        //    //else
        //    //{
        //    UIController.Instance.ShowPanel<HomeUI>(new HomeUIProperties(true));
        //    //}
        //}


        //private void OnRecruit()
        //{
        //    UIController.Instance.ShowPanel<RecruitUI>();
        //}
        //private void OnShop()
        //{
        //    AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        //    UIController.Instance.ShowPanel<ShopUI>(new ShopUIProperties(ShopUI.SubUIID.Diamond));
        //}

    }
}
