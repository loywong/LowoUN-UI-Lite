using System.Collections;
using LowoUN.Module.UI;
using LowoUN.Util;
using UnityEngine;

public class Entry : MonoBehaviour {
    public static Entry Instance;

    void Awake () {
        DontDestroyOnLoad (gameObject);

        Instance = this;
    }
    void Start () {
        LowoUN.Util.Log.Init (true);

        StartCoroutine (Init ());
    }

    IEnumerator Init () {
        yield return null;

        // UIRootController.Self.Init (() => {
        //     Debug.Log ("UIRootController.Self.Init Over.");

            UIManager.Self.Init (()=>{
                // InputData.Init ();

                // TEST loading some ui panels
                // 1 overlay ui
                // --- top layer (waitingUI, checkDlgUI)
                // --- middle layer (PopUI)
                // --- floor layer (HudUI)
                UIManager.Self.CreateUIViewAsync_Hud<UISample_Hud> ("UISample_Hud", UICameraType.SS_Overlay);
                // UIManager.Self.CreateUIViewAsync<UISample_Popup_FullScreen> ("UISample_Popup_FullScreen", UIManager.Self._camCanvas.transform); //uiRoot.MidLayer

                // 2 camera UI (PopUI)
                UIManager.Self.CreateUIViewAsync<UISample_Popup> ("UISample_Popup", UIManager.Self._camCanvas.transform); //uiRoot.MidLayer

                // 3 world ui 
            });
        // });

    }

    void Update () {
        TimeMgr.Self.Update ();
    }

#if UNITY_EDITOR
    void OnApplicationFocus (bool hasFocus) {
        // Debug.LogError($"OnApplicationFocus hasFocus:{hasFocus}");

        // 在Windows平台，焦点丢失可以模拟暂停
        if (Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.WindowsEditor) {
            OnApplicationPause (!hasFocus);
        }
    }
#endif

    void OnApplicationPause (bool pauseStatus) {

    }
    void OnApplicationQuit () {

    }
    void OnDestroy () {

    }
}