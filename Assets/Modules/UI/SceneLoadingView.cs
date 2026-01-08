using System;
using System.Collections.Generic;
using LowoUN.Util;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

// using UnityEngine.AddressableAssets;
// using UnityEngine.ResourceManagement.AsyncOperations;

namespace LowoUN.Module.UI {
    public class SceneLoadingView : UIViewBase {
        [LabelText ("背景图Layer"), SerializeField] Transform sceneLoad_Layer;
        [LabelText ("动画对象"), SerializeField] GameObject animObj;

        readonly List<string> bg_name = new List<string> { "LoadingBg_1", "LoadingBg_2", "LoadingBg_3", "LoadingBg_4" };

        // AsyncOperationHandle loadPrefabHandle = default;

        List<uint> temp_tipIndex = new List<uint> ();
        string tit_content;

        List<string> temp_bgName = new List<string> ();
        string load_bgName;
        GameObject curBg; // 上一次显示的BG，需要在下一次显示新Bg前被清理

        void Awake () {
            animObj.SetActive (false);
            sceneLoad_Layer.gameObject.SetActive (false);
        }

        void FindLoadBgName () {
            if (temp_bgName.Count == 0) { ResetTempBgName (); }
            Debug.Assert (bg_name.Count > 1);
            int index_bg = 0;
            if (temp_bgName.Count > 1) index_bg = UnityEngine.Random.Range (0, temp_bgName.Count);
            load_bgName = temp_bgName[index_bg];
            temp_bgName.RemoveAt (index_bg);
        }

        void FindTipText () {
            if (temp_tipIndex.Count == 0) { ResetTipIndex (); }

            int index = 0;
            if (temp_tipIndex.Count > 1) index = UnityEngine.Random.Range (0, temp_tipIndex.Count);

            tit_content = string.Empty;

            // var langId = temp_tipIndex[index];
            // var config = Configs.Instance.TipsTable.Find (langId);
            // if (config != null) tit_content = config.LanguageID;
            //         }
            // #if UNITY_EDITOR
            //         else Debug.LogError ($"langId:{langId}, has no config in TipsTable");
            // #endif
            temp_tipIndex.RemoveAt (index);
        }

        public void RefreshLoadBg (Action cb) {
            // if (loadPrefabHandle.IsValid ())
            //     Addressables.Release (loadPrefabHandle);

            FindLoadBgName ();
            // 随机切换背景图
            if (load_bgName.IsValid ()) {
                animObj.SetActive (false);
                sceneLoad_Layer.gameObject.SetActive (false);

                // 1 先清除老的背景图
                if (curBg != null)
                    Destroy (curBg);

                var go = Resources.Load<GameObject> (load_bgName);
                // ResUtils.LoadAsync_DontDestroy<GameObject> (load_bgName, (loadHandler, go) => {
                if (!this.IsValid ()) {
                    cb.Invoke ();
                    return;
                }

                // loadPrefabHandle = loadHandler;

                // 2 load 新背景图
                curBg = Instantiate (go, sceneLoad_Layer);
                sceneLoad_Layer.gameObject.SetActive (true);
                animObj.SetActive (true);

                // // 3 随机显示Tip文字
                // FindTipText ();
                // curBg.transform.GetChild (0).GetChild (0).gameObject.GetComponent<TextMeshProUGUI> ().text = "tit_content";//Utils.MultilingualTrans (tit_content);

                Log.Green ("flow", "_______________________ loadingView.Show 2");

                cb.Invoke ();
                // });
            } else {
                animObj.SetActive (true);
                sceneLoad_Layer.gameObject.SetActive (true);
                Debug.LogError ($"RefreshLoadBg load_bgName is null.");
                cb.Invoke ();
            }
        }

        private void ResetTipIndex () {
            temp_tipIndex.Clear ();
            // foreach (var target in Configs.Instance.TipsTable)
            //     temp_tipIndex.Add (target.Value.ID);
            temp_tipIndex.Add (1);
            temp_tipIndex.Add (2);
            temp_tipIndex.Add (3);
            temp_tipIndex.Add (4);
        }
        private void ResetTempBgName () {
            temp_bgName.Clear ();
            foreach (var target in bg_name)
                temp_bgName.Add (target);
        }

        public void SetProgress (float progress) { }

        public void SetShowContent (GameObject content) { }
    }
}