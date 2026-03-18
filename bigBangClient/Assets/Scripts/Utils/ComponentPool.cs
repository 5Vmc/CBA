using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using UnityEngine;


namespace Utils
{
    public class ComponentPool<T> where T : UnityEngine.Component
    {
        private int initCount = 5;
        private Transform parentNode = null;
        private readonly Stack<T> componentPool = new();
        private GameObject nodePrefab = null;
        private readonly HashSet<T> outComponentSet = new();
        private Action<T> initOneCallBack = null;

        /// <summary>
        /// 从对象池中获取node的component
        /// </summary>
        /// <returns>component</returns>
        public T GetComponentFormPool()
        {
            T t = null;
            if (this.componentPool.Count > 0)
            {
                t = this.componentPool.Pop();
                if (t == null)
                {
                    Debug.LogError("Error : getComponentFormPool t pop null ");
                    return null;
                }
            }
            else
            {
                t = this.CreateNewComponent();
            }
            t.gameObject.SetActive(true);
            this.outComponentSet.Add(t);
            return t;
        }

        /// <summary>
        /// 将component还给对象池
        /// </summary>
        /// <param name="t">要返还的对象</param>
        /// <param name="check">检查后再返还</param>
        /// <returns>成功</returns>
        public bool ReturnComponentToPool(T t, bool check = true)
        {
            if (check == true && this.outComponentSet.Contains(t) == false)
            {
                return false;//防止重复归还造成错误
            }
            t.transform.DOKill();
            if (t.transform.parent != this.parentNode)
            {
                t.transform.SetParent(this.parentNode);
            }
            t.gameObject.SetActive(false);
            if (this.outComponentSet.Contains(t) == true)
            {
                this.outComponentSet.Remove(t);
            }
            this.componentPool.Push(t);
            return true;
        }
        /// <summary>
        /// 检查对象池是否使用过
        /// </summary>
        public bool IsInited()
        {
            if (this.componentPool.Count > 0) return true;
            if (this.outComponentSet.Count > 0) return true;
            if (this.nodePrefab != null && this.parentNode != null) return true;
            return false;
        }

        /// <summary>
        /// 初始化对象池
        /// </summary>
        /// <param name="nodePrefab">要复制的预制体</param>
        /// <param name="initCount">提前创建多少对象</param>
        /// <param name="parentNode">创建后挂到哪个节点下</param>
        /// <param name="initOneCallBack">节点创建回调（每个节点）</param>
        /// <returns>此对象池</returns>
        public ComponentPool<T> InitComponentPool(GameObject nodePrefab, int initCount, Transform parentNode, Action<T> initOneCallBack = null)
        {
            if (this.IsInited() == true)
            {
                Debug.LogError("Error : initNodePool , Do not reinit ");
                return this;
            }
            if (nodePrefab == null)
            {
                Debug.LogError("Error : initComponentPool nodePrefab is null ");
                return null;
            }
            this.nodePrefab = nodePrefab;
            if (initCount < 0)
            {
                Debug.LogError("Error : initComponentPool InitCount < 0 ");
                return null;
            }
            this.initCount = initCount;
            if (parentNode == null)
            {
                Debug.LogError("Error : initComponentPool parentNode is null ");
                return null;
            }
            this.parentNode = parentNode;
            this.initOneCallBack = initOneCallBack;
            for (int i = 0; i < this.initCount; i++)
            {
                T t = this.CreateNewComponent();
                this.ReturnComponentToPool(t, false);
            }
            return this;
        }
        private T CreateNewComponent()
        {
            GameObject node = GameObject.Instantiate(this.nodePrefab, this.parentNode);
            if (node == null)
            {
                Debug.LogError("Error : createNewComponent instantiate node null ");
                return null;
            }
            T t = node.transform.GetComponent<T>();
            if (t == null)
            {
                Debug.LogError("Error : createNewComponent getComponent null className = " + node.name);
                return null;
            }
            initOneCallBack?.Invoke(t);
            return t;
        }

        /// <summary>
        /// 将在外的对象归还到对象池
        /// </summary>
        public void ClearOutComponent()
        {
            HashSet<T> swapSet = new(this.outComponentSet);
            foreach (T outComponent in swapSet)
            {
                this.ReturnComponentToPool(outComponent);
            }
            this.outComponentSet.Clear();
        }

        /// <summary>
        /// 返回所有在外的对象
        /// 仅供遍历使用，请勿对其进行修改
        /// </summary>
        /// <returns>所有在外的对象</returns>
        public HashSet<T> GetOutComponentSet()
        {
            return this.outComponentSet;
        }

        /// <summary>
        /// 销毁所有内外节点
        /// 并将对象池恢复到未初始化的状态
        /// </summary>
        public void DestoryAll()
        {
            ClearOutComponent();
            while (componentPool.Count > 0)
            {
                GameObject.Destroy(componentPool.Pop().gameObject);
            }
            this.componentPool.Clear();
            this.nodePrefab = null;
            this.parentNode = null;
            this.initOneCallBack = null;
        }
    }
}