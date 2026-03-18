using Babu;
using BigBang.Animation;
using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{
    public class CreatePlayerUISelectClothesPad : MonoBehaviour
    {
        [SerializeField] private CreatePlayerUICreateIconPad previousPad;
        [SerializeField] private CreatePlayerUISelectPlayerPad nextPad;
        [SerializeField] private ColorSelectItem backgroundColor;
        [SerializeField] private ColorSelectItem patternColor;
        [SerializeField] private JerseyItem homeJersey;
        [SerializeField] private JerseyItem awayJersey;
        [SerializeField] private JerseyItem alternativeJersey;
        [SerializeField] private BabuToggleGroup jerseyGroup;
        [SerializeField] private BabuToggleGroup shapeGroup;
        [SerializeField] private BabuToggle homeToggle;
        [SerializeField] private BabuToggle awayToggle;
        [SerializeField] private BabuToggle alternativeToggle;
        [SerializeField] private BabuButton nextBtn;
        [SerializeField] private BabuButton previousBtn;
        [SerializeField] private BabuButton homeRandomBtn;
        [SerializeField] private BabuButton awayRandomBtn;
        [SerializeField] private BabuButton alternativeRandomBtn;


        [SerializeField] public SelectClothesPadAnim Anim;

        private StateValue homeValue = null;
        private StateValue awayValue = null;
        private StateValue alternativeValue = null;

        public StateValue HomeValue { get => homeValue; }
        public StateValue AwayValue { get => awayValue; }
        public StateValue AlternativeValue { get => alternativeValue; }

        private System.Random random = new System.Random();

        private int shapeIndex = 0;
        private int color1Index = 1;
        private int color2Index = 2;
        private int shapeCount = 5;
        private int colorCount = 11;

        public static bool IsInit = false;

        private void OnEnable()
        {
            nextBtn.OnClick += OnNext;
            previousBtn.OnClick += OnPrevious;
            backgroundColor.OnValueChanged += OnBackgroundColorChanged;
            patternColor.OnValueChanged += OnPatternColorChanged;
            shapeGroup.OnValueChanged += OnShapeChanged;
            homeRandomBtn.OnClick += OnRandomHome;
            awayRandomBtn.OnClick += OnRandomAway;
            alternativeRandomBtn.OnClick += OnRandomAlternative;
            homeToggle.OnSelect += OnHomeSelect;
            awayToggle.OnSelect += OnAwaySelect;
            alternativeToggle.OnSelect += OnAlternativeSelect;
            jerseyGroup.OnValueChanged += OnJerseyChanged;
            backgroundColor.OnReleaseItem += OnReleaseBackground;

            Anim.ToBig += ToBig;
        }

        private void OnDisable()
        {
            nextBtn.OnClick -= OnNext;
            previousBtn.OnClick -= OnPrevious;
            shapeGroup.OnValueChanged -= OnShapeChanged;
            backgroundColor.OnValueChanged -= OnBackgroundColorChanged;
            patternColor.OnValueChanged -= OnPatternColorChanged;
            homeRandomBtn.OnClick -= OnRandomHome;
            awayRandomBtn.OnClick -= OnRandomAway;
            alternativeRandomBtn.OnClick -= OnRandomAlternative;
            homeToggle.OnSelect -= OnHomeSelect;
            awayToggle.OnSelect -= OnAwaySelect;
            alternativeToggle.OnSelect -= OnAlternativeSelect;
            jerseyGroup.OnValueChanged -= OnJerseyChanged;
            backgroundColor.OnReleaseItem -= OnReleaseBackground;

            Anim.ToBig -= ToBig;
            IsInit = false;
        }

        private void OnJerseyChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            var grayColor = new Color(0.5f, 0.5f, 0.5f, 1);
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            }
            if (oldToggle == homeToggle)
            {
                homeJersey.GetComponent<Image>().DOColor(grayColor, 0.15f);
            }
            else if (oldToggle == awayToggle)
            {
                awayJersey.GetComponent<Image>().DOColor(grayColor, 0.15f);
            }
            else
            {
                alternativeJersey.GetComponent<Image>().DOColor(grayColor, 0.15f);
            }

            if (newToggle == homeToggle)
            {
                homeJersey.GetComponent<Image>().DOColor(Color.white, 0.15f);
            }
            else if (newToggle == awayToggle)
            {
                awayJersey.GetComponent<Image>().DOColor(Color.white, 0.15f);
            }
            else
            {
                alternativeJersey.GetComponent<Image>().DOColor(Color.white, 0.15f);
            }
        }

        private void OnReleaseBackground()
        {
            
        }

        private void OnHomeSelect()
        {
            MoveCursor(homeValue);
        }

        private void OnAwaySelect()
        {
            MoveCursor(awayValue);
        }

        private void OnAlternativeSelect()
        {
            MoveCursor(alternativeValue);
        }

        private bool flag = true;

        private void OnShapeChanged(BabuToggle oldToggle, BabuToggle newToggle)
        {
            Vector3 big = new Vector3(1, 1.18490566037736f, 1);
            Vector3 normal = new Vector3(0.8f, 0.8f * 1.18490566037736f, 0.8f);
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_1);
            }
            if (homeToggle.isOn)
            {
                homeValue[0] = newToggle.transform.GetSiblingIndex();
                homeJersey.SetIcon(homeValue);
                if (flag) return;
                homeJersey.transform.DOScale(big, 0.1f);
                awayJersey.transform.DOScale(normal, 0.1f);
                alternativeJersey.transform.DOScale(normal, 0.1f);
            }
            if (awayToggle.isOn)
            {
                awayValue[0] = newToggle.transform.GetSiblingIndex();
                awayJersey.SetIcon(awayValue);
                if (flag) return;
                homeJersey.transform.DOScale(normal, 0.1f);
                awayJersey.transform.DOScale(big, 0.1f);
                alternativeJersey.transform.DOScale(normal, 0.1f);
            }
            if (alternativeToggle.isOn)
            {
                alternativeValue[0] = newToggle.transform.GetSiblingIndex();
                alternativeJersey.SetIcon(alternativeValue);
                if (flag) return;
                homeJersey.transform.DOScale(normal, 0.1f);
                awayJersey.transform.DOScale(normal, 0.1f);
                alternativeJersey.transform.DOScale(big, 0.1f);
            }
        }

        public void ToNormal()
        {
            homeJersey.transform.localScale =
            awayJersey.transform.localScale =
            alternativeJersey.transform.localScale = new Vector3(0.8f, 0.8f * 1.18490566037736f, 0.8f);
            flag = true;
        }

        public void ToBig()
        {
            var big = new Vector3(1, 1.18490566037736f, 1);
            var normal = new Vector3(0.8f, 0.8f * 1.18490566037736f, 0.8f);
            if (homeToggle.isOn)
            {
                homeJersey.transform.DOScale(big, 0.1f);
                awayJersey.transform.DOScale(normal, 0.1f);
                alternativeJersey.transform.DOScale(normal, 0.1f);
            }
            if (awayToggle.isOn)
            {
                homeJersey.transform.DOScale(normal, 0.1f);
                awayJersey.transform.DOScale(big, 0.1f);
                alternativeJersey.transform.DOScale(normal, 0.1f);
            }
            if (alternativeToggle.isOn)
            {
                homeJersey.transform.DOScale(normal, 0.1f);
                awayJersey.transform.DOScale(normal, 0.1f);
                alternativeJersey.transform.DOScale(big, 0.1f);
            }
            flag = false;
        }

        private void OnBackgroundColorChanged(int oldColor, int newColor)
        {
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            }
            if (homeToggle.isOn)
            {
                homeValue[1] = newColor;
                homeJersey.SetIcon(homeValue);
            }
            if (awayToggle.isOn)
            {
                awayValue[1] = newColor;
                awayJersey.SetIcon(awayValue);
            }
            if (alternativeToggle.isOn)
            {
                alternativeValue[1] = newColor;
                alternativeJersey.SetIcon(alternativeValue);
            }
        }

        private void OnPatternColorChanged(int oldColor, int newColor)
        {
            if (IsInit)
            {
                AudioManager.Instance.PlaySound(AudioNames.BTN_2);
            }
            if (homeToggle.isOn)
            {
                homeValue[2] = newColor;
                homeJersey.SetIcon(homeValue);
            }
            if (awayToggle.isOn)
            {
                awayValue[2] = newColor;
                awayJersey.SetIcon(awayValue);
            }
            if (alternativeToggle.isOn)
            {
                alternativeValue[2] = newColor;
                alternativeJersey.SetIcon(alternativeValue);
            }
        }

        // 初始化
        public void Initialize()
        {
            if (!(homeValue == null || awayValue == null || alternativeValue == null)) return;

            homeValue = new StateValue();
            awayValue = new StateValue();
            alternativeValue = new StateValue();
            var colors1 = Enumerable.Range(0, colorCount).Random(3).ToArray();
            Random(homeValue, homeJersey, colors1[0]);
            Random(awayValue, awayJersey, colors1[1]);
            Random(alternativeValue, alternativeJersey, colors1[2]);
            jerseyGroup.Switch(homeToggle);

            homeJersey.GetComponent<Image>().color = Color.white;
            awayJersey.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);
            alternativeJersey.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1);

            MoveCursor(homeValue);
        }

        // 随机
        private void Random(StateValue jerseyValue, JerseyItem item, int color1)
        {
            int shape = random.Next(0, shapeCount);
            int color2 = random.Next(0, colorCount);
            // 形状
            jerseyValue[shapeIndex] = shape;
            // 背景色
            jerseyValue[color1Index] = color1;
            // 图案色
            jerseyValue[color2Index] = color2;
            item.SetIcon(jerseyValue);
        }

        // 移动游标指针
        private void MoveCursor(StateValue stateValue)
        {
            shapeGroup.Switch(stateValue[0]);
            backgroundColor.SetSelection(stateValue[1]);
            patternColor.SetSelection(stateValue[2]);
        }

        // 随机主场球衣
        private void OnRandomHome(BabuButton sender)
        {
            Random(homeValue, homeJersey, random.Next(0, colorCount));
            MoveCursor(homeValue);
        }

        // 随机客场球衣
        private void OnRandomAway(BabuButton sender)
        {
            Random(awayValue, awayJersey, random.Next(0, colorCount));
            MoveCursor(awayValue);
        }

        // 随机替换球衣
        private void OnRandomAlternative(BabuButton sender)
        {
            Random(alternativeValue, alternativeJersey, random.Next(0, colorCount));
            MoveCursor(alternativeValue);
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
                previousPad.ShowStyleSubPad();
                previousPad.Anim.PlayEnter();
                CreatePlayerUICreateIconPad.IsInit = true;
            });
        }

        // 下一步
        private void OnNext(BabuButton sender)
        {

            // if (homeValue[color1Index] == awayValue[color1Index]
            //     || homeValue[color1Index] == alternativeValue[color1Index]
            //     || awayValue[color1Index] == alternativeValue[color1Index])
            // {
            //     Tips.PopError(ErrorID.SameJersey);
            //     return;
            // }
            //if (lockAnim) return;
            //lockAnim = true;
            TouchManager.Instance.DisableTouch();
            Anim.PlayExit(() =>
            {
              //  lockAnim = false;
                TouchManager.Instance.EnableTouch();
                gameObject.SetActive(false);
                nextPad.gameObject.SetActive(true);
                nextPad.Lottery();
                nextPad.Anim.PlayEnter();
            });
        }
    }
}