using BigBang.Animation;
using deVoid.UIFramework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class TipsUIProperties : WindowProperties
    {
        public string Content { get; private set; }

        public TipsUIProperties(string content)
        {
            Debug.Log("Tips , content = " + content);
            Content = content;
        }
    }

    public class TipsUI : AWindowController<TipsUIProperties>
    {
        [SerializeField] private TipsUIComponent com;
        [SerializeField] private Image info;
        [SerializeField] private Transform container;
        [SerializeField] private TipsUIAnim anim;

        private int count = 1;
        protected override void OnPropertiesSet()
        {
            AudioManager.Instance.PlaySound(AudioNames.ANI_TIPS);

            List<string> list = Properties.Content.Split("|").ToList();
            Image[] objList = container.GetComponentsInChildren<Image>(true);
            count = objList.Count() - 1;
            int _count = System.Math.Max(count, list.Count);

            for (var index = 0; index < _count; index++) {
                Image item;
                if (index >= count)
                {
                    item = Instantiate(info, container);
                    count++;
                }
                else item = objList[index + 1];

                if (index < list.Count)
                {
                    item.gameObject.SetActive(true);
                    item.GetComponentInChildren<TMP_Text>().text = list[index];
                }
                else {
                    item.gameObject.SetActive(false);
                }
                
            }

            anim.Play(Disappear);
        }

        private void Disappear()
        {
            //Hide();
            UIController.Instance.CloseWindow<TipsUI>();
        }
    }
}
