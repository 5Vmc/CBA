using Babu;
using BigBang.Animation;
using deVoid.UIFramework;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class BigBreakthroughProperties : WindowProperties
    {
        public string Name { get; private set; }
        public string Name1 { get; private set; }
        public string Count { get; private set; }
        public string Count1 { get; private set; }
        public string Level { get; private set; }

        public BigBreakthroughProperties(string name, string count, string level, string count1, string name1)
        {
            Name = name;
            Count = count;
            Level = level;
            Count1 = count1;
            Name1 = name1;

        }
    }

    public class BigBreakthroughUI : AWindowController<BigBreakthroughProperties>
    {
        [SerializeField] private BigBreakthroughUIAnim anim;
        [SerializeField] private BigBreakthroughComponent com;

        protected override void AddListeners()
        {
            com.CloseBtn.onClick.AddListener(OnClose);
        }

        protected override void RemoveListeners()
        {
            com.CloseBtn.onClick.RemoveListener(OnClose);
        }

        protected override void OnPropertiesSet()
        {
            com.NameText.text = Properties.Name;
            for (int i = 0; i < com.Txts.Count; i++)
            {
                if (i < Properties.Count.Length)
                {
                    com.Txts[i].text = Properties.Count[i].ToString();
                    com.Txts[i].gameObject.SetActive(true);
                }
                else
                {
                    com.Txts[i].gameObject.SetActive(false);
                }
            }
            if (Properties.Count1[1].ToString() == "0")
            {
                com.Image1.gameObject.SetActive(false);
            }
            else
            {
                com.Image1.gameObject.SetActive(true);
                com.NameText1.text = Properties.Name1;
                for (int i = 0; i < com.Txts1.Count; i++)
                {
                    if (i < Properties.Count1.Length)
                    {
                        com.Txts1[i].text = Properties.Count1[i].ToString();
                        com.Txts1[i].gameObject.SetActive(true);
                    }
                    else
                    {
                        com.Txts1[i].gameObject.SetActive(false);
                    }
                }
            }

            com.LevelText.text = Properties.Level + Lang.Get(LangID.LvTxt);
            TouchManager.Instance.DisableTouch();
            anim.Play(OnAnin1Finshed);
            AudioManager.Instance.PlaySound(AudioNames.EVENT_BREAK);
        }

        public void OnAnin1Finshed()
        {
            TouchManager.Instance.EnableTouch();
        }

        public void OnAnim2Finshed()
        {
            TouchManager.Instance.EnableTouch();
            UIController.Instance.CloseWindow<BigBreakthroughUI>();
            //显示下一条消息
            Player.TrainManager.ShowMessage();
        }

        public void OnClose()
        {
            anim.PlayNext(OnAnim2Finshed);
            Player.CalFightPoint(true);
            //EventManager.Instance.Dispatch(EventID.OnStrenthChanged);
        }
    }
}