using TimeWheel;
using UnityEngine;

public class MainTest : MonoBehaviour
{
    private TimeWheelMgr m_timeWheelInstance;

    private void Awake()
    {
        m_timeWheelInstance = new TimeWheelMgr();
        m_timeWheelInstance.Run();

        // TimeWheelFuncTest();
        TimeWheelStressTest();
    }

    private void TimeWheelStressTest()
    {
        // Stopwatch watch = new();
        // watch.Start();
        for (int i = 0; i < 1000000; i++)
        {
            m_timeWheelInstance.AddScheduleTask(1, (id) => {});
        }
        // watch.Stop();
        // UnityEngine.Debug.Log("添加100万个定时器运行耗时：" + watch.Elapsed.TotalMilliseconds + " s");
    }

    private void TimeWheelFuncTest()
    {
        #region 添加、删除定时器
        // 执行一个间隔2s，只执行5次的定时器
        m_timeWheelInstance.AddScheduleTask(2, (id) => { UnityEngine.Debug.Log("定时器1：正在执行间隔2s，只执行5次的定时器"); }, 0, 5);
        
        // 延迟执行方法
        m_timeWheelInstance.DelayInvoke(5, (id) => { UnityEngine.Debug.Log("定时器2：延迟5s执行的方法"); });

        // 执行间隔为1s的定时器
        int tick = 0;
        m_timeWheelInstance.AddScheduleTask(1, (id) =>
        {
            tick++;     // 在定时回调中执行到一定条件后取消定时器
            UnityEngine.Debug.Log("定时器3：执行次数：" + tick);
            if (tick >= 5)
            {
                m_timeWheelInstance.RemoveScheduleTask(id);
                UnityEngine.Debug.Log("定时器3：执行次数达到上限，删除定时器");
            }
        });
        #endregion

        # region 修改定时器

        #endregion
    }
}
