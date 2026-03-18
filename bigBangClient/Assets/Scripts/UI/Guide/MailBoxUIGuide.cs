using System;
using Babu;
using BigBang.Animation;
using BigBang.Battle;
using DG.Tweening;
using GameConfig;
using UnityEngine;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class MailBoxUIGuide : MonoBehaviour
    {
        [SerializeField] private Image guideLayer = null;
        [SerializeField] private MailAdapter mailAdapter;

        private void OnEnable()
        {
            EventManager.Instance.Register(EventID.OnClickMailBoxUIMail, OnClickMailBoxUIMail);
        }
        private void OnDisable()
        {
            EventManager.Instance.Unregister(EventID.OnClickMailBoxUIMail, OnClickMailBoxUIMail);
        }

        public void CheckGuide()
        {
            if (GuideManager.IsGuideDoing(GuideID.UseGuideMail))
            {
                OnGuideClickMailStart();
                return;
            }
        }

        [SerializeField] private RectTransform fingerPanel = null;

        #region 引导点击经典赛国家界面点列表中最后一个俱乐部

        private bool isGuideClickMailDoing = false;
        public bool IsGuideClickMailDoing
        {
            get
            {
                return isGuideClickMailDoing;
            }
        }
        EmailItem endEmailItem = null;
        public void OnGuideClickMailStart()
        {
            isGuideClickMailDoing = true;
            guideLayer.gameObject.SetActive(true);
            guideLayer.SetAlpha(0);
            Timer.Register(this.gameObject, 0.35f, () =>
            {
                endEmailItem = mailAdapter.GetItemViewsHolder(mailAdapter.VisibleItemsCount - 1).emailItem;
                mailAdapter.enabled = false;
                endEmailItem.transform.SetParent(guideLayer.transform);
                guideLayer.TweenAlpha(100 / 255f, 0.8f).OnComplete(() =>
                {
                    fingerPanel.gameObject.SetActive(true);
                    Transform endEmailItemTrans = endEmailItem.transform;
                    Vector3 offset = new Vector3(20, 0, 0);
                    fingerPanel.localPosition = Utility.ConvertLocalPosition(endEmailItemTrans.parent, endEmailItemTrans.localPosition, fingerPanel.parent) + offset;
                    fingerPanel.gameObject.SetAlpha(0);
                    fingerPanel.gameObject.DOFade(1, 0.8f);
                    fingerPanel.SetAsLastSibling();
                });
            });
        }
        private void OnClickMailBoxUIMail(object[] _)
        {
            OnGuideClickMailEnd();
        }
        public void OnGuideClickMailEnd()
        {
            if (isGuideClickMailDoing == false) return;
            if (endEmailItem != null)
            {
                endEmailItem.transform.SetParent(mailAdapter.Content);
                endEmailItem = null;
                mailAdapter.enabled = true;
            }
            fingerPanel.gameObject.SetActive(false);
            guideLayer.gameObject.SetActive(false);
            isGuideClickMailDoing = false;
        }

        #endregion
    }
}