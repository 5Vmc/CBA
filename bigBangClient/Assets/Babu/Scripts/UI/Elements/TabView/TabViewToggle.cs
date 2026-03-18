using UnityEngine.UI;

namespace Babu.UI
{
    public class TabViewToggle : Toggle
    {
        StatusController _statusController;

        void Awake()
        {
            _statusController = GetComponent<StatusController>();
            onValueChanged.AddListener((show) =>
            {
                _statusController.SetStatus(show);
            });
        }
    }
}
