using System;
using System.Collections.Generic;
using BigBang;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IPageViewVirtualItem
{
    void SetData(object data);
}

/// <summary>
/// 子物体需继承此类
/// </summary>
public class PageViewVirtualItemBase : MonoBehaviour, IPageViewVirtualItem
{
    public virtual void SetData(object data)
    {

    }
}

/// <summary>
/// 用两个来回循环的虚拟左右页面列表
/// </summary>
public class PageViewVirtual : MonoBehaviour//, IBeginDragHandler, IEndDragHandler, IDragHandler//当前需求不需要拖拽，会影响外层的纵向滑动
{

    #region 外部接口

    [HideInInspector] public Action OnResetData;//数据更改

    [HideInInspector] public Action<int> OnStartScroll;//开始滚动，参数为：新index
    [HideInInspector] public Action<int> OnEndScroll;//结束滚动，参数为：新index

    [SerializeField] public bool enableAutoMove = true;//开启自动滚动
    [SerializeField] public float autoMoveTime = 3.0f;//自动滚动到下一个的间隔时间


    [SerializeField] public bool enableMoveButton = true;//使用前后按钮
    [SerializeField] public Button lastButton;//左面的，前往上一个按钮，可以没有
    [SerializeField] public Button nextButton;//右面的，前往下一个按钮，可以没有

    [SerializeField] public bool enableDrag = true;//使用拖拽

    #endregion

    #region 初始化

    private void Start()
    {
        ResetCount();
        lastButton?.onClick.AddListener(OnClickLastButton);
        nextButton?.onClick.AddListener(OnClickNextButton);
    }
    private void OnEnable()
    {
        RestartAutoMove();
    }
    private void OnDisable()
    {
        StopAutoMove();
    }

    public float itemWidth = 720f;

    private int Remainder(int dividend, int divisor)
    {
        if (divisor <= 0) return 0;
        while (dividend <= 0) dividend += divisor;
        return dividend % divisor;
    }

    private int childCount = 0;

    private int dataCount = 0;
    private int nowIndex = 0;
    /// <summary>
    /// 需要两个物体才能循环,需在编辑器设置
    /// </summary>
    public List<RectTransform> rectList = new();

    private List<object> dataList = new();
    public void SetData<T>(List<T> dataList)
    {
        if (isSetItemCount == false) ResetCount();
        this.dataList.Clear();
        foreach (var item in dataList)
        {
            this.dataList.Add(item);
        }
        dataCount = dataList.Count;
        ResetMovePageButton();
        ResetChildPos();
        RestartAutoMove();
    }
    public void MoveTo(int index)
    {
        nowIndex = index;
        ResetMovePageButton();
        ResetChildPos();
        RestartAutoMove();
    }
    private bool isSetItemCount = false;
    /// <summary>
    /// 子物体数量改变时手动调用一下
    /// </summary>
    public void ResetCount()
    {
        isSetItemCount = true;
        childCount = rectList.Count;
    }

    private void ResetMovePageButton()
    {
        bool isItemEnough = childCount >= 2;//至少需要2个子物体来循环起来，只有1个的时候隐藏了移动按钮切不许移动
        bool isDataEnough = dataCount >= 2;//至少需要2个数据来循环起来，只有1个的时候隐藏了移动按钮切不许移动
        bool isCanMove = enableMoveButton && isItemEnough && isDataEnough;
        lastButton?.gameObject.SetActive(isCanMove);
        nextButton?.gameObject.SetActive(isCanMove);
    }

    private void ResetChildPos()
    {
        if (isSetItemCount == false) ResetCount();

        moveSeq?.Kill();
        moveSeq = null;

        OnStartScroll?.Invoke(nowIndex);

        foreach (var item in rectList)
        {
            item.gameObject.SetActive(false);
        }

        int itemIndex = Remainder(nowIndex, childCount);
        RectTransform rect = rectList[itemIndex];

        if (dataCount > 0)
        {
            PageViewVirtualItemBase pageItem = rect.GetComponent<PageViewVirtualItemBase>();
            object data = dataList[Remainder(nowIndex, dataCount)];
            pageItem.SetData(data);
        }

        rect.gameObject.SetActive(true);
        Vector3 oldPos = rect.localPosition;
        oldPos.x = 0;
        rect.localPosition = oldPos;

        OnEndScroll?.Invoke(nowIndex);
    }

    #endregion

    #region 移动

    private Sequence moveSeq;
    private void MoveOnce(bool next)
    {
        if (moveSeq != null) return;
        if (childCount < 2) return;
        if (dataCount < 2) return;

        int nextIndex = nowIndex;
        if (next)
        {
            nextIndex++;
        }
        else
        {
            nextIndex--;
        }
        OnStartScroll?.Invoke(nextIndex);

        moveSeq = DOTween.Sequence();

        RectTransform rectOld = rectList[Remainder(nowIndex, childCount)];
        RectTransform rectNew = rectList[Remainder(nowIndex + (next ? 1 : -1), childCount)];
        PageViewVirtualItemBase item = rectNew.GetComponent<PageViewVirtualItemBase>();
        object data = dataList[Remainder(nowIndex + (next ? 1 : -1), dataCount)];
        item.SetData(data);
        rectNew.gameObject.SetActive(true);

        Vector3 oldPos = rectNew.localPosition;
        oldPos.x = next ? itemWidth : -itemWidth;
        rectNew.localPosition = oldPos;

        moveSeq.Append(rectNew.DOLocalMoveX(0, 0.3f));
        moveSeq.Join(rectOld.DOLocalMoveX(next ? -itemWidth : itemWidth, 0.3f));
        moveSeq.AppendCallback(() =>
        {
            nowIndex = nextIndex;
            rectOld.gameObject.SetActive(false);
            moveSeq = null;
            RestartAutoMove();
            OnEndScroll?.Invoke(nowIndex);
        });
    }


    private void OnClickLastButton()
    {
        if (enableMoveButton == false) return;
        AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        MoveOnce(false);
    }
    private void OnClickNextButton()
    {
        if (enableMoveButton == false) return;
        AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        MoveOnce(true);
    }

    private bool isMovedByDrag = false;
    private bool isDragging = false;
    private Vector2 startPos;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!enableDrag) return;
        isMovedByDrag = false;
        isDragging = true;
        startPos = eventData.pointerCurrentRaycast.screenPosition;
        StopAutoMove();
    }
    private float minMovePixel = 5;
    public void OnDrag(PointerEventData eventData)
    {
        if (!enableDrag) return;
        if (!isDragging) return;
        float offset = eventData.pointerCurrentRaycast.screenPosition.x - startPos.x;
        if (Mathf.Abs(offset) > minMovePixel)
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            MoveOnce(offset < 0);
            isDragging = false;
            isMovedByDrag = true;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!enableDrag) return;
        isDragging = false;
        if (isMovedByDrag == false)
        {
            RestartAutoMove();
        }
    }

    #endregion

    #region 自动移动

    UnityTimer.Timer autoMoveTimer;
    private void RestartAutoMove()
    {
        StopAutoMove();
        if (enableAutoMove)
        {
            autoMoveTimer = UnityTimer.Timer.Register(this.gameObject, autoMoveTime, MoveNext, null, true);
        }
    }
    private void StopAutoMove()
    {
        if (autoMoveTimer != null)
        {
            UnityTimer.Timer.Cancel(autoMoveTimer);
            autoMoveTimer = null;
        }
    }
    private void MoveNext()
    {
        MoveOnce(true);
    }

    #endregion

}