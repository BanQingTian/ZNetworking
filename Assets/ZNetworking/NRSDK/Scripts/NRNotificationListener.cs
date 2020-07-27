/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace NRKernal
{
    public class NRNotificationListener : MonoBehaviour
    {
        public enum Level
        {
            High,
            Middle,
            Low
        }

        public class Notification
        {
            protected NRNotificationListener NotificationListener;

            public Notification(NRNotificationListener listener)
            {
                this.NotificationListener = listener;
            }

            public virtual void UpdateState() { }

            public virtual void OnStateChanged(Level level)
            {
                NotificationListener.Dispath(this, level);
            }
        }

        public class LowPowerNotification : Notification
        {
            public enum PowerState
            {
                Full,
                Middle,
                Low
            }
            private PowerState currentState = PowerState.Full;

            public LowPowerNotification(NRNotificationListener listener) : base(listener)
            {
            }

            private PowerState GetStateByValue(float val)
            {
                if (val < 0.15f)
                {
                    return PowerState.Low;
                }
                else if (val < 0.3f)
                {
                    return PowerState.Middle;
                }
                return PowerState.Full;
            }

            public override void UpdateState()
            {
                //Debug.Log("[LowPowerNotification] UpdateState:" + SystemInfo.batteryLevel);
                var state = GetStateByValue(SystemInfo.batteryLevel);
                if (state != currentState)
                {
                    if (state == PowerState.Low)
                    {
                        this.OnStateChanged(Level.High);
                    }
                    else if (state == PowerState.Middle)
                    {
                        this.OnStateChanged(Level.Middle);
                    }
                    this.currentState = state;
                }
            }
        }

        public class SlamStateNotification : Notification
        {
            private enum SlamState
            {
                None,
                LostTracking,
                TrackingReady
            }
            private SlamState m_CurrentState = SlamState.None;

            public SlamStateNotification(NRNotificationListener listener) : base(listener)
            {
                NRHMDPoseTracker.OnHMDLostTracking += OnHMDLostTracking;
                NRHMDPoseTracker.OnHMDPoseReady += OnHMDPoseReady;
            }

            private void OnHMDPoseReady()
            {
                NRDebugger.Log("[SlamStateNotification] OnHMDPoseReady.");
                m_CurrentState = SlamState.TrackingReady;
            }

            private void OnHMDLostTracking()
            {
                NRDebugger.Log("[SlamStateNotification] OnHMDLostTracking.");
                if (m_CurrentState != SlamState.LostTracking)
                {
                    this.OnStateChanged(Level.Middle);
                    m_CurrentState = SlamState.LostTracking;
                }
            }
        }

        public class TemperatureLevelNotification : Notification
        {
            private GlassesTemperatureLevel currentState = GlassesTemperatureLevel.TEMPERATURE_LEVEL_NORMAL;

            public TemperatureLevelNotification(NRNotificationListener listener) : base(listener)
            {
            }

            public override void UpdateState()
            {
                base.UpdateState();

                var level = NRDevice.Instance.TemperatureLevel;
                if (currentState != level)
                {
                    if (level != GlassesTemperatureLevel.TEMPERATURE_LEVEL_NORMAL)
                    {
                        this.OnStateChanged(level == GlassesTemperatureLevel.TEMPERATURE_LEVEL_HOT
                            ? Level.High : Level.Middle);
                    }

                    this.currentState = level;
                }
            }
        }

        [Header("Whether to open the low power prompt")]
        public bool EnableLowPowerTips;
        public NRNotificationWindow LowPowerNotificationPrefab;
        [Header("Whether to open the slam state prompt")]
        public bool EnableSlamStateTips;
        public NRNotificationWindow SlamStateNotificationPrefab;
        [Header("Whether to open the over temperature prompt")]
        public bool EnableHighTempTips;
        public NRNotificationWindow HighTempNotificationPrefab;

        protected List<Notification> NotificationList = new List<Notification>();
        private Dictionary<Level, float> TipsLastTime = new Dictionary<Level, float>() {
            { Level.High,3.5f},
            { Level.Middle,2.5f},
            { Level.Low,1.5f}
        };

        public struct NotificationMsg
        {
            public Notification notification;
            public Level level;
        }
        private Queue<NotificationMsg> NotificationQueue = new Queue<NotificationMsg>();
        private float m_LockTime = 0f;

        void Awake()
        {
            LowPowerNotificationPrefab.gameObject.SetActive(false);
            SlamStateNotificationPrefab.gameObject.SetActive(false);
            HighTempNotificationPrefab.gameObject.SetActive(false);
        }

        public void Start()
        {
            DontDestroyOnLoad(gameObject);
            RegistNotification();
        }

        protected virtual void RegistNotification()
        {
            if (EnableLowPowerTips) NotificationList.Add(new LowPowerNotification(this));
            if (EnableSlamStateTips) NotificationList.Add(new SlamStateNotification(this));
            if (EnableHighTempTips) NotificationList.Add(new TemperatureLevelNotification(this));

            NRKernalUpdater.Instance.OnUpdate += OnUpdate;
        }

        private void OnUpdate()
        {
            foreach (var item in NotificationList)
            {
                item.UpdateState();
            }

            if (m_LockTime < float.Epsilon)
            {
                if (NotificationQueue.Count != 0)
                {
                    var msg = NotificationQueue.Dequeue();
                    this.OprateNotificationMsg(msg);
                    m_LockTime = TipsLastTime[msg.level];
                }
            }
            else
            {
                m_LockTime -= Time.deltaTime;
            }
        }

        public void Dispath(Notification notification, Level lev)
        {
            NotificationQueue.Enqueue(new NotificationMsg()
            {
                notification = notification,
                level = lev
            });
        }

        protected virtual void OprateNotificationMsg(NotificationMsg msg)
        {
            NRNotificationWindow prefab = null;
            Notification notification_obj = msg.notification;
            Level notification_level = msg.level;
            float duration = TipsLastTime[notification_level];

            // Notification window will not be destroyed automatic when lowpower and high level warning
            // Set it's duration to -1
            if (notification_obj is LowPowerNotification)
            {
                prefab = LowPowerNotificationPrefab;
                if (notification_level == Level.High)
                {
                    duration = -1f;
                }
            }
            else if (notification_obj is SlamStateNotification)
            {
                prefab = SlamStateNotificationPrefab;
            }
            else if (notification_obj is TemperatureLevelNotification)
            {
                prefab = HighTempNotificationPrefab;
            }

            if (prefab != null)
            {
                NRDebugger.Log("[NRNotificationListener] Dispath:" + notification_obj.GetType().ToString());
                NRNotificationWindow notification = Instantiate(prefab);
                notification.gameObject.SetActive(true);
                notification.transform.SetParent(transform);
                notification.FillData(notification_level, duration);
            }
        }
    }
}