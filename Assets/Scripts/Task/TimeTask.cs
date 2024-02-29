using System;

namespace TimeWheelDemo
{
    /*
        时间调度任务，普通任务
        间隔时间定时器
    */
    public class TimeTask : IScheduleTask
    {
        public DateTime DateTime { get { return m_DateTime; } }
        private DateTime m_DateTime;
        private float m_Interval;
        private int m_LoopTimes;
        private int m_CurScheduleTimes;     // 当前调度次数
        
        public TimeTask(float interval, float delay = 0, int loopTimes = -1)
        {
            m_Interval = interval;
            m_LoopTimes = loopTimes;
            m_CurScheduleTimes = 0;
            m_DateTime = DateTime.Now.AddSeconds((interval + delay));
        }

        public bool CheckLoop()
        {
            m_CurScheduleTimes++;
            if (m_LoopTimes > 0 && m_CurScheduleTimes >= m_LoopTimes || m_Interval <= 0)
            {
                return false;
            }

            m_DateTime = DateTime.Now.AddSeconds(m_Interval);
            return true;
        }

        public void ModifyParams(params object[] args)
        {
            float newInterval = args.Length > 0 ? (float)args[0] : -1;
            int newLoopTimes = args.Length > 1 ? (int)args[1] : -2;     // 随机指定一个负数，区别于-1为无限执行
            if (newInterval != -1 && newInterval != m_Interval)
            {
                m_Interval = newInterval;
            }
            if ((newLoopTimes == -1 || newLoopTimes > 0) && newLoopTimes != m_LoopTimes)
            {
                m_LoopTimes = newLoopTimes;
                m_CurScheduleTimes = 0;     // 重新计算执行次数
            }
        }
    }
}
