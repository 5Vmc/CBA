using BigBang;
using UnityEngine;

public class ContinentTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //Babu.EventManager.Instance.Dispatch(EventID.OnFlagIn, other.transform.parent.GetSiblingIndex() + 1);
    }

    private void OnTriggerExit(Collider other)
    {
        //Babu.EventManager.Instance.Dispatch(EventID.OnFlagOut);
    }
}
