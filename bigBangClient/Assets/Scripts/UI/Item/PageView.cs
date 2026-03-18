using System;
using System.Collections.Generic;
using BigBang;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 不需要其他的组件配合
/// 可参考HomeUICenterItem来使用
/// </summary>
public class PageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{

    #region 外部接口

    [HideInInspector] public Action<int> OnResetCount;//结束滚动，参数为：新index

    [HideInInspector] public Action<int> OnStartScroll;//开始滚动，参数为：新index
    [HideInInspector] public Action<int> OnEndScroll;//结束滚动，参数为：新index

    [SerializeField] public bool enableAutoMove = true;//开启自动滚动
    [SerializeField] public float autoMoveTime = 3.0f;//自动滚动到下一个的间隔时间

    [SerializeField] public Button lastButton;//左面的，前往上一个按钮，可以没有
    [SerializeField] public Button nextButton;//右面的，前往下一个按钮，可以没有

    #endregion

    #region 初始化

    private void Start()
    {
        ResetItemWidth();
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

    float itemWidth = 720f;
    private void ResetItemWidth()
    {
        itemWidth = transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x;
    }

    private int Remainder(int dividend, int divisor)
    {
        if (dividend < 0) dividend += childCount;
        return dividend % divisor;
    }

    private int childCount = 0;
    private int nowIndex = 0;
    private List<RectTransform> rectList = new();
    /// <summary>
    /// 子物体数量改变时手动调用一下
    /// </summary>
    public void ResetCount()
    {
        nowIndex = 0;
        childCount = transform.childCount;

        bool isCanMove = childCount > 1;//至少需要2个子物体来循环起来，只有1个的时候隐藏了移动按钮切不许移动
        lastButton?.gameObject.SetActive(isCanMove);
        nextButton?.gameObject.SetActive(isCanMove);

        rectList.Clear();
        for (int i = 0; i < childCount; i++)
        {
            GameObject go = transform.GetChild(i).gameObject;
            rectList.Add(go.GetComponent<RectTransform>());
        }
        OnResetCount?.Invoke(childCount);
        ResetChildPos();
        RestartAutoMove();
    }

    private void ResetChildPos()
    {
        moveSeq?.Kill();
        moveSeq = null;

        OnStartScroll?.Invoke(nowIndex);

        foreach (var item in rectList)
        {
            item.gameObject.SetActive(false);
        }

        int itemIndex = Remainder(nowIndex, childCount);
        RectTransform rect = rectList[itemIndex];
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

        int nextIndex = nowIndex;
        if (next)
        {
            nextIndex++;
            if (nextIndex >= childCount) nextIndex -= childCount;
        }
        else
        {
            nextIndex--;
            if (nextIndex < 0) nextIndex += childCount;
        }
        OnStartScroll?.Invoke(nextIndex);

        moveSeq = DOTween.Sequence();

        RectTransform rectOld = rectList[Remainder(nowIndex, childCount)];
        RectTransform rectNew = rectList[Remainder(nowIndex + (next ? 1 : -1), childCount)];
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
        moveSeq.AddTo(this.gameObject);
    }


    private void OnClickLastButton()
    {
        AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        MoveOnce(false);
    }
    private void OnClickNextButton()
    {
        AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
        MoveOnce(true);
    }

    private bool isMovedByDrag = false;
    private bool isDragging = false;
    private Vector2 startPos;
    public void OnBeginDrag(PointerEventData eventData)
    {
        isMovedByDrag = false;
        isDragging = true;
        startPos = eventData.pointerCurrentRaycast.screenPosition;
        StopAutoMove();
    }
    private float minMovePixel = 5;
    public void OnDrag(PointerEventData eventData)
    {
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
    private void OnDestroy()
    {
        StopAutoMove();
    }
    private void MoveNext()
    {
        MoveOnce(true);
    }

    #endregion

}