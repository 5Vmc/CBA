using BigBang.Animation;
using GameConfig;
using System;
using TMPro;
using UnityEngine;

namespace BigBang.UI
{
    public class GuideDialogueItem : MonoBehaviour
    {
        [SerializeField] private GuidePlayerIconItem portrait;
        [SerializeField] private TMP_Text nameTxt;
        [SerializeField] private TMP_Text contentTxt;

        [SerializeField] public GuideDialogueItemAnim Anim;

        public event Action OnDialogueFinished;

        public async void SetData(int cardID, string content)
        {
            var cfg = Configs.CardModel.GetConfig(cardID);
            if (cfg == null) return;
            portrait.SetIcon(await SpriteProxy.GetPlayerPortrait(cfg.Portrait));
            nameTxt.text = cfg.Name;
            contentTxt.text = content;
            Anim.PlayAnim(OnDialogueFinished);
        }

        public void SetData(Sprite icon, string name, string content)
        {
            portrait.SetIcon(icon);
            nameTxt.text = name;
            contentTxt.text = content;
            Anim.PlayAnim(OnDialogueFinished);
        }
    }
}