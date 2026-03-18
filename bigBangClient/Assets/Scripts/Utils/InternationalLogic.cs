using Babu.Globalization;

namespace Babu
{
    public class InternationalLogic
    {
        public static bool IsInternationalVersion()
        {
            return Globalizer.Instance.IsInternationalVersion();
        }

        public static bool IsChinese()
        {
            if (Globalizer.Instance.GetCurLanguageType() == Globalizer.LanguageType.Chinese ||
                Globalizer.Instance.GetCurLanguageType() == Globalizer.LanguageType.TraditionalChinese)
            {
                return true;
            }

            return false;
        }
    }
}