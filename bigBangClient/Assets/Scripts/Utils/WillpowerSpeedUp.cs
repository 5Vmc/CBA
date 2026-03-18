using UnityEngine;

public class WillpowerSpeedUp : MonoBehaviour
{
    private Animator animator;

    private void Start()
    {
        animator=GetComponent<Animator>();
    }

    public void SpeedUp()
    {
        animator.speed = 2;
    }

    public void Normal()
    {
        animator.speed = 1;
    }    
}
