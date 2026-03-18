using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using System;
using TMPro;
using Protocol;
using Utils;
using Babu;
using System.Collections.Generic;
using Utils.GameItem;
using System.Linq;
using System.Text.RegularExpressions;
using Babu.SDK;

namespace BigBang.UI
{

    public class SettingsUI : AWindowController
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button nameReviseBtn;
        [SerializeField] private Button giftBtn;
        [SerializeField] private ClubIconItem clubIcon;
        [SerializeField] private TMP_Text clubNameText;
        [SerializeField] private TMP_Text clubIdText;
        [SerializeField] private TMP_Text clubEnergyText;
        [SerializeField] private TMP_Text lengueCountText;
        [SerializeField] private TMP_Text cupCountText;
        [SerializeField] private TMP_InputField codeField;
        [SerializeField] private Button musicOnBtn;
        [SerializeField] private Button musicOffBtn;
        [SerializeField] private Button soundOnBtn;
        [SerializeField] private Button soundOffBtn;
        [SerializeField] private DropdownBox languageDropdown;
        [SerializeField] private RectTransform qQPanel = null;
        [SerializeField] private RectTransform eMailPanel = null;
        [SerializeField] private TMP_Text serverTimeText = null;
        [SerializeField] private TMP_Text serverNameText = null;

        #region 使用密码打开 Debug面板：123123234 热更新面板：123123222 百分数据查询面板：123123111

        [SerializeField] private BabuButton passButton1 = null;
        [SerializeField] private BabuButton passButton2 = null;
        [SerializeField] private BabuButton passButton3 = null;
        [SerializeField] private BabuButton passButton4 = null;
        private void OnClickPassButton1(BabuButton _)
        {
            InputPassNum(1);
            InputPassNum2(1);
            InputPassNum3(1);
        }
        private void OnClickPassButton2(BabuButton _)
        {
            InputPassNum(2);
            InputPassNum2(2);
            InputPassNum3(2);
        }
        private void OnClickPassButton3(BabuButton _)
        {
            InputPassNum(3);
            InputPassNum2(3);
            InputPassNum3(3);
        }
        private void OnClickPassButton4(BabuButton _)
        {
            InputPassNum(4);
            InputPassNum2(4);
            InputPassNum3(4);
        }

        private readonly List<int> passwordList = new() { 1, 2, 3, 1, 2, 3, 2, 3, 4 };
        private List<int> inputPassList = new();
        private void InputPassNum(int num)
        {
            inputPassList.Add(num);
            while (inputPassList.Count > passwordList.Count)
            {
                inputPassList.RemoveAt(0);
            }
            if (inputPassList.Count != passwordList.Count)
            {
                return;
            }
            for (int i = 0; i < inputPassList.Count; i++)
            {
                if (inputPassList[i] != passwordList[i])
                {
                    return;
                }
            }
            AudioManager.Instance.PlaySound(AudioNames.BTN_UNLOCK);
            if (Player.isDeveloper) UIController.Instance.OpenWindow<DevelopUI>();
        }

        private readonly List<int> passwordList2 = new() { 1, 2, 3, 1, 2, 3, 2, 2, 2 };
        private List<int> inputPassList2 = new();
        private void InputPassNum2(int num)
        {
            inputPassList2.Add(num);
            while (inputPassList2.Count > passwordList2.Count)
            {
                inputPassList2.RemoveAt(0);
            }
            if (inputPassList2.Count != passwordList2.Count)
            {
                return;
            }
            for (int i = 0; i < inputPassList2.Count; i++)
            {
                if (inputPassList2[i] != passwordList2[i])
                {
                    return;
                }
            }
            AudioManager.Instance.PlaySound(AudioNames.BTN_UNLOCK);
            UIController.Instance.OpenWindow<TestServerUI>();
        }

        private readonly List<int> passwordList3 = new() { 1, 2, 3, 1, 2, 3, 1, 1, 1 };
        private List<int> inputPassList3 = new();
        private void InputPassNum3(int num)
        {
            inputPassList3.Add(num);
            while (inputPassList3.Count > passwordList3.Count)
            {
                inputPassList3.RemoveAt(0);
            }
            if (inputPassList3.Count != passwordList3.Count)
            {
                return;
            }
            for (int i = 0; i < inputPassList3.Count; i++)
            {
                if (inputPassList3[i] != passwordList3[i])
                {
                    return;
                }
            }
            AudioManager.Instance.PlaySound(AudioNames.BTN_UNLOCK);
            UIController.Instance.CloseWindow<SettingsUI>();
            UIController.Instance.ShowPanel<HundredDataUI>();
        }

        #endregion

        private CourseTeamData myTeam;
        private int expireTime = 0;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            nameReviseBtn.onClick.AddListener(OnReseiveName);
            musicOnBtn.onClick.AddListener(OnTurnOffMusic);
            musicOffBtn.onClick.AddListener(OnTurnOnMusic);
            soundOnBtn.onClick.AddListener(OnTurnOffSound);
            soundOffBtn.onClick.AddListener(OnTurnOnSound);
            giftBtn.onClick.AddListener(GetCodeGift);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            passButton1.OnClick += OnClickPassButton1;
            passButton2.OnClick += OnClickPassButton2;
            passButton3.OnClick += OnClickPassButton3;
            passButton4.OnClick += OnClickPassButton4;

            EventManager.Instance.Register(EventID.OnPlayerHeadChange, UpdatePlayerName);

            zhuXiaoBtn.onClick.AddListener(OnZhuXiao);
            policyButton.OnClick += OnClickPolicyButton;
            SecondUpdateManager.Instance.RegistAction(RefreshServerTime);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            nameReviseBtn.onClick.RemoveListener(OnReseiveName);
            musicOnBtn.onClick.RemoveListener(OnTurnOffMusic);
            musicOffBtn.onClick.RemoveListener(OnTurnOnMusic);
            soundOnBtn.onClick.RemoveListener(OnTurnOffSound);
            soundOffBtn.onClick.RemoveListener(OnTurnOnSound);
            giftBtn.onClick.RemoveListener(GetCodeGift);
            languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
            passButton1.OnClick -= OnClickPassButton1;
            passButton2.OnClick -= OnClickPassButton2;
            passButton3.OnClick -= OnClickPassButton3;
            passButton4.OnClick -= OnClickPassButton4;
            EventManager.Instance.Unregister(EventID.OnPlayerHeadChange, UpdatePlayerName);
            zhuXiaoBtn.onClick.RemoveListener(OnZhuXiao);
            policyButton.OnClick -= OnClickPolicyButton;
            SecondUpdateManager.Instance.UnRegistAction(RefreshServerTime);
        }

        private void RefreshServerTime()
        {
            serverTimeText.text = "服务器时间：<color=#243745>{0}</color>".SafeFormat(DataConvUtil.ServerDateTime.ToString("yyyy/MM/dd HH:mm:ss"));
        }

        /// <summary>
        /// 领取礼包码
        /// </summary>
        private void GetCodeGift()
        {
            if (Utils.DataConvUtil.ServerTime < expireTime)
            {
                Tips.PopTips("请稍后重试..." + (Math.Abs(Utils.DataConvUtil.ServerTime - expireTime)).ToString());
                return;
            }
            var txtGiftCode = codeField.text;
            Regex regex = new Regex("[a-zA-Z0-9]{6,100}");
            if (!regex.IsMatch(txtGiftCode))
            {
                Tips.PopTips("请输入正确的礼包码");
                return;
            }
            expireTime = (int)DataConvUtil.ServerTime + 7;
            NetworkManager.Instance.GetCodeGift(txtGiftCode, (resp) =>
            {
                if (resp.ReceiveSucceed)
                {
                    var list = GameItemUtils.CreateGameItems(resp.Reward).ToList();
                    UIController.Instance.OpenWindow<InventoryObtainedUI>(new InventoryObtainedUIProperties(list));
                    codeField.text = "";
                }
                else
                {
                    Tips.PopTips("礼包码错误或已领取过该礼包码");
                    Debug.Log("resp.ReceiveSucceed == false , " + resp.Msg);
                }
            });
        }

        private void OnLanguageChanged(int index)
        {

        }

        [SerializeField] private TMP_Text clientVersionText = null;
        [SerializeField] private TMP_Text resVersionText = null;
        [SerializeField] private TMP_Text majorVersionText = null;
        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            clubIcon.SetIcon(Player.Icon);
            clubNameText.text = Player.Name;
            clubIdText.text = Player.GbId;

            clubEnergyText.text = Player.Strength.ToString();
            //releaseText.text = "版本号" + ColorString.GetColorString("#243745", ReleaseShow());
            //majorVersionText.text = Lang.Get(LangID.VersionTxt) + ColorString.GetColorString("#243745", ReleaseShow());
            majorVersionText.text = "主版本号：" + ColorString.GetColorString("#243745", Babu.Environment.GetValue("major_version", "").ToString());
            resVersionText.text = "资源版本号：" + ColorString.GetColorString("#243745", Babu.Environment.bundleCreatTime);
            clientVersionText.text = "客户端版本号：" + ColorString.GetColorString("#243745", Babu.Environment.GetValue("client_creat_time", "").ToString());

            cupCountText.text = Player.PVPManager.CupTrophyCount.ToString();
            lengueCountText.text = Player.PVPManager.LeagueTrophyCount.ToString();

            musicOnBtn.gameObject.SetActive(AudioManager.Instance.IsMusicEnable);
            musicOffBtn.gameObject.SetActive(!AudioManager.Instance.IsMusicEnable);

            soundOnBtn.gameObject.SetActive(AudioManager.Instance.IsSoundEnable);
            soundOffBtn.gameObject.SetActive(!AudioManager.Instance.IsSoundEnable);

            qQPanel.gameObject.SetActive(ChannelManager.Instance.EnableQQ);
            eMailPanel.gameObject.SetActive(ChannelManager.Instance.EnableMail);
            RefreshServerTime();
            serverNameText.text = "所属服务器：<color=#243745>【{0}区】{1}</color>".SafeFormat(Player.ServerData.Id, Player.ServerData.AliasName);
        }

        public void UpdatePlayerName(object[] args)
        {
            clubNameText.text = Player.Name;
        }
        //版本号
        private string ReleaseShow()
        {
            string version = Application.version.Replace(".", "") + Babu.Environment.GetValue<string>("minor_version", "");
            return version.Split('-')[0];
        }

        //打开音乐
        private void OnTurnOnMusic()
        {
            musicOffBtn.gameObject.SetActive(false);
            musicOnBtn.gameObject.SetActive(true);
            AudioManager.Instance.EnableMusic();
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        }
        //关闭音乐
        private void OnTurnOffMusic()
        {
            musicOffBtn.gameObject.SetActive(true);
            musicOnBtn.gameObject.SetActive(false);
            AudioManager.Instance.DisableMusic();
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        }
        //打开音效
        private void OnTurnOnSound()
        {
            soundOnBtn.gameObject.SetActive(true);
            soundOffBtn.gameObject.SetActive(false);
            AudioManager.Instance.EnableSound();
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        }
        //关闭音效
        private void OnTurnOffSound()
        {
            soundOnBtn.gameObject.SetActive(false);
            soundOffBtn.gameObject.SetActive(true);
            AudioManager.Instance.DisableSound();
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        }

        //关闭窗口
        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.CloseWindow<SettingsUI>();
        }

        //修改玩家名字窗口打开
        private void OnReseiveName()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.OpenWindow<Settings2UI>();
        }

        [SerializeField] private Button zhuXiaoBtn = null;
        //注销账号（华为渠道要求）
        //修改建议: 请在游戏内提供帐号注销服务，功能选项名称需表述清晰，需使用如“注销帐号”等表述
        //A:游戏自己做个注销账号的按钮，然后调用logout注销接口即可
        //quickSDK 未提供真正的注销账号功能，使用服务器的注销接口，注销之后直接退出游戏
        private void OnZhuXiao()
        {
            //UIController.Instance.CloseWindow<SettingsUI>();
            //SDKManagerBeforeHotFix.Instance.quickManager.LogOut();
            UIController.Instance.OpenWindow<DeletePlayerUI>();
        }

        [SerializeField] private BabuButton policyButton = null;
        //隐私协议（oppo 渠道要求）
        private void OnClickPolicyButton(BabuButton _)
        {
            var url = PolicyConst.GetPolicyUrl();
            Application.OpenURL(url);
        }
        /*1.游戏存在【基础功能问题】，游戏存在内须添加游戏存在方的客服联系方式，联系方式要能正常运作；
2.开发者你好，游戏首次登入查看隐私政策后，游戏内需要可以再次找到隐私政策按钮；游戏存在【隐私政策要求问题】，隐私政策文本需易于访问和阅读；（不高于4次点击能够访问到），
3.开发者你好，游戏里未屏蔽涉政、涉黄敏感词，如：看片
修改建议：
1、在设置页面，增加客服QQ的文字信息；
2、在设置页面的注销账号下放，增加隐私协议的跳转按钮，点击跳转至：https://xmzt.ximiplay.com/web/ximi/privacy%20policy.html
3、屏蔽词库中加入“看片”这个词。*/
    }
}