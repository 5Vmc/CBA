using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Babu.UI
{
    internal class TabView : ToggleGroup
    {
        [SerializeField] List<TabViewToggle> _tabViewToggles;
        [SerializeField] List<TabViewElement> _tabViewElements;
        [SerializeField] int _defaultSelectedIndex = 0;

        public event Action<int> OnToggleChanged;

        private int _curSelectedIndex = -1;

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < _tabViewToggles.Count; i++)
            {
                var toggle = _tabViewToggles[i];
                toggle.group = this;

                int index = i;
                toggle.onValueChanged.AddListener((v) =>
                {
                    if (v == true)
                    {
                        SwitchTo(index);
                    }
                });
            }

            for (int i = 0; i < _tabViewElements.Count; i++)
            {
                _tabViewElements[i].gameObject.SetActive(false);
            }
        }

        protected override void Start()
        {
            base.Start();
            _tabViewToggles[_defaultSelectedIndex].isOn = true;
        }

        public void SwitchTo(int index)
        {
            if (_curSelectedIndex == index)
            {
                return;
            }

            if (_curSelectedIndex != -1)
            {
                _tabViewElements[_curSelectedIndex].Hide();
            }

            _tabViewElements[index].Show(GetSelectArgs(index));
            _curSelectedIndex = index;

            OnToggleChanged?.Invoke(index);
        }

        protected object[] GetSelectArgs(int index)
        {
            return null;
        }
    }
}
