using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using BigBang.UI;
using TMPro;

namespace BigBang.Animation
{
    public class RegularTrainItemLevelAnim : MonoBehaviour
    {
        [SerializeField] private RegularTrainItemComponent com;

        [SerializeField] private List<Tween> tweens = new List<Tween>();

        public void Play(TMP_Text obj, int originScale,int changeScale)
        {
            Kill();
            obj.rectTransform.localScale = Vector3.one;
            //com.ProjectLevelText.rectTransform.localScale = Vector3.one;
            //字体由小变大    com.ProjectLevelText
            tweens.Add(DOTween.To(value => obj.fontSize = value, originScale, changeScale, 0.1f).OnComplete(() =>
            {
                //字体由大变小
                tweens.Add(DOTween.To(value => obj.fontSize = value, changeScale, originScale, 0.1f));
            }));
            //字体发光
            tweens.Add(com.FillIncomeText.DOColor(Color.grey, 0.2f).OnComplete(() =>
            {
                //字体不发光
                tweens.Add(com.FillIncomeText.DOColor(new Color(45/255f,64/255f,84/255f,255/255f),0.2f));
            }));
            //字体发光
            //tweens.Add(DOTween.To(value => obj.fontMaterial.SetFloat("_GlowOuter", value), 0, fontOuter, 0.2f).OnComplete(() =>
            //{
            //    //字体不发光
            //    tweens.Add(DOTween.To(value => obj.fontMaterial.SetFloat("_GlowOuter", value), fontOuter, 0, 0.1f));
            //}));
            //if (obj == com.FillIncomeText)
            //{
            //    tweens.Add(DOTween.To(value => obj.fontMaterial.SetFloat("_GlowOffset", value), 0, 0.26f, 0.2f).OnComplete(() =>
            //    {
            //        tweens.Add(DOTween.To(value => obj.fontMaterial.SetFloat("_GlowOffset", value), 0.26f, 0, 0.1f));
            //    }));
            //}
        }
        private void Kill()
        {
            tweens.ForEach(item => item.Kill());
            tweens.Clear();
        }

        private void OnDisable()
        {
            Kill();
        }
    }
}