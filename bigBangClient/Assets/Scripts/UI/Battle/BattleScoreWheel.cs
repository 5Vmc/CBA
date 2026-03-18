using System.Collections.Generic;
using BigBang.Animation;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BattleScoreWheel : MonoBehaviour
{
    private void OnDisable()
    {
        Clear();
    }

    public void AddScore(int addScore, bool useAni = true, float delay = -1f)
    {
        SetScore(lastInputScore + addScore, useAni, delay);
    }

    public int lastInputScore = 0;
    public void SetScore(int newScore, bool useAni = true,float delay=-1f)
    {
        if (useAni == true)
        {
            if (newScore != lastInputScore)
            {
                AddScoreQueue(newScore, delay);
                lastInputScore = newScore;
            }
        }
        else
        {
            SetToScoreFast(newScore);
            lastInputScore = newScore;
        }
    }
    public void ChangeSpeedToVeryFast()
    {
        ChangeSpeed(0.5f);
    }
    public void ChangeSpeedToFast()
    {
        ChangeSpeed(0.9f);
    }
    public void ChangeSpeedToSlow()
    {
        ChangeSpeed(2.5f);
    }

    private void ChangeSpeed(float ballDropOffsetTime = 0.6f)
    {
        scoreItemMoveTime = ballDropOffsetTime * 0.2f;
        scoreItemWaitTime = ballDropOffsetTime * 0.1f;
    }

    private void SetToScoreFast(int newScore)
    {
        if (isInitScoreQuqueOnce == false) InitScoreQuque();
        scoreQueue.Clear();
        scoreMoveSequence?.Kill();
        scoreMoveSequence = null;
        isScoreQuqueDoing = false;
        for (int i = 0; i < scoreItemTransList.Count; i++)
        {
            scoreItemTransList[i].localPosition = scoreItemPointList[i];
            scoreItemTransList[i].gameObject.SetAlphaInChildren(1f);
            scoreTextList[i].text = newScore.ToString();
        }
        queueIndex = 1;
    }

    private Queue<int> scoreQueue = new();
    private List<Sequence> addSeqList = new();
    private void AddScoreQueue(int score, float delay = -1f)//添加评论到评论队列中（并开始播放）
    {
        if (delay <= 0)
        {
            scoreQueue.Enqueue(score);
            CheckStartScoreQuque();
        }
        else
        {
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(delay);
            sequence.AppendCallback(() =>
            {
                scoreQueue.Enqueue(score);
                CheckStartScoreQuque();
            });
            addSeqList.Add(sequence);
        }

    }

    [SerializeField] private List<RectTransform> scoreItemTransList = new();//3个文本框的节点
    [SerializeField] private List<TMP_Text> scoreTextList = new();//3个分数文本
    private List<Vector3> scoreItemPointList = new();//5个文本框的位置
    private bool isInitScoreQuqueOnce = false;//是否初始化过一次
    public void InitScoreQuque(float ballDropOffsetTime = 0.6f)//初始化评论队列
    {
        ChangeSpeed(ballDropOffsetTime);

        Clear();

        if (isInitScoreQuqueOnce == false)
        {
            scoreItemPointList.Clear();
            foreach (var item in scoreItemTransList)
            {
                scoreItemPointList.Add(item.localPosition);
            }
            isInitScoreQuqueOnce = true;
        }

        for (int i = 0; i < scoreItemTransList.Count; i++)
        {
            scoreItemTransList[i].localPosition = scoreItemPointList[i];
            scoreItemTransList[i].gameObject.SetAlphaInChildren(1f);
        }

    }
    public void Clear()
    {
        scoreMoveSequence?.Kill();
        scoreMoveSequence = null;
        isScoreQuqueDoing = false;

        foreach (var item in scoreTextList)
        {
            item.text = "0";
        }
        queueIndex = 0;

        scoreQueue.Clear();

        foreach (var item in addSeqList)
        {
            item?.Kill();
        }
        addSeqList.Clear();
    }
    private bool isScoreQuqueDoing = false;//是否正在播放评论队列
    private void CheckStartScoreQuque()//检查是否应该启动播放评论
    {
        if (isScoreQuqueDoing == true) return;
        if (scoreQueue.Count <= 0) return;
        StartScoreQuque();
    }
    private float scoreItemMoveTime = 0.5f;//文本框移动时间
    private float scoreItemWaitTime = 0.5f;//两次文本框移动之间的间隔
    private int queueIndex = 0;//用于控制循环
    Sequence scoreMoveSequence = null;//文本框移动动画
    private void StartScoreQuque()//播放评论队列中的一个
    {
        int showCount = scoreItemTransList.Count - 1;
        queueIndex %= showCount;
        queueIndex++;

        isScoreQuqueDoing = true;
        scoreMoveSequence?.Kill();
        scoreMoveSequence = DOTween.Sequence();
        //scoreMoveSequence.timeScale = frameSpeed;
        int score = scoreQueue.Dequeue();
        for (int i = 0; i < showCount; i++)
        {
            RectTransform scoreItemTrans = scoreItemTransList[i + 1];
            int posIndex = (i + showCount - queueIndex) % showCount;

            if (posIndex == showCount - 1)
            {
                scoreItemTrans.localPosition = scoreItemPointList[showCount];
                scoreTextList[i + 1].text = score.ToString();
            }

            Vector3 scoreItemPoint = scoreItemPointList[posIndex];
            if (i == 0)
            {
                scoreMoveSequence.Append(scoreItemTrans.DOLocalMoveY(scoreItemPoint.y, scoreItemMoveTime));
            }
            else
            {
                scoreMoveSequence.Join(scoreItemTrans.DOLocalMoveY(scoreItemPoint.y, scoreItemMoveTime));
            }
        }
        scoreMoveSequence.AppendInterval(scoreItemWaitTime);
        scoreMoveSequence.AppendCallback(() =>
        {
            if (scoreQueue.Count > 0)
            {
                StartScoreQuque();
            }
            else
            {
                isScoreQuqueDoing = false;
                scoreMoveSequence?.Kill();
                scoreMoveSequence = null;
            }
        });
    }

}
