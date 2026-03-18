using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using UnityTimer;
using BigBang.Animation;
using UnityEngine.UI;
using System.Linq;
using Utils;

namespace BigBang.UI
{
    public class AchievementTipsUIProperties : WindowProperties
    {
        public int AchievementID;

        public AchievementTipsUIProperties(int achievementID)
        {
            AchievementID = achievementID;
        }
    }

    public class AchievementTipsUI : AWindowController<AchievementTipsUIProperties>
    {
        [SerializeField] private TMP_Text nameTxt;
        [SerializeField] private TMP_Text descTxt;
        [SerializeField] private TMP_Text pointTxt1;
        [SerializeField] private TMP_Text pointTxt2;

        [SerializeField] private Image icon;

        [SerializeField] private AchievementTipsUIAnim Anim;

        [SerializeField] private Image point = null;

        protected override async void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            var data = Player.AchievementManager.GetAchievementData(Properties.AchievementID);
            nameTxt.text = data.Config.Name;

            var current = 0;
            var MaxProgress = 0;
            if (data.Config.Fungroup == 1033)
            {
                //对于通关的成就，max = 1
                current = data.Current >= data.Config.Id ? 1 : 0;
                MaxProgress = 1;
            }
            else
            {
                current = data.Current;
                MaxProgress = data.Config.Target[0];
            }
            var descStr = "";
            if (current >= MaxProgress)
            {
                descStr += " <size=24>" + string.Format(data.Config.Desc, "<color=#C1AB6B>" + current.ToString() + "</color>", MaxProgress) + "</size>";
            }
            else
            {
                descStr += " <size=24>" + string.Format(data.Config.Desc, current.ToString(), MaxProgress) + "</size>";
            }

            descTxt.text = descStr;

            if (data.Config.Module != (int)TriggerModuleType.Honour)
            {
                point.gameObject.SetActive(false);
                icon.sprite = await SpriteProxy.GetHonourCup(data.Config.Icon);
            }
            else
            {
                point.gameObject.SetActive(true);
                pointTxt1.text = pointTxt2.text = data.Config.Point.ToString();
                icon.sprite = await SpriteProxy.GetAchievementIcon(data.Config.Fungroup);
            }

            Anim.PlayEnter();
            Timer.Register(this.gameObject, 5, () =>
            {
                Anim.PlayExit(OnClose);
            });

        }

        private void OnClose()
        {
            UIController.Instance.CloseWindow<AchievementTipsUI>();
        }
    }
}