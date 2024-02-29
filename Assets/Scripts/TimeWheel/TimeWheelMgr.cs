using System;

namespace TimeWheelDemo
{
    public class TimeWheelMgr
    {
        private const int k_SecondPerMintue = 60;
        private const int k_MinutePerHours = 60;
        private const int k_HoursPerDay = 24;
        private const int k_MilliSecondLevel = 20;
        private const int k_MilliSecondDelta = 50;

        private TimeWheel m_TimeWheel;

        private float m_TickSpan;
        private DateTime m_CurTime;
        private DateTime m_StartTime;

        private long m_IdSeed = 0;      // 内部自增ID

        public TimeWheelMgr(float tickSpan)
        {
            m_TickSpan = tickSpan;
            m_StartTime = DateTime.Now;
            m_CurTime = m_StartTime;

            InitTimeWheel();
        }

        private void InitTimeWheel()
        {
            var hourWheel = new TimeWheel((int)m_TickSpan * k_SecondPerMintue * k_MinutePerHours, k_HoursPerDay, m_StartTime);
            var minuteWheel = new TimeWheel((int)m_TickSpan * k_SecondPerMintue, k_MinutePerHours, m_StartTime, hourWheel);
            m_TimeWheel = new TimeWheel((int)m_TickSpan, k_SecondPerMintue, m_StartTime, minuteWheel);
        }
        
        private void AddTask(IJob job)
        {
            var addStatus = m_TimeWheel.AddTask(job);
            if (!addStatus)
            {
                // 过期任务需要执行
                job.Excute();
                if (job.CheckLoop())
                {
                    AddTask(job);
                }
            }
        }

        private bool RemoveTask(TimeSolt[] wheel, long id)
        {
            foreach (var solt in wheel)
            {
                foreach (var task in solt.TimeTasks)
                {
                    if (task.ID == id)
                    {
                        solt.RemoveTask(task);
                        return true;
                    }
                }
            }

            return false;
        }

        private IJob FilterTask(TimeSolt[] wheel, long id)
        {
            foreach (var solt in wheel)
            {
                var targetTask = solt.FilterTask(id);
                if (targetTask != null)
                {
                    return targetTask;
                }
            }

            return null;
        }

        private IJob FilterWheelTask(long id)
        {
            // todo
            // IJob targetTask = FilterTask(m_HourWheel, id);
            // if (targetTask != null)
            //     return targetTask;
            
            // targetTask = FilterTask(m_MinuteWheel, id);
            // if (targetTask != null)
            //     return targetTask;
            
            // targetTask = FilterTask(m_SecondWheel, id);
            // return targetTask;
            return null;
        }

        #region 外部接口
        
        // 由外部驱动时间轮转动
        public void Step()
        {
            m_CurTime = m_CurTime.AddSeconds(m_TickSpan);
           
            m_TimeWheel.Step(m_CurTime, AddTask);
        }

        public long SetInterval(float interval, Action<long, object, object> callback, float delay = 0, int loopTimes = -1, object args1 = default, object args2 = default)
        {
            m_IdSeed++;
            IJob job = new Job(m_IdSeed, new TimeTask(interval, delay, loopTimes), callback, args1, args2);
            AddTask(job);
            return job.ID;
        }

        public void SetDelay(float delay, Action callback)
        {
            SetInterval(0, (id, _, _) => { callback.Invoke(); }, delay, 1);
        }

        public void SetDelayWithParams(float delay, Action<object, object> callback, object args1 = default, object args2 = default)
        {
            SetInterval(0, (id, args1, args2) => { callback.Invoke(args1, args2); }, delay, 1, args1, args2);
        }

        public void RemoveInterval(long id)
        {
            // todo
            // if (RemoveTask(m_HourWheel, id)) 
            //     return;
            // if (RemoveTask(m_MinuteWheel, id))
            //     return;
            // if (RemoveTask(m_SecondWheel, id))
            //     return;
        }

        /*
            修改定时任务属性
        */
        public void ModifyTaskInterval(long id, float interval)
        {
            var targetTask = FilterWheelTask(id);
            if (targetTask != null)
            {
                targetTask.ModifyTaskParams(interval);
            }
        }

        public void ModifyTaskLoopTimes(long id, int loopTimes)
        {
            var targetTask = FilterWheelTask(id);
            if (targetTask != null)
            {
                targetTask.ModifyTaskParams(-1, loopTimes);
            }
        }

        public void ModifyTaskAction(long id, Action<long, object, object> callback)
        {
            var targetTask = FilterWheelTask(id);
            if (targetTask != null)
            {
                targetTask.ModifyExcute(callback);
            }
        }

        #endregion
    }
}
