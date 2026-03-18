using Babu;
using BigBang.Animation;
using Coffee.UIEffects;
using DG.Tweening;
using System.Linq;
using UnityEngine;
using Utils;

namespace BigBang.UI
{
    public class CreatePlayerUICreateIconPad : MonoBehaviour
    {
        [SerializeField] private BabuButton nextBtn;
        [SerializeField] private BabuButton previousBtn;
        [SerializeField] private BabuToggleGroup toggleGroup;
        [SerializeField] private BabuToggle styleToggle;
        [SerializeField] private BabuToggle colorToggle;
        [SerializeField] private GameObject styleSubPad;
        [SerializeField] private GameObject colorSubPad;
        [SerializeField] private CreatePlayerUICreateNamePad previousPad;
        [SerializeField] private CreatePlayerUISelectClothesPad nextPad;
        [SerializeField] private ClubIconItem clubIconItem;
        [SerializeField] private BabuButton randomBtn;
        [SerializeField] private BabuToggleGroup shapeGroup;
        [SerializeField] private BabuToggleGroup backgroundGroup;
        [SerializeField] private BabuToggleGroup patternGroup;
        [SerializeField] private ColorSelectItem shapeColor;
        [SerializeField] private ColorSelectItem backgroundColor;
        [SerializeField] private ColorSelectItem patternColor;
        [SerializeField] private UIEffect randomEffect;

        [SerializeField] public CreateIconPadAnim Anim;

        public static bool IsInit = false;

        private StateValue clubIcon = null;

        public StateValue ClubIcon { get => clubIcon; }

        private int[] colorIndex = Enumerable.Range(0, 11).ToArray();

        private void Awake()
        {
            randomBtn.Anim = null;
        }

        private bool playColorAnim = true;

        private void OnEnable()
        {
            nextBtn.OnClick += OnNext;
            previousBtn.OnClick += OnPrevious;
            randomBtn.OnClick += OnRandom;
            shapeGroup.OnValueChanged += OnShapeChanged;
            backgroundGroup.OnValueChanged += OnBackgroundChanged;
            patternGroup.OnValueChanged += OnPatternChanged;
            colorToggle.OnSelect += OnColorSelect;
            styleToggle.OnSelect += OnStyleSelect;
            shapeColor.OnValueChanged += OnShapeColorChanged;
            backgroundColor.OnValueChanged += OnBackgroundColorChanged;
            patternColor.OnValueChanged += OnPatternColorChanged;
            playColorAnim = true;
        }

        private void OnDisable()
        {
            nextBtn.OnClick -= OnNext;
            previousBtn.OnClick -= OnPrevious;
            randomBtn.OnClick -= OnRandom;
            shapeGroup.OnValueChanged -= OnShapeChanged;
            backgroundGroup.OnValueChanged -= OnBackgroundChanged;
            patternGroup.OnValueChanged -= OnPatternChanged;
            colorToggle.OnSelect -= OnColorSelect;
            styleToggle.OnSelect -= OnStyleSelect;
            shapeColor.OnValueChanged -= OnShapeColorChanged;
            backgroundColor.OnValueChanged -= OnBackgroundColorChanged;
            patternColor.OnValueChanged -= OnPatternColorChanged;
            CreatePlayerUICreateIconPad.IsInit = false;
        }

        public void Initialize()
        {
            if (clubIcon == null)
            {
                clubIcon = new StateValue();
                clubIcon[ClubIconItem.FlagIndex] = ClubIconItem.FromCustom;
                RandomShape();
                RandomColor();
            }
            styleSubPad.SetActive(true);
            colorSubPad.SetActive(false);
            ShowStyleSubPad();
        }

        private void OnShapeChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            }
            clubIcon[ClubIconItem.FrameTextureIndex] = shapeGroup.EnableIndex;
            clubIconItem.SetIcon(clubIcon);
        }

        private void OnBackgroundChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            }
            clubIcon[ClubIconItem.PatternTextureIndex] = backgroundGroup.EnableIndex;
            clubIconItem.SetIcon(clubIcon);
        }

        private void OnPatternChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            }
            clubIcon[ClubIconItem.FlagTextureIndex] = patternGroup.EnableIndex;
            clubIconItem.SetIcon(clubIcon);
        }

        private void OnRandom(BabuButton sender)
        {
            DOTween.To(value => randomEffect.colorFactor = value, 0, 1, 0.1f).OnComplete(() =>
            {
                DOTween.To(value => randomEffect.colorFactor = value, 1, 0, 0.1f);
            });
            RandomShape();
            RandomColor();
        }

        public void RandomShape()
        {
            System.Random random = new System.Random();
            // 随机样式
            shapeGroup.Switch(random.Next(0, shapeGroup.Count));
            backgroundGroup.Switch(random.Next(0, backgroundGroup.Count));
            patternGroup.Switch(random.Next(0, patternGroup.Count));
        }

        public void RandomColor()
        {
            // 背景色1只随机黑白
            var color1 = Random.Range(9, 11);
            // 背景色2与图案颜色，不相邻
            var randomColor = Enumerable.Range(0, 9);
            // 随机颜色(11种颜色）
            shapeColor.SetSelection(color1);
            var color2 = randomColor.Random();
            var color3 = randomColor.Where(item => item != color2 && item != color2 + 1 && item != color2 - 1).Random();
            backgroundColor.SetSelection(color2);
            patternColor.SetSelection(color3);
        }

        private void OnShapeColorChanged(int oldColor, int newColor)
        {
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            }
            clubIcon[ClubIconItem.Color1Index] = colorIndex[newColor];
            clubIconItem.SetIcon(clubIcon);
        }

        private void OnBackgroundColorChanged(int oldColor, int newColor)
        {
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            }
            clubIcon[ClubIconItem.Color2Index] = colorIndex[newColor];
            clubIconItem.SetIcon(clubIcon);
        }

        private void OnPatternColorChanged(int oldColor, int newColor)
        {
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            }
            clubIcon[ClubIconItem.Color3Index] = colorIndex[newColor];
            clubIconItem.SetIcon(clubIcon);
        }

        private bool lockAnim = false;
        // 上一步
        private void OnPrevious(BabuButton sender)
        {
            if (lockAnim) return;
            lockAnim = true;
            TouchManager.Instance.DisableTouch();
            Anim.PlayExit(() =>
            {
                lockAnim = false;
                TouchManager.Instance.EnableTouch();
                gameObject.SetActive(false);
                previousPad.gameObject.SetActive(true);
                previousPad.Anim.PlayEnter();

                styleSubPad.SetActive(true);
                colorSubPad.SetActive(false);
            });
        }

        // 下一步
        private void OnNext(BabuButton sender)
        {
            if (lockAnim) return;
            lockAnim = true;
            TouchManager.Instance.DisableTouch();
            Anim.PlayExit(() =>
            {
                lockAnim = false;
                TouchManager.Instance.EnableTouch();
                gameObject.SetActive(false);
                nextPad.gameObject.SetActive(true);
                nextPad.Initialize();
                nextPad.ToNormal();
                nextPad.Anim.PlayEnter();
                CreatePlayerUISelectClothesPad.IsInit = true;
                styleSubPad.SetActive(true);
                colorSubPad.SetActive(false);
            });
        }

        public void ShowStyleSubPad()
        {
            toggleGroup.Switch(styleToggle);
        }

        public void ShowColorSubPad()
        {
            toggleGroup.Switch(colorToggle);
        }

        public void OnStyleSelect()
        {
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            }
            styleSubPad.SetActive(true);
            colorSubPad.SetActive(false);
        }

        public void OnColorSelect()
        {
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.SWITCH_TAB);
            }
            if (!colorSubPad.activeInHierarchy)
            {
                if (playColorAnim)
                {
                    Anim.InitColor();
                    Anim.PlayColor();
                }
            }
            playColorAnim = false;
            styleSubPad.SetActive(false);
            colorSubPad.SetActive(true);
        }
    }
}