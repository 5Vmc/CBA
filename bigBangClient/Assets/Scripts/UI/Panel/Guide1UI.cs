using UnityEngine;
using deVoid.UIFramework;
using TMPro;
using BigBang.Animation;
using System.Collections.Generic;
using Babu.Client.Fsm;

namespace BigBang.UI
{
    public class Guide1UI : APanelController
    {
        [SerializeField] private List<TMP_Text> content;
        [SerializeField] private BabuButton closeBtn;

        [SerializeField] public Guide1UIAnim Anim;

        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.OnClick += OnClose;
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.OnClick -= OnClose;
        }

        private List<string> contentStrList = new();
        protected override void Awake()
        {
            foreach (var item in content)
            {
                contentStrList.Add(item.text);
            }
        }

        protected override void OnPropertiesSet()
        {
            base.OnPropertiesSet();
            GuideManager.DoGuide(GuideID.directorsLetter);
            //for (int i = 0; i < contentStrList.Count; i++)
            //{
            //    content[i].text = contentStrList[i].Replace("{name}", Player.Name);
            //}
            for (int i = 0; i < contentStrList.Count; i++)//此界面移动到了取名界面之前，此时并不清楚俱乐部名字
            {
                content[i].text = contentStrList[i].Replace("{name}", "");
            }
            Anim.PlayEnter();
        }

        private void OnClose(BabuButton sender)
        {
            Anim.PlayExit(() =>
            {
                // 完成新手引导1
                //GuideManager.Finish(GuideID.directorsLetter);此时还没有登录呢，第一次登录算过
                FsmManager.Instance.ChangeToState<StateCreatePlayer>(new StateCommonUserData()
                {
                    OpenUIAction = async () =>
                    {
                        await UIController.Instance.ShowPanel<CreatePlayerUI>();
                    }
                });

            });
        }
    }
}