using TMPro;
using UnityEngine;

namespace Babu.Globalization
{
    [RequireComponent(typeof(TMP_Text))]
    public class GlobalizationTMPFiller : MonoBehaviour
    {
        TMP_Text _text;
        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _text.text = Globalizer.Instance.GetGlobalizationText(_text.text).Replace("<br/>", "\n");

            if (Globalizer.Instance.GetCurLanguageType() == Globalizer.LanguageType.English)
            {
                _text.font = Resources.Load<TMP_FontAsset>("Fonts/English SDF");
                _text.enableAutoSizing = true;
            }
            else if (Globalizer.Instance.GetCurLanguageType() == Globalizer.LanguageType.TraditionalChinese)
            {
                _text.font = Resources.Load<TMP_FontAsset>("Fonts/Tc SDF");
                _text.enableAutoSizing = false;
            }
            else
            {
                _text.enableAutoSizing = false;
            }
        }
    }
}
