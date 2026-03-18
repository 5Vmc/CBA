using BigBang.Animation;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Coffee.UIEffects;
using TMPro;
using UnityTimer;
using Babu;

namespace BigBang.UI
{
    public class SkillIcon : SkillIconBase
    {
        [SerializeField] private Image lockBg;
        [SerializeField] private Image lockImg;
        [SerializeField] private Image trainingImage;

        [SerializeField] private TMP_Text levelText;

        [SerializeField] private HorizontalLayoutGroup textLayout;

        private Skill _skill;

        public SkillIconAnim Anim;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="showLevel"></param>
        /// <param name="showState"></param>
        /// <param name="useage">0 不存， 1 特技面板要存</param>
        public void SetData(Skill skill, bool showLevel, bool showState, int saveRedDotData = 0)
        {
            if (skill == null)
            {
                UpdateNullInfo();
                return;
            }
            _skill = skill;
            base.SetData(_skill.Config);
            skillImg.gameObject.SetActive(true);

            if (showLevel)
            {
                levelText.gameObject.SetActive(true);

                levelText.text = "Lv." + skill.Level.ToString();
                LayoutRebuilder.ForceRebuildLayoutImmediate(textLayout.GetComponent<RectTransform>());
            }
            else
            {
                levelText.gameObject.SetActive(false);
            }

            UpdateStateInfo(showState);
        }

        // public void SetLevel()

        [SerializeField] private Image dotNodeImg = null;
        public void UpdateStateInfo(bool showState)
        {
            dotNodeImg.gameObject.SetActive(false);
            if (showState)
            {
                var skillState = _skill.GetSkillState();
                if (skillState == SkillState.ConditionsNotMet || skillState == SkillState.ConditionsMetLock)
                {
                    // 初始化位置
                    lockImg.rectTransform.anchoredPosition = new Vector2(40.3f, -35.4f);
                    // 初始化透明度
                    lockImg.SetAlpha(1);
                    lockBg.GetComponent<UIDissolve>().effectFactor = 0;
                    lockBg.gameObject.SetActive(true);
                    if(skillState == SkillState.ConditionsMetLock)
                    {
                        dotNodeImg.gameObject.SetActive(true);
                    }
                }
                else
                {
                    lockBg.gameObject.SetActive(false);
                }
                trainingImage.gameObject.SetActive(skillState == SkillState.UnlockTraining);
            }
            else
            {
                lockBg.gameObject.SetActive(false);
                trainingImage.gameObject.SetActive(false);
            }
        }

        private void UpdateNullInfo()
        {
            skillImg.gameObject.SetActive(false);
            lockBg.gameObject.SetActive(false);
            trainingImage.gameObject.SetActive(false);
        }

        public int SkillId
        {
            get { return _skill.Id; }
            private set { }
        }

        public void RefreshLevel()
        {

            levelText.text = "Lv." + _skill.Level.ToString();
        }


    }
}