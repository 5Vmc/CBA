using BigBang.UI;

namespace Utils
{
    public class FightPoint
    {

        public static void PopTips(int oldValue, int addValue)
        {
            if (FightPointUI.Instance == null)
            {
                UIController.Instance.OpenWindow<FightPointUI>(new FightPointUIProperties(oldValue, addValue));
            }
            else {
                FightPointUI.Instance.ContinuePlay(addValue);
            }
            
        }
    }
}