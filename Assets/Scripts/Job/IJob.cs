using System;

namespace TimeWheel
{
    public interface IJob
    {
        public long ID { get; }      // 任务唯一ID
        public void Excute();
        public void Cancel();
        public DateTime? GetNextTime();
        public void ModifyExcute(Action<long, object, object> action, object args1 = default, object args2 = default);
        public void ModifyTaskParams(params object[] args);
    }
}
