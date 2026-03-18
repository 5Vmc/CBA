using System.Collections;
using System.Collections.Generic;
using BigBang.UI;
using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

public class ChristmasTreeItem : MonoBehaviour
{
    [SerializeField] private BabuButton christmasTreeItem = null;
    [SerializeField] private List<Image> qualityBgImageList = new();
    [SerializeField] private Image lightBgImage = null;
    [SerializeField] private Image itemIconImage = null;
    [SerializeField] private TMP_Text itemNumText = null;
    [SerializeField] private TMP_Text itemNumDarkText = null;
    [SerializeField] private SkeletonGraphic lightSkeletonGraphic = null;

    private void OnEnable()
    {
        christmasTreeItem.OnClick += OnClickChristmasTreeItem;
        StartMove();
    }
    private void OnDisable()
    {
        christmasTreeItem.OnClick -= OnClickChristmasTreeItem;
        StopMove();
    }
    private void OnClickChristmasTreeItem(BabuButton _)
    {
        UIController.Instance.OpenWindow<ItemtipsUI>(new ItemtipsUIProperties(gameItem));
    }

    public GameItem gameItem = null;
    public void SetData(GameItem gameItem)
    {
        this.gameItem = gameItem;

        SetLight(false);

        RefreshData();
    }
    public async void RefreshData()
    {
        if (gameItem == null) return;
        itemNumText.text = "x" + gameItem.Count.ToString("N0");
        itemNumDarkText.text = "x" + gameItem.Count.ToString("N0");
        itemIconImage.sprite = await gameItem.GetIcon();
    }

    public bool isLight = false;
    public bool isEnd = false;
    public void SetLight(bool isLight,bool isEnd = false)
    {
        this.isEnd = isEnd;
        this.isLight = isLight;
        lightBgImage.gameObject.SetActive(isLight && !isEnd);
        lightSkeletonGraphic.gameObject.SetActive(isLight && isEnd);
        itemNumDarkText.gameObject.SetActive(isLight);
        itemNumText.gameObject.SetActive(!isLight);
        if (isLight)
        {
            foreach (var item in qualityBgImageList)
            {
                item.gameObject.SetActive(false);
            }
        }
        else
        {
            int quality = 1;
            if (gameItem != null) quality = gameItem.GetQuality();
            for (int i = 0; i < qualityBgImageList.Count; i++)
            {
                qualityBgImageList[i].gameObject.SetActive(i + 1 == quality);
            }
        }
    }

    private Sequence seq = null;
    public void StartMove()
    {
        seq = DOTween.Sequence();
        seq.AddTo(this.gameObject);
        seq.Append(this.transform.DOLocalRotate(new Vector3(0, 0, 10), 5f));
        seq.AppendInterval(0.1f);
        seq.Append(this.transform.DOLocalRotate(new Vector3(0, 0, -10), 5f));
        seq.AppendInterval(0.1f);
        seq.SetLoops(-1);
    }
    public void StopMove()
    {
        seq?.Kill();
        seq = null;
        this.transform.SetLocalRotationZ(-10);
    }

}
