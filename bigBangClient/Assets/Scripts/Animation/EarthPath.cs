using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EarthPath : MonoBehaviour
{
    public GameObject cubeplane; //飞机
    // private LineRenderer lineRender;
    private List<Vector3> _path;
    private float heightrate = 1f;   //飞行高度
    private Transform targetPlace;
    // Start is called before the first frame update
    void Start()
    {
        //PlayPlaneAni(0, 1, 2f);
    }

    public Tween PlayPlaneAni(string start, string end, float time)
    {
        int pointCount = 10;
        heightrate = 1.02f;
        _path = new List<Vector3>();
        List<Transform> positionList = new List<Transform>();
        // lineRender = GameObject.Find("WorldMap").AddComponent<LineRenderer>();
        // lineRender.startWidth = lineRender.endWidth = 0.02f;
        // lineRender.startColor = lineRender.endColor = Color.white;
        // lineRender.useWorldSpace = true;
        int startId = int.Parse(start.Replace("chNode", "")) - 1;
        int endId = int.Parse(end.Replace("chNode", "")) - 1;

        foreach (Transform nodeTrans in transform)
        {
            positionList.Add(nodeTrans);
        }

        cubeplane.transform.position = positionList[startId].position;
        targetPlace = positionList[endId];

        //保证飞机的朝向和背面向着球心
        Vector3 up = cubeplane.transform.position.normalized;
        Vector3 targetDir = targetPlace.transform.position.normalized;
        Vector3 forward = targetDir - up * Vector3.Dot(targetDir, up);
        cubeplane.transform.rotation = Quaternion.LookRotation(forward.normalized, up.normalized);

        calPath(positionList[startId].position, positionList[endId].position, pointCount);
        //lineRender.positionCount = _path.Count;
        //lineRender.SetPositions(_path.ToArray());
        //cubeplane.transform.rotation = positionList[0].rotation;

        BigBang.AudioManager.Instance.PlaySound(BigBang.AudioNames.PLANE);
        cubeplane.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

        return cubeplane.transform.DOPath(_path.ToArray(), time);
    }

    //飞机变小的动画
    public Tween Land()
    {
        return cubeplane.transform.DOScale(0.001f, 0.5f);
    }
    //起飞
    public Tween TakeOff()
    {
        return cubeplane.transform.DOScale(0.005f, 0.5f);
    }

    public void calPath(Vector3 point1, Vector3 point2, int pointCount)
    {
        for (int i = 0; i < pointCount; i++)
        {
            var t = (i + 1) / (float)pointCount;
            _path.Add(Vector3.Slerp(point1, point2, i * t) * heightrate);
        }
    }
}
