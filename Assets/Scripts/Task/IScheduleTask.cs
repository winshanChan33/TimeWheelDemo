using System;

namespace TimeWheelDemo
{
    // 时间调度方式 接口
    public interface IScheduleTask
    {
        public DateTime DateTime { get; }
        public bool CheckLoop();
        public void ModifyParams(params object[] args);         // 定时器参数修改器
    }
}
