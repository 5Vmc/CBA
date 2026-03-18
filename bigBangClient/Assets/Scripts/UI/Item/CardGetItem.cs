using BigBang.Animation;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class CardGetItem : MonoBehaviour
    {
        // 前往按钮
        [SerializeField] private Button goBtn;
        // 标题文字
        [SerializeField] private TMP_Text itemText;
        [SerializeField] private TMP_Text txtOpenInfo;

        private int moduleId;

        private void OnEnable()
        {
            goBtn.onClick.AddListener(OnGo);
        }

        private void OnDisable()
        {
            goBtn.onClick.RemoveListener(OnGo);
        }

        public void SetData(int index, int way, string modulename, string desc, int itemId, int needCount)
        {
            this.moduleId = way;
            this.itemId = itemId;
            this.needCount = needCount;
            // 设置途径描述
            itemText.text = "<color=#C16800>" + modulename + "</color>" + desc;

            if (index % 2 == 0)
            {
                this.GetComponent<Image>().SetAlpha(0f);
            }
            else
            {
                this.GetComponent<Image>().SetAlpha(0.2f);
            }

            txtOpenInfo.text = TriggerManager.Instance.GetModuleOpenInfo(moduleId);
            txtOpenInfo.gameObject.SetActive(txtOpenInfo.text != "");
            goBtn.gameObject.SetActive(txtOpenInfo.text == "");
        }

        private int itemId = -1;
        private int needCount = 1;

        private void OnGo()
        {
            UIController.Instance.CloseWindow<SupplementUI>();
            //Debug.Log(cfg.Explain);
            goBtn.GetComponent<ButtonAnim>().Play(() =>
            {
                TriggerManager.Instance.JumpPanel(moduleId, false, itemId, needCount);
            }, playAudio: false, audioCallback: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            });
        }

    }
}