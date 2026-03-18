using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    // 方向(单位向量)
    public Vector2 Direction;
    // 速度
    public float Speed;
    // 位移
    public float Distance;

    private float gap = 0.05f;
    private float sumTime = 0;

    private bool isDown = false;
    private bool isIn = false;
    private Vector2 lastPos = Vector2.zero;

    private Vector2 downPos = Vector2.zero;

    private void Update()
    {
        if (!isIn) return;
        if (!isDown) return;
        if (sumTime >= gap)
        {
            Vector2 pos = Input.mousePosition;
            var dif = pos - lastPos;
            Direction = dif.normalized;
            Speed = dif.magnitude / sumTime;
            lastPos = Input.mousePosition;
            sumTime = 0;
        }
        Distance = ((Vector2)Input.mousePosition - downPos).magnitude;
        sumTime += Time.deltaTime;
    }

    public void Release()
    {
        isDown = false;
        Speed = 0;
        Direction = Vector2.zero;
        Distance = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
        lastPos = Input.mousePosition;
        downPos = Input.mousePosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isIn = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isIn = false;
        sumTime = 0;
        Direction = Vector2.zero;
        Speed = 0;
        Distance = 0;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
        sumTime = 0;
        Direction = Vector2.zero;
        Speed = 0;
        Distance = 0;
    }
}