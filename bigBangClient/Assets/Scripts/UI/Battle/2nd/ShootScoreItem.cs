using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Utils;

namespace BigBang.Battle
{

    [DisallowMultipleComponent]
    public class ShootScoreItem : MonoBehaviour
    {
        [SerializeField] public RectTransform rect;
        [SerializeField] private RectTransform topBgTrans;
        [SerializeField] private RectTransform bottomBgTrans;
        [SerializeField] private RectTransform topMoveTrans;
        [SerializeField] private RectTransform bottomMoveTrans;
        [SerializeField] private TMP_Text topBgText;
        [SerializeField] private TMP_Text bottomBgText;
        [SerializeField] private TMP_Text topMoveText;
        [SerializeField] private TMP_Text bottomMoveText;

        public int nowNum = 0;
        public void SetNum(int num)
        {
            ClearAni();
            num = Mathf.Clamp(num, 0, 9);
            nowNum = num;
            topBgText.text = num.ToString();
            bottomBgText.text = num.ToString();
            topMoveText.text = num.ToString();
            bottomMoveText.text = num.ToString();
        }

        private Queue<int> numQue = new();
        public void ChangeToNum(int num)
        {
            num = Mathf.Clamp(num, 0, 9);
            numQue.Enqueue(num);
            ChangeToNum();
        }

        private Sequence changeNumSeq;
        private void ChangeToNum()
        {
            if (changeNumSeq != null) return;
            if (numQue.Count == 0)
            {
                ClearAni();
                return;
            }
            int num = numQue.Dequeue();
            if(nowNum == num)
            {
                ChangeToNum();
                return;
            }
            nowNum = num;
            Debug.Log("nowNum = " + nowNum);
            changeNumSeq = DOTween.Sequence();
            bottomBgText.text = num.ToString();
            topMoveText.text = num.ToString();
            topMoveTrans.SetLocalScaleY(0);
            changeNumSeq.Append(bottomMoveTrans.DOScaleY(0, 0.3f).SetEase(Ease.InCubic));
            changeNumSeq.Append(topMoveTrans.DOScaleY(1, 0.3f).SetEase(Ease.OutCubic));
            changeNumSeq.AppendCallback(() =>
            {
                bottomMoveTrans.SetLocalScaleY(1);
                topBgText.text = num.ToString();
                bottomMoveText.text = num.ToString();
            });
            changeNumSeq.AppendInterval(0.3f);
            changeNumSeq.OnComplete(() =>
            {
                changeNumSeq = null;
                ChangeToNum();
            });
        }

        private void ClearAni()
        {
            changeNumSeq?.Kill();
            changeNumSeq = null;
            topBgTrans.SetLocalScaleY(1);
            bottomBgTrans.SetLocalScaleY(1);
            topMoveTrans.SetLocalScaleY(1);
            bottomMoveTrans.SetLocalScaleY(1);
        }

        public void ResetToZero()
        {
            numQue.Clear();
            SetNum(0);
        }


    }



}