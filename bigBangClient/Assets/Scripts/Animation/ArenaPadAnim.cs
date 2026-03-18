
using UnityEngine;
using DG.Tweening;
using Utils;
using BigBang.UI;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityTimer;
using TMPro;
using UnityEngine.UI;

namespace BigBang.Animation
{
    public class ArenaPadAnim : MonoBehaviour
    {
        [SerializeField] private RectTransform topRect;

        [SerializeField] private RectTransform topRewardRect;

        [SerializeField] private ArenaOpponentItem[] opponentRectList;

        [SerializeField] private ArenaRankItem[] rankRectList;

        [SerializeField] private RectTransform moreBtn;

        [SerializeField] private RectTransform freshOpponentTimesBtn;

        [SerializeField] private RectTransform recordBtn;

        [SerializeField] private RectTransform myInfoRect;

        [SerializeField] private RectTransform noRankBgRect;

        //[SerializeField] private TMP_Text scoreText;
        //[SerializeField] private TMP_Text tzcsText;
        //[SerializeField] private TMP_Text changeOpponentText;

        private void InitAnim()
        {
            moreBtn?.gameObject.SetAlpha(0);
            freshOpponentTimesBtn?.gameObject.SetAlpha(0);
            recordBtn?.gameObject.SetAlpha(0);
            myInfoRect?.gameObject.SetAlpha(0);
            topRewardRect?.gameObject.SetAlpha(0);
            if (noRankBgRect)
                noRankBgRect.localScale = new Vector3(1, 0, 1);
            HideOpponenets();
            HideRankItems();
        }

        private void Awake()
        {
            InitAnim();
        }
        void OnEnable()
        {
            InitAnim();
            this.PlayEnter();
        }
        public void PlayEnter()
        {
            AudioManager.Instance.PlaySound(AudioNames.ENT_COMMON);
            // 顶部栏下移
            topRect?.DoRelativeAnchorPosY(200, 0.3f).From();

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(0.3f);
            seq.AppendCallback(() =>
            {

                //top reward
                if (this.topRewardRect)
                    topRewardRect.gameObject.DOFade(1, 0.3f);


                this.ShowRankItem();

                noRankBgRect?.DOScaleY(1, 0.3f);//gameObject//DOFade(1, 0.3f);
                float duration = 0.2f;
                //更多按钮
                Timer.Register(this.gameObject, duration, () =>
                {
                    this.moreBtn?.gameObject.DOFade(1, 0.1f);
                    this.freshOpponentTimesBtn?.gameObject.DOFade(1, 0.1f).SetDelay(0.1f);
                    this.recordBtn?.gameObject.DOFade(1, 0.1f).SetDelay(0.1f);
                });

                duration += 0.17f;

                Timer.Register(this.gameObject, duration, () =>
                {
                    duration += this.ShowOpponents();
                });

                Timer.Register(this.gameObject, duration + 0.2f, () =>
                {
                    this.myInfoRect?.gameObject.DOFade(1, 0.1f);
                });
            });

        }


        private void _setRotation(GameObject go, float angle)
        {
            go.transform.eulerAngles = new Vector3(0, angle, 0);
        }

        public void HideRankItems()
        {
            foreach (ArenaRankItem item in rankRectList)
            {
                item?.InitAnimState();
            }
        }

        public float ShowRankItem()
        {
            int index = 0;
            float? duration = 0;
            foreach (ArenaRankItem item in rankRectList)
            {
                duration += item?.PlayAnim(index);
                index++;
            }

            return duration ?? 0;
        }

        public void HideOpponenets()
        {
            foreach (ArenaOpponentItem item in opponentRectList)
            {
                item?.InitAnimState();
            }
        }
        public float ShowOpponents()
        {
            int index = 0;
            float? duration = 0;
            foreach (ArenaOpponentItem item in opponentRectList)
            {
                duration += item?.PlayAnim(index);
                index++;
            }


            return duration ?? 0;
        }

        public void DoTextAnim(TMP_Text text)
        {
            text.DOFlash(4, 0.2f, 0.1f);
        }

    }
}