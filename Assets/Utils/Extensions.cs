using UnityEngine;

namespace LowoUN.Utils {
    public static class Extensions {
        public static bool IsValid (this string str) {
            return !string.IsNullOrEmpty (str);
        }
        public static bool IsValid (this MonoBehaviour monoObj) {
            return monoObj != null && monoObj.gameObject != null && monoObj.gameObject.activeInHierarchy;
        }
    }
}