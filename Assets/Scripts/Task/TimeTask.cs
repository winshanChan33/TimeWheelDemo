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
            if (m_loopTimes > 0 && m_curScheduleTimes >= m_loopTimes || m_interval == 0)
            {
                return null;
            }

            m_curScheduleTimes++;
            var interval = m_initFlag && m_delay > 0 ? m_delay : m_interval;
            m_initFlag = false;
            return DateTime.Now.AddSeconds(interval);
        }

    }
}
