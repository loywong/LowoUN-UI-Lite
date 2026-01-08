using System;
using UnityEngine;

namespace LowoUN.Module.UI {
    public class UIViewBase : MonoBehaviour { //, IUIView
        public Vector2 safeAreaAndroid_anchorMin;
        public Vector2 safeAreaAndroid_anchorMax;
        public virtual bool IsFullScreen { get => false; }

        // 逻辑上标记 界面开始进入关闭流程
        public bool isStartClose { get; private set; }
        public void SetStartClose () {
            if (isStartClose) {
#if !SRV_ALIYUN_PRODUCTION
                Debug.LogWarning ($"UI ViewBase SetStartClose, repeat set!!! name:{name}");
#endif
            }
            isStartClose = true;
        }

        // TODO 移除掉
        public Action OnCloseEvent;
        protected virtual void OnUpdate () { }
        void Update () {
            // ApplySafeArea ();
            Rect safeArea = Screen.safeArea;
            safeAreaAndroid_anchorMin = safeArea.position;
            safeAreaAndroid_anchorMax = safeArea.position + safeArea.size;

            OnUpdate ();
        }

        // public virtual void AndroidSafeAreaAdjust () { }
        // void ApplySafeArea () {
        //     Rect safeArea = Screen.safeArea;
        //     safeAreaAndroid_anchorMin = safeArea.position;
        //     safeAreaAndroid_anchorMax = safeArea.position + safeArea.size;

        //     //safeAreaAndroid_anchorMin.x /= Screen.width;
        //     //safeAreaAndroid_anchorMin.y /= Screen.height;
        //     //safeAreaAndroid_anchorMax.x /= Screen.width;
        //     //safeAreaAndroid_anchorMax.y /= Screen.height;

        //     // AndroidSafeAreaAdjust();
        // }
        public virtual void Show () {
            if (this == null || this.gameObject == null)
                return;
            gameObject.SetActive (true);
        }
        public virtual void Hide () {
            if (this == null || this.gameObject == null)
                return;
            gameObject.SetActive (false);
        }

        public Action<GameObject> ClearDataAction;
        //禁止通过此接口关闭UI
        public virtual void Release () {
#if UNITY_EDITOR
            Debug.Log ($"@@@ UI ViewBase Release(), name:{name}");
#endif
            GameObject.Destroy (gameObject);
        }
        public T GetViewObject<T> () where T : class {
            if (gameObject == null)
                return null;
            return gameObject as T;
        }
    }
}