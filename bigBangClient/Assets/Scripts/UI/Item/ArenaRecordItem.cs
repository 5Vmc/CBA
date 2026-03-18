using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using BigBang.Animation;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;

namespace BigBang.UI
{
    public class ArenaRecordItem : MonoBehaviour
    {
        [SerializeField] private Image resultVictoryIcon;
        [SerializeField] private Image resultFailIcon;
        [SerializeField] private TMP_Text textTime;
        [SerializeField] private TMP_Text textMessage;

        public void SetData(Protocol.ArenaLogInfo log)
        {
            textTime.text = DataConvUtil.FormatDateTime(log.Time);
            ArenaBattleResultConfig cfg = Configs.ArenaBattleResult.GetConfig(log.Type);
            resultVictoryIcon.gameObject.SetActive(cfg.Result == 1);
            resultFailIcon.gameObject.SetActive(cfg.Result == 2);
           
            textMessage.text =  string.Format(cfg.Desc, log.Name, log.Rank, log.TargetRank);
        }
    }
}