using UnityEngine;
using Utils;
namespace BigBang.UI
{
    public class FormationSlotItem : Slot
    {
        public override void Init()
        {
            dropAction += DropCallback;
            pointerEnterAction += EnterCallback;
            pointerExitAction += ExitCallback;
        }

        private void DropCallback()
        {
            Debug.Log("Formation Slot Drop CallBack: ");
        }
        private void EnterCallback()
        {

        }
        private void ExitCallback()
        {

        }
    }
}
