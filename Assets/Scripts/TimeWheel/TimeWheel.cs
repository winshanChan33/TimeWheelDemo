using System;

namespace TimeWheelDemo
{
    public class TimeWheel
    {
        private TimeSolt[] m_Solts;
        private int m_SoltCnt;
        private int m_WheelSpan;            // 一个时间间隔跨度
        private TimeWheel m_NextWheel;      // 当前时间轮指向下一个时间轮
        private int m_CurNeedle = 0;        // 当前时间轮指针
        private DateTime m_StartTime;       // 初始时间

        public TimeWheel(int span, int soltCnt, DateTime startTime, TimeWheel nextWheel = null)
        {
            m_WheelSpan = span;

            // 初始化时间轮
            m_SoltCnt = soltCnt;
            m_Solts = new TimeSolt[soltCnt];
            for (int i = 0; i < soltCnt; i++)
            {
                m_Solts[i] = new TimeSolt();
            }

            m_StartTime = startTime;
            m_NextWheel = nextWheel;
        }

        public bool AddTask(IJob job)
        {
            var nextTime = job.GetNextTime();
            if (nextTime < m_StartTime.AddSeconds(m_CurNeedle * m_WheelSpan))
            {
                return false;
            }

            if (nextTime < m_StartTime.AddSeconds(m_SoltCnt * m_WheelSpan))
            {
                // 计算定时任务所属时间槽
                var deltaSecond = (nextTime - m_StartTime).TotalSeconds;
                int targetNeedle = (int)deltaSecond / m_WheelSpan;
                m_Solts[targetNeedle - 1].AddTask(job);
            }
            else
            {
                // 超出当前时间轮，放入下一轮
                m_NextWheel?.AddTask(job);
            }
            return true;
        }

        // 转动一次时间轮
        public void Step(DateTime time, Action<IJob> func)
        {
            if (time >= m_StartTime.AddSeconds(m_CurNeedle * m_WheelSpan))
            {
                var needle = (int)(time - m_StartTime).TotalSeconds / m_WheelSpan;
                if (needle != m_CurNeedle)
                {
                    m_CurNeedle = needle;
                    if (m_CurNeedle > 0 && m_CurNeedle <= m_SoltCnt)
                    {
                        m_Solts[m_CurNeedle - 1].FlushTasks(func);
                    }

                    if (needle >= m_SoltCnt)
                    {
                        // 转完一个轮
                        m_StartTime = m_StartTime.AddSeconds(m_SoltCnt * m_WheelSpan);
                    }
                }

                // 同时推进下一轮转动
                m_NextWheel?.Step(time, func);
            }
        }
    }
}
