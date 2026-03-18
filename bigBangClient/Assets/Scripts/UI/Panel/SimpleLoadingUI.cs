using Babu;

namespace BigBang.UI
{
    public class SimpleLoadingUI : BabuSingleton<SimpleLoadingUI>
    {
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}