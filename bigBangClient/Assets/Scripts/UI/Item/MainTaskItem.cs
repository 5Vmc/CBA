using BigBang.Animation;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class MainTaskItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text content;
        [SerializeField] private Image lockImg;
        [SerializeField] private Image inProgressImg;
        [SerializeField] private Image completedImg;
        [SerializeField] private Button btn;

        [HideInInspector] public MainTaskState State;
        [HideInInspector] public MainTaskType Type;
        [HideInInspector] public int Index;
        [HideInInspector] public int Day;

        private static Color normal = new Color(69 / 255f, 73 / 255f, 100 / 255f, 1);
        private static Color red = new Color(208 / 255f, 19 / 255f, 54 / 255f, 1);

        private void OnEnable()
        {
            btn.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            btn.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            Babu.EventManager.Instance.Dispatch(EventID.OnMainTaskItemSelected, Index);
        }

        public void SetData(MainTaskState state, int day)
        {
            Day = day;
            State = state;
            inProgressImg.SetAlpha(1);
            switch (state)
            {
                case MainTaskState.Lock:
                    lockImg.gameObject.SetActive(true);
                    inProgressImg.gameObject.SetActive(false);
                    completedImg.gameObject.SetActive(false);
                    content.gameObject.SetActive(false);
                    break;
                case MainTaskState.InProgress:
                    lockImg.gameObject.SetActive(false);
                    inProgressImg.gameObject.SetActive(true);
                    completedImg.gameObject.SetActive(false);
                    content.gameObject.SetActive(true);
                    content.color = normal;
                    content.text = Lang.Get(LangID.InProgressTxt);
                    break;
                case MainTaskState.Completed:
                    lockImg.gameObject.SetActive(false);
                    inProgressImg.gameObject.SetActive(true);
                    completedImg.gameObject.SetActive(true);
                    content.gameObject.SetActive(true);
                    content.color = red;
                    content.text = Lang.Get(LangID.DayTxt).Replace("{value}", day.ToString());
                    break;
            }
        }

        public void PlayAnim()
        {
            // 初始化缩放
            completedImg.rectTransform.localScale = Vector3.one * 4;
            // 初始化透明度
            completedImg.SetAlpha(0);
            // 淡入
            completedImg.DOFade(1, 0.5f);
            // 砸入
            completedImg.rectTransform.DOScale(1, 0.5f).SetEase(Ease.InExpo);
            inProgressImg.DOFade(0.5f, 0.5f);
            // 完成时间打字机淡入
            content.DOText(0.1f).SetDelay(0.5f);
        }
    }
}
