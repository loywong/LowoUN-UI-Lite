using UnityEngine;

namespace LowoUN.Modules.UI {
    public class UIWaiting : MonoBehaviour {
        public GameObject waiting4EventMask;
        public GameObject waitingPanel;
        void Awake () {
            Reset ();
        }
        public void Reset () {
            waiting4EventMask.SetActive (false);
            waitingPanel.SetActive (false);
        }
    }
}