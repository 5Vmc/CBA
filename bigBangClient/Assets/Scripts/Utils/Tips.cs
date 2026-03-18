using BigBang.UI;

namespace Utils
{
    public class Tips
    {
        public static void PopTips(LangID tipsLangID)
        {
            UIController.Instance.OpenWindow<TipsUI>(new TipsUIProperties(Lang.Get(tipsLangID)));
        }

        /// <summary>
        /// 支持多个文本，用竖杠分割
        /// </summary>
        /// <param name="tips"></param>
        public static void PopTips(string tips)
        {
            UIController.Instance.OpenWindow<TipsUI>(new TipsUIProperties(tips));
        }

        public static void PopError(ErrorID errorID)
        {
            UIController.Instance.OpenWindow<TipsUI>(new TipsUIProperties(Lang.Error(errorID)));
        }
        public static void PopError(string errortipStr)
        {
            UIController.Instance.OpenWindow<TipsUI>(new TipsUIProperties(errortipStr));
        }
    }
}