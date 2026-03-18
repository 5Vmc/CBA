using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using BigBang.Animation;
using DG.Tweening;

namespace BigBang.UI
{
    public class ArenaRankItem : MonoBehaviour
    {
        [SerializeField] private Image rankIcon;

        [SerializeField] private Image dargBg;

        [SerializeField] private TMP_Text textRank;

        [SerializeField] private Image titleIcon;

        [SerializeField] private TMP_Text textTitleCount;

        [SerializeField] private ClubIconItem teamLogo;

        [SerializeField] private TMP_Text textTeamName;

        [SerializeField] private TMP_Text textVicCount;

        private RectTransform rectTrans;

        [SerializeField] private Button infoIcon;

        private string teamId;
        private void OnEnable()
        {
           infoIcon.onClick.AddListener(OnClickInfo);
        }

        private void OnDisable()
        {
           infoIcon.onClick.RemoveListener(OnClickInfo);
        }

        private void OnClickInfo()
        {
            UIController.Instance.OpenWindow<ArenaFirstInfoUI>(new ArenaFirstInfoUIProperties(null, this.teamId));
        }

        public void Active(bool b)
        {
            gameObject.SetActive(b);
        }
        public async void SetData(Protocol.ArenaRankInfo data)
        {
            this.teamId = data.Gbid;
            textTitleCount.gameObject.SetActive(false);
            gameObject.SetActive(true);
            if(data.Rank <= 3){
                rankIcon.gameObject.SetActive(true);
                textRank.gameObject.SetActive(false);
                rankIcon.sprite = await SpriteProxy.GetArenaRankIcon(data.Rank);

                titleIcon.gameObject.SetActive(true);
                titleIcon.sprite = await SpriteProxy.GetArenaTitleIcon(data.Rank);
            }
            else{
                rankIcon.gameObject.SetActive(false);
                textRank.gameObject.SetActive(true);
                textRank.text = data.Rank.ToString();

                titleIcon.gameObject.SetActive(false);
                textTitleCount.gameObject.SetActive(false);
            }

            //teamLogo.sprite = await SpriteProxy.Log
            //teamLogo.sprite = await SpriteProxy.GetClubIcon(data.Icon.ToString());
            teamLogo.SetIcon(data.Icon);

            textTeamName.text = data.Name;
            if(data.Record >= 0)
                textVicCount.text = data.Record.ToString() + "连胜";
            else{
                textVicCount.text = Math.Abs(data.Record).ToString() + "连败";
            }
            
            if(data.Rank % 2 == 1){
                dargBg.gameObject.SetActive(true);
            }
            else{
                dargBg.gameObject.SetActive(false);
            }
        }

        public void InitAnimState()
        {
            if(rectTrans==null) rectTrans = gameObject.GetComponent<RectTransform>();
            
            gameObject.SetAlpha(0);
            //rectTrans.SetAnchoredPositionY(0);
        }

        public float PlayAnim(int index)
        {
            //整体
            float druation = 0.07f;
            gameObject.DOFade(1, 0.3f).SetDelay(index * 0.1f);
            return druation;
        }

        public interface ISelectListener
        {
            void OnSelect(ArenaRankItem item);

           
        }
    }
}