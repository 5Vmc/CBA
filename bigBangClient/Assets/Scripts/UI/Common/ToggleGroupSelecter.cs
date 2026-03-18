using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BigBang.UI
{
    public class ToggleGroupSelecter : MonoBehaviour
    {
        [SerializeField] private List<ToggleWithValue> _toggles;
        public List<ToggleWithValue> ToggleList
        {
            get
            {
                return _toggles;
            }
        }

        private ISelectListener _listener;

        private int _selectValue = 0;

        public ISelectListener Listener
        {
            set => _listener = value;
        }

        public UnityEvent<int> onValueChange = new UnityEvent<int>();
        private void OnEnable()
        {
            for (int i = 0; i < _toggles.Count; i++)
            {
                var toggle = _toggles[i];
                toggle.Toggle.onValueChanged.AddListener(flag =>
                {
                    toggle.SetStatus(flag);
                    if (flag)
                    {
                        DoSelect(toggle.Value);
                    }
                });
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i].Toggle.onValueChanged.RemoveAllListeners();
            }
        }

        private void DoSelect(int value)
        {
            if (_selectValue == value) return;
            _selectValue = value;
            _listener?.SelectOne(value);
            onValueChange?.Invoke(value);
        }

        public void SetValueSelected(int value)
        {
            _selectValue = value;
            for (int i = 0; i < _toggles.Count; i++)
            {
                _toggles[i].Toggle.SetIsOnWithoutNotify(_toggles[i].Value == value); 
                _toggles[i].SetStatus(_toggles[i].Value == value);
            }
        }

        public interface ISelectListener
        {
            void SelectOne(int value);
        }
    }
}