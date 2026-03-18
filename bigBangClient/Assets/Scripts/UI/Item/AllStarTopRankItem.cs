using System.Collections.Generic;
using DG.Tweening;
using GameConfig;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class AllStarTopRankItem : MonoBehaviour
    {
        [SerializeField] private ClubIconItem clubIcon = null;
        [SerializeField] private TMP_Text clubNameText = null;
        [SerializeField] private Image combatBgImage = null;
        [SerializeField] private RectTransform combatLayout = null;
        [SerializeField] private ImageFont combatImageFont = null;
        [SerializeField] private TMP_Text emptyText = null;

        [SerializeField] private Color whiteLight = new Color();
        [SerializeField] private Color greenLight = new Color();

        public AllStarRankInfo allStarRankInfo = null;
        public void SetData(AllStarRankInfo allStarRankInfo)
        {
            this.allStarRankInfo = allStarRankInfo;
            bool isEmpty = allStarRankInfo == null;

            clubIcon.gameObject.SetActive(!isEmpty);
            clubNameText.gameObject.SetActive(!isEmpty);
            combatBgImage.gameObject.SetActive(!isEmpty);
            combatLayout.gameObject.SetActive(!isEmpty);
            combatImageFont.gameObject.SetActive(!isEmpty);
            emptyText.gameObject.SetActive(isEmpty);

            if (!isEmpty)
            {
                clubIcon.SetIcon(allStarRankInfo.Icon);
                clubNameText.text = allStarRankInfo.Name;
                combatImageFont.text = allStarRankInfo.Record.ToString("N0");
                clubNameText.color = allStarRankInfo.Gbid == Player.GbId ? greenLight : whiteLight;
            }
        }
    }
}