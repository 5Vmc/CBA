using GameConfig;
using GameConfig.Config;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.GameItem;
using BigBang.Animation;

namespace BigBang.UI
{
    public class ProgressItem : MonoBehaviour
    {
        [SerializeField] private Image progressValue;
        [SerializeField] public ProgressItemAnim Anim;
        public float Value;

        private void Awake()
        {
        }

        private void OnEnable()
        {    }

        private void OnDisable()
        {}

        public void SetData(float value, float maxvalue, bool playAnim = true)
        {
            float newPt = Value / maxvalue;

            SetData(newPt);
        }

        public void SetData(float newPt, bool playAnim = true)
        {
            if (!playAnim)
            {
                progressValue.fillAmount = newPt;
            }
            else
            {
                Anim.PlayAnim(newPt);
            }
        }
    }
}
