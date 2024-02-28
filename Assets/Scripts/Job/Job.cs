using System;

namespace TimeWheel
{
    public class Job : IJob
    {
        private string m_id;
        public string ID
        {
            get { return m_id; }
        }

        private Action<string> m_action;                // 任务执行回调
        private IScheduleTask m_scheduleTask;   // 时间调度任务
        private bool m_isCancel = false;

        public Job(string id, IScheduleTask task, Action<string> action = null)
        {
            m_id = id;
            m_action = action;
            m_scheduleTask = task;
        }

        public void Cancel()
        {
            m_isCancel = true;
        }

        public void Excute()
        {
            if (!m_isCancel)
            {
                m_action?.Invoke(ID);
            }
        }

        public DateTime? GetNextTime()
        {
            return m_scheduleTask.GetNextTime();
        }

    }
}
