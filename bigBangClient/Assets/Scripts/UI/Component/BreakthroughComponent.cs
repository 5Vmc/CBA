using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigBang.UI
{
    public class BreakthroughComponent : MonoBehaviour
    {
        public TMP_Text NameText;
        public TMP_Text LevelText;
        public Button CloseBtn;
        public RectTransform Boxing;
        public GameObject Title;
        public Image Background;
        public List<TMP_Text> Txts;
        public Image YellowBoxing;
        public Image HighlightImage;
        public Image Ghost1;
        public Image Ghost2;
        public Image FlashBoard;
        public ParticleSystem Particle;

        public Image Image0;
        public Image Image1;

        public TMP_Text NameText1;

        public List<TMP_Text> Txts1;

        public List<GameObject> TextsGroup;
        public GameObject Texts1;
    }
}
