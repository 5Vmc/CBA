using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Babu;

namespace deVoid.UIFramework {
    /// <summary>
    /// Base class for UI Layers. Layers implement custom logic
    /// for Screen types when opening, closing etc.
    /// </summary>
    public abstract class AUILayer<TScreen> : MonoBehaviour where TScreen : IUIScreenController {
        protected Dictionary<string, TScreen> registeredScreens;

        /// <summary>
        /// Shows a screen
        /// </summary>
        /// <param name="screen">The ScreenController to show</param>
        public abstract void ShowScreen(TScreen screen);

        /// <summary>
        /// Shows a screen passing in properties
        /// </summary>
        /// <param name="screen">The ScreenController to show</param>
        /// <param name="properties">The data payload</param>
        /// <typeparam name="TProps">The type of the data payload</typeparam>
        public abstract void ShowScreen<TProps>(TScreen screen, TProps properties) where TProps : IScreenProperties;

        /// <summary>
        /// Hides a screen
        /// </summary>
        /// <param name="screen">The ScreenController to be hidden</param>
        public abstract void HideScreen(TScreen screen);

        /// <summary>
        /// Initialize this layer
        /// </summary>
        public virtual void Initialize() {
            registeredScreens = new Dictionary<string, TScreen>();
            StartCoroutine(StartCheck());
            UIFrame.FitScreen(transform as RectTransform);
        }

        /// <summary>
        /// Reparents the screen to this Layer's transform
        /// </summary>
        /// <param name="controller">The screen controller</param>
        /// <param name="screenTransform">The Screen Transform</param>
        public virtual void ReparentScreen(IUIScreenController controller, Transform screenTransform) {
            screenTransform.SetParent(transform, false);
        }

        /// <summary>
        /// Register a ScreenController to a specific ScreenId
        /// </summary>
        /// <param name="screenId">Target ScreenId</param>
        /// <param name="controller">Screen Controller to be registered</param>
        public void RegisterScreen(string screenId, TScreen controller) {
            if (!registeredScreens.ContainsKey(screenId)) {
                ProcessScreenRegister(screenId, controller);
            }
            else {
                Debug.LogError("[AUILayerController] Screen controller already registered for id: " + screenId);
            }
        }

        /// <summary>
        /// Unregisters a given controller from a ScreenId
        /// </summary>
        /// <param name="screenId">The ScreenId</param>
        /// <param name="controller">The controller to be unregistered</param>
        public void UnregisterScreen(string screenId, TScreen controller) {
            if (registeredScreens.ContainsKey(screenId)) {
                ProcessScreenUnregister(screenId, controller);
            }
            else {
                Debug.LogError("[AUILayerController] Screen controller not registered for id: " + screenId);
            }
        }

        public void UnregisterAllScreen()
        {
            registeredScreens.Clear();
        }

        public void Destroy(string screenId, TScreen controller)
        {
            Debug.Log("Destroy: " + screenId);
            UnregisterScreen(screenId, controller);
            controller.Destroy();
        }

        /// <summary>
        /// Attempts to find a registered screen that matches the id
        /// and shows it.
        /// </summary>
        /// <param name="screenId">The desired ScreenId</param>
        public void ShowScreenById(string screenId) {
            TScreen ctl;
            if (registeredScreens.TryGetValue(screenId, out ctl)) {
                ShowScreen(ctl);
            }
            else {
                Debug.LogError("[AUILayerController] Screen ID " + screenId + " not registered to this layer!");
            }
        }

        /// <summary>
        /// Attempts to find a registered screen that matches the id
        /// and shows it, passing a data payload.
        /// </summary>
        /// <param name="screenId">The Screen Id (by default, it's the name of the Prefab)</param>
        /// <param name="properties">The data payload for this screen to use</param>
        /// <typeparam name="TProps">The type of the Properties class this screen uses</typeparam>
        public void ShowScreenById<TProps>(string screenId, TProps properties) where TProps : IScreenProperties {
            TScreen ctl;
            if (registeredScreens.TryGetValue(screenId, out ctl)) {
                ShowScreen(ctl, properties);
            }
            else {
                Debug.LogError("[AUILayerController] Screen ID " + screenId + " not registered!");
            }
        }

        /// <summary>
        /// Attempts to find a registered screen that matches the id
        /// and hides it
        /// </summary>
        /// <param name="screenId">The id for this screen (by default, it's the name of the Prefab)</param>
        public void HideScreenById(string screenId) {
            TScreen ctl;
            if (registeredScreens.TryGetValue(screenId, out ctl)) {
                HideScreen(ctl);
            }
            else {
                //Debug.LogError("[AUILayerController] Could not hide Screen ID " + screenId + " as it is not registered to this layer!");
            }
        }

        /// <summary>
        /// Checks if a screen is registered to this UI Layer
        /// </summary>
        /// <param name="screenId">The Screen Id (by default, it's the name of the Prefab)</param>
        /// <returns>True if screen is registered, false if not</returns>
        public bool IsScreenRegistered(string screenId) {
            return registeredScreens.ContainsKey(screenId);
        }
        
        /// <summary>
        /// Hides all screens registered to this layer
        /// </summary>
        /// <param name="shouldAnimateWhenHiding">Should the screen animate while hiding?</param>
        public virtual void HideAll(bool shouldAnimateWhenHiding = true) {
            foreach (var screen in registeredScreens) {
                screen.Value.Hide(shouldAnimateWhenHiding);
            }
        }

        protected virtual void ProcessScreenRegister(string screenId, TScreen controller) {
            controller.ScreenId = screenId;
            registeredScreens.Add(screenId, controller);
            controller.ScreenDestroyed += OnScreenDestroyed;
        }

        protected virtual void ProcessScreenUnregister(string screenId, TScreen controller) {
            controller.ScreenDestroyed -= OnScreenDestroyed;
            registeredScreens.Remove(screenId);
        }

        private void OnScreenDestroyed(IUIScreenController screen) {
            if (!string.IsNullOrEmpty(screen.ScreenId)
                && registeredScreens.ContainsKey(screen.ScreenId)) {
                UnregisterScreen(screen.ScreenId, (TScreen) screen);
            }
        }

        private List<TScreen> _destroyScreenList = new List<TScreen>();
        private IEnumerator StartCheck()
        {
            yield return null;
            while (true)
            {
                if (UIController.Instance.CheckPanelInterval > 0)
                {
                    yield return new WaitForSeconds(UIController.Instance.CheckPanelInterval);
                }
                else
                {
                    yield return null;
                }

                long now = TimeUtils.NowEx();
                _destroyScreenList.Clear();
                foreach (var iter in registeredScreens)
                {
                    var screen = iter.Value;
                    if (screen.IsVisible == false && screen.LastCloseTime + UIController.Instance.ClosedPanelDestroyTime * 1000 < now)
                    {
                        _destroyScreenList.Add(screen);
                    }
                }

                for (int i = 0; i < _destroyScreenList.Count; i++)
                {
                    UIController.Instance.DestroyScreen(_destroyScreenList[i]);
                }
            }
        }
    }
}
