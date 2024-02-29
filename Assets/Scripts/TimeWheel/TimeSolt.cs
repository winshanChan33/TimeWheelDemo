using System;
using System.Collections.Generic;

namespace TimeWheelDemo
{
    public class TimeSolt
    {
        public LinkedList<IJob> TimeTasks { get { return m_TimeTasks; } }
        private LinkedList<IJob> m_TimeTasks = new();

        public void AddTask(IJob task)
        {
            m_TimeTasks.AddLast(task);
        }

        public IJob RemoveTask()
        {
            var task = m_TimeTasks.First.Value;
            m_TimeTasks.RemoveFirst();
            return task;
        }

        public void RemoveTask(IJob task)
        {
            m_TimeTasks.Remove(task);
        }

        // 执行该时间槽内所有任务
        public void FlushTasks(Action<IJob> func)
        {
            while (m_TimeTasks.Count > 0)
            {
                var task = RemoveTask();
                func(task);
            }
        }

        // TODO 同一时刻执行任务数多，遍历速率低问题？
        public IJob FilterTask(long id)
        {
            foreach (var task in m_TimeTasks)
            {
                if (task.ID == id)
                {
                    return task;
                }
            }

            return null;
        }
    }
}
