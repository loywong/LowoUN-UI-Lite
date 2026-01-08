using System;
using UnityEngine;

namespace LowoUN.Util {
    public enum TimerObjState {
        NONE = 0, // 新建的
        Running,
        Pause,
        Done // 使用过的，会重复利用
    }
    public class TimerObj {
        public long id;
        float exeTime;
        Action done;
        Func<bool> bindCondition;
        // public bool isRealTimer;
        bool isFrameType;
        bool isIgnoreTimeScale;

        float curTime;
        TimerObjState curState;
        public TimerObjState CurState => curState;

        // public TimerObj (long id, float endTick, Action done, bool isRealTimer) {
        public TimerObj (long id, float exeTime, Action done, Func<bool> bindCondition, bool isFrameType, bool isIgnoreTimeScale) {
            this.id = id;
            Init (exeTime, done, bindCondition, isFrameType, isIgnoreTimeScale);
        }
        public void ReInit (long id, float exeTime, Action done, Func<bool> bindCondition, bool isFrameType, bool isIgnoreTimeScale) {
            this.id = id;
            Init (exeTime, done, bindCondition, isFrameType, isIgnoreTimeScale);
        }
        void Init (float exeTime, Action done, Func<bool> bindCondition, bool isFrameType, bool isIgnoreTimeScale) {
            this.exeTime = exeTime;
            this.done = done;
            // this.isRealTimer = isRealTimer;
            this.bindCondition = bindCondition;
            this.isFrameType = isFrameType;
            this.isIgnoreTimeScale = isIgnoreTimeScale;
        }

        public void Start () {
            curTime = 0;
            SetState (TimerObjState.Running);
        }

        public void SetState_Stop () {
            SetState (TimerObjState.Done);
        }

        void SetState (TimerObjState s) {
            this.curState = s;

            if (s == TimerObjState.Done)
                SetStateDone ();
        }

        public void SetState_Pause () { SetState (TimerObjState.Pause); }
        public void SetState_Resume () { SetState (TimerObjState.Running); }

        void SetStateDone () {
            try {
                if (this == null) {
                    Debug.LogError ("TimerObj is null");
                    return;
                }

                if (bindCondition == null || (bindCondition != null && bindCondition.Invoke () == true)) {
                    done?.Invoke ();
                }
                Reset ();
                TimeMgr.Self.DoneToRecycle (this);
            } catch (System.Exception e) {
                Debug.LogError ($"TimerObj error, e:{e}");
            }
        }

        void Reset () {
            exeTime = 0;
            done = null;
            curState = TimerObjState.Done;
        }

        public void SetUpdate () {
            if (curState != TimerObjState.Running)
                return;

            if (isFrameType) {
                if (Time.timeScale > 0)
                    curTime += 1;
            } else {
                if (isIgnoreTimeScale)
                    curTime += Time.unscaledDeltaTime;
                else
                    curTime += Time.deltaTime;
            }

            if (curTime >= exeTime) {
                SetStateDone ();
            }
        }
    }
}