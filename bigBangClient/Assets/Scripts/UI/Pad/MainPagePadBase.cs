using BigBang;
using BigBang.Animation;
using BigBang.UI;
using Coffee.UIEffects;
using deVoid.UIFramework;
using DG.Tweening;
using GameConfig;
using GameConfig.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class MainPagePadBase : MonoBehaviour
{
    [SerializeField] public Button playerImage = null;
    [SerializeField] public BabuButton challengeButton = null;
    [SerializeField] public BabuButton helpButton = null;
    [SerializeField] public UIShiny challengeBtnUIShiny = null;
    [SerializeField] private PeakImage peakImage = null;

    /// <summary>
    /// 主页卡牌的 ID
    /// </summary>
    protected virtual int GetCardId()
    {
        return CardId.ChenGuoHao;
    }
    protected virtual void OnClickGoto()
    {

    }

    private void OnEnable()
    {
        if (playerImage != null) playerImage.onClick.AddListener(OnClickPlayerImage);
        if (challengeButton != null) challengeButton.OnClick += OnClickChallengeButton;
        if (helpButton != null) helpButton.OnClick += OnClickHelpButton;
    }
    private void OnDisable()
    {
        if (playerImage != null) playerImage.onClick.RemoveListener(OnClickPlayerImage);
        if (challengeButton != null) challengeButton.OnClick -= OnClickChallengeButton;
        if (helpButton != null) helpButton.OnClick -= OnClickHelpButton;
    }

    #region 屏幕适配

    private void Start()
    {
        ScreenFix();
    }

    private float challengeButtonY169 = 236f;
    private float challengeButtonY189 = 271f;
    private void ScreenFix()
    {
        float t = GetFixScreenLerpT();
        float fixY = Mathf.Lerp(challengeButtonY169, challengeButtonY189, t);
        challengeButton.GetComponent<RectTransform>().SetAnchoredPositionY(fixY);
    }

    /// <summary>
    /// 获取Lerp用的T值(适配后)
    /// 16:9为0，21:9为1，可能会超过0和1
    /// 请尽量使用此方法
    /// </summary>
    public float GetFixScreenLerpT()
    {
        float hw169 = 16.0f / 9.0f;
        float hw189 = 18.0f / 9.0f;
        float hwScreen = (float)UIFrame.height / (float)UIFrame.width;
        float screenT = (hwScreen - hw169) / (hw189 - hw169);
        return screenT;
    }

    #endregion

    private void OnClickHelpButton(BabuButton _)
    {
        ShowPlayerDetail();
    }
    private void OnClickPlayerImage()
    {
        ShowPlayerDetail();
    }
    private void OnClickChallengeButton(BabuButton _)
    {
        OnClickGoto();
    }
    private void ShowPlayerDetail()
    {
        UIController.Instance.OpenWindow<CardDetailUI>(new CardDetailProperties(GetCardId()));
    }
    protected CardModelConfig cardModelConfig = null;
    public void OnShow()
    {
        cardModelConfig = Configs.CardModel.GetConfig(GetCardId());
        if (cardModelConfig == null)
        {
            Debug.LogError("NewYearChallengePad , OnShow , cardModelConfig == null , cardId = " + GetCardId());
            return;
        }
        RefreshPlayerDetail();
        AfterShow();
        PlayEnterAnim();
    }
    protected virtual void AfterShow()
    {

    }
    [SerializeField] private TMP_Text nameText = null;
    [SerializeField] private TMP_Text numberText = null;
    [SerializeField] private TMP_Text positionText = null;
    private void RefreshPlayerDetail()
    {
        nameText.text = cardModelConfig.Name;
        numberText.text = cardModelConfig.Number.ToString();
        positionText.text = Configs.SeparatedPosition.GetConfig(cardModelConfig.AdaptPosition[0]).Abbreviation;
        peakImage?.SetData(cardModelConfig);
    }

    #region 入场动画

    [SerializeField] public Image bgImage = null;
    [SerializeField] public Image bgImageL = null;
    [SerializeField] public Image bgImageR = null;
    [SerializeField] public RectTransform tipPanel = null;
    [SerializeField] public RectTransform detailPanel = null;

    private float bgImageLStartRotateZ = 48f;
    private float bgImageLEndRotateZ = 0f;
    private float bgImageRStartRotateZ = 48f;
    private float bgImageREndRotateZ = 0f;
    [SerializeField] public Vector3 playerStartPos = new Vector3(-428f, -299f, 0);
    [SerializeField] public Vector3 playerEndPos = new Vector3(23.7f, 103.1f, 0);
    public Vector3 detailStartPos = new Vector3(445f, 584f, 0);
    public Vector3 detailEndPos = new Vector3(242.4f, 415.1544f, 0);
    private Vector3 tipStartPos = new Vector3(0f, -549f, 0);
    private Vector3 tipEndPos = new Vector3(0f, -170.44f, 0);

    private Sequence seq = null;
    private void PlayEnterAnim()
    {
        seq?.Kill();
        seq = DOTween.Sequence();
        bgImageL.transform.SetLocalRotationZ(bgImageLStartRotateZ);
        bgImageR.transform.SetLocalRotationZ(bgImageRStartRotateZ);
        tipPanel.gameObject.SetAlpha(0f);
        playerImage.gameObject.SetAlpha(0f);
        playerImage.transform.SetLocalPosition(playerStartPos);
        detailPanel.gameObject.SetAlpha(0f);
        detailPanel.transform.SetLocalPosition(detailStartPos);
        tipPanel.gameObject.SetAlpha(0f);
        tipPanel.transform.SetLocalPosition(tipStartPos);
        challengeButton.gameObject.SetAlpha(0f);
        challengeButton.transform.SetLocalScale(0);

        seq.Append(bgImageL.transform.DOLocalRotate(new Vector3(0, 0, bgImageLEndRotateZ), 0.3f));
        seq.Join(bgImageR.transform.DOLocalRotate(new Vector3(0, 0, bgImageREndRotateZ), 0.3f));
        seq.Append(playerImage.transform.DOLocalMove(playerEndPos, 0.3f));
        seq.Join(playerImage.gameObject.DOFade(1, 0.3f));
        seq.Join(detailPanel.transform.DOLocalMove(detailEndPos, 0.3f));
        seq.Join(detailPanel.gameObject.DOFade(1, 0.3f));
        seq.Join(tipPanel.transform.DOLocalMove(tipEndPos, 0.3f));
        seq.Join(tipPanel.gameObject.DOFade(1, 0.3f));
        seq.Append(challengeButton.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack));
        seq.Join(challengeButton.gameObject.DOFade(1, 0.5f));
    }

    #endregion

}
