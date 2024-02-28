using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TimeWheel
{
    public class TimeWheelMgr
    {
        // 时间调度列表，ts->任务id列表 映射
        private ConcurrentDictionary<long, HashSet<long>> m_TimeTasksMap = new();
        private List<long> m_DelTaskList = new();
        // 任务列表，任务id->Job 映射
        private ConcurrentDictionary<long, IJob> m_ScheduleTasksMap = new();
        private bool m_IsRunning = false;

        private long m_IdSeed = 0;      // 内部自增ID

        public void Run()
        {
            m_IsRunning = true;
            Task.Run(() =>
            {
                // 驱动时间轮转动，由于while会阻塞主线程，所以开启一个新的线程
                while(m_IsRunning)
                {
                    var timeStamp = GetTimeStamp(DateTime.Now);
                    Trigger(timeStamp);
                    // 修正时间间隔
                    var offset = 500 - DateTime.Now.Millisecond;
                    SpinWait.SpinUntil(() => false, 1000 + offset);     // 毫秒
                }
            });
        }

        public void Stop()
        {
            m_IsRunning = false;
        }

        private void Trigger(long timeStamp)
        {
            var lastTs = timeStamp - 1;
            m_DelTaskList.Clear();
            foreach (var item in m_TimeTasksMap)
            {
                if (item.Key <= lastTs)
                {
                    m_DelTaskList.Add(item.Key);
                }
            }
            
            m_TimeTasksMap.TryGetValue(timeStamp, out var result);
            if (result?.Count > 0)
            {
                foreach (var id in result)
                {
                    if (m_ScheduleTasksMap.TryGetValue(id, out IJob job))
                    {
                        Task.Run(() => job.Excute());
                        var nextTime = job.GetNextTime();
                        if (nextTime.HasValue && nextTime >= DateTime.Now)
                        {
                            AddTimeTask(nextTime.Value, id);
                        }
                        else
                        {
                            // todo 定时失效的任务会执行进行删除
                            RemoveScheduleTask(job.ID);
                        }
                    }
                }
            }
        }


        // 加入时间轮转队列中
        private void AddTimeTask(DateTime dateTime, long id)
        {
            var timeStamp = GetTimeStamp(dateTime);
            m_TimeTasksMap.AddOrUpdate(timeStamp, new HashSet<long> { id }, (k, v) => 
            {
                v.Add(id);
                return v;
            });
        }

        private long GetTimeStamp(DateTime dateTime)
        {
            return new DateTimeOffset(dateTime.ToUniversalTime()).ToUnixTimeSeconds();
        }

        private void AddTask(IJob job)
        {
            if (m_ScheduleTasksMap.ContainsKey(job.ID))
            {
                throw new ArgumentException($"任务：{job.ID} 重复");
            }
            else
            {
                m_ScheduleTasksMap.TryAdd(job.ID, job);
            }
            var nextTime = job.GetNextTime();
            if (nextTime.HasValue && nextTime >= DateTime.Now)
            {
                AddTimeTask(nextTime.Value, job.ID);
            }
        }

        #region 外部接口
        public void DelayInvoke(int delay, Action callback)
        {
            AddScheduleTask(1, (id, _, _) => { callback.Invoke(); }, delay, 1);     // interval传入1，避免NextTime返回空值
        }

        public void DelayInvokeWithParams(int delay, Action<object, object> callback, object args1 = default, object args2 = default)
        {
            AddScheduleTask(1, (id, args1, args2) => { callback.Invoke(args1, args2); }, delay, 1, args1, args2);
        }

        public long AddScheduleTask(int interval, Action<long, object, object> callback, int delay = 0, int loopTimes = -1, object args1 = default, object args2 = default)
        {
            m_IdSeed++;
            IJob job = new Job(m_IdSeed, new TimeTask(interval, delay, loopTimes), callback, args1, args2);
            AddTask(job);
            return job.ID;
        }

        public void RemoveScheduleTask(long id)
        {
            if (m_ScheduleTasksMap.TryGetValue(id, out IJob job))
            {
                job.Cancel();
                m_ScheduleTasksMap.TryRemove(id, out _);
            }
        }

        /*
            修改定时任务属性
        */
        public void ModifyScheduleTaskInterval(long id, int interval)
        {
            if (m_ScheduleTasksMap.TryGetValue(id, out IJob job))
            {
                job.ModifyTaskParams(interval);
            }
        }

        public void ModifyScheduleTaskLoopTimes(long id, int loopTimes)
        {
            if (m_ScheduleTasksMap.TryGetValue(id, out IJob job))
            {
                job.ModifyTaskParams(-1, loopTimes);
            }
        }

        public void ModifyScheduleTaskAction(long id, Action<long, object, object> callback)
        {
            if (m_ScheduleTasksMap.TryGetValue(id, out IJob job))
            {
                job.ModifyExcute(callback);
            }
        }

        #endregion
    }
}
