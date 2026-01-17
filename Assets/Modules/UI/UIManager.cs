using System;
using System.Collections.Generic;
using LowoUN.Util;
using UnityEngine;
using UnityEngine.Events;

namespace LowoUN.Module.UI {
    public enum UICameraType : byte {
        NONE = 0,
        SS_Overlay, // 屏幕空间-覆盖
        SS_Camera, // 屏幕空间-相机深度
        WS // 世界空间
    }

    public class UIManager : SingletonSimple<UIManager> {
        public Camera _uiCamera { private set; get; }
        public void SetUICamera (Camera cam) { _uiCamera = cam; }

        public Canvas _camCanvas { private set; get; }
        public void SetCamCanvas (Canvas can) { _camCanvas = can; }

        public Canvas _worldCanvas { private set; get; }
        public void SetWorldCanvas (Canvas can) { _worldCanvas = can; }

        public Canvas _screenCanvas;
        Transform hudLayer;
        Transform floorLayer;
        Transform midLayer;
        Transform topLayer;
        public Transform ParentLayer_SceneHud => hudLayer;
        public Transform ParentLayer_HudNewAdd => floorLayer;
        public Transform ParentLayer_Popup => midLayer;
        public Transform ParentLayer_GlobalNotice => topLayer;

        void SetHudUI_Show () {
            ParentLayer_SceneHud.gameObject.SetActive (true);
        }
        void SetHudUI_Hide () {
            ParentLayer_SceneHud.gameObject.SetActive (false);
        }

        // viewbase 目前 指可能是一种资源名 对应一个界面实例，不存在一对多的关系，所以以资源名作为Key是合理的
        public Dictionary<string, UIViewBase> ViewMap => viewMap;
        private Dictionary<string, UIViewBase> viewMap = new Dictionary<string, UIViewBase> ();
        // private UITipManager tipManager;

        
        public void Init (Action cb_init) {
            Debug.Log ("------ UIManager --> Init");
            UIRootController.Self.Init();
            Debug.Log ("UIRootController.Self.Init Over.");

            // if (tipManager == null)
            //     tipManager = new UITipManager();

            var uIRootView = UIRootController.Self.GetUIRootView ();
            topLayer = uIRootView.TopLayer;
            midLayer = uIRootView.MidLayer;
            floorLayer = uIRootView.FloorLayer;
            hudLayer = uIRootView.HUDRoot;

            cb_init.Invoke();
        }

        public bool CheckIfExist (string view_name) {
            return viewMap.ContainsKey (view_name);
        }

        /// <summary>
        /// 处理+-UI界面
        /// </summary>
        // 老的 ifCloseScreenCanvas 决定 ParentLayer_SceneHud 这一层UI是否隐藏，对于popup的全屏界面来说，不管是Lobby还是Battle场景的hudUI应该适用同样的规则
        public void CreateUIViewAsync_Hud<T> (string viewName, UICameraType cameraType, Action<UIViewBase> done = null) where T : UIViewBase {
            if (cameraType == UICameraType.WS) {
                Debug.LogError ("UIManager -- CreateUIViewAsync_Hud -- cameraType must not be WS type.");
                return;
            }
            Transform root = null;
            if (cameraType == UICameraType.SS_Overlay) root = ParentLayer_SceneHud;
            else if (cameraType == UICameraType.SS_Camera) root = _camCanvas.transform;
            CreateUIViewAsync_Base<T> (viewName, root, done, true);
        }
        // , bool ifCloseScreenCanvas = false
        public void CreateUIViewAsync<T> (string viewName, Transform parent, Action<UIViewBase> done = null) where T : UIViewBase {
            CreateUIViewAsync_Base<T> (viewName, parent, done, false);
        }
        // public void CreateUIViewAsync_Base<T>(string viewName, Transform parent, Action<ViewBase> done, bool ifCloseScreenCanvas) where T : ViewBase
        void CreateUIViewAsync_Base<T> (string viewName, Transform parent, Action<UIViewBase> done, bool isHudUI) where T : UIViewBase {
            if (!viewName.IsValid ()) {
                Debug.LogError ("UIManager -- CreateUIViewAsync -- viewName is IsNullOrEmpty");
                return;
            }

            void HandleValidView (UIViewBase view_show) {
                done?.Invoke (view_show);
                if (view_show.IsFullScreen) {
                    Log.TEST ($"view:{viewName}, IsFullScreen:{view_show.IsFullScreen}");
                    EndOrDisEndMainCameraRender (false, isHudUI);
                    if (!isHudUI)
                        ScreenCanvas_Disable ();
                }
            }

            // 如果已存在，则显示 并返回实例对象
            if (viewMap.ContainsKey (viewName)) {
                if (viewMap[viewName] == null || !viewMap[viewName].ToString ().IsValid ()) {
                    viewMap.Remove (viewName);
                } else {
                    var view_show = ShowViewNew (viewName);
                    HandleValidView (view_show);
                    return;
                }
            }

            // 否则加载
            var prefab = Resources.Load<GameObject> (viewName);
            // ResUtils.LoadAsync<GameObject>(viewName, (prefab) =>
            // {
            // if(!GameController.Self.OldSceneEndAndNewSceneLoad_Start) {
            //     #if UNITY_EDITOR || !SRV_ALIYUN_PRODUCTION
            //     Debug.LogError($"[临时测试] ResUtils.LoadAsync -- !GameController.Self.OldSceneEndAndNewSceneLoad_Start viewName:{viewName}");
            //     #endif
            //     // done?.Invoke(null);
            //     return;
            // }

            if (prefab == null) {
                Debug.LogError ($"no prefab with name: {viewName}");
                done?.Invoke (null);
                return;
            }

            var view = LoadedUIView<T> (viewName, prefab, parent);
            if (view != null)
                HandleValidView (view);
            // });
        }

        T LoadedUIView<T> (string viewName, GameObject prefab, Transform parent) where T : UIViewBase {
            var viewObj = GameObject.Instantiate (prefab, parent);
            string viewKey = null;
            var component = viewObj.GetComponent (typeof (UIViewBase));
            if (component != null)
                viewKey = component.GetType ().Name;
            else {
                Debug.LogError ($"====> View预制体{viewName}未挂载继承 ViewBase 接口");
                viewKey = viewName;
            }

            T view = viewObj.GetComponent<T> ();
            if (view == null) {
                Debug.LogError ($"viewObj:{viewObj.name} has unmatched script: {typeof(T).Name}");
                return null;
            }

            if (viewMap.ContainsKey (viewKey)) {
                Log.Orange ("ui", "should not exist two same type class UI instances");
                GameObject.Destroy (viewMap[viewKey]);
                viewMap.Remove (viewKey);
            }

            viewMap.Add (viewKey, view); //typeof(T).Name
            Log.Print ($"ui ___ HandleUI AddUI CreateUIViewAsync(), viewKey:{viewKey}, view prefab:{viewObj}, <T> name: {typeof(T).Name}");

            return view;
        }
        UIViewBase ShowViewNew (string viewName) {
            if (viewMap.ContainsKey (viewName)) {
                viewMap[viewName].Show ();
                return viewMap[viewName];
            }

            Log.Orange ("ui", $"ShowUIViewNew ====> not found {viewName} view");
            return null;
        }

        // viewBase 关闭接口，只能通过这个
        // 检测是否开启渲染3D场景与角色，只要打开的UI界面中，又至少一个全屏类型的UI，则不开启3D场景角色渲染，必须没有全屏类型的界面才开启!!!
        public void OnReleaseViewNew2 (GameObject go, Action done = null) {
            // TODO loywong 除了 _camCanvas.transform 应该还保包括 midLayer中的popup类型的Panels也需要检测

            // Debug.LogError($"UIManager -- OnReleaseViewNew2 -- ifCanvasShow:{ifCanvasShow}");
            if (_screenCanvas != null && _screenCanvas.gameObject != null && !ifCanvasShow) // && _screenCanvas.gameObject.GetComponent<CanvasGroup>().alpha == 0
            {
                bool isHaveActiveChild = false;
                for (int i = 0; i < _camCanvas.transform.childCount; i++) {
                    Transform childTransform = _camCanvas.transform.GetChild (i);
                    UIViewBase viewBase = childTransform.GetComponent<UIViewBase> ();
                    // Debug.LogError($"UIManager -- OnReleaseViewNew2 -- viewBase name:{viewBase.name} viewBase.IsFullScreen:{viewBase.IsFullScreen}");
                    if (childTransform.gameObject != go && childTransform.gameObject.activeSelf == true && viewBase != null && viewBase.IsFullScreen == true) {
                        isHaveActiveChild = true;
                        break;
                    }
                }
                if (!isHaveActiveChild)
                    ScreenCanvas_Enable ();
            }

            if (go == null) {
                Log.Red ("ui", "HandleUI RmvUI Destroy failed, go is null!!!");
                return;
            }

            bool RmvViewBaseObjectCheck (string viewKey) {
                if (viewMap.ContainsKey (viewKey)) {
                    Log.Green ("ui", $"HandleUI RmvUI Succ, by OnReleaseViewNew(), prefab:{viewKey}");
                    var view = viewMap[viewKey];
                    viewMap.Remove (viewKey);
                    view.ClearDataAction?.Invoke (go);
                    view.Release ();
                    return true;
                } else {
                    Log.Trace ("ui", $"viewMap has no view:{viewKey}");
                    return false;
                }
            }

            string viewName = go.name;
            viewName = viewName.Replace ("(Clone)", "");
            if (!RmvViewBaseObjectCheck (viewName)) {
                var component = go.GetComponent (typeof (UIViewBase));
                if (component != null) {
                    viewName = component.GetType ().Name;
                    RmvViewBaseObjectCheck (viewName);
                } else
                    Debug.LogError ($"====> View预制体{viewName}未挂载继承 ViewBase 接口");
            }

            // Facade.Events.Notify(CustomEvents.CloseView, viewName);
            var vb = go.GetComponent<UIViewBase> ();
            vb.SetStartClose ();
            vb.OnCloseEvent?.Invoke ();
            GameObject.Destroy (go);

            done?.Invoke ();
        }

        public void ClearSceneAll () {
            void ClearChild (Transform child) {
                var components = child.GetComponents (typeof (UIViewBase));
                if (components == null || components.Length == 0) {
                    GameObject.Destroy (child.gameObject);
                    Log.Green ("ui", $"HandleUI RmvUI Succ, by Destroy(), prefab:{child}");
                } else {
                    foreach (Component component in components) {
                        Log.Trace ("ui", $"Component name:{component.GetType().Name}");
                        // UIManager.Self.OnReleaseViewNew(component.GetType().Name);
                        UIManager.Self.OnReleaseViewNew2 (component.gameObject);
                    }
                }
            }

            foreach (Transform child in topLayer) {
                Log.Print ($"HandleUI RmvUI ClearSceneAll() ui topLayer ___ name:{child.name}, child GetType().Name:{child.GetType().Name}");
                if (child.name.Equals ("UIWaiting") || child.name.Equals ("UIWaiting(Clone)"))
                    continue;
                ClearChild (child);
            }
            foreach (Transform child in midLayer) {
                Log.Print ($"HandleUI RmvUI ClearSceneAll() ui midLayer ___ name:{child.name}, child GetType().Name:{child.GetType().Name}");
                ClearChild (child);
            }
            foreach (Transform child in floorLayer) {
                Log.Print ($"HandleUI RmvUI ClearSceneAll() ui floorLayer ___ name:{child.name}, child GetType().Name:{child.GetType().Name}");
                ClearChild (child);
            }
            foreach (Transform child in _worldCanvas.transform) {
                Log.Trace ("ui", $"HandleUI RmvUI ClearSceneAll() ui worldCanvas ___ nameL{child.name}, child GetType().Name{child.GetType().Name}");
                ClearChild (child);
            }
            foreach (Transform child in _camCanvas.transform) {
                Log.Trace ("ui", $"HandleUI RmvUI ClearSceneAll() ui camCanvas ___ nameL{child.name}, child GetType().Name{child.GetType().Name}");
                ClearChild (child);
            }

            viewMap.Clear ();
            // tipManager.ClearTips();
        }

        // 手动特殊处理: 在一些全屏UI FadeOut的时候，需要在开始fade动画时，提前打开场景3D渲染，否则只会显示天空求或者背景色
        public void ForceMainCameraRender3D () {
            EndOrDisEndMainCameraRender (true, false);
        }
        // 开关3D场景与角色的渲染
        void EndOrDisEndMainCameraRender (bool ifEnable, bool isLoadHudUIEvent) {
            Log.TEST ($"UIManager -- EndOrDisEndMainCameraRender -- ifEnable:{ifEnable} isJustLoadHudUI:{isLoadHudUIEvent}");
            if (isLoadHudUIEvent)
                return;
            if (Camera.main == null) {
                // #if !SRV_ALIYUN_PRODUCTION
                // Debug.LogError($"[!SRV_ALIYUN_PRODUCTION] Camera.main==null, game state:{GameController.Self.CurState}");
                // #else
                // Debug.LogWarning($"Camera.main==null, game state:{GameController.Self.CurState}");
                // #endif
                return;
            }
            if (ifEnable) { Camera.main.cullingMask = ~0; } else { Camera.main.cullingMask = 0; }
        }

        //重置Cam-Canvas
        public void ResSetCamCanvas () {
            _camCanvas.planeDistance = 100f;
            _camCanvas.worldCamera = _uiCamera;
        }
        //设置Cam-Canvas渲染Cam为MainCamera(透视摄像机)
        public void ChangeCamCanvasRenderToMainCamera () {
            Log.Warn ("ChangeCamCanvasRender To MainCamera");
            _camCanvas.planeDistance = 1f;
            _camCanvas.worldCamera = Camera.main;

            UICamera._instance.MainCamAddUICamStrack ();
        }
        //设置Cam-Canvas渲染Cam为UICamera(正交摄像机)
        public void ChangeCamCanvasRenderToUICamera () {
            Log.Warn ("ChangeCamCanvasRender To UICamera");
            _camCanvas.planeDistance = 100f;
            _camCanvas.worldCamera = _uiCamera;

            UICamera._instance.MainCamAddUICamStrack ();
        }

        //启用screen屏幕Canvas==>CamCanvas禁用时
        // 开启Screen-Canvas的可见性与射线检测
        void ScreenCanvas_Enable () {
            // Debug.LogError("ScreenCanvas_Enable");
            if (_screenCanvas != null && _screenCanvas.gameObject != null) {
                // _screenCanvas.gameObject.GetComponent<CanvasGroup>().alpha = 1;
                // _screenCanvas.gameObject.GetComponent<GraphicRaycaster>().enabled = true;
                SetHudUI_Show ();
            }

            // ScreenJoystick.Self?.ResetTouchCount();

            // 3D场景 渲染重新开启
            EndOrDisEndMainCameraRender (true, false);
        }
        //禁用screen屏幕Canvas==>CamCanvas启用时(避免遮挡摄像机Canvas)
        // 关闭Screen-Canvas的可见性与射线检测
        void ScreenCanvas_Disable () {
            // Debug.LogError("ScreenCanvas_Disable");
            if (_screenCanvas != null && _screenCanvas.gameObject != null) {
                // _screenCanvas.gameObject.GetComponent<CanvasGroup>().alpha = 0;
                // _screenCanvas.gameObject.GetComponent<GraphicRaycaster>().enabled = false;
                SetHudUI_Hide ();
            }

            // ScreenJoystick.Self?.ResetTouchCount();
        }
        // public void ShowTip(string message)
        // {
        //     if(tipManager !=null)
        //         tipManager.ShowTip(message);
        // }

        GameObject uiCheckDlg;
        public void HideCheckDlg () {
            if (uiCheckDlg != null)
                GameObject.Destroy (uiCheckDlg);
        }
        public void ShowCheckDlg (string title, string message, UnityAction onClickConfirmBtn, UnityAction onClickCancelBtn = null) {
            var prefab = Resources.Load<GameObject> ("UIDlg_Check");
            // ResUtils.LoadAsync<GameObject> ("UIDlg_Check", (prefab) => {
            uiCheckDlg = GameObject.Instantiate (prefab, UIManager.Self.ParentLayer_GlobalNotice.transform);
            var scr = uiCheckDlg.GetComponent<UIDlg_Check> ();
            scr.Show (message, onClickConfirmBtn, onClickCancelBtn, title);
            // Time.timeScale = 0;
            // });
        }

        public bool ifCanvasShow => ParentLayer_SceneHud.gameObject.activeInHierarchy;
    }
}