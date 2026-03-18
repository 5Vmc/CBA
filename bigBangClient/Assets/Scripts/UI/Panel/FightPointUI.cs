using BigBang.Animation;
using deVoid.UIFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class FightPointUIProperties : WindowProperties
    {
        public int addValue { get; private set; }
        public int oldValue;

        public FightPointUIProperties(int _oldValue, int _addValue)
        {
            Debug.Log("Tips , content = " + _addValue.ToString());
            addValue = _addValue;
            oldValue = _oldValue;
        }
    }

    public class FightPointUI : AWindowController<FightPointUIProperties>
    {

        [SerializeField] private Image com;
        [SerializeField] private Transform scorePanel;
        [SerializeField] private ImageFont txtFightPoint;
        private int start = 0;
        private int end = 0;
        private Tween numTween;
        private Tween overTween;
        private static FightPointUI _instance;

        public static FightPointUI Instance
        {
            get {
                return _instance;
            }
        }
        protected override void OnPropertiesSet()
        {
            _instance = this;
            AudioManager.Instance.PlaySound(AudioNames.ANI_TIPS);
            Play(Properties.oldValue, Properties.addValue);
        }

        protected override void Awake()
        {
            
        }

        public void Play(int oldValue, int addValue)
        {
            //初始化
            txtFightPoint.text = "";

            start = oldValue;
            end = start + addValue;
            com.SetAlpha(1f);
            numTween = DOTween.To(value => txtFightPoint.text = ((int)value).ToString(), start, end, .5f).SetDelay(0.2f);
            start = end;
            overTween = com.DOFade(0f, 0.3f).SetDelay(2f);
            overTween.OnComplete(() =>
            {
                PlayExit();
            });
        }

        public void ContinuePlay(int AddPoint) {
            if (numTween != null && numTween.active) {
                numTween.Kill(true);
            }
            if (overTween != null && overTween.active) {
                overTween.Kill(false);
            }
            com.SetAlpha(1f);
            end = start + AddPoint;
            numTween = DOTween.To(value => txtFightPoint.text = ((int)value).ToString(), start, end, .5f);
            start = end;

            overTween = com.DOFade(0f, 0.3f).SetDelay(2f);
            overTween.OnComplete(() =>
            {
                PlayExit();
            });
        }

        public void PlayExit()
        {
            Debug.Log("exit--------------");
            _instance = null;
            UIController.Instance.CloseWindow<FightPointUI>();
        }
    }
}
