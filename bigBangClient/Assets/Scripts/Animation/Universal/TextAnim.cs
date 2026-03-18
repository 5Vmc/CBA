using BigBang.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BigBang.Animation
{
    /// <summary>
    /// 文本升级动画
    /// </summary>
    public static class TextAnim
    {
        [SerializeField] private static RegularTrainItemComponent com;
        public static Sequence DOLight(this TMP_Text obj, int originScale, int changeScale, float fontOuter, bool isSpecial = false)
        {
            if (isSpecial)
            {
                obj.fontMaterial.SetFloat("_GlowInner", 0);
            }
            Sequence sequence = DOTween.Sequence();
            obj.rectTransform.localScale = Vector3.one;
            //字体由小变大    com.ProjectLevelText
            sequence.Append(DOTween.To(value => obj.fontSize = value, originScale, changeScale, 0.2f));
            sequence.Join(DOTween.To(value => obj.fontSize = value, changeScale, originScale, 0.1f));

            //字体发光
            sequence.Append(DOTween.To(value => obj.fontMaterial.SetFloat("_GlowOuter", value), 0, fontOuter, 0.2f));
            sequence.Join(DOTween.To(value => obj.fontMaterial.SetFloat("_GlowOuter", value), fontOuter, 0, 0.1f));

            if (isSpecial)
            {
                sequence.Append(DOTween.To(value => obj.fontMaterial.SetFloat("_GlowOffset", value), 0, 1, 0.2f));
                sequence.Join(DOTween.To(value => obj.fontMaterial.SetFloat("_GlowOffset", value), 1, 0, 0.2f));
            }


            return sequence;
        }
    }
}
