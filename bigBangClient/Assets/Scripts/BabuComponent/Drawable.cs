using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// <para>该组件可实现拖动效果，且拖动具有平滑过渡的效果</para>
/// <para>包含了进入、离开、按下、松开、点击、拖动、拖动结束等事件</para>
/// <para>一个完整的拖动释放执行顺序</para>
/// <para>拖动流程：OnEnter -> OnPress -> OnDragStart -> OnDrag -> OnDragEnd -> OnRelease -> onExit</para>
/// <para>点击流程：OnEnter -> OnPress -> OnRelease -> OnClicl -> OnExit</para>
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class Drawable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// 拖动模式
    /// 时间模式：通过按下时间来触发拖动事件
    /// 距离模式：通过按下的位置与当前位置的距离来触发拖动事件
    /// 方向模式：通过按下位置与当前位置的角度以及距离来触发拖动事件
    /// </summary>
    public enum DrawableMode
    {
        [InspectorName("时间模式")]
        Time,
        [InspectorName("距离模式")]
        Distance,
        [InspectorName("方向模式")]
        Direction
    }

    [Header("过渡比率")]
    [Range(0, 1)]
    public float TweenRate = 0.5f;
    [Header("过度时间")]
    public float TweenTime = 0.5f;
    [Header("触发模式")]
    public DrawableMode Mode;
    [Header("[时间模式] 拖动触发时间")]
    public float Time = 0.2f;
    [Header("[距离模式|方向模式] 拖动触发距离")]
    public float Distance = 100f;
    [Header("[方向模式] 拖动触发方向")]
    public Vector3 Direction = Vector3.up;
    [Header("[方向模式] 角度范围")]
    public float Angle = 45;
    [Header("忽略X轴方向")]
    public bool IgnoreX = false;
    [Header("忽略Y轴方向")]
    public bool IgnoreY = false;
    [Header("偏移")]
    public bool bias = false;
    // 按下后会一直执行直到松开
    public event Action OnPress;
    public event Action OnRelease;
    public event Action<Drawable> OnDragStart;
    // 拖动时会一直执行直到停止拖动
    public event Action<Drawable> OnDrag;
    public event Action<Drawable> OnDragEnd;
    public event Action OnClick;
    public event Action OnEnter;
    public event Action OnExit;
    public Vector3 DragStartPosition { get; private set; }
    public Vector3 CurrentPosition { get => rectTransform.position; }

    public RectTransform Rect { get => rectTransform; }

    private int count = 0;
    private float sumTime = 0;
    private bool isEnter = false;
    private bool isDown = false;
    private bool isDrag = false;
    private bool isDragStart = false;
    private Vector3 pressPoint;
    private RectTransform rectTransform;
    public Dictionary<int, object> Args { get; private set; } = new Dictionary<int, object>();

    // 距离拖动点距离中心点的距离
    private Vector2 dPos = Vector2.zero;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (isDown)
        {
            OnPress?.Invoke();
        }
        if (isDrag)
        {
            if (sumTime <= TweenTime)
            {
                sumTime += UnityEngine.Time.deltaTime;
            }
            MoveToTarget();
        }
        //距离检测
        DistanceCheck();
        //方向检测
        DirectionCheck();
    }

    public void Release()
    {
        OnPointerUp(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
        DragStartPosition = rectTransform.position;
        pressPoint = Input.mousePosition;
        dPos = rectTransform.position - UIController.Instance.GetCamera().ScreenToWorldPoint(Input.mousePosition);
        if (Mode == DrawableMode.Time)
        {
            //时间检测
            Timewait(++count);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isDrag)
        {
            OnDragEnd?.Invoke(this);
        }
        OnRelease?.Invoke();
        if (!isDrag && isEnter)
        {
            OnClick?.Invoke();
        }
        isDown = false;
        isDrag = false;
        sumTime = 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isEnter = true;
        OnEnter?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isEnter = false;
        OnExit?.Invoke();
    }

    //拖动效果
    private void MoveToTarget()
    {
        if (!(IgnoreX && IgnoreY))
        {
            Vector3 tPoint = Vector3.zero;
            if (isDragStart)
            {
                OnDragStart?.Invoke(this);
                isDragStart = false;
            }
            if (!IgnoreX && !IgnoreY)
            {
                tPoint = Input.mousePosition;
            }
            else
            {
                tPoint.x = Input.mousePosition.x;
                tPoint.y = Input.mousePosition.y;
                tPoint.z = Input.mousePosition.z;
            }
            var vPoint = UIController.Instance.GetCamera().ScreenToWorldPoint(tPoint);
            vPoint.x = IgnoreX ? rectTransform.position.x : vPoint.x;
            vPoint.y = IgnoreY ? rectTransform.position.y : vPoint.y;
            vPoint.z = rectTransform.position.z;
            if (bias)
            {
                // 加上偏移值
                vPoint.x += dPos.x;
                vPoint.y += dPos.y;
            }
            //平滑过渡
            rectTransform.position = Vector3.Lerp(rectTransform.position, vPoint, TweenTime == 0 ? 1 : TweenRate * (sumTime / TweenTime));
            //触发拖动事件
            OnDrag?.Invoke(this);
        }
    }

    //时间检测
    private async void Timewait(int flag)
    {
        await Task.Delay(TimeSpan.FromSeconds(Time));
        if (count == flag)
        {
            isDrag = isEnter && isDown;
            isDragStart = isDrag;
            count = 0;
        }
    }

    //距离检测
    private void DistanceCheck()
    {
        if (isDown && !isDrag && Mode == DrawableMode.Distance)
        {
            isDrag = (Input.mousePosition - pressPoint).magnitude >= Distance;
            isDragStart = isDrag;
        }
    }

    //方向检测
    private void DirectionCheck()
    {
        if (isDown && !isDrag && Mode == DrawableMode.Direction)
        {
            isDrag = (Input.mousePosition - pressPoint).magnitude >= Distance && Vector3.Angle(Input.mousePosition - pressPoint, Direction) <= Angle;
            isDragStart = isDrag;
        }
    }
}