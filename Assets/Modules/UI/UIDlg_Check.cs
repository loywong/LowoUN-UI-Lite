using LowoUN.Util;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LowoUN.Module.UI {
    public class UIDlg_Check : UIViewBase {
        [SerializeField]
        GameObject con;
        [SerializeField]
        Button btn_comfirm;
        [SerializeField]
        Button btn_cancel;
        [SerializeField]
        Text txt_Title;
        [SerializeField]
        Text txt_Content;
        public void Show (string content, UnityAction cb_confirm, UnityAction cb_cancel = null, string title = null) {
            con.SetActive (true);

            txt_Content.text = content;
            if (title.IsValid ()) txt_Title.text = title;

            if (btn_comfirm != null) {
                if (cb_confirm != null) {
                    btn_comfirm.gameObject.SetActive (true);
                    btn_comfirm.onClick.AddListener (cb_confirm);
                } else btn_comfirm.gameObject.SetActive (false);
            }

            if (btn_cancel != null) {
                if (cb_cancel != null) {
                    btn_cancel.gameObject.SetActive (true);
                    btn_cancel.onClick.AddListener (cb_cancel);
                } else btn_cancel.gameObject.SetActive (false);
            }
        }

        public override void Hide () {
            base.Hide ();
            // UIManager.Self.ScreenCanvas_Enable();
            con.SetActive (false);
        }
    }
}