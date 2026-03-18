using System.Collections.Generic;
using UnityEngine;

public class FixPosition : MonoBehaviour
{
    [SerializeField] private Transform target = null;

    private Vector3 dp;

    public void Fix(Transform target)
    {
        this.target = target;
        dp = transform.position - target.position;
    }

    public void Release()
    {
        target = null;
    }

    private void Update()
    {
        //同步相对位置
        if (target != null)
        {
            transform.position = target.position + dp;
        }
    }
}
