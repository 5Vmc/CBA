using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FingerMove : MonoBehaviour
{
    [SerializeField] private RectTransform fingerImageRect = null;
    [SerializeField] private Vector2 startPosition = new Vector2(0, 0);
    [SerializeField] private Vector2 endPosition = new Vector2(40, -40);
    [SerializeField] private float moveTime = 0.5f;

    private Sequence fingerMoveSequence = null;
    private void OnEnable()
    {
        StopfingerMove();
        fingerImageRect.localPosition = startPosition;
        fingerMoveSequence = DOTween.Sequence();
        fingerMoveSequence.Append(fingerImageRect.DOLocalMove(endPosition, moveTime));
        fingerMoveSequence.Append(fingerImageRect.DOLocalMove(startPosition, moveTime));
        fingerMoveSequence.SetLoops(-1);
    }

    private void OnDisable()
    {
        StopfingerMove();
    }

    private void StopfingerMove()
    {
        fingerMoveSequence?.Kill();
        fingerMoveSequence = null;
    }
}
