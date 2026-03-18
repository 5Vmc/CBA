using UnityEngine;
using UnityEngine.UI;

namespace Babu.Globalization
{

    [RequireComponent(typeof(Text))]
    public class GlobalizationTextFiller : MonoBehaviour
    {
        Text _text;

        void Awake()
        {
            _text = GetComponent<Text>();
            _text.text = Globalizer.Instance.GetGlobalizationText(_text.text).Replace("<br/>", "\n");

            if (Globalizer.Instance.GetCurLanguageType() == Globalizer.LanguageType.English)
            {
                _text.font = Resources.Load<Font>("Fonts/English");
            }
            else if (Globalizer.Instance.GetCurLanguageType() == Globalizer.LanguageType.TraditionalChinese)
            {
                _text.font = Resources.Load<Font>("Fonts/Tc");
            }
        }
    }
}
