using System;

namespace Babu
{
    public class UIUtils
    {
        public static void CloseAllPanels()
        {
            //直接跳转到解雇UI
            while(UIController.Instance.GetCurrentShowPanelName() != "HomeUI"){
                UIController.Instance.HideTopestPanel();
            }
        }
    }
}