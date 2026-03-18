using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OtherPlayerTeamTrainItem : MonoBehaviour
{
    [SerializeField] public RectTransform otherPlayerTeamTrainItem = null;
    [SerializeField] public Image bgImage = null;
    [SerializeField] public Image iconImage = null;
    [SerializeField] public TMP_Text trainTitleText = null;
    [SerializeField] public TMP_Text trainNumText = null;
}
