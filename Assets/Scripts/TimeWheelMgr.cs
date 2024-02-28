using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TimeWheel
{
    public class TimeWheelMgr
    {
        // 时间调度列表，ts->任务id列表 映射
        private ConcurrentDictionary<long, HashSet<string>> m_timeTasks = new();
        // 任务列表，任务id->Job 映射
        private ConcurrentDictionary<string, IJob> m_scheduleTasks = new();
        private bool m_isRunning = false;

        private long m_idSeed = 0;      // 内部自增ID

        public void Run()
        {
            m_isRunning = true;
            Task.Run(() =>
            {
                // 驱动时间轮转动，由于while会阻塞主线程，所以开启一个新的线程
                while(m_isRunning)
                {
                    var timeStamp = GetTimeStamp(DateTime.Now);
                    Task.Run(() =>
                    {
                        Stopwatch watch = new();
                        watch.Start();
                        Trigger(timeStamp);
                        watch.Stop();
                        // UnityEngine.Debug.Log("执行100万个定时器耗时：" + watch.Elapsed.TotalMilliseconds + " s");
                    });
                    
                    // 修正时间间隔
                    var offset = 500 - DateTime.Now.Millisecond;
                    SpinWait.SpinUntil(() => false, 1000 + offset);     // 毫秒
                }
            });
        }

        public void Stop()
        {
            m_isRunning = false;
        }

        private void Trigger(long timeStamp)
        {
            var lastTs = timeStamp - 1;
            var oldList = m_timeTasks.Keys.Where(t => t <= lastTs).ToList();
            foreach (var item in oldList)
            {
                m_timeTasks.TryRemove(item, out _);
            }
            m_timeTasks.TryGetValue(timeStamp, out var result);
            if (result?.Any() == true)
            {
                foreach (var id in result)
                {
                    if (m_scheduleTasks.TryGetValue(id, out IJob job))
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
        private void AddTimeTask(DateTime dateTime, string id)
        {
            var timeStamp = GetTimeStamp(dateTime);
            m_timeTasks.AddOrUpdate(timeStamp, new HashSet<string> { id }, (k, v) => 
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
            if (m_scheduleTasks.ContainsKey(job.ID))
            {
                throw new ArgumentException($"任务：{job.ID} 重复");
            }
            else
            {
                m_scheduleTasks.TryAdd(job.ID, job);
            }
            var nextTime = job.GetNextTime();
            if (nextTime.HasValue && nextTime >= DateTime.Now)
            {
                AddTimeTask(nextTime.Value, job.ID);
            }
        }

        #region 外部接口
        public void DelayInvoke(int delay, Action<string> callback)
        {
            AddScheduleTask(1, callback, delay, 1);     // interval传入1，避免NextTime返回空值
        }

        public IJob AddScheduleTask(int interval, Action<string> callback, int delay = 0, int loopTimes = -1)
        {
            m_idSeed++;
            IJob job = new Job("TimeTask" + m_idSeed, new TimeTask(interval, delay, loopTimes), callback);
            AddTask(job);
            return job;
        }

        public void RemoveScheduleTask(string id)
        {
            var ids = m_scheduleTasks.Values.Where(t => t.ID == id)?.Select(t => t.ID).ToList();
            if (ids.Any() == true)
            {
                foreach (var rid in ids)
                {
                    if (m_scheduleTasks.TryGetValue(rid, out IJob job))
                    {
                        job.Cancel();
                        m_scheduleTasks.TryRemove(rid, out _);
                    }
                }
            }
        }

        /*
            修改定时任务属性
        */
        public void ModifyScheduleTask(string id, int interval, int loopTimes = -1, Action callback = null)
        {
            // todo
        }

        #endregion
    }
}