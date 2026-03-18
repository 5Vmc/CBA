//using System.Collections.Generic;
//using GameConfig.Config;
//using UnityEngine;
//using Utils;

//namespace BigBang.UI
//{
//    public class HomeUICenterItem : MonoBehaviour
//    {
//        [SerializeField] private PageView pageView;
//        [SerializeField] private HomeActivityItem prefab;
//        [SerializeField] private RectTransform rect;

//        List<ActivityConfig> listConfigNow = new();
//        public void LoadActivity()
//        {
//            List<ActivityConfig> list = ActivityController.Instance.GetHomeUIActivity();

//            bool isSame = listConfigNow.Count == list.Count;
//            if (isSame)
//            {
//                for (int i = 0; i < listConfigNow.Count; i++)
//                {
//                    if (listConfigNow[i].Id != list[i].Id)
//                    {
//                        isSame = false;
//                        break;
//                    }
//                }
//            }
//            if (isSame)
//            {
//                RefreshActivityList();
//            }
//            else
//            {
//                listConfigNow = list;
//                RebuildActivityList();
//            }
//        }

//        List<HomeActivityItem> homeActivityItemList = new();
//        private void RebuildActivityList()
//        {
//            List<HomeActivityItem> homeActivityItemListNew = new();

//            var maxCount = Mathf.Max(homeActivityItemList.Count, listConfigNow.Count);

//            for (var index = 0; index < maxCount; index++)
//            {
//                HomeActivityItem item;
//                if (index >= homeActivityItemList.Count)
//                {
//                    item = Instantiate(prefab, pageView.transform);
//                    item.GetComponent<RectTransform>().SetLocalPosition(Vector3.zero);
//                }
//                else
//                {
//                    item = homeActivityItemList[index];
//                }

//                if (index >= listConfigNow.Count)
//                {
//                    item.gameObject.SetActive(false);
//                    item.RemoveListener();
//                    GameObject.DestroyImmediate(item.gameObject);
//                }
//                else
//                {
//                    item.SetData(listConfigNow[index]);
//                    item.rectTransform.SetSizeDeltaHeight(rect.sizeDelta.y);
//                    item.AddListeners();
//                    homeActivityItemListNew.Add(item);
//                }
//            }
//            homeActivityItemList = homeActivityItemListNew;
//            pageView.ResetCount();
//        }
//        private void RefreshActivityList()
//        {
//            foreach (HomeActivityItem item in homeActivityItemList)
//            {
//                item.UpdateUI();
//            }
//        }

//    }
//}