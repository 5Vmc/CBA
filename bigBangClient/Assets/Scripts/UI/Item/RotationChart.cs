using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class RotationChart : MonoBehaviour
{
    [SerializeField] private List<RotationChartCardShowItem> cardShowItems;
    [SerializeField] private float itemWidth = 0;
    [SerializeField] private float itemSpace = 10;
    [SerializeField] private int backGroundCount = 3;
    [SerializeField] private DragActionComponent dragActionComponent;
    private readonly float screenWidth = 720f;
    private readonly float halfScreenWidth = 360f;

    [SerializeField] private float halfBgWidth = 102f;

    private void Awake()
    {
        dragActionComponent.DragBeginAction = DragBeginAction;
        dragActionComponent.DragMoveAction = DragMoveAction;
        dragActionComponent.DragEndAction = DragEndAction;
    }
    private void DragBeginAction(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isNeedAutoMove = false;
    }
    private void DragMoveAction(UnityEngine.EventSystems.PointerEventData eventData)
    {
        float deltaX = eventData.delta.x;
        if (deltaX < 0)
            deltaX = Utility.KeepInRange(deltaX, -200, 0);
        else
            deltaX = Utility.KeepInRange(deltaX, 0, 200);
        moveTotalDeltaX += deltaX;
        RefreshPosition(deltaX > 0);
    }
    private void DragEndAction(UnityEngine.EventSystems.PointerEventData eventData)
    {
        isNeedAutoMove = true;
    }

    private List<int> cardIdList = new List<int>();
    public void SetData(List<int> cardIdList)
    {
        this.cardIdList = cardIdList;
        ResetShow();
    }

    public void ResetShow()
    {
        if (cardIdList.Count == 0)
        {
            return;
        }
        if (backGroundCount == 0)
        {
            return;
        }
        isNeedAutoMove = false;
        moveTotalDeltaX = 0f;
        for (int i = 0; i < cardShowItems.Count; i++)
        {
            float moveX = -halfScreenWidth + itemWidth * i + itemSpace * i;
            cardShowItems[i].transform.localPosition = new Vector3(moveX, 0, 0);
            cardShowItems[i].SetData(i, moveX, i % backGroundCount, i, cardIdList);
        }
        isNeedAutoMove = true;
    }

    private bool isNeedAutoMove = false;
    [SerializeField] private float autoMoveSpeed = 10f;
    private float moveTotalDeltaX = 0f;
    void Update()
    {
        if (isNeedAutoMove)
        {
            moveTotalDeltaX += Time.deltaTime * autoMoveSpeed;
            RefreshPosition(autoMoveSpeed > 0);
        }
    }

    private void RefreshZIndex()
    {
        List<RotationChartCardShowItem> cardShowItemOrderList = cardShowItems.OrderBy(x => x.transform.localPosition.x).ToList();
        for (int i = 0; i < cardShowItemOrderList.Count; i++)
        {
            cardShowItemOrderList[i].transform.SetSiblingIndex(i);
        }
    }

    private void RefreshPosition(bool isMoveRight)
    {
        bool isOrderChange = false;
        for (int i = 0; i < cardShowItems.Count; i++)
        {
            RotationChartCardShowItem cardShowItem = cardShowItems[i];
            Transform cardTrans = cardShowItem.transform;
            if (isMoveRight)
            {
                float newX = cardShowItem.startX - (itemWidth + itemSpace) * cardShowItems.Count * cardShowItem.circleCount + moveTotalDeltaX;
                if (newX > halfScreenWidth + halfBgWidth)
                {
                    cardShowItem.circleCount++;
                    newX = cardShowItem.startX - (itemWidth + itemSpace) * cardShowItems.Count * cardShowItem.circleCount + moveTotalDeltaX;
                    int newDataIndex = cardShowItem.dataIndex - cardShowItems.Count;
                    for (int j = 0; j < 1000; j++)
                    {
                        if (newDataIndex < 0)
                        {
                            newDataIndex += cardIdList.Count;
                        }
                        else
                        {
                            break;
                        }
                    }
                    newDataIndex = newDataIndex % cardIdList.Count;
                    int newBgIndex = cardShowItem.bgIndex - cardShowItems.Count;
                    for (int j = 0; j < 1000; j++)
                    {
                        if (newBgIndex < 0)
                        {
                            newBgIndex += backGroundCount;
                        }
                        else
                        {
                            break;
                        }
                    }
                    newBgIndex = newBgIndex % backGroundCount;
                    cardShowItem.RefreshData(newBgIndex, newDataIndex);
                    isOrderChange = true;
                }
                cardTrans.localPosition = new Vector3(newX, 0, 0);
            }
            else
            {
                float newX = cardShowItem.startX - (itemWidth + itemSpace) * cardShowItems.Count * cardShowItem.circleCount + moveTotalDeltaX;
                if (newX < -halfScreenWidth - halfBgWidth)
                {
                    cardShowItem.circleCount--;
                    newX = cardShowItem.startX - (itemWidth + itemSpace) * cardShowItems.Count * cardShowItem.circleCount + moveTotalDeltaX;
                    int newDataIndex = cardShowItem.dataIndex + cardShowItems.Count;
                    for (int j = 0; j < 1000; j++)
                    {
                        if (newDataIndex < 0)
                        {
                            newDataIndex += cardIdList.Count;
                        }
                        else
                        {
                            break;
                        }
                    }
                    newDataIndex = newDataIndex % cardIdList.Count;
                    int newBgIndex = cardShowItem.bgIndex + cardShowItems.Count;
                    for (int j = 0; j < 1000; j++)
                    {
                        if (newBgIndex < 0)
                        {
                            newBgIndex += backGroundCount;
                        }
                        else
                        {
                            break;
                        }
                    }
                    newBgIndex = newBgIndex % backGroundCount;
                    cardShowItem.RefreshData(newBgIndex, newDataIndex);
                    isOrderChange = true;
                }
                cardTrans.localPosition = new Vector3(newX, 0, 0);
            }
        }
        if (isOrderChange) RefreshZIndex();
    }
}
