/****************************************************************************
* Copyright 2019 Nreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.nreal.ai/        
* 
*****************************************************************************/

namespace NRKernal
{
    using System.Collections.Generic;
    using System.Threading;
    using UnityEngine;
    using System;

    /// <summary>
    /// Only works at Android runtime.
    /// </summary>
    public class AsyncTaskExecuter : SingleTon<AsyncTaskExecuter>
    {
        public static Queue<Action> m_TaskQueue = new Queue<Action>();

#if !UNITY_EDITOR
        public AsyncTaskExecuter()
        {
            Thread thread = new Thread(RunAsyncTask);
            thread.IsBackground = true;
            thread.Name = "AsyncTaskExecuter";
            thread.Start();
        }

        private void RunAsyncTask()
        {
            while (true)
            {
                Thread.Sleep(10);
                if (m_TaskQueue.Count != 0)
                {
                    lock (m_TaskQueue)
                    {
                        var task = m_TaskQueue.Dequeue();
                        try
                        {
                            task?.Invoke();
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("[AsyncTaskExecuter] Execute async task error:" + e.ToString());
                            throw;
                        }
                    }
                }
            }
        }
#endif

        public void RunAction(Action task)
        {
            lock (m_TaskQueue)
            {
#if !UNITY_EDITOR
                m_TaskQueue.Enqueue(task);
#else
                task?.Invoke();
#endif
            }
        }
    }
}