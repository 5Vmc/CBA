using Babu;
using UnityEngine;

namespace deVoid.UIFramework
{
    /// <summary>
    /// Properties common to all panels
    /// </summary>
    [System.Serializable]
    public class PanelProperties : IPanelProperties
    {
        [SerializeField]
        [Tooltip("Panels go to different para-layers depending on their priority. You can set up para-layers in the Panel Layer.")]
        private PanelPriority priority;
        private string _modulepath;

        public PanelProperties() {
        }

        public PanelPriority Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        [SerializeField]
        [Tooltip("Move the transform to the start/end of the local transform list.")]
        private PanelSibling sibling;

        public PanelSibling Sibling
        {
            get { return sibling; }
            set { sibling = value; }
        }
    }
}
