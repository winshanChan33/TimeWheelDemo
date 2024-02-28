using System.Diagnostics;
using TimeWheel;
using UnityEngine;

public class MainTest : MonoBehaviour
{
    public bool runFuncTest = false;
    private bool m_runFuncFlag = false;
    public bool runStressTest = false;
    private bool m_runStressFlag = false;

    private TimeWheelMgr m_timeWheelInstance;

    private void Awake()
    {
        m_timeWheelInstance = new TimeWheelMgr();
        m_timeWheelInstance.Run();
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (runFuncTest && !m_runFuncFlag)
        {
            TimeWheelFuncTest();
            m_runFuncFlag = true;     // 限制只执行一次，避免重复创建定时器，TODO 清空队列？
        }

        if (runStressTest)
        {
            if (!m_runStressFlag)
            {
                TimeWheelStressTest();      // 此处测试添加定时器耗时，只执行一次
                m_runStressFlag = true;
            }
        }
        m_timeWheelInstance.CheckExctuteTime = runStressTest;
    }

    private void TimeWheelStressTest()
    {
        Stopwatch watch = new();
        watch.Start();
        AddSchedules(1000000);
        watch.Stop();
        UnityEngine.Debug.Log("添加100万个定时器运行耗时：" + watch.Elapsed.TotalMilliseconds / 1000 + "秒");
    }

    private void AddSchedules(int count)
    {
        for (int i = 0; i < count; i++)
        {
            m_timeWheelInstance.AddScheduleTask(1, (id) => {});
        }
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
        int modifyTick = 0;
        var scheduleId = m_timeWheelInstance.AddScheduleTask(5, (id) =>
        {
            UnityEngine.Debug.Log("定时器4：正在执行间隔5s");

            modifyTick++;
            if (modifyTick >= 5)        // 执行5次之后，修改时间间隔为2s
            {
                m_timeWheelInstance.ModifyScheduleTaskInterval(id, 2);
                m_timeWheelInstance.ModifyScheduleTaskAction(id, (id) => { UnityEngine.Debug.Log("定时器4：正在执行间隔2s"); });
            }
        });
        m_timeWheelInstance.DelayInvoke(50, (id) =>
        {
            UnityEngine.Debug.Log("在定时器4添加之后，延迟50s将定时器4的执行次数修改为5次");
            m_timeWheelInstance.ModifyScheduleTaskLoopTimes(scheduleId, 5);
        });
        #endregion
    }
}
