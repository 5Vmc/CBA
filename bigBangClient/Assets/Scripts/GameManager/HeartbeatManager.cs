using System;
using System.Collections.Generic;
using Babu;
using UnityEngine;
using Utils;

namespace BigBang
{
    public interface IHeartbeat
    {
        void OnHeartbeatUpdate(TimeSpan dif);
    }

    public abstract class HeartbeatBase : IHeartbeat
    {
        //心跳频率
        private float _delta;
        private float _timeUnit;

        protected HeartbeatBase(float timeUnit)
        {
            _timeUnit = timeUnit;
        }

        public void OnHeartbeatUpdate(TimeSpan dif)
        {
            _delta += Time.deltaTime;
            if (_delta >= _timeUnit)
            {
                _delta -= _timeUnit;
                OnHeartbeat();
            }
        }

        protected abstract void OnHeartbeat();
    }

    public class IncomeHeartbeat : HeartbeatBase
    {
        public IncomeHeartbeat(float timeUnit) : base(timeUnit)
        {
        }

        protected override void OnHeartbeat()
        {
            Player.TrainManager.CheckAllIncome(); //这个都是本地的，没有网络请求
        }
    }

    public class SyncTrainHeartbeat : HeartbeatBase
    {
        public SyncTrainHeartbeat(float timeUnit) : base(timeUnit)
        {
        }

        protected override void OnHeartbeat()
        {
            Player.TrainManager.SyncTrainEvents();
        }
    }

    public class SyncHandshakeHeartbeat : HeartbeatBase
    {
        private int failCount = 0;
        private bool stop = false;
        public SyncHandshakeHeartbeat(float timeUnit) : base(timeUnit)
        {
        }

        protected override void OnHeartbeat()
        {
            if (stop == true) return;
            this.failCount++;
            if(failCount != 1)Debug.Log("心跳超时 , this.failCount = " + this.failCount);
            if (failCount > 1)
            {
                
            }
            if (this.failCount >= 3)
            {
                stop = true;
                Debug.Log("因心跳超时，断开连接");
                EventManager.Instance.Dispatch(EventID.HEART_BEAT_OVERTIME);
            }
            NetworkManager.Instance.SyncHandshakeHeartbeat((resp) =>
            {
                //DataConvUtil.SetServerTime(resp.ServerTime);
                this.failCount = 0;
            });
        }

    }


    /// <summary>
    /// A manager class that can handle subscribing and unsubscribeing in the same update.
    /// </summary>
    public sealed class HeartbeatManager : BabuSingleton<HeartbeatManager>
    {
        private List<IHeartbeat> Heartbeats = new List<IHeartbeat>();
        private IHeartbeat[] UpdateArray;
        private DateTime LastUpdate = DateTime.MinValue;

        private bool starting = false;

        public void Subscribe(IHeartbeat heartbeat)
        {
            lock (Heartbeats)
            {
                if (!Heartbeats.Contains(heartbeat))
                    Heartbeats.Add(heartbeat);
            }
        }

        public void Unsubscribe(IHeartbeat heartbeat)
        {
            lock (Heartbeats)
                Heartbeats.Remove(heartbeat);
        }

        public void ClearAllSubscribe()
        {
            lock (Heartbeats)
                Heartbeats.Clear();
        }

        public void Update()
        {
            if (LastUpdate == DateTime.MinValue)
                LastUpdate = DateTime.UtcNow;
            else
            {
                TimeSpan dif = DateTime.UtcNow - LastUpdate;
                LastUpdate = DateTime.UtcNow;

                int count = 0;

                lock (Heartbeats)
                {
                    if (UpdateArray == null || UpdateArray.Length < Heartbeats.Count)
                        Array.Resize(ref UpdateArray, Heartbeats.Count);

                    Heartbeats.CopyTo(0, UpdateArray, 0, Heartbeats.Count);

                    count = Heartbeats.Count;
                }

                for (int i = 0; i < count; ++i)
                {
                    try
                    {
                        UpdateArray[i].OnHeartbeatUpdate(dif);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}