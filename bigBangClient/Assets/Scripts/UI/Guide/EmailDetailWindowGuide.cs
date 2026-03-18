using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;

namespace BigBang.UI
{
    public class EmailDetailWindowGuide : MonoBehaviour
    {
        [SerializeField] private RectTransform guideLayer;
        [SerializeField] private Image clickMaskImage = null;
        [SerializeField] private BlackHole blackHole = null;

        public void CheckGuide()
        {
            if (!GuideManager.IsFinished(GuideID.guideUpLevelPlayer))
            {
                OnGuideClickUpStart();
                return;
            }
        }

        private void OnEnable()
        {
            ConfirmBtn.onClick.AddListener(OnGuideClickConfirmEnd);
        }
        private void OnDisable()
        {
            ConfirmBtn.onClick.RemoveListener(OnGuideClickConfirmEnd);
        }

        #region 引导点击领取

        [SerializeField] private Button ConfirmBtn = null;
        [SerializeField] private Image Paper = null;
        bool isGuideClickConfirmDoing = false;
        public void OnGuideClickUpStart()
        {
            isGuideClickConfirmDoing = true;
            guideLayer.gameObject.SetActive(true);
            blackHole.gameObject.SetActive(true);
            blackHole.Radius = 5000;
            Timer.Register(this.gameObject, 0.35f, () =>
            {
                ConfirmBtn.transform.SetParent(guideLayer.transform);
                if (ConfirmBtn.gameObject.activeInHierarchy == false)
                {
                    OnGuideClickConfirmEnd();
                    return;
                }
                blackHole.Locate(ConfirmBtn.transform);
                // 黑圈缩小
                DOTween.To(value => blackHole.Radius = value, 5000, 150, 0.8f).OnComplete(() =>
                {
                    if (ConfirmBtn.gameObject.activeInHierarchy == false)
                    {
                        OnGuideClickConfirmEnd();
                    }
                });
            });
        }
        public void OnGuideClickConfirmEnd()
        {
            if (isGuideClickConfirmDoing == false) return;
            guideLayer.gameObject.SetActive(false);
            blackHole.gameObject.SetActive(false);
            ConfirmBtn.transform.SetParent(Paper.transform);
            isGuideClickConfirmDoing = false;
            GuideManager.Finish(GuideID.UseGuideMail);
            UIController.Instance.CloseAllPanelAndWindow();
            UIController.Instance.ShowPanel<HomeUI>();
        }

        #endregion
    }
}