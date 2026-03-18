using BigBang;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainTaskTabItem : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private RectTransform hinge;
    [SerializeField] private Image progressValue;
    [SerializeField] private Image tabImg;
    [SerializeField] private Sprite deselect;
    [SerializeField] private Sprite selected;
    [SerializeField] private Image background;
    [SerializeField] private Image redDot;

    [HideInInspector] public RenderTexture RenderTex;

    public MainTaskType Type;

    private void OnEnable()
    {
        RenderTex = RenderTexture.GetTemporary(150, 150, 24);
        camera.targetTexture = RenderTex;
        camera.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        RenderTexture.ReleaseTemporary(RenderTex);
        camera.gameObject.SetActive(false);
    }

    public void PlayAnim()
    {
        var angle = Random.Range(20, 40);
        hinge.transform.DOLocalRotate(new Vector3(angle, 0, 0), 0.2f, RotateMode.LocalAxisAdd).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            hinge.transform.DOLocalRotate(Vector3.zero, 0.4f).SetEase(Ease.InQuad);
        });
    }

    public void SetProgress(float progress)
    {
        progressValue.fillAmount = progress;
    }

    public void SetIcon(Sprite sprite)
    {
        tabImg.sprite = sprite;
    }

    // 选中状态
    public void Selected()
    {
        background.sprite = selected;
    }

    // 未选中状态
    public void Deselectd()
    {
        background.sprite = deselect;
    }

    public void ClaimTip(bool claim)
    {
        redDot.gameObject.SetActive(claim);
    }
}
