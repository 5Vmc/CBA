using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace deVoid.UIFramework
{
    /// <summary>
    /// This is the centralized access point for all things UI.
    /// All your calls should be directed at this.
    /// </summary>
    public class UIFrame : MonoBehaviour
    {
        [Tooltip("Set this to false if you want to manually initialize this UI Frame.")]
        [SerializeField] private bool initializeOnAwake = true;

        private GameObject touchMaskLayer;
        private PanelUILayer panelLayer;
        private WindowUILayer windowLayer;

        private Canvas mainCanvas;
        private GraphicRaycaster graphicRaycaster;
        private Dictionary<string, bool> openingFlag = new Dictionary<string, bool>();
        private Dictionary<string, bool> closingFlag = new Dictionary<string, bool>();

        public Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();
        private Dictionary<string, AssetOperationHandle> _handleDict = new Dictionary<string, AssetOperationHandle>();

        /// <summary>
        /// The main canvas of this UI
        /// </summary>
        public Canvas MainCanvas
        {
            get
            {
                if (mainCanvas == null)
                {
                    mainCanvas = GetComponent<Canvas>();
                }

                return mainCanvas;
            }
        }

        /// <summary>
        /// The Camera being used by the Main UI Canvas
        /// </summary>
        public Camera UICamera
        {
            get { return MainCanvas.worldCamera; }
        }

        private void Awake()
        {
            if (initializeOnAwake)
            {
                Initialize();
            }
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// Initializes this UI Frame. Initialization consists of initializing both the Panel and Window layers.
        /// Although literally all the cases I've had to this day were covered by the "Window and Panel" approach,
        /// I made it virtual in case you ever need additional layers or other special initialization.
        /// </summary>
        public virtual void Initialize()
        {
            if (touchMaskLayer == null)
            {
                touchMaskLayer = transform.Find("TouchMaskLayer").gameObject;
                if (touchMaskLayer == null)
                {
                    Debug.LogError("[UI Frame] UI Frame lacks TouchMask Layer!");
                }
            }
            if (panelLayer == null)
            {
                panelLayer = gameObject.GetComponentInChildren<PanelUILayer>(true);
                if (panelLayer == null)
                {
                    Debug.LogError("[UI Frame] UI Frame lacks Panel Layer!");
                }
                else
                {
                    panelLayer.Initialize();
                }
            }

            if (windowLayer == null)
            {
                windowLayer = gameObject.GetComponentInChildren<WindowUILayer>(true);
                if (windowLayer == null)
                {
                    Debug.LogError("[UI Frame] UI Frame lacks Window Layer!");
                }
                else
                {
                    windowLayer.Initialize();
                    windowLayer.RequestScreenBlock += OnRequestScreenBlock;
                    windowLayer.RequestScreenUnblock += OnRequestScreenUnblock;
                }
            }

            graphicRaycaster = MainCanvas.GetComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// 按屏幕比例适配后，设置Layer缩放前的，屏幕宽度
        /// </summary>
        public static float width = 720f;
        /// <summary>
        /// 按屏幕比例适配后，设置Layer缩放前的，屏幕高度
        /// </summary>
        public static float height = 1280f;
        /// <summary>
        /// 按屏幕比例适配后，Layer的缩放
        /// </summary>
        public static float scale = 1f;

        private static Vector2 pivot = Vector2.one / 2;
        private static Vector3 localScale = Vector3.one;
        private static Vector2 anchorMax = Vector2.one;
        private static Vector2 anchorMin = Vector2.zero;
        private static Vector2 sizeDelta = new Vector2(720f, 1280f);
        private static Vector2 offsetMax = new Vector2(0, -200);

        private static bool isInitFitDataInited = false;
        public static void InitFitData()
        {
            if (isInitFitDataInited == true) return;
            isInitFitDataInited = true;

            CalculateFitData();
        }

        public static void CalculateFitData()
        {
            float maxScreenRate = 720f / 1280;
            float currentScreenRate = Screen.width * 1.0f / Screen.height;

            if (currentScreenRate > maxScreenRate)
            {
                pivot = new Vector2(0.5f, 0.5f);
                anchorMax = anchorMin = new Vector2(0.5f, 0.5f);
                sizeDelta = new Vector2(720f, 1280f);
                var newRate = Screen.height * 720 / 1280f / Screen.width;
                localScale = new Vector2(newRate, newRate);
                width = Screen.height * 720 / 1280f;
                height = Screen.height;
                scale = newRate;
            }
            else
            {
                width = Screen.width;
                height = Screen.height;
                scale = 1f;
            }
        }

        /// <summary>
        /// 适配ipad，微信
        /// </summary>
        public static void FitScreen(RectTransform trans)
        {
            InitFitData();

            float maxScreenRate = 720f / 1280;
            float currentScreenRate = Screen.width * 1.0f / Screen.height;

            if (currentScreenRate > maxScreenRate)
            {
                trans.pivot = pivot;
                trans.anchorMax = anchorMax;
                trans.anchorMin = anchorMin;
                trans.sizeDelta = sizeDelta;
                trans.localScale = localScale;
            }
        }
        /// <summary>
        /// UI屏幕坐标转3D屏幕坐标
        /// 当UI经过适配后，3D摄像机渲染的图只在UI的一部分上显示
        /// 不能直接使用屏幕坐标作为转换中间量，需经过这个函数的转换
        /// </summary>
        public static Vector3 ChangeUIScreenPointTo3DScreenPoint(Vector3 uiCameraScreenPoint)
        {
            return uiCameraScreenPoint + new Vector3(-(Screen.width - width) / 2, 0, 0);
        }
        /// <summary>
        /// 3D屏幕坐标转屏幕坐标
        /// 当UI经过适配后，3D摄像机渲染的图只在UI的一部分上显示
        /// 不能直接使用屏幕坐标作为转换中间量，需经过这个函数的转换
        /// </summary>
        public static Vector3 Change3DScreenPointToUIScreenPoint(Vector3 d3CameraScreenPoint)
        {
            return d3CameraScreenPoint - new Vector3(-(Screen.width - width) / 2, 0, 0);
        }
        /// <summary>
        /// 获得适配后的新的屏幕中心点，在UI相机的屏幕空间上的坐标
        /// </summary>
        public static Vector3 GetFixMidScreenPointInUiCamera()
        {
            return new Vector3(Screen.width / 2, (height * scale) / 2, 0);
        }
        /// <summary>
        /// 获取Lerp用的T值(适配后)
        /// 16:9为0，21:9为1，可能会超过0和1
        /// 请尽量使用此方法
        /// </summary>
        public static float GetFixScreenLerpT()
        {
            float hw169 = 16.0f / 9.0f;
            float hw219 = 21.0f / 9.0f;
            float hwScreen = (float)UIFrame.height / (float)UIFrame.width;
            float screenT = (hwScreen - hw169) / (hw219 - hw169);
            return screenT;
        }

        public void ReleaseAll()
        {
            foreach (var iter in _handleDict)
            {
                iter.Value.Release();
            }

            _handleDict.Clear();
            Prefabs.Clear();
        }

        /// <summary>
        /// Shows a panel by its id, passing no Properties.
        /// </summary>
        /// <param name="screenId">Panel Id</param>
        public void ShowPanel(string screenId)
        {
            panelLayer.ShowScreenById(screenId);
        }

        /// <summary>
        /// Shows a panel by its id, passing parameters.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        /// <param name="properties">Properties.</param>
        /// <typeparam name="T">The type of properties to be passed in.</typeparam>
        /// <seealso cref="IPanelProperties"/>
        public void ShowPanel<T>(string screenId, T properties) where T : IPanelProperties
        {
            panelLayer.ShowScreenById<T>(screenId, properties);
        }

        /// <summary>
        /// Hides the panel with the given id.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        public void HidePanel(string screenId)
        {
            if (openingFlag.ContainsKey(screenId))
            {
                closingFlag[screenId] = true;
            }

            if (panelLayer.IsScreenRegistered(screenId) == false)
            {
                return;
            }

            panelLayer.HideScreenById(screenId);
            //panelLayer.UnregisterScreen(screenId, Prefabs[screenId].GetComponent<IPanelController>());

            //_handleDict[screenId].Release();
            //_handleDict.Remove(screenId);
            //Prefabs.Remove(screenId);
        }

        /// <summary>
        /// Opens the Window with the given Id, with no Properties.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        public void OpenWindow(string screenId)
        {
            windowLayer.ShowScreenById(screenId);
        }

        /// <summary>
        /// Closes the Window with the given Id.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        public void CloseWindow(string screenId)
        {
            if (openingFlag.ContainsKey(screenId))
            {
                closingFlag[screenId] = true;
            }

            if (windowLayer.IsScreenRegistered(screenId) == false)
            {
                return;
            }

            windowLayer.HideScreenById(screenId);
            //windowLayer.UnregisterScreen(screenId, Prefabs[screenId].GetComponent<IWindowController>());

            //_handleDict[screenId].Release();
            //_handleDict.Remove(screenId);
            //Prefabs.Remove(screenId);
        }

        /// <summary>
        /// Closes the currently open window, if any is open
        /// </summary>
        public void CloseCurrentWindow()
        {
            if (windowLayer.CurrentWindow != null)
            {
                CloseWindow(windowLayer.CurrentWindow.ScreenId);
            }
        }

        /// <summary>
        /// Opens the Window with the given id, passing in Properties.
        /// </summary>
        /// <param name="screenId">Identifier.</param>
        /// <param name="properties">Properties.</param>
        /// <typeparam name="T">The type of properties to be passed in.</typeparam>
        /// <seealso cref="IWindowProperties"/>
        public void OpenWindow<T>(string screenId, T properties) where T : IWindowProperties
        {
            windowLayer.ShowScreenById<T>(screenId, properties);
        }

        public static readonly string PanelsPath = "Panels/";
        /// <summary>
        /// Searches for the given id among the Layers, opens the Screen if it finds it
        /// </summary>
        /// <param name="screenId">The Screen id.</param>
        public Task<bool> ShowScreen(string screenId, IScreenProperties property = null)
        {
            Debug.Log("ShowScreen " + screenId);

            Type type;
            if (IsScreenRegistered(screenId, out type))
            {
                if (type == typeof(IWindowController))
                {
                    OpenWindow(screenId, property as IWindowProperties);
                }
                else if (type == typeof(IPanelController))
                {
                    ShowPanel(screenId, property as IPanelProperties);
                }
                return Task.FromResult(true);
            }
            else
            {
#if !UNITY_WEBGL
                {
                    // 同步加载
                    var handle = YooAssets.LoadAssetSync<GameObject>(PanelsPath + screenId + ".prefab");
                    var screenInstance = handle.InstantiateSync();
                    Prefabs.Add(screenId, screenInstance);
                    _handleDict.Add(screenId, handle);
                    var screenController = screenInstance.GetComponent<IUIScreenController>();
                    if (screenController != null)
                    {
                        RegisterScreen(screenId, screenController, screenInstance.transform);
                        IWindowController window = screenController as IWindowController;
                        if (window != null)
                        {
                            OpenWindow(screenId, property as IWindowProperties);
                            return Task.FromResult(true);
                        }

                        IPanelController panel = screenController as IPanelController;
                        if (panel != null)
                        {
                            ShowPanel(screenId, property as IPanelProperties);
                            return Task.FromResult(true);
                        }
                    }
                    return Task.FromResult(true);
                }
#else
                {
                    if (!openingFlag.ContainsKey(screenId))
                    {
                        openingFlag.Add(screenId, false);
                    }

                    //防止重复加载
                    closingFlag[screenId] = false;
                    if (openingFlag[screenId])
                    {
                        return Task.FromResult(false);
                    }
                    openingFlag[screenId] = true;

                    TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

                    // 异步加载
                    var handle = YooAssets.LoadAssetAsync<GameObject>(PanelsPath + screenId + ".prefab");
                    handle.Completed += _ =>
                    {
                        openingFlag[screenId] = false;
                        if (closingFlag.TryGetValue(screenId, out var flag) && flag == true)
                        {
                            closingFlag[screenId] = false;
                            // 已经关闭
                            handle.Release();
                            tcs.SetResult(false);
                            return;
                        }
                        var screenInstance = handle.InstantiateSync();
                        Prefabs.Add(screenId, screenInstance);
                        _handleDict.Add(screenId, handle);
                        var screenController = screenInstance.GetComponent<IUIScreenController>();
                        if (screenController != null)
                        {
                            RegisterScreen(screenId, screenController, screenInstance.transform);
                            IWindowController window = screenController as IWindowController;
                            if (window != null)
                            {
                                OpenWindow(screenId, property as IWindowProperties);
                                tcs.SetResult(true);
                            }

                            IPanelController panel = screenController as IPanelController;
                            if (panel != null)
                            {
                                ShowPanel(screenId, property as IPanelProperties);
                                tcs.SetResult(true);
                            }
                        }
                    };
                    return tcs.Task;
                }
#endif
            }
        }

        public void ShowScreen(IUIScreenController controller, GameObject instance, IScreenProperties property = null)
        {
            string screenId = controller.GetType().Name;
            if (IsScreenRegistered(screenId) == false)
            {
                RegisterScreen(screenId, controller, instance.transform);
            }
            ShowScreen(screenId, property);
        }

        public void HideScreen(string screenId)
        {
            if (openingFlag.ContainsKey(screenId))
            {
                closingFlag[screenId] = true;
            }

            if (windowLayer.IsScreenRegistered(screenId))
            {
                CloseWindow(screenId);
                return;
            }

            if (panelLayer.IsScreenRegistered(screenId))
            {
                HidePanel(screenId);
                return;
            }
        }

        /// <summary>
        /// Registers a screen. If transform is passed, the layer will
        /// reparent it to itself. Screens can only be shown after they're registered.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <param name="screenTransform">Screen transform. If not null, will be reparented to proper layer</param>
        public void RegisterScreen(string screenId, IUIScreenController controller, Transform screenTransform)
        {
            IWindowController window = controller as IWindowController;
            if (window != null)
            {
                windowLayer.RegisterScreen(screenId, window);
                if (screenTransform != null)
                {
                    windowLayer.ReparentScreen(controller, screenTransform);
                }

                return;
            }

            IPanelController panel = controller as IPanelController;
            if (panel != null)
            {
                panelLayer.RegisterScreen(screenId, panel);
                if (screenTransform != null)
                {
                    panelLayer.ReparentScreen(controller, screenTransform);
                }
            }
        }

        /// <summary>
        /// Registers the panel. Panels can only be shown after they're registered.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TPanel">The Controller type.</typeparam>
        public void RegisterPanel<TPanel>(string screenId, TPanel controller) where TPanel : IPanelController
        {
            panelLayer.RegisterScreen(screenId, controller);
        }

        /// <summary>
        /// Unregisters the panel.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TPanel">The Controller type.</typeparam>
        public void UnregisterPanel<TPanel>(string screenId, TPanel controller) where TPanel : IPanelController
        {
            panelLayer.UnregisterScreen(screenId, controller);
        }

        /// <summary>
        /// Registers the Window. Windows can only be opened after they're registered.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TWindow">The Controller type.</typeparam>
        public void RegisterWindow<TWindow>(string screenId, TWindow controller) where TWindow : IWindowController
        {
            windowLayer.RegisterScreen(screenId, controller);
        }

        /// <summary>
        /// Unregisters the Window.
        /// </summary>
        /// <param name="screenId">Screen identifier.</param>
        /// <param name="controller">Controller.</param>
        /// <typeparam name="TWindow">The Controller type.</typeparam>
        public void UnregisterWindow<TWindow>(string screenId, TWindow controller) where TWindow : IWindowController
        {
            windowLayer.UnregisterScreen(screenId, controller);
        }

        /// <summary>
        /// Checks if a given Panel is open.
        /// </summary>
        /// <param name="panelId">Panel identifier.</param>
        public bool IsPanelOpen(string panelId)
        {
            return panelLayer.IsPanelVisible(panelId);
        }

        /// <summary>
        /// Hide all screens
        /// </summary>
        /// <param name="animate">Defines if screens should the screens animate out or not.</param>
        public void HideAll(bool animate = true)
        {
            CloseAllWindows(animate);
            HideAllPanels(animate);

            //ReleaseAll();
        }

        /// <summary>
        /// Hide all screens on the Panel Layer
        /// </summary>
        /// <param name="animate">Defines if screens should the screens animate out or not.</param>
        private void HideAllPanels(bool animate = true)
        {
            panelLayer.HideAll(animate);
            //panelLayer.UnregisterAllScreen();
        }

        /// <summary>
        /// Hide all screens in the Window Layer
        /// </summary>
        /// <param name="animate">Defines if screens should the screens animate out or not.</param>
        private void CloseAllWindows(bool animate = true)
        {
            windowLayer.HideAll(animate);
            //windowLayer.UnregisterAllScreen();
        }

        /// <summary>
        /// Checks if a given screen id is registered to either the Window or Panel layers
        /// </summary>
        /// <param name="screenId">The Id to check.</param>
        public bool IsScreenRegistered(string screenId)
        {
            if (windowLayer.IsScreenRegistered(screenId))
            {
                return true;
            }

            if (panelLayer.IsScreenRegistered(screenId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if a given screen id is registered to either the Window or Panel layers,
        /// also returning the screen type
        /// </summary>
        /// <param name="screenId">The Id to check.</param>
        /// <param name="type">The type of the screen.</param>
        public bool IsScreenRegistered(string screenId, out Type type)
        {
            if (windowLayer.IsScreenRegistered(screenId))
            {
                type = typeof(IWindowController);
                return true;
            }

            if (panelLayer.IsScreenRegistered(screenId))
            {
                type = typeof(IPanelController);
                return true;
            }

            type = null;
            return false;
        }

        private void OnRequestScreenBlock()
        {
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = false;
            }
        }

        private void OnRequestScreenUnblock()
        {
            if (graphicRaycaster != null)
            {
                graphicRaycaster.enabled = true;
            }
        }

        /// <summary>
        /// 是否显示一层透明图来屏蔽点击UI
        /// </summary>
        public bool IsTouchMaskShow
        {
            get
            {
                return touchMaskLayer.activeSelf;
            }
            set
            {
                touchMaskLayer.SetActive(value);
            }
        }

        public void DestroyScreen(IUIScreenController screen)
        {
            if (screen is IWindowController window)
            {
                windowLayer.Destroy(screen.ScreenId, window);
            }
            else if (screen is IPanelController panel)
            {
                panelLayer.Destroy(screen.ScreenId, panel);
            }

            Prefabs.Remove(screen.ScreenId);
            closingFlag.Remove(screen.ScreenId);
            openingFlag.Remove(screen.ScreenId);

            _handleDict[screen.ScreenId].Release();
            _handleDict.Remove(screen.ScreenId);
        }
    }
}
