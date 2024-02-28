using System;

namespace TimeWheel
{
    public class Job : IJob
    {
        private long m_Id;
        public long ID
        {
            get { return m_Id; }
        }

        private Action<long, object, object> m_Action;                // 任务执行回调
        private object m_Args1;
        private object m_Args2;
        private IScheduleTask m_ScheduleTask;   // 时间调度任务
        private bool m_IsCancel = false;

        public Job(long id, IScheduleTask task, Action<long, object, object> action = null, object args1 = default, object args2 = default)
        {
            m_Id = id;
            m_Action = action;
            m_Args1 = args1;
            m_Args2 = args2;
            m_ScheduleTask = task;
        }

        public void Cancel()
        {
            m_IsCancel = true;
        }

        public void Excute()
        {
            if (!m_IsCancel)
            {
                m_Action?.Invoke(ID, m_Args1, m_Args2);
            }
        }

        public DateTime? GetNextTime()
        {
            return m_ScheduleTask.GetNextTime();
        }

        public void ModifyTaskParams(params object[] args)
        {
            m_ScheduleTask.ModifyParams(args);
        }

        public void ModifyExcute(Action<long, object, object> action, object args1, object args2)
        {
            m_Action = action;
            m_Args1 = args1;
            m_Args2 = args2;
        }
    }
}
