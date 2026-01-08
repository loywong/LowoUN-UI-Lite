using System;
using LowoUN.Util;
using UnityEngine;

namespace LowoUN.Module.UI {
    public class UIRootController : SingletonSimple<UIRootController> {
        UIRootView _view;

        Action initEndCallback;

        public void Init (Action initEndCallback) {
            this.initEndCallback = initEndCallback;
            Start ();
        }

        public void Start () {
            GameObject h = Resources.Load<GameObject> ("UIRootView");
            // ResUtils.LoadAsync_DontDestroy<GameObject>("UIRootView", (loadingHandler,h) =>
            // {
            GameObject hResult = h;
            var canvas = GameObject.Find ("Canvas");
            GameObject.DontDestroyOnLoad (canvas);
            UIManager.Self._screenCanvas = canvas.GetComponent<Canvas> ();
            var camCamvas = GameObject.Find ("CamCanvas");
            GameObject.DontDestroyOnLoad (camCamvas);
            UIManager.Self.SetCamCanvas (camCamvas.GetComponent<Canvas> ());
            var worldCamvas = GameObject.Find ("WorldCanvas");
            GameObject.DontDestroyOnLoad (worldCamvas);
            UIManager.Self.SetWorldCanvas (worldCamvas.GetComponent<Canvas> ());

            var gameObject = GameObject.Instantiate (hResult, canvas.transform);
            gameObject.name = hResult.name;
            _view = gameObject.GetComponent<UIRootView> ();

            HideDownloading ();

            this.initEndCallback.Invoke ();
        }

        // /// <summary>
        // /// 显示Toast提示信息
        // /// </summary>
        // void OnShowToast(string txt)
        // {
        //     _view.ShowToast(txt);
        // }

        /// <summary>
        /// 获取UIRoot
        /// </summary>
        public UIRootView GetUIRootView () {
            return _view;
        }

        public void ShowWaitingUI (bool isUsrClickUI) {
            // Debug.LogError("UIRootPresenter -- ShowWaitingUI");
            _view.ShowWaitingUI (isUsrClickUI);
        }
        public void HideWaitingUI () {
            // Debug.LogError("UIRootPresenter -- HideWaitingUI");
            _view.HideWaitingUI ();
        }

        public void ForceHideWaitingUI () {
            // Debug.LogError("UIRootPresenter -- ForceHideWaitingUI");
#if !SRV_ALIYUN_PRODUCTION
            Debug.Log ("[!SRV_ALIYUN_PRODUCTION] ------ ForceHideWaitingUI");
#endif
            _view.ForceHideWaitingUI ();
        }
        public void TEST_DESTROY_WaitingUI () {
            _view.TEST_UnloadWaitingUI ();
        }

        // asset downloading
        public void ShowDownloading () {
            _view.con_downloading.gameObject.SetActive (true);
        }
        public void HideDownloading () {
            _view.txt_downloadingPercent.text = "";
            _view.con_downloading.gameObject.SetActive (false);
        }

        string progressStr = "";
        public void UpdateDownloading (float progress) {
            if (progress < 0) progress = 0;
            progress = (int) (progress * 100);
            progressStr = $"{progress}%";

#if UNITY_EDITOR
            Debug.Log ($"progressStr:{progressStr}");
#endif
            _view.txt_downloadingPercent.text = progressStr;
        }
    }
}