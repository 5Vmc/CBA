using System.Collections;
using System.Collections.Generic;
using Babu;
using BigBang;
using BigBang.Animation;
using BigBang.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

public class AchievementPad : MonoBehaviour
{
    public enum SubUIID
    {
        Team = 0,
        Player = 1,
        Develop = 2,
        Total = 3,
    }

    [SerializeField] private BabuToggleGroup bottomToggleGroup;
    [SerializeField] private RectTransform listPad;
    [SerializeField] private RectTransform previewPad;
    [SerializeField] private AchievementUIAdapter adapter;
    [SerializeField] private TMP_Text scoreTxt;
    [SerializeField] private TMP_Text titleTxt;
    [SerializeField] private TMP_Text addTxt;
    [SerializeField] private TMP_Text teamProgressTxt;
    [SerializeField] private TMP_Text playerProgressTxt;
    [SerializeField] private TMP_Text developProgressTxt;
    [SerializeField] private TMP_Text pointTxt;
    [SerializeField] private Button team;
    [SerializeField] private Button player;
    [SerializeField] private Button develop;
    [SerializeField] private AchievementProgressItem progressItem;
    [SerializeField] public List<Transform> RedDotList;

    [SerializeField] public AchievementPadAnim Anim;

    private void OnEnable()
    {
        bottomToggleGroup.OnValueChanged += OnToggleChanged;

        team.onClick.AddListener(OnTeam);
        player.onClick.AddListener(OnPlayer);
        develop.onClick.AddListener(OnDevelop);

        progressItem.OnProgressAnimCompleted += OnProgressAnimCompleted;
        progressItem.OnDelChangePoint += OnDelChangePoint;
        EventManager.Instance.Register(EventID.RefreshUIRedDot, RefreshRedDot);
        EventManager.Instance.Register(EventID.OnAfterGetAchievementReward, RefreshAchievementPoint);

        RefreshRedDot();
    }
    private void OnDisable()
    {
        bottomToggleGroup.OnValueChanged -= OnToggleChanged;

        team.onClick.RemoveListener(OnTeam);
        player.onClick.RemoveListener(OnPlayer);
        develop.onClick.RemoveListener(OnDevelop);

        progressItem.OnProgressAnimCompleted -= OnProgressAnimCompleted;
        progressItem.OnDelChangePoint -= OnDelChangePoint;
        EventManager.Instance.Unregister(EventID.RefreshUIRedDot, RefreshRedDot);
        EventManager.Instance.Unregister(EventID.OnAfterGetAchievementReward, RefreshAchievementPoint);
    }

    private void OnToggleChanged(BabuToggle oldToggle, BabuToggle newToggle)
    {
        int selectedIndex = bottomToggleGroup.EnableIndex;
        ShowPad((SubUIID)selectedIndex);
    }
    private void ShowPad(SubUIID padIndex)
    {
        HideAllPad();
        Player.AchievementManager.CountAllData();
        switch (padIndex)
        {
            case SubUIID.Team: OnShowTeam(); break;
            case SubUIID.Player: OnShowPlayer(); break;
            case SubUIID.Develop: OnShowDevelop(); break;
            case SubUIID.Total: OnShowTotal(); break;
        }
    }
    private void HideAllPad()
    {
        previewPad.gameObject.SetActive(false);
        listPad.gameObject.SetActive(false);
    }
    private void OnShowTeam()
    {
        listPad.gameObject.SetActive(true);
        adapter.SetData(Player.AchievementManager.AchGroupData[(int)AchievementType.Team].Where(item => item.list[0].Config.IsHide == 0).ToList());
        pointTxt.text = Player.AchievementManager.GetOwnPoint(AchievementType.Team) + "/" + Player.AchievementManager.GetTotalPoint(AchievementType.Team);
        Anim.PlayListPadAnim();
        adapter.PlayAnim();
        titleTxt.text = "球队成就";
    }
    private void OnShowPlayer()
    {
        listPad.gameObject.SetActive(true);
        adapter.SetData(Player.AchievementManager.AchGroupData[(int)AchievementType.Player].Where(item => item.list[0].Config.IsHide == 0).ToList());
        pointTxt.text = Player.AchievementManager.GetOwnPoint(AchievementType.Player) + "/" + Player.AchievementManager.GetTotalPoint(AchievementType.Player);
        Anim.PlayListPadAnim();
        adapter.PlayAnim();
        titleTxt.text = "球员成就";
    }
    private void OnShowDevelop()
    {
        listPad.gameObject.SetActive(true);
        adapter.SetData(Player.AchievementManager.AchGroupData[(int)AchievementType.Develop].Where(item => item.list[0].Config.IsHide == 0).ToList());
        pointTxt.text = Player.AchievementManager.GetOwnPoint(AchievementType.Develop) + "/" + Player.AchievementManager.GetTotalPoint(AchievementType.Develop);
        Anim.PlayListPadAnim();
        adapter.PlayAnim();
        titleTxt.text = "养成成就";
    }
    private void OnShowTotal()
    {
        previewPad.gameObject.SetActive(true);
        scoreTxt.text = Player.AchievementManager.GetOwnPoint(AchievementType.All) + "/" + Player.AchievementManager.GetTotalPoint(AchievementType.All);
        titleTxt.text = "成就总览";
    }

    public void RefreshAchievementPoint(object[] _)
    {
        Player.AchievementManager.CountAllData();
        switch (bottomToggleGroup.EnableIndex)
        {
            case 0:
                pointTxt.text = Player.AchievementManager.GetOwnPoint(AchievementType.Team) + "/" + Player.AchievementManager.GetTotalPoint(AchievementType.Team);
                break;
            case 1:
                pointTxt.text = Player.AchievementManager.GetOwnPoint(AchievementType.Player) + "/" + Player.AchievementManager.GetTotalPoint(AchievementType.Player);
                break;
            case 2:
                pointTxt.text = Player.AchievementManager.GetOwnPoint(AchievementType.Develop) + "/" + Player.AchievementManager.GetTotalPoint(AchievementType.Develop);
                break;
            case 3:
                scoreTxt.text = Player.AchievementManager.GetOwnPoint(AchievementType.All) + "/" + Player.AchievementManager.GetTotalPoint(AchievementType.All);
                break;
        }
    }

    public void RefreshRedDot(object[] args = null)
    {
        RedDotNode node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Achieve, "/1");
        node.IsRed(RedDotList[0]);
        node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Achieve, "/2");
        node.IsRed(RedDotList[1]);
        node = RedDotManager.Instance.ConfirmNode(PanelNodePath.Home_Achieve, "/3");
        node.IsRed(RedDotList[2]);
    }

    public void OnShow()
    {
        Player.AchievementManager.CountAllData();
        bottomToggleGroup.Switch(3);
        teamProgressTxt.text = $"{Player.AchievementManager.GetCompletedCount(AchievementType.Team)}<color=#BCD1E2>/{Player.AchievementManager.GetCount(AchievementType.Team)}</color>";
        playerProgressTxt.text = $"{Player.AchievementManager.GetCompletedCount(AchievementType.Player)}<color=#BCD1E2>/{Player.AchievementManager.GetCount(AchievementType.Player)}</color>";
        developProgressTxt.text = $"{Player.AchievementManager.GetCompletedCount(AchievementType.Develop)}<color=#BCD1E2>/{Player.AchievementManager.GetCount(AchievementType.Develop)}</color>";
        scoreTxt.text = $"{Player.AchievementManager.GetOwnPoint(AchievementType.All)}/{Player.AchievementManager.GetTotalPoint(AchievementType.All)}";
        int ownPoint = Player.AchievementManager.GetOwnPoint(AchievementType.All);
        int totalPoint = Player.AchievementManager.GetTotalPoint(AchievementType.All);
        addTxt.gameObject.SetActive(Player.AchievementManager.GetPointChangeValue() > 0);
        addTxt.text = $"+{Player.AchievementManager.GetPointChangeValue()}";
        RefreshRedDot();
        progressItem.InitAnim();
        Anim.PlayEnter(() =>
        {
            progressItem.PlayAnim();
        });
    }

    private void OnProgressAnimCompleted()
    {
        addTxt.DOFade(0, 0.3f).OnComplete(() =>
        {
            addTxt.gameObject.SetActive(false);
        });
    }

    private void OnDelChangePoint(int point)
    {
        addTxt.text = $"+{point}";
        scoreTxt.text = Player.AchievementManager.GetOwnPoint(AchievementType.All) - point + "/" + Player.AchievementManager.GetTotalPoint(AchievementType.All);
    }

    private void OnTeam()
    {
        Anim.PlayTeamAnim(() =>
        {
            bottomToggleGroup.Switch(0);
        });
    }

    private void OnPlayer()
    {
        Anim.PlayPlayerAnim(() =>
        {
            bottomToggleGroup.Switch(1);
        });
    }

    private void OnDevelop()
    {
        Anim.PlayDevelopAnim(() =>
        {
            bottomToggleGroup.Switch(2);
        });
    }
}
