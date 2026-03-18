namespace deVoid.UIFramework
{
    /// <summary>
    /// Base class for panels that need no special Properties
    /// </summary>
    public abstract class APanelController : APanelController<PanelProperties> { }

    /// <summary>
    /// Base class for Panels
    /// </summary>
    public abstract class APanelController<T> : AUIScreenController<T>, IPanelController where T : IPanelProperties
    {
        public PanelPriority Priority
        {
            get
            {
                if (Properties != null)
                {
                    return Properties.Priority;
                }
                else
                {
                    return PanelPriority.None;
                }
            }
        }

        protected sealed override void SetProperties(T props)
        {
            base.SetProperties(props);
        }

        protected override void HierarchyFixOnShow()
        {
            if (Properties == null)
            {
                transform.SetAsLastSibling();
            }
            else
            {
                switch (Properties.Sibling)
                {
                    case PanelSibling.First:
                        transform.SetAsFirstSibling();
                        break;
                    default:
                        transform.SetAsLastSibling();
                        break;
                }
            }
        }
    }
}
