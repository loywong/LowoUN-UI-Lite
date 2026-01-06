using LowoUN.Utils;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LowoUN.Modules.UI {
    public enum CamRenderType {
        Perspective, //透视
        Orthographic, //正交
    }

    public class UICamera : MonoBehaviour {
        public static UICamera _instance;
        private Camera _uiCamera;

        void Awake () {
            DontDestroyOnLoad (gameObject);
            _instance = this;
            _uiCamera = GetComponent<Camera> ();

            UIManager.Self.SetUICamera (_uiCamera);
        }
        //添加主摄像机摄像机堆叠
        public void MainCamAddUICamStrack () {
            Log.Warn ($"MainCamAddUICamStrack Camera.main:{Camera.main}");
            if (Camera.main == null)
                return;

            var camStack = Camera.main.GetUniversalAdditionalCameraData ().cameraStack;
            Log.Warn ($"MainCamAddUICamStrack Camera.main:{Camera.main}, camStack:{camStack.Count}, _uiCamera:{_uiCamera}");
            if (camStack != null && !camStack.Contains (_uiCamera)) {
#if UNITY_EDITOR
                foreach (var item in camStack) {
                    Debug.Log ($"main camera stack item:{item}");
                }
#endif
                camStack.Add (_uiCamera);
            }
        }

        //更改UICamera的渲染类型正交/透视
        public void ChangeUICamRenderType (CamRenderType camRenderType) {
            switch (camRenderType) {
                case CamRenderType.Perspective:
                    _uiCamera.orthographic = false;
                    break;
                case CamRenderType.Orthographic:
                    _uiCamera.orthographic = true;
                    break;
            }
        }
    }
}