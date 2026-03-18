using BigBang.Animation;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace BigBang.UI
{

    //战斗核心UI
    public class BattleResultPad : MonoBehaviour
    {
        private void OnEnable()
        {
            RegEvents();
        }
        private void OnDisable()
        {
            UnRegEvents();
        }
        #region 外部使用接口

        public Action onClickCloseButton;

        private bool isEventHasReg = false;
        public void RegEvents()
        {
            if (isEventHasReg == true) return;
            isEventHasReg = true;

            closeBtn.onClick.AddListener(OnClose);
        }
        public void UnRegEvents()
        {
            if (isEventHasReg == false) return;
            isEventHasReg = false;

            closeBtn.onClick.RemoveListener(OnClose);
        }
        #endregion

        #region 初始化

        public BattleResultPadAnim anim;

        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text PosText;
        [SerializeField] private TMP_Text LevelText;
        [SerializeField] private TMP_Text SuccessText;
        [SerializeField] private GameObject winBgRoot;
        [SerializeField] private GameObject loseBgRoot;
        [SerializeField] private Image blueBg = null;
        [SerializeField] private Image redBg = null;

        #endregion

        #region

        private void OnClose()
        {
            onClickCloseButton?.Invoke();
        }
        [SerializeField] private List<GameObject> StartList = new();
        private void SetLightStar(int starCount)
        {
            starCount = Utility.KeepInRange(starCount, 1, 3);
            for (int i = 0; i < StartList.Count; i++)
            {
                StartList[i].SetActive(i < starCount);
            }
        }

        public void SetData(bool isWin, string successStr, string mapNameStr, string mapLevelStr, int starCount)
        {
            blueBg.gameObject.SetActive(isWin);
            redBg.gameObject.SetActive(!isWin);
            winBgRoot.SetActive(isWin);
            loseBgRoot.SetActive(!isWin);
            PosText.text = mapNameStr;
            LevelText.text = mapLevelStr;
            SuccessText.text = successStr;
            SetLightStar(starCount);
        }

        #endregion

    }
}
