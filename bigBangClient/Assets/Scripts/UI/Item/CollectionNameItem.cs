using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Utils;

namespace BigBang.UI
{
    public class CollectionNameItem : MonoBehaviour
    {
        [SerializeField] public RectTransform ItemTrans;
        [SerializeField] public TMP_Text NameText;
        [SerializeField] public TMP_Text HashPos;
        [SerializeField] public GameObject Owned;
        [SerializeField] public GameObject NotOwned;
        [SerializeField] public GameObject OwnedHash;
        [SerializeField] public GameObject NotOwnedHash;
    }
}