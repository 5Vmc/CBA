using Babu;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BigBang
{
    public class TimeTickManager : BabuSingleton<TimeTickManager>
    {
        private TimeTickManager()
        {
        }

        Dictionary<Action, float> actionDict = new Dictionary<Action, float>();
        private float tick = 0;
        public void RegistAction(Action act)
        {
            if(actionDict.ContainsKey(act))
            {
                Debug.LogError("already regist action: " + act.ToString());
            }
            else{
                actionDict.Add(act, tick);
            }
        }

        public void UnRegistAction(Action act)
        {
            actionDict.Remove(act);
        }
        private void Update() {
            tick += Time.deltaTime;
            if(tick >= 1){
                tick = 0;
                foreach(Action act in actionDict.Keys)
                {
                    act.Invoke();
                }
            }
        }

    }
}