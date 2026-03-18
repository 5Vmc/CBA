using BigBang.Animation;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityTimer;
using Utils;

namespace BigBang.UI
{
    public class RecruitBtn : MonoBehaviour
    {
        [SerializeField] private Button btn;
        [SerializeField] private Image bg;

        [SerializeField] private TMP_Text descText;
        [SerializeField] private TMP_Text resourceCountText;
        [SerializeField] private Image resourceImg;

        private RecruitCountType _countType = RecruitCountType.Once;

        private Action<RecruitCountType, RecruitCostType> clickAction;

        private Timer timer;

        private void Start()
        {
        }
        private void OnEnable()
        {
            btn.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            btn.onClick.RemoveListener(OnClick);
            IsClick = false;
            timer?.Cancel();
        }


        public void SetClickAction(Action<RecruitCountType, RecruitCostType> call)
        {
            clickAction = call;
        }
        private bool IsClick = false;
        private void OnClick()
        {
            if (IsClick) return;
            IsClick = true;
            timer = Timer.Register(this.gameObject, 1f, () => { IsClick = false; });

            btn.GetComponent<ButtonAnim>().Play(() =>
            {
                clickAction?.Invoke(_countType, _costType);
            }, playAudio: false, audioCallback: () =>
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_SCOUT);

            });
        }

        private string GetTextColor(bool isActivity)
        {
            if (_countType == RecruitCountType.Once)
            {
                if (!isActivity)
                    return "#3F5F0C";
                else return "#7F2B22";
            }
            else if (_countType == RecruitCountType.Ten)
            {
                if (!isActivity)
                    return "#483161";
                else return "#353670";
            }

            return "#3F5F0C";
        }
        private Task<Sprite> GetBg(bool isActivity)
        {
            if (_countType == RecruitCountType.Once)
            {
                if (!isActivity)
                    return SpriteManager.GetSprite(AtlasNames.Scout, SpriteNames.Scout.RecruitBtn + "_1_1");
                else return SpriteManager.GetSprite(AtlasNames.Scout, SpriteNames.Scout.RecruitBtn + "_2_1");
            }
            else if (_countType == RecruitCountType.Ten)
            {
                if (!isActivity)
                    return SpriteManager.GetSprite(AtlasNames.Scout, SpriteNames.Scout.RecruitBtn + "_1_2");
                else return SpriteManager.GetSprite(AtlasNames.Scout, SpriteNames.Scout.RecruitBtn + "_2_2");
            }

            return null;
        }

        private RecruitCostType _costType = RecruitCostType.Goods;
        private void ShowRecruitGoods(bool isActivity)
        {
            string resourceCountTextString = "";
            int recruitCount = RecruitLogic.GetRecruitCount(_countType);
            if (!isActivity)
            {
#if UNITY_ANDROID
                SpriteManager.GetSprite(AtlasNames.Scout, SpriteNames.Scout.RecruitGoods + "_1", (s) => { resourceImg.sprite = s; });
                resourceCountTextString = $"×{recruitCount}";
                _costType = RecruitCostType.Goods;
#else
                if (Player.PackageManager.IsGoodsEnough(GoodsId.RecruitPoint, recruitCount))
                {
                    SpriteManager.GetSprite(AtlasNames.Scout, SpriteNames.Scout.RecruitGoods + "_1", (s) => { resourceImg.sprite = s; });
                    resourceCountTextString = $"×{recruitCount}";
                    _costType = RecruitCostType.Goods;
                }
                else
                {
                    int recruitDiamond = RecruitLogic.GetCostDiamond(_countType);
                    SpriteManager.GetSprite(AtlasNames.Scout, SpriteNames.Scout.RecruitDiamond, (s) => { resourceImg.sprite = s; });
                    resourceCountTextString = $"×{recruitDiamond}";
                    _costType = RecruitCostType.Diamond;
                }
#endif
            }
            else
            {
#if UNITY_ANDROID
                SpriteManager.GetSprite(AtlasNames.Scout, SpriteNames.Scout.RecruitGoods + "_2", (s) => { resourceImg.sprite = s; });
                resourceCountTextString = $"×{recruitCount}";
                _costType = RecruitCostType.Goods;
#else
                if (Player.PackageManager.IsGoodsEnough(GoodsId.ActRecruitPoint, recruitCount))
                {
                    SpriteManager.GetSprite(AtlasNames.Scout, SpriteNames.Scout.RecruitGoods + "_2", (s) => { resourceImg.sprite = s; });
                    resourceCountTextString = $"×{recruitCount}";
                    _costType = RecruitCostType.Goods;
                }
                else
                {
                    int recruitDiamond = RecruitLogic.GetCostDiamond(_countType);
                    SpriteManager.GetSprite(AtlasNames.Scout, SpriteNames.Scout.RecruitDiamond, (s) => { resourceImg.sprite = s; });
                    resourceCountTextString = $"×{recruitDiamond}";
                    _costType = RecruitCostType.Diamond;
                }
#endif
            }

            resourceCountText.text = ColorString.GetColorString(GetTextColor(isActivity), resourceCountTextString);
        }

        public async void SetButtonStyle(RecruitCountType type, bool isActivity)
        {
            _countType = type;
            if (_countType == RecruitCountType.Once)
            {
                descText.text = ColorString.GetColorString(GetTextColor(isActivity), Lang.Get(LangID.RecruitOnce));//Lang.Get(LangID.RecruitOnce);
            }
            else if (_countType == RecruitCountType.Ten)
            {
                descText.text = ColorString.GetColorString(GetTextColor(isActivity), Lang.Get(LangID.RecruitTen));//Lang.Get(LangID.RecruitTen);
            }

            bg.sprite = await GetBg(isActivity);

            ShowRecruitGoods(isActivity);
        }
    }
}