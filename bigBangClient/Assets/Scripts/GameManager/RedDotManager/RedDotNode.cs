using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BigBang
{
    public class RedDotNode
    {
        private Dictionary<string, RedDotNode> childNodes = new Dictionary<string, RedDotNode>();
        private RedDotNode _parentNode;
        private int _count;
        private bool addeddotimg = false;
        public RedDotNode(RedDotNode parent)
        {
            _parentNode = parent;
            _count = 0;
        }

        public RedDotNode GetOrAddChild(string key)
        {
            if (childNodes.TryGetValue(key, out RedDotNode node))
            {
                return node;
            }
            var newNode = new RedDotNode(this);
            childNodes.Add(key, newNode);
            return newNode;
        }

        /// <summary>
        /// 增加小红点就+1，减少小红点就-1，内部值有判断，不会变化为-1.
        /// </summary>
        /// <param name="incre"></param>
        public void AddValue(int incre)
        {
            if (childNodes.Count == 0)
            {
                SetValue(incre);
            }
            else
            {
                if (_count + incre < 0)
                {
                    _parentNode?.AddValue(-_count);
                    _count = 0;
                }
                else
                {
                    _count += incre;
                    //纠错，红点计数不可能变化为 -1；
                    _parentNode?.AddValue(incre);
                }
            }
        }

        private void SetValue(int red)
        {
            int newValue = _count + red;
            if (newValue > 1) newValue = 1;
            if (newValue < 0) newValue = 0;
            if (newValue > _count)
            {
                _count = newValue;
                _parentNode?.AddValue(1);
            }
            else if (newValue < _count)
            {
                _count = newValue;
                _parentNode?.AddValue(-1);
            }
        }

        public bool IsRed(Transform obj)
        {
            //_count = 1;
            obj?.gameObject.SetActive(_count > 0);
            return _count > 0;
        }

        /// <summary>
        /// 算了，这个位置不好定，各个控件锚点可能都不一样，还是各个控件里自己放小红点。
        /// </summary>
        /// <param name="baseObj"></param>
        /// <returns></returns>
        //private bool IsRedWithPoint(RectTransform baseObj) {
        //    _count = 1;
        //    if (_count > 0) {
        //        if (!addeddotimg) { 
        //            var p = UnityEngine.Object.Instantiate(RedDotManager.Instance.dotImg, baseObj, false);
        //            p.name = "redpoint";
        //            Vector2 size = baseObj.rect.size;
        //            p.transform.localPosition = new Vector3(baseObj.rect.width - 10, baseObj.rect.height - 10, 0f);
        //            addeddotimg = true;
        //        }

        //        else {
        //            baseObj.Find("redpoint").gameObject.SetActive(false);
        //        }
        //    }
        //    return IsRed(baseObj);
        //}

        public int GetCount()
        {
            return _count;
        }

        public void ClearChilds() {
            _count = 0;
            childNodes.Clear();
        }

        public void Destroy()
        {
        }
    }
}