using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 只适用于Editor环境下的开发日志输出
namespace LowoUN.Util {
	public static class Log {
		// 是否允许Unity运行时日志
		static void SetLogEnabled (bool isRuntimeLogEnabled) {
#if UNITY_EDITOR
			Debug.unityLogger.logEnabled = true;
#else
			Debug.unityLogger.logEnabled = isRuntimeLogEnabled;
#endif
		}
		// 设置Unity输出日志等级
		static void SetLogLevel () {
			// 用于错误类型
			var logType = LogType.Error;
			// Debug.unityLogger.Log(LogType.Error);
			// 仅输出 托管堆栈跟踪
			var traceType = StackTraceLogType.ScriptOnly;
			Application.SetStackTraceLogType (logType, traceType);
		}

		public static void Init (bool isDebug) {
			if (!isDebug) {
				Log.SetOpen (0);
				return;
			}

			Log.SetOpen (1);

			TextAsset txt = Resources.Load ("Setting_Log") as TextAsset;
			// 以换行符作为分割点，将该文本分割成若干行字符串，并以数组的形式来保存每行字符串的内容
			string[] str = txt.text.Split ('\n');
			// 将每行字符串的内容以逗号作为分割点，并将每个逗号分隔的字符串内容遍历输出
			for (int i = 0; i < str.Length; i++) {
				// Debug.Log("___"+str[i]);
				if (i == 0) {
					// MARK loywong 由项目GameSettings面板值决定
					// Log.SetOpen (int.Parse (str[0]));
					continue;
				}

				// Debug.Log("________"+str[i]);
				if (string.IsNullOrWhiteSpace (str[i]))
					continue;

				string[] ss = str[i].Split ('#');
				// Debug.Log("________ "+ss.Length);
				if (ss.Length == 1)
					Log.OpenTag (str[i].Trim ());
			}
		}

		private static Dictionary<string, string> tags = new Dictionary<string, string> ();

		private static bool isOpen = false;
		// public static bool IsOpen => isOpen;
		static void SetOpen (int openState) {
			isOpen = openState == 1;
		}

		static void OpenTag (string tag) {
			if (!isOpen) return;

			tags[tag] = tag.ToString ();
			// Debug.Log("tag:"+tag);
		}

		// [System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Exception (params object[] msg) {
			// if (!isOpen) return;
			throw new System.Exception ("【Editor临时-Exception】" + ParseMsg (msg));
		}

		// TODO loywong 计划移除，真正需要提示Error的地方，使用UnityEngine本身的Error
		// Error和Warn不需要标签
		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Error (params object[] msg) {
			if (!isOpen) return;

			// Debug.LogError ("【Editor临时】" + ParseMsg (msg));
			HandleWithColor ("FF5C95", ParseMsg (msg));
		}

		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void TEST (params object[] msg) {
			if (!isOpen) return;

			// Debug.LogError ("【Editor临时-临时】" + ParseMsg (msg));
			HandleWithColor ("FF5C95", ParseMsg (msg));
		}

		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Warn (params object[] msg) {
			if (!isOpen) return;

			// Debug.LogWarning ("【Editor临时】" + ParseMsg (msg));
			HandleWithColor ("FFAE00", ParseMsg (msg));
		}

		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Print (params object[] msg) {
			if (!isOpen) return;

			Debug.Log ("【Editor临时】" + ParseMsg (msg));
		}
		// MARK 避免和ET的Log.Console混淆
		// [System.Diagnostics.Conditional ("PROJECT_LOG")]
		// public static void Console (params object[] msg) {
		// 	if (!isOpen) return;

		// 	Debug.Log ("【Editor临时】" + ParseMsg (msg));
		// }
		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Output (params object[] msg) {
			if (!isOpen) return;

			Debug.Log ("【Editor临时】" + ParseMsg (msg));
		}

		// -------------------------------------------------------------
		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Trace (string tag, params object[] msg) {
			if (!isOpen) return;

			HandleWithTagAndColor (tag, "FFFFFF", ParseMsg (msg));
		}

		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void White (string tag, params object[] msg) {
			if (!isOpen) return;

			HandleWithTagAndColor (tag, "FFFFFF", ParseMsg (msg));
		}

		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Red (string tag, params object[] msg) {
			if (!isOpen) return;

			HandleWithTagAndColor (tag, "FF5C95", ParseMsg (msg));
		}

		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Green (string tag, params object[] msg) {
			if (!isOpen) return;

			HandleWithTagAndColor (tag, "90FF81", ParseMsg (msg));
		}

		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Orange (string tag, params object[] msg) {
			if (!isOpen) return;

			HandleWithTagAndColor (tag, "FFAE00", ParseMsg (msg));
		}

		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Gray (string tag, params object[] msg) {
			if (!isOpen) return;

			HandleWithTagAndColor (tag, "606060", msg);
		}

		[System.Diagnostics.Conditional ("PROJECT_LOG")]
		public static void Blue (string tag, params object[] msg) {
			if (!isOpen) return;

			HandleWithTagAndColor (tag, "3A5FCD", msg);
		}

		private static void HandleWithTagAndColor (string tag, string color, params object[] paramsMsg) {
			if (!isOpen) return;

			if (!tags.ContainsKey (tag))
				return;

			object msg = ParseMsg (paramsMsg);

			Debug.Log ("<color=#" + color + ">" + "【Editor临时-[ " + tags[tag] + " ]】 " + msg + "</color>");
		}
		private static void HandleWithColor (string color, params object[] paramsMsg) {
			if (!isOpen) return;

			object msg = ParseMsg (paramsMsg);

			Debug.Log ("<color=#" + color + ">" + "【Editor临时】 " + msg + "</color>");
		}

		// [System.Diagnostics.Conditional("PROJECT_LOG")]
		// 解第一层
		private static string ParseMsg (params object[] msg) {
			// Debug.Log ("ParseObjects() length: " + msg.Length);
			if (msg.Length == 1)
				return GetString (msg[0]);

			var str = "";

			for (int i = 0; i < msg.Length; i++) {
				var s = (i == 0 ? "" : ", ") + GetString (msg[i]);
				str += s;
			}

			return str;
		}

		// 解第二层
		// [System.Diagnostics.Conditional("PROJECT_LOG")]
		private static string GetString (object msg) {
			string detail = "";
			if (msg is ICollection)
				detail = Stringify (msg as ICollection);
			else
				detail = msg.ToString ();

			return detail;
		}

		// [System.Diagnostics.Conditional("PROJECT_LOG")]
		private static string Stringify (ICollection col) {
			var str = "";
			var isFirst = true;
			foreach (var item in col) {
				// item 还是有可能是集合，不递归解下去了！！！
				if (isFirst) {
					str += item.ToString ();
					isFirst = false;
				} else
					str += "+" + item.ToString ();
			}
			return str;
		}
	}
}