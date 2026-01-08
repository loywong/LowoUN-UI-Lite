using System;
using System.Collections.Generic;
using UnityEngine;

// MARK loywong 当timer生命周期与某个GameObject同步时（即当GameObject生命周期结束时，强制清理其绑定的所有定时器）
namespace LowoUN.Util {
    public class TimeMgr : SingletonSimple<TimeMgr> {
        // 有效游戏时间，排除了登录，切换场景。
        private float gameTime_cur;

        // private float gameTime_start;
        // 真实自然时间 --- 适合用于性能测量和精确计时，比如代码耗时
        // private float gameStartRealTime;
        // 真实自然时间 --- 用于做定时恢复数值的情况
        // private float gameRealCurrentTime;
        // 程序自身已持续运行了多长时间（不是真实时间，如果timeScale=0暂停，则不计算在内）
        public float GameRunningTime => gameTime_cur; //{ get { return gameTime_cur - gameTime_start; } }

        // 引用所有的TimerObj,包括工作中的(running 和 pause的) 和 已完成工作（被缓存起来的）
        private long timerId = 0;

        // 正在工作中的
        private Dictionary<long, TimerObj> timerMap = new Dictionary<long, TimerObj> ();
        // 已使用完成的
        private Stack<TimerObj> pool = new Stack<TimerObj> ();

        // 开启的
        // TODO loywong 需要绑定一个唯一Key，以便判断是否为曾经使用过的Obj，如果时，则在再次使用前，可选手动强制停止上一个Obj
        private List<TimerObj> startList = new List<TimerObj> ();
        // // 正常使用完成的
        // private List<long> timerDoneList = new List<long> ();
        // 主动停止的
        private List<long> stopList = new List<long> ();

        public void Awake () {
            // gameStartRealTime = Time.realtimeSinceStartup;
            // gameTime_start = 0;
            gameTime_cur = 0;
        }

        bool isWork;
        public void SetWork () {
            Log.Print ("TimeMgr SetWork()");
            isWork = true;
        }
        public void SetUnWork () {
            Log.Print ("TimeMgr SetUnWork()");
            isWork = false;
        }

        // 清理对象池，在切换场景的时候（确保不支持跨场景的timeobj）
        public void ClearAll () {
#if UNITY_EDITOR
            TEST ();
#endif

            // Debug.LogError("ClearAll when scene changed!!!");
            timerId = 0;

            // 每一帧临时记录 容器
            startList.Clear ();
            // timerDoneList.Clear ();
            stopList.Clear ();

            // 存储容器
            List<TimerObj> removeList = new ();
            foreach (var item in timerMap)
                removeList.Add (item.Value);
            for (int i = 0; i < removeList.Count; i++)
                removeList[i] = null;
            removeList = null;
            timerMap.Clear ();
            pool.Clear ();
        }

        public void Update () {
            // 真实时间
            // // gameRealCurrentTime = Time.realtimeSinceStartup;

            if (!isWork)
                return;

            // 游戏运行时间，游戏禁止工作和Timescale都不统计
            gameTime_cur += Time.deltaTime; // * Time.timeScale;
            // //Debug.Log($"{Time.deltaTime} --- {Time.timeScale} -- {gameCurrectTime} -- {Time.realtimeSinceStartup - gameStartClientTime}");
            // Debug.Log("Update frame");
            UpdateTimers ();
        }

        private void UpdateTimers () {
            if (stopList.Count > 0) {
                foreach (var stopId in stopList) {
                    // if (timerMap.ContainsKey(stopId))
                    //     timerMap[stopId].SetState(TimerObjState.Done);
                    //     // timerMap.Remove(stopId);
                    if (timerMap.ContainsKey (stopId)) {
                        pool.Push (timerMap[stopId]);
                        timerMap.Remove (stopId);
                    }
                    // else Debug.LogError($"stopId:{stopId} not in timerMap");

                }
                stopList.Clear ();
            }

            foreach (var item in timerMap) {
                item.Value.SetUpdate ();
            }

            if (startList.Count > 0) {
                foreach (var startData in startList) {
                    timerMap.Add (startData.id, startData);
                }
                startList.Clear ();
            }
        }

        // 延迟x时间，执行一次，忽略timeScale
        public long StartTimer_IgnoreTimeScale (float time, Action done, Func<bool> bindCondition = null) {
            return StartTimer_Base (time, done, bindCondition, false, true);
        }
        // 延迟x帧 执行一次（Time.timeScale 必须大于 0）
        public long StartTimer_Frames (uint frames, Action done, Func<bool> bindCondition = null) {
            return StartTimer_Base (frames, done, bindCondition, true);
        }
        // 每经过x时间，执行一次，无效执行
        // public long StartTimer_Loop (float time, Action done, Func<bool> bindCondition = null) {}
        // 每经过x时间，执行一次，总共执行多次
        // public long StartTimer_Multi (float time, uint exeNums, Action done, Func<bool> bindCondition = null) {
        //     if(exeNums<=1) {
        //         Debug.LogError("exeNums should be bigger than 1");
        //         return 0;
        //     }
        //     return StartTimer_Base (time, done, bindCondition, true);
        // }
        // 延迟x时间，执行一次
        public long StartTimer (float time, Action done, Func<bool> bindCondition = null) {
            return StartTimer_Base (time, done, bindCondition);
        }
        long StartTimer_Base (float time, Action done, Func<bool> bindCondition = null, bool isFrameType = false, bool isIgnoreTimeScale = false) {
            TimerObj tobj;
            if (pool.Count > 0) {
                // Debug.LogError($"StartTimer -- pool.Count:{pool.Count}");
                tobj = pool.Pop ();

                timerId += 1;
                tobj.ReInit (timerId, time, done, bindCondition, isFrameType, isIgnoreTimeScale);
            } else {
                timerId += 1;
                // long id = ++timerId;
                // Debug.LogError($"StartTimer -- Create new TimerObj id:{timerId}");
                tobj = new TimerObj (timerId, time, done, bindCondition, isFrameType, isIgnoreTimeScale);
            }

            startList.Add (tobj); //gameTime_cur + time,
            tobj.Start ();

            return tobj.id;
        }

        // xxx Id 需要一直更新即可 // MARK loywong 不需要主动停止了，回收之后会给其他事件重复使用，停止会导致逻辑异常
        // 主动停止 -- 对象是那些 正在工作中的obj
        public void StopTimer (long id) {
            if (id < 1) {
#if UNITY_EDITOR
                Debug.Log ($"[Editor临时] TimeMgr -- StopTimer -- id:{id} < 1");
#endif
                return;
            }
            // stopList.Add(id);
            if (timerMap.ContainsKey (id))
                timerMap[id].SetState_Stop ();
#if UNITY_EDITOR
            else Debug.Log ($"[Editor临时] TimeMgr -- obj with id:{id} has been recycled or reused.");
#endif
        }

        public void Pause () {
            foreach (var item in timerMap) {
                if (item.Value != null && item.Value.CurState == TimerObjState.Running)
                    item.Value.SetState_Pause ();
            }
        }
        public void Resume () {
            foreach (var item in timerMap) {
                if (item.Value != null && item.Value.CurState != TimerObjState.Done)
                    item.Value.SetState_Resume ();
            }
        }
        public void PauseObj (long id) {
            if (timerMap.ContainsKey (id)) {
                var obj = timerMap[id];
                if (obj != null && obj.CurState == TimerObjState.Running)
                    obj.SetState_Pause ();
            }
            // else Debug.LogError ($"obj with id:{id} has been stopped");
        }
        public void ResumeObj (long id) {
            if (timerMap.ContainsKey (id)) {
                var obj = timerMap[id];
                if (obj != null && obj.CurState == TimerObjState.Running)
                    obj.SetState_Resume ();
            }
            // else Debug.LogError ($"obj with id:{id} has been stopped");
        }

        public void DoneToRecycle (TimerObj tobj) {
            // Debug.LogError ($"DoneToRecycle id:{tobj.id}");
            stopList.Add (tobj.id);
        }

        public void TEST () {
            Debug.Log ($"[Editor临时] TimeMgr -- Max timerId:{timerId}, gameTime_cur:{gameTime_cur}, timerMap Count:{timerMap.Count}, pool Count:{pool.Count}, startList:{startList.Count},stopList:{stopList.Count}");
        }
    }
}