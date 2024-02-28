using System;

namespace TimeWheel
{
    /*
        时间调度任务，普通任务
        间隔时间定时器
    */
    public class TimeTask : IScheduleTask
    {
        private int m_Interval;
        private int m_Delay;
        private bool m_InitFlag;
        private int m_LoopTimes;
        private int m_CurScheduleTimes;     // 当前调度次数

        public TimeTask(int interval, int delay = 0, int loopTimes = -1)
        {
            m_Interval = interval;
            m_Delay = delay;
            m_InitFlag = true;
            m_LoopTimes = loopTimes;
            m_CurScheduleTimes = 0;
        }
        public DateTime? GetNextTime()
        {
            m_CurScheduleTimes++;
            if (m_LoopTimes > 0 && m_CurScheduleTimes > m_LoopTimes || m_Interval <= 0)
            {
                return null;
            }

            var interval = m_InitFlag && m_Delay > 0 ? m_Delay : m_Interval;
            m_InitFlag = false;
            return DateTime.Now.AddSeconds(interval);
        }

        public void ModifyParams(params object[] args)
        {
            int newInterval = args.Length > 0 ? (int)args[0] : -1;
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
