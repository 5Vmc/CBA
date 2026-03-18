using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using Protocol;
using System.Linq;
using System.Collections.Generic;
using Utils;
using static BigBang.BattleManager;

namespace BigBang.UI
{
    public class MyGameUIProperties : PanelProperties
    {
        public int CompitionID;
        public int LeagueID;
        public string CompitionName;

        // 联赛数据
        public MyGameUIProperties(int compitionID, int leagueID, string compitionName)
        {
            CompitionID = compitionID;
            LeagueID = leagueID;
            CompitionName = compitionName;
        }
    }

    public class MyGameUI : APanelController<MyGameUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuToggle myGameToggle;
        //[SerializeField] private BabuToggle lastGameToggle;
        [SerializeField] private BabuToggle previewToggle;
        [SerializeField] private GamePreviewPad gamePreviewPad;
        [SerializeField] private MyCoursePad myCoursePad;

        public bool isReviewing = false;
        private GetLeagueCourseResponse courseResponse;
        private CourseTeamData myTeam;
        private List<LeagueCourseItemData> courses = new List<LeagueCourseItemData>();

        protected override void Awake()
        {
            base.Awake();
            //myGameToggle.DisableStatusControl();
            //lastGameToggle.DisableStatusControl();
            //previewToggle.DisableStatusControl();
        }

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            myCoursePad.AddItemClickListener(OnMyCourseItemClick);
            toggleGroup.OnValueChanged += OnToggleGroupChanged;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            myCoursePad.RemoveItemClickListener(OnMyCourseItemClick);
            toggleGroup.OnValueChanged -= OnToggleGroupChanged;
        }

        private void OnToggleGroupChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            //if (newToggle == lastGameToggle)
            //{
            //    var coursess = courseResponse.LeagueCourseItemList.ToList();
            //    var mylastCourse = coursess.FindLast(item =>
            //    {
            //        if (item.HomeGoal == -1 || item.AwayGoal == -1) return false;
            //        if (item.HomeTeam.TeamId == Player.GbId || item.AwayTeam.TeamId == Player.GbId) return true;
            //        return false;
            //    });
            //    if (mylastCourse == null)
            //    {
            //        Debug.Log("没有上一场比赛");
            //        Tips.PopError(ErrorID.NoMyGameTemporarily);
            //        //oldToggle.isOn = true;
            //        //newToggle.isOn = false;
            //        toggleGroup.Switch(oldToggle, true);
            //        return;
            //    }
            //    else
            //    {
            //        if (oldToggle == previewToggle)
            //        {
            //            OnPreviewDeselect();
            //        }
            //        if (oldToggle == myGameToggle)
            //        {
            //            OnMyGameToggleDeselect();
            //        }
            //        OnLastGameToggleSelect();
            //    }
            //}
            //if (newToggle == previewToggle)
            //{
            //    //if (oldToggle == lastGameToggle)
            //    //{
            //    //    OnLastGameToggleDeselect();
            //    //}
            //    if (oldToggle == myGameToggle)
            //    {
            //        OnMyGameToggleDeselect();
            //    }
            //    OnPreviewSelect();
            //}
            //if (newToggle == myGameToggle)
            //{
            //    if (oldToggle == previewToggle)
            //    {
            //        OnPreviewDeselect();
            //    }
            //    //if (oldToggle == lastGameToggle)
            //    //{
            //    //    OnLastGameToggleDeselect();
            //    //}
            //    OnMyGameToggleSelect();
            //}
            //oldToggle.GetComponent<StatusControl>().SetStatus(false);
            //newToggle.GetComponent<StatusControl>().SetStatus(true);

            if (oldToggle == myGameToggle) OnMyGameToggleDeselect();
            if (oldToggle == previewToggle) OnPreviewDeselect();
            if (newToggle == myGameToggle) OnMyGameToggleSelect();
            if (newToggle == previewToggle) OnPreviewSelect();
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            if (Player.BattleManager.battleEnterType == BattleManager.BattleEnterType.MyGameUI || Player.BattleManager.battleEnterType == BattleManager.BattleEnterType.MyGameUI_MyCoursePad)
            {
                toggleGroup.Switch(myGameToggle, true);
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.Unknown;
            }
            else
            {
                // 默认打开比赛预览界面
                toggleGroup.Switch(previewToggle, true);
            }
            // 比赛预览数据
            //NetworkManager.Instance.GetGamePreviewData(Properties.CompitionID, response =>
            //{
            //    gamePreviewPad.SetData(Properties.CompitionID, response);
            //    myTeam = response.HomeTeam.Team.TeamId == Player.GbId ? response.HomeTeam : response.AwayTeam;
            //});
            //// 我的赛程数据
            //NetworkManager.Instance.GetLeagueCourse(Properties.CompitionID, Properties.LeagueID, GetLeagueCourseType.Mine, response =>
            //{
            //    Player.BattleManager.getLeagueCourseResponse = response;
            //    courseResponse = response;
            //});
        }

        private void OnMyCourseItemClick(LeagueCourseItemData leagueCourseItemData, BattleEnterType battleEnterType)
        {
            if (string.IsNullOrEmpty(leagueCourseItemData.FightId))
            {
                Debug.Log("比赛未进行");
                return;
            }
            NetworkManager.Instance.GetFightReport(leagueCourseItemData.FightId, response =>
            {
                Player.BattleManager.battleEnterType = battleEnterType;
                if (Properties.CompitionID == CompitionID.Cup)
                {
                    Player.BattleManager.SetFightInfo(FightType.Cup, response);
                }
                if (Properties.CompitionID == CompitionID.League)
                {
                    Player.BattleManager.SetFightInfo(FightType.League, response);
                }
                Player.BattleManager.StartPlayFight();
            });

            //isReviewing = true;
            //OpenGameReview(leagueCourseItemData);
        }

        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_BACK);
            if (!isReviewing)
            {
                UIController.Instance.ShowPanel<HomeUI>();
            }
            else
            {
                CloseGameReview();
            }
        }

        public void HidGroup()
        {
            var canvasGroup = toggleGroup.GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.alpha = 0;
        }

        public void ShowGroup()
        {
            var canvasGroup = toggleGroup.GetComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.alpha = 1;
        }

        // 关闭比赛回顾界面
        public void CloseGameReview()
        {
            Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.MyGameUI_MyCoursePad;
            myCoursePad.gameObject.SetActive(true);
            isReviewing = false;
            ShowGroup();
            toggleGroup.Switch(myGameToggle, true);
        }

        private void OnMyGameToggleSelect()
        {
            myCoursePad.gameObject.SetActive(false);
            // 显示我的赛程界面
            NetworkManager.Instance.GetGamePreviewData(Properties.CompitionID, response =>
            {
                NetworkManager.Instance.GetLeagueCourse(Properties.CompitionID, Properties.LeagueID, GetLeagueCourseType.Mine, responseCourse =>
                {
                    myTeam = response.HomeTeam.Team.TeamId == Player.GbId ? response.HomeTeam : response.AwayTeam;
                    courseResponse = responseCourse;
                    Player.BattleManager.getLeagueCourseResponse = responseCourse;
                    myCoursePad.gameObject.SetActive(true);
                    myCoursePad.SetData(courseResponse, myTeam, Properties.CompitionID, Properties.CompitionName, BattleManager.BattleEnterType.MyGameUI_MyCoursePad);
                });
            });
        }

        //不用这个了
        private void OnLastGameToggleSelect()
        {
            NetworkManager.Instance.GetLeagueCourse(Properties.CompitionID, Properties.LeagueID, GetLeagueCourseType.Mine, response =>
            {
                courseResponse = response;
                // 查询上一场比赛
                courses = courseResponse.LeagueCourseItemList.ToList();
                var mylastCourse = courses.FindLast(item =>
                {
                    if (item.HomeGoal == -1 || item.AwayGoal == -1) return false;
                    if (item.HomeTeam.TeamId == Player.GbId || item.AwayTeam.TeamId == Player.GbId) return true;
                    return false;
                });
                if (mylastCourse == null)
                {
                    Debug.Log("没有上一场比赛");
                    Tips.PopError(ErrorID.NoMyGameTemporarily);
                    return;
                }

                // 显示上场赛况界面
                Player.BattleManager.battleEnterType = BattleManager.BattleEnterType.MyGameUI_MyLastGamePad;
                Player.BattleManager.getLeagueCourseResponse = response;
            });

            // var courses = courseResponse.LeagueCourseItemList.ToList();

        }

        private void OnPreviewSelect()
        {
            gamePreviewPad.gameObject.SetActive(false);
            // 联赛            
            NetworkManager.Instance.GetGamePreviewData(Properties.CompitionID, response =>
            {
                gamePreviewPad.gameObject.SetActive(true);
                // 显示比赛预览界面
                gamePreviewPad.SetData(Properties.CompitionID, response);
            });
        }

        private void OnMyGameToggleDeselect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            // 关闭我的赛程界面
            myCoursePad.gameObject.SetActive(false);
        }

        private void OnLastGameToggleDeselect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
        }

        private void OnPreviewDeselect()
        {
            AudioManager.Instance.PlaySound(AudioNames.SWITCH_COL);
            // 关闭比赛预览界面
            gamePreviewPad.gameObject.SetActive(false);
        }
    }
}