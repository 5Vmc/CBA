using BigBang.Animation;
using DG.Tweening;
using GameConfig.Config;
using TMPro;
using UnityEngine;

namespace BigBang.UI
{
    public class WishItem : MonoBehaviour
    {
        // 球员姓名
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private CardSelectIcon icon;
        [SerializeField] private GameObject blurText;
        [SerializeField] private IllusionAnim illusionAnim;
        [SerializeField] private GameObject addImg1;
        [SerializeField] private GameObject addImg2;

        public void SetData(CardModelConfig config, bool isSelect = false)
        {
            nameText.text = PlayerCard.GetFullName(config);
            icon.gameObject.SetActive(true);
            blurText.SetActive(true);
            icon.SetData(config, isSelect);
            addImg1.SetActive(true);
            addImg2.SetActive(false);

            DOTween.Kill(addImg2);
        }

        public void SetEmpty()
        {
            nameText.text = string.Empty;
            icon.gameObject.SetActive(false);
            blurText.SetActive(false);
            addImg1.SetActive(false);
            addImg2.SetActive(true);
            addImg2.DOBreath(0.9f, 1.1f, 2, 1).SetLoops(-1);
        }

        public void PlayIllusion()
        {
            illusionAnim.PlayLoop(1.5f, 0, 0.3f, 1);
            // illusionAnim.GetComponent<UIShiny>().Play();
        }

        public void StopIllusion()
        {
            illusionAnim.StopLoop();
            // illusionAnim.GetComponent<UIShiny>().Stop();
        }

        public void StopAni()
        {
            DOTween.Kill(addImg2);
        }

        void OnDestroy()
        {
            StopIllusion();
            StopAni();
        }
    }
}