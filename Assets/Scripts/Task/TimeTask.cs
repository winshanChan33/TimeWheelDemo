using System;

namespace TimeWheel
{
    /*
        时间调度任务，普通任务
        间隔时间定时器
    */
    public class TimeTask : IScheduleTask
    {
        private int m_interval;
        private int m_delay;
        private bool m_initFlag;
        private int m_loopTimes;
        private int m_curScheduleTimes;     // 当前调度次数

        public TimeTask(int interval, int delay = 0, int loopTimes = -1)
        {
            m_interval = interval;
            m_delay = delay;
            m_initFlag = true;
            m_loopTimes = loopTimes;
            m_curScheduleTimes = 0;
        }
        public DateTime? GetNextTime()
        {
            m_curScheduleTimes++;
            if (m_loopTimes > 0 && m_curScheduleTimes > m_loopTimes || m_interval <= 0)
            {
                return null;
            }

            var interval = m_initFlag && m_delay > 0 ? m_delay : m_interval;
            m_initFlag = false;
            return DateTime.Now.AddSeconds(interval);
        }

        public void ModifyParams(params object[] args)
        {
            int newInterval = args.Length > 0 ? (int)args[0] : -1;
            int newLoopTimes = args.Length > 1 ? (int)args[1] : -2;     // 随机指定一个负数，区别于-1为无限执行
            if (newInterval != -1 && newInterval != m_interval)
            {
                m_interval = newInterval;
            }
            if ((newLoopTimes == -1 || newLoopTimes > 0) && newLoopTimes != m_loopTimes)
            {
                m_loopTimes = newLoopTimes;
                m_curScheduleTimes = 0;     // 重新计算执行次数
            }
        }
    }
}
