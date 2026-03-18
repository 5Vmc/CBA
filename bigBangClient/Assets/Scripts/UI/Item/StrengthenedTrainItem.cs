using Babu;
using BigBang.Animation;
using GameConfig.Config;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class StrengthenedTrainItem : MonoBehaviour
    {
        [SerializeField] private StrengthenItemComponent com;
        [SerializeField] private StrengthenItemAnim anim;

        private StrengthenConfig cfg;
        private BuffTypeConfig buffCfg;
        private PlayerStrengthenItem item;

        private Color redColor = new Color(187 / 255f, 48 / 255f, 49 / 255f, 1);
        private Color normalColor = new Color(193 / 255f, 202 / 255f, 208 / 255f, 1);
        private bool isInit = false;
        private void OnEnable()
        {
            Babu.EventManager.Instance.Register(EventID.OnExpChanged, OnExpChanged);
            Babu.EventManager.Instance.Register(EventID.OnTrainAllCompleted, OnExpChanged);
            com.StrengthenBtn.onClick.AddListener(OnStrengthen);
        }

        private void OnDisable()
        {
            Babu.EventManager.Instance.Unregister(EventID.OnExpChanged, OnExpChanged);
            Babu.EventManager.Instance.Unregister(EventID.OnTrainAllCompleted, OnExpChanged);
            com.StrengthenBtn.onClick.RemoveListener(OnStrengthen);
        }

        private void OnExpChanged(object[] args)
        {
            if (StrengthenTrainPadAnim.isPlaying) return;
            if (!isInit) return;
            if (Player.TrainManager.Exp < item.GetCost())
            {
                com.CostText.color = redColor;
                SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.BlackBtnImg, s => com.StrengthenBtn.image.sprite = s);
                if (com.comEnabled) {
                    com.comEnabled = false;
                }
            }
            else
            {
                com.CostText.color = normalColor;
                SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.BlueBtnImg, s => com.StrengthenBtn.image.sprite = s);
                if (!com.comEnabled)
                {
                    com.comEnabled = true;
                }
            }
        }

        private void OnStrengthen()
        {
            if (Player.TrainManager.StrengthenController.CanStrengthen(item.ConfigId))
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_STREN);
                //播放点击动画
                anim.PlayClick(callback: () =>
                {
                    TouchManager.Instance.DisableTouch();
                    gameObject.SetActive(false);
                    Babu.EventManager.Instance.Dispatch(EventID.OnStrengthen);
                    Babu.DelayTaskService.Instance.Run(this.gameObject, 0.2f, () =>
                    {
                        TouchManager.Instance.EnableTouch();
                        transform.SetAsLastSibling();
                        SpriteManager.GetSprite(AtlasNames.Public, SpriteNames.Public.BlackBtnImg, s => com.StrengthenBtn.image.sprite = s);
                        Player.TrainManager.StrengthenController.DoStrengthen(item.ConfigId);
                    });
                });
            }
            else
            {
                com.BtnAnim.PlayNull();
                Tips.PopError(ErrorID.ExpNotEnough);
            }
        }

        public async void SetItem(int itemId)
        {
            item = Player.TrainManager.StrengthenController.GetStrengthenItem(itemId);
            cfg = item.GetConfig();
            buffCfg = item.Buff.GetConfig();
            //描述文本
            com.DescriptionText.text = buffCfg.DescPart1 + "<color=#FFFFFF>" + buffCfg.DescPart2 + "</color> <color=#0EDE35>" + buffCfg.DescOperator + cfg.BuffValueShow + "</color>";
            //强化花费
            com.CostText.text = item.GetCost().ToFormatStrengthString();
            //设置强化Icon
            com.Icon.sprite = await SpriteProxy.GetStrengthenIcon(buffCfg.TrainIconName);
            isInit = true;
        }
    }
}