using System;
using Babu;
using BigBang.UI;
using DG.Tweening;
using UnityEngine;
using UnityTimer;
using Utils;

namespace BigBang.Animation
{
    public class RegularTrainItemProgressAnim : MonoBehaviour
    {
        [SerializeField] private RegularTrainItemComponent com;

        private PlayerTrainItem trainItem;

        private int lastTimeSpan = 0; //上一次显示的倒计时
        private string fillIncomeText = ""; //收入

        private bool isPlaying = false;
        public void StartPlay()
        {
            trainItem = null;
            if (isPlaying) return;
            isPlaying = true;
            // if (com.ProjectNameText.text == "身体")
            // {
            //     Debug.LogErrorFormat("StartPlay , com.ProjectNameText.text = {0} , time = {1}", com.ProjectNameText.text, Time.time);

            // }
            Play();
        }

        private void Play()
        {
            //DebugLog
            // if (com.ProjectNameText.text == "身体")
            // {
            //     Debug.LogWarningFormat("Play , com.ProjectNameText.text = {0} , time = {1}", com.ProjectNameText.text, Time.time);

            // }


            //游戏关闭时，this可能为空导致后续代码报错，看起来是此脚本被释放后Play仍被调用，ReplayAsync修改为了使用Timer来规避这个错误
            if (trainItem == null)
            {
                trainItem = GetComponent<RegularTrainItem>().Item;
            }
            //获得进度条开始位置
            var timeUnit = trainItem.GetInComeTimeUnit();

            var pastTime = (Utils.DataConvUtil.ServerTimeEx - trainItem.LastIncomeTimeStamp) / 1000.0f;
            if (pastTime < 0)
            {
                Debug.LogWarningFormat("pastTime < 0 , ServerTimeEx = {0} , LastIncomeTimeStamp = {1}", Utils.DataConvUtil.ServerTimeEx, trainItem.LastIncomeTimeStamp);
                ReplayAsync();
                return;
            }
            var startValue = pastTime / timeUnit;
            //设置进度条开始位置
            com.Progress.fillAmount = (float)startValue.ToDouble();
            if (timeUnit > 0.2f)
            {

                fillIncomeText = (trainItem.GetInComePerSecond() * trainItem.GetInComeTimeUnit()).ToFormatString();

                double time = (timeUnit - pastTime).ToDouble();
                time = Mathf.Clamp((float)time, 0, (float)time);
                com.Pattern.SetActive(false);
                if (time > 1)
                {
                    // if (com.ProjectNameText.text == "身体")
                    // {
                    //     Debug.LogWarningFormat("time > 1 , com.ProjectNameText.text = {0} , time = {1}", com.ProjectNameText.text, Time.time);

                    // }
                    //大于1秒显示倒计时
                    DOTween.To(value =>
                    {
                        com.Progress.fillAmount = value;

                        int tempS = Mathf.CeilToInt(((1 - value) * (float)timeUnit.ToDouble()));
                        if (this.lastTimeSpan != tempS)
                        {
                            this.lastTimeSpan = tempS;
                            TimeSpan timeSpan = new TimeSpan(0, 0, this.lastTimeSpan);
                            com.IncomeText.text = timeSpan.ToString(@"mm\:ss");
                        }
                        if (fillIncomeText != com.FillIncomeText.text)
                            com.FillIncomeText.text = fillIncomeText;


                    }, (float)startValue.ToDouble(), 1, (float)time)
                    .SetEase(Ease.Linear).OnComplete(Play).AddTo(this.gameObject); ;
                }
                else
                {
                    // if (com.ProjectNameText.text == "身体")
                    // {
                    //     Debug.LogWarningFormat("trainItem.LastIncomeTimeStamp = {0}", trainItem.LastIncomeTimeStamp);
                    //     Debug.LogWarningFormat("time <= 1 , com.ProjectNameText.text = {0} , time = {1}", com.ProjectNameText.text, Time.time);
                    //     Debug.LogWarningFormat("timeUnit = {0} , pastTime = {1} , time = {2}", timeUnit, pastTime, time);
                    //     //固定的数据结算时间间隔和进度条使用的剩余时间不匹配
                    // }
                    com.Progress.DOFillAmount(1, Utility.KeepInRange((float)time, 0.05f, float.MaxValue)).SetEase(Ease.Linear).OnComplete(Play).AddTo(this.gameObject); ;
                    //进度条满的收益
                    if (timeUnit > 1f)
                    {
                        com.IncomeText.text = "00:00";
                    }
                    else
                    {
                        com.IncomeText.text = " ";
                    }
                    com.FillIncomeText.text = (trainItem.GetInComePerSecond() * trainItem.GetInComeTimeUnit()).ToFormatString();
                }
            }
            else
            {
                com.IncomeText.text = " ";
                com.FillIncomeText.text = trainItem.GetInComePerSecond().ToFormatString() + "/" + Lang.Get(LangID.SecondTxt);
                com.Progress.fillAmount = 1;
                com.Pattern.SetActive(true);
                ReplayAsync();
            }
        }

        //private async void ReplayAsync()
        //{
        //    await System.Threading.Tasks.Task.Delay(100);
        //    Play();
        //}
        private void ReplayAsync()
        {
            //DebugLog
            // if (com.ProjectNameText.text == "身体")
            // {
            //     Debug.LogWarningFormat("ReplayAsync , com.ProjectNameText.text = {0} , time = {1}", com.ProjectNameText.text, Time.time);

            // }
            Timer.Register(this.gameObject, 0.1f, Play);
        }
    }
}