using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using deVoid.UIFramework;
using BigBang.Animation;
using Utils;
using Babu;

namespace BigBang.UI
{
    public class BreakthroughProperties : WindowProperties
    {
        public string Name { get; private set; }
        public string Name1 { get; private set; }
        public string Count { get; private set; }
        public string Count1 { get; private set; }
        public string Level { get; private set; }

        public BreakthroughProperties(string name, string count, string level, string count1, string name1)
        {
            Name = name;
            Count = count;
            Level = level;
            Count1 = count1;
            Name1 = name1;

        }
    }

    public class BreakthroughUI : AWindowController<BreakthroughProperties>
    {
        [SerializeField] private BreakthroughUIAnim anim;
        [SerializeField] private BreakthroughComponent com;

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
            UIController.Instance.CloseWindow<BreakthroughUI>();
            //显示下一条消息
            Player.TrainManager.ShowMessage();
        }

        public void OnClose()
        {
            anim.PlayNext(OnAnim2Finshed);

           
        }
    }
}