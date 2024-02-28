using System;

namespace TimeWheel
{
    public interface IJob
    {
        public string ID { get; }      // 任务唯一ID
        public void Excute();
        public void Cancel();
        public DateTime? GetNextTime();
    }
}
