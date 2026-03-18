using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using BigBang.Animation;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityTimer;

namespace BigBang.UI
{
    class ArenaOpponentItem : MonoBehaviour
    {
        RectTransform rectTrans;
        
        [SerializeField] private ClubIconItem teamLogo;

        [SerializeField] private TMP_Text textAbility;

        [SerializeField] private Image tierArrowUp;
        [SerializeField] private Image tierArrowDown;
        [SerializeField] private TMP_Text  textTeanRank;
        [SerializeField] private TMP_Text textTeamName;

        [SerializeField] private Image tierIcon;

        [SerializeField] private Button challengeButton;

        [SerializeField] private RectTransform contentPanel;

        [SerializeField] private Button firstButton;

        private Protocol.ArenaTeamData _data;


        public async void SetData(Protocol.ArenaTeamData data, int myStage)
        {
            this._data = data;
            //teamLogo.sprite = await SpriteProxy.GetClubIcon(data.Icon.ToString());
            teamLogo.SetIcon(data.Icon);
            textAbility.text = data.CombatEffectiveness.ToString();
            if(data.Stage > myStage){
                tierArrowUp.gameObject.SetActive(true);
                tierArrowDown.gameObject.SetActive(false);
            }
            else if(data.Stage == myStage){
                tierArrowUp.gameObject.SetActive(false);
                tierArrowDown.gameObject.SetActive(false);
            }
            else{
                tierArrowUp.gameObject.SetActive(false);
                tierArrowDown.gameObject.SetActive(true);
            }

            if(data.Rank == 0){
                textTeanRank.text = "+" + data.AddScore + "分";
            }
            else{
                textTeanRank.text = string.Format("第{0}名", data.Rank.ToString());
            }
           
            textTeamName.text = data.Name;

            ArenaStageConfigTable conf = Configs.ArenaStage;
            string tIcon = conf.GetConfig(data.Stage).Icon;
            this.tierIcon.sprite = await SpriteProxy.GetArenaTierIcon(tIcon);
        }

        public Button ChallengeButton{
            get{
                return this.challengeButton;
            }
            private set{}
        }

        public Button FirstButton{
            get{
                return this.firstButton;
            }
            private set{}
        }

      

        public void InitAnimState()
        {
            if(rectTrans==null) rectTrans = gameObject.GetComponent<RectTransform>();

            rectTrans.DOComplete(false);

            gameObject.SetAlpha(0);
            
            contentPanel.gameObject.SetAlpha(0);
            challengeButton.gameObject.SetAlpha(0);
            teamLogo.transform.eulerAngles = new Vector3(0, 90, 0);
        }

        public float PlayAnim(int index)
        {
            float duration = 0.07f;
            //整体
            gameObject.DOFade(1, 0.3f).SetDelay(index * duration);
            // 整体下移
            rectTrans.DoRelativeAnchorPosY(50, 0.3f).From().SetDelay(index * duration);
            // 整体缩放
            rectTrans.DOScale(1.1f, 0.3f).From().SetDelay(index * duration);

            //除了底图 DOFade
            contentPanel.gameObject.DOFade(1, 0.1f).SetDelay(index * 0.1f);

            //俱乐部logo旋转
            teamLogo.gameObject.transform.DORotate(new Vector3(0, 0, 0), 0.5f).SetDelay(index * 0.15f);

            //挑战按钮
            challengeButton.gameObject.DOFade(1, 0.3f).SetDelay(index * 0.1f);

            return duration;
        }

        

        public Protocol.ArenaTeamData Data{
            get{
                return this._data;
            }
            private set{}
        }
    }
}