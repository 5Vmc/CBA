using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using TMPro;
using System;
using System.Collections.Generic;
using Utils;
using BigBang.Animation;
using DG.Tweening;

namespace BigBang.UI
{
    public class ChangeNumberUIProperties : WindowProperties
    {
        public PlayerCard Card { get; set; }
        public TMP_Text NumberText { get; set; }
        public ChangeNumberUIProperties(PlayerCard card, TMP_Text numberText)
        {
            Card = card;
            NumberText = numberText;
        }
    }

    public class ChangeNumberUI : AWindowController<ChangeNumberUIProperties>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button changeBtn;
        [SerializeField] private ScrollRect numberAdapter;
        [SerializeField] private Transform numberContent;
        [SerializeField] private TMP_Text numItemPrefab;
        [SerializeField] private ChangeNumberAnim anim;

        private List<int> numberList;
        private List<PlayerCard> cardList;
        private int clikNumber;
        protected override void AddListeners()
        {
            closeBtn.onClick.AddListener(OnClose);
            changeBtn.onClick.AddListener(OnClickChange);
            numberAdapter.onValueChanged.AddListener(OnNumberChanged);
        }

        protected override void RemoveListeners()
        {
            closeBtn.onClick.RemoveListener(OnClose);
            changeBtn.onClick.RemoveListener(OnClickChange);
            numberAdapter.onValueChanged.RemoveListener(OnNumberChanged);
        }

        

        protected override void OnPropertiesSet()
        {           
            numberContent.localPosition =new Vector3(0,-3008,0);//3074  -3138
            numberList = SetNumberData();
            cardList = Player.CardManager.GetCardList();
            base.OnPropertiesSet();
            //显示数字列表
            InstantiatePrefab();
            SetItems();
            InitNumberColor();

            anim.PlayEnter(() =>
            {
                TouchManager.Instance.EnableTouch();
            });
        }

        //初始化预制体
        public void InstantiatePrefab()
        {
            while(numberContent.childCount<numberList.Count+4)
            {
                var item = Instantiate(numItemPrefab, numberContent);
                item.gameObject.SetActive(true);
            }
        }
        //赋值内容
        private void SetItems()
        {
            numberContent.GetChild(0).GetChild(0).gameObject.SetActive(false);
            numberContent.GetChild(1).GetChild(0).gameObject.SetActive(false);
            numberContent.GetChild(101).GetChild(0).gameObject.SetActive(false);
            numberContent.GetChild(102).GetChild(0).gameObject.SetActive(false);
            for (int i = 0; i < numberList.Count; i++)
            {
                numberContent.GetChild(i+2).GetComponent<TMP_Text>().text = numberList[i].ToString();
                if (IsNumberUsed(numberList[i]))
                {
                    if (GetNumberCardName(numberList[i]).Length>12)
                    {
                        //超过六个字
                        string str = GetNumberCardName(numberList[i]).Substring(0,10);
                        numberContent.GetChild(i + 2).GetChild(0).GetComponent<TMP_Text>().text = str + "...";
                    }
                    else
                    {
                        //不超过六个字
                        numberContent.GetChild(i + 2).GetChild(0).GetComponent<TMP_Text>().text = GetNumberCardName(numberList[i]);
                    }
                    
                }
                else
                {
                    numberContent.GetChild(i + 2).GetChild(0).GetComponent<TMP_Text>().text = "";
                }
                //numberContent.GetChild(i).gameObject.SetActive(true);
            }
        }
        //初始化字体颜色
        public void InitNumberColor()
        {
            for (int i = 0; i < numberContent.childCount; i++)
            {
                ColorUtility.TryParseHtmlString("#95a1ab", out Color nowColor);
                numberContent.GetChild(i).GetComponent<TMP_Text>().color = nowColor;
                numberContent.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color = nowColor;
                if ((i-1) == GetMemberIndex())
                {
                    ColorUtility.TryParseHtmlString("#243745", out Color newColor);
                    numberContent.GetChild(i).GetComponent<TMP_Text>().color = newColor;
                    numberContent.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color = newColor;
                }
            }
        }
        //设置字体颜色
        private void OnNumberChanged(Vector2 arg0)
        {
            for (int i = 0; i < numberContent.childCount; i++)
            {
                ColorUtility.TryParseHtmlString("#95a1ab", out Color nowColor);
                numberContent.GetChild(i).GetComponent<TMP_Text>().color = nowColor;
                numberContent.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color = nowColor;
                if ((i-1) == GetMemberIndex())
                {
                    ColorUtility.TryParseHtmlString("#243745", out Color newColor);
                    numberContent.GetChild(i).GetComponent<TMP_Text>().color = newColor;
                    numberContent.GetChild(i).GetChild(0).GetComponent<TMP_Text>().color = newColor;
                }
            }
        }
        //获取第几个对象
        private int GetMemberIndex()
        {
            var centerPos = numberAdapter.transform.localPosition;
            var contentPos = numberContent.transform.localPosition;
            var distance = contentPos.y + 3138 + centerPos.y;//3074
            var index = Math.Ceiling(distance / 64);//+1
            return (int)index;
        }
        
        private List<int> SetNumberData()
        {
            var numberList = new List<int>();
            for (int i = 0; i < 99; i++)
            {
                numberList.Add(i+1);
            }
            return numberList;
        }

        private void OnClose()
        {
            anim.PlayExit(() =>
            {
                UIController.Instance.CloseWindow<ChangeNumberUI>();
            });

        }
        public void OnClickChange()
        {
            clikNumber = GetMemberIndex();
            //Debug.Log("111   "+clikNumber);
            OnChangeNumber(clikNumber);
        }
        //数字定位
        public void NumberPositioning()
        {
            clikNumber = GetMemberIndex();    
            var distanceCA = numberAdapter.transform.position.y - numberContent.GetChild(clikNumber).transform.position.y;
            Debug.Log("11   " + distanceCA + "  " + clikNumber);          
            var currentPos = numberContent.transform.position.y + distanceCA;
            numberContent.transform.DOMoveY(currentPos, 0.2f);
        }
        //监听手指不滚动
        public void OnLeaveFinger()
        {
        }
        public void OnChangeNumber(int numberValue)
        {
            if (IsNumberUsed(numberValue))
            {
                //存在号码对应球员 跳出弹框
                UIController.Instance.OpenWindow<ConfirmationBoxUI>(
                    new ConfirmationBoxUIProperties(Lang.Get(LangID.ChangeNumberConfirm).Replace("{numberValue}", numberValue.ToString()).Replace("{Name}", GetNumberCardName(numberValue)),
                    ()=>
                    {
                        ExchangeCardNumber(Properties.Card.CardId, GetNumberCardID(numberValue));
                        Babu.DelayTaskService.Instance.Run(this.gameObject, () =>
                            {
                                anim.PlayExit(()=>
                                {
                                    UIController.Instance.CloseWindow<ChangeNumberUI>();
                                });
                                
                            });
                        
                    }));

            }
            else
            {
                //不存在对应球员
                ChangeCardNumber(Properties.Card.CardId, numberValue);
                anim.PlayExit(() =>
                {
                    UIController.Instance.CloseWindow<ChangeNumberUI>();
                });
               
            }
        }
        //判断号码是否重复
        public bool IsNumberUsed(int numbervalue)
        {
            foreach(var item in cardList)
            {
                if(numbervalue == item.PlayerCardNumber)
                {
                    return true;
                }
            }
            return false;
        }
        //获取对应球员名称
        public string GetNumberCardName(int number)
        {
            var name = "";
            foreach (var item in cardList)
            {
                if(item.PlayerCardNumber == number)
                {
                    return name = PlayerCard.GetFullName(item.Config);
                }
            }
            return name;
        }
        //获取号码对应球员ID
        public int GetNumberCardID(int number)
        {
            var cardId = 0;
            foreach (var item in cardList)
            {
                if (item.PlayerCardNumber == number)
                {
                    return cardId = item.CardId;
                }
            }
            return cardId;
        }
        //选择号码交换
        private void ChangeCardNumber(int cardId, int num)
        {
            NetworkManager.Instance.ChangePlayerCardNumber(cardId, num,response =>
            {
                RefreshDetailsUI();
            });
        }
        //两个球员号码交换
        private void ExchangeCardNumber(int cardId1, int cardId2)
        {
            NetworkManager.Instance.ExchangePlayerCardNumber(cardId1, cardId2, response =>
            {
                RefreshDetailsUI();
            });
        }

        //刷新界面
        private void RefreshDetailsUI()
        {
            Properties.NumberText.text = Properties.Card.PlayerCardNumber.ToString();
        }
    }
}