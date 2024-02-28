using System;

namespace TimeWheel
{
    // 时间调度方式 接口
    public interface IScheduleTask
    {
        public DateTime? GetNextTime();     // 获取下一个时间点
        // public void ModifyParams();         // todo
    }
}
