using BigBang.Animation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class MyGamePadMiddleItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text score1;
        [SerializeField] private TMP_Text score2;
        [SerializeField] private Image progressBar1;
        [SerializeField] private Image progressBar2;

        public MyGamePadMiddleItemAnim Anim;

        public void SetData(int value1, int value2)
        {
            score1.text = value1.ToString();
            score2.text = value2.ToString();
            try
            {
                progressBar1.fillAmount = (float)value1 / (value1 + value2);
                progressBar2.fillAmount = (float)value2 / (value1 + value2);
            }
            catch
            {

            }
            Anim.PlayEnter();
        }
    }
}