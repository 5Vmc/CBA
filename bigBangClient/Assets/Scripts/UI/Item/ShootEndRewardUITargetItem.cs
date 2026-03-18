using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Utils.GameItem;

namespace BigBang.UI
{
    public class ShootEndRewardUITargetItem : MonoBehaviour
    {
        [SerializeField] public GameObject falsePanel;
        [SerializeField] public GameObject truePanel;
        [SerializeField] public TMP_Text targetText;
    }
}
