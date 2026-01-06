using LowoUN.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace LowoUN.Modules.UI {
    public class UIRootView : UIViewBase {
        [Header ("UILayers层")]
        [SerializeField]
        Transform _UILayers;
        [Header ("顶层UI")]
        [SerializeField]
        Transform _topLayer;
        [SerializeField]
        [Header ("中层UI")]
        Transform midLayer;
        [SerializeField]
        [Header ("底层UI")]
        Transform floorLayer;
        [SerializeField]
        [Header ("HUDRoot")]
        Transform hudRoot;
        // [SerializeField]
        // [Header("UI背景")]
        // Image uibg;
        // [SerializeField]
        // [Header("Toast提示信息节点")]
        // GameObject toast;
        // [SerializeField]
        // [Header("Toast提示信息文字")]
        // TextMeshProUGUI toastText;
        // [Header("确认消息框")]
        // public GameObject msgbox;

        // [Header("等待界面")]
        // public GameObject waitingPanel;
        // [SerializeField] private GameObject waiting4EventMask;
        // GameObject waitingPanel;
        // GameObject waiting4EventMask;

        [Header ("场景加载界面")]
        public SceneLoadingView sceneLoadingView;
        [Header ("资源下载进度_容器")]
        public Transform con_downloading;
        [Header ("资源下载进度_容器")]
        public Text txt_downloadingPercent;
        public Transform TopLayer => _topLayer;

        public Transform MidLayer => midLayer;

        public Transform FloorLayer => floorLayer;

        Transform HUDRoot => hudRoot;

        // Image UIBackground => uibg;

        public SceneLoadingView SceneLoadingView => sceneLoadingView;

        // protected override void Awake() {
        //     Facade.Events.AddListener<string>(CustomEvents.CloseView, UIManager.Self.OnCloseView);
        // }
        // void OnDestroy() {
        //     Facade.Events.RemoveListener<string>(CustomEvents.CloseView, UIManager.Self.OnCloseView);
        // }

        void Update () {
            // MARK loywong 禁用 软件侧 退出应用程序的操作界面
            if (Input.GetKeyDown (KeyCode.Escape)) { }
        }

        void ShowOrHideHUD (bool isShow) {
            hudRoot.gameObject.SetActive (isShow);
        }

        void ShowOrHideUILayers (bool isShow) {
            midLayer.gameObject.SetActive (isShow);
            floorLayer.gameObject.SetActive (isShow);
        }

        [SerializeField] UIWaiting uiWaiting;
        long curWaitingUITimer;
        bool isShowWaitingUI;
        // 延迟x秒显示可视化的waiting ui
        readonly float delayShowWaitingVisiualUI = 2f;
        public void ShowWaitingUI (bool isUsrClickUI) {
            // Debug.LogError($"ShowWaitingUI:{isShowWaitingUI}, uiWaiting:{uiWaiting}");
            if (isShowWaitingUI) return;
            isShowWaitingUI = true;

            void Show () {
                // 在弹出waiting ui之前 提前挡住界面 屏蔽用户操作
                if (isUsrClickUI) {
                    // if(waiting4EventMask!=null)
                    uiWaiting.waiting4EventMask.SetActive (true);
                }

                // LowoUN.Util.Log.TEST("ShowWaitingUI 1");
                if (curWaitingUITimer > 0) {
                    TimeMgr.Self.StopTimer (curWaitingUITimer);
                    curWaitingUITimer = 0;
                }
                curWaitingUITimer = TimeMgr.Self.StartTimer (delayShowWaitingVisiualUI, () => {
                    curWaitingUITimer = 0;
                    // LowoUN.Util.Log.TEST($"ShowWaitingUI 2 curWaitingUITimer:{curWaitingUITimer}, isShowWaitingUI:{isShowWaitingUI}");
                    if (isShowWaitingUI) {
                        // 非UI操作的网络通信(一般指战斗中)，一旦超过设定时间无数据返回，也需要遮挡界面触发的事件
                        // if(waiting4EventMask!=null)
                        uiWaiting.waiting4EventMask.SetActive (true);

                        RealToggleWaitingUI (true);

                        // if(ScreenJoystick.Self.IsValid())
                        //     ScreenJoystick.Self.CancelOnPointerUp();
                    }
                });
            }

            // if(uiWaiting==null) {
            //     // Debug.LogError($"ShowWaitingUI ResUtils.LoadAsync 111");
            //     InitWaitingUI();
            // }
            // else Show();
            Show ();
        }

        // Action InitEnd;
        // public void Init(Action InitEnd) {
        //     this.InitEnd = InitEnd;

        //     if(uiWaiting==null) 
        //         InitWaitingUI();
        //     #if UNITY_EDITOR
        //     else Debug.LogError("uiWaiting repeatly create 1");
        //     #endif
        // }
        // void InitWaitingUI () {
        //     // 可以用同步
        //     ResUtils.LoadAsync<GameObject> ("UIWaiting", (prefab) => {
        //         // Debug.LogError($"ShowWaitingUI ResUtils.LoadAsync 222");

        //         // if(uiWaiting!=null) {
        //         //     Destroy(uiWaiting);
        //         //     Debug.LogError("uiWaiting repeatly create 2");
        //         // }

        //         // uiWaiting = GameObject.Instantiate (prefab, UIManager.Self._camCanvas.transform);
        //         var go = GameObject.Instantiate(prefab, UIManager.Self.ParentLayer_GlobalNotice);//_screenCanvas.transform
        //         uiWaiting = go.GetComponent<UIWaiting>();
        //         // waiting4EventMask = scr.waiting4EventMask;
        //         // waitingPanel = scr.waitingPanel;

        //         if(isShowWaitingUI) Show();
        //         // else Hide();

        //         InitEnd.Invoke();
        //     });
        // }

        public void ForceHideWaitingUI () {
#if !SRV_ALIYUN_PRODUCTION
            Debug.Assert (isShowWaitingUI == false);
            Debug.Assert (curWaitingUITimer == 0);
            Debug.Assert (uiWaiting.waiting4EventMask.activeInHierarchy == false);
            if (isShowWaitingUI || curWaitingUITimer > 0 || uiWaiting.waiting4EventMask.activeInHierarchy)
                Debug.LogError ($"UIWaiting -- isShowWaitingUI:{isShowWaitingUI}, curWaitingUITimer>0?:{curWaitingUITimer} uiWaiting.waiting4EventMask.activeInHierarchy:{uiWaiting.waiting4EventMask.activeInHierarchy}");
#endif
            if (uiWaiting != null) uiWaiting.Reset ();
            isShowWaitingUI = false;
            if (curWaitingUITimer > 0) {
                // LowoUN.Util.Log.TEST($"HideWaitingUI 2 curWaitingUITimer:{curWaitingUITimer}, isShowWaitingUI:{isShowWaitingUI}");
                TimeMgr.Self.StopTimer (curWaitingUITimer);
                curWaitingUITimer = 0;
            }
        }

        public void HideWaitingUI () {
            // Debug.LogError($"HideWaitingUI:{isShowWaitingUI}, uiWaiting:{uiWaiting}");
            if (!isShowWaitingUI)
                return;

            isShowWaitingUI = false;

            // LowoUN.Util.Log.TEST($"HideWaitingUI 1");
            if (curWaitingUITimer > 0) {
                // LowoUN.Util.Log.TEST($"HideWaitingUI 2 curWaitingUITimer:{curWaitingUITimer}, isShowWaitingUI:{isShowWaitingUI}");
                TimeMgr.Self.StopTimer (curWaitingUITimer);
                curWaitingUITimer = 0;
            }

            uiWaiting.waiting4EventMask.SetActive (false);
            // 判断waitingUI是否 真正 从显示状态 切换为 隐藏状态
            if (uiWaiting.waitingPanel.activeInHierarchy) {
                RealToggleWaitingUI (false);
            }
        }

        public void TEST_UnloadWaitingUI () {
            if (uiWaiting != null)
                Destroy (uiWaiting);
        }

        // 只有实际 触发waitingui从显示到隐藏 或者 从隐藏到显示 才重置 摇杆TouchCount
        void RealToggleWaitingUI (bool isRealShowWaitingUI) {
            uiWaiting.waitingPanel.SetActive (isRealShowWaitingUI);
            // ToggleOtherUI(!isRealShowWaitingUI);
        }
    }
}