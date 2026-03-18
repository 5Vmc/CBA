using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using System.Text.RegularExpressions;
using System.Linq;
using Utils;
using Babu;
using System;

namespace BigBang.UI
{

    public class Settings2UI : AWindowController
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button cancelBtn;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button confirmBtn;
        protected override void AddListeners()
        {
            base.AddListeners();
            closeBtn.onClick.AddListener(OnClose);
            cancelBtn.onClick.AddListener(OnCancel);
            confirmBtn.onClick.AddListener(OnRequestReviseName);
        }

        protected override void RemoveListeners()
        {
            base.RemoveListeners();
            closeBtn.onClick.RemoveListener(OnClose);
            cancelBtn.onClick.RemoveListener(OnCancel);
            confirmBtn.onClick.RemoveListener(OnRequestReviseName);
        }

        //获取名字
        public string GetInputName()
        {
            return inputField.text;
        }

        //改名申请
        private void OnRequestReviseName()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            string newName = GetInputName();
            IllegalCharacter.IsNameCanNotUse(newName, false, (bool isCanNotUse) =>
            {
                if (isCanNotUse)
                {
                    
                }
                else
                {
                    NetworkManager.Instance.ReviseName(GetInputName(), response =>
                    {
                        Debug.Log("改名成功，当前名字 " + Player.Name);
                        Tips.PopTips("改名成功");
                        OnCancel();
                    });
                }
            });
        }

        //取消按钮
        private void OnCancel()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.CloseWindow<Settings2UI>();
        }

        //关闭窗口
        private void OnClose()
        {
            AudioManager.Instance.PlaySound(AudioNames.BTN_CLICK);
            UIController.Instance.CloseWindow<Settings2UI>();
        }
    }
}