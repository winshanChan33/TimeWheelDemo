using System.Diagnostics;
using TimeWheel;
using UnityEngine;

public class MainTest : MonoBehaviour
{
    public bool runFuncTest = false;
    private bool m_RunFuncFlag = false;
    public bool runStressTest = false;
    private bool m_RunStressFlag = false;

    private TimeWheelMgr m_TimeWheelInstance;

    private void Awake()
    {
        m_TimeWheelInstance = new TimeWheelMgr();
        m_TimeWheelInstance.Run();
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (runFuncTest && !m_RunFuncFlag)
        {
            TimeWheelFuncTest();
            m_RunFuncFlag = true;     // 限制只执行一次，避免重复创建定时器，TODO 清空队列？
        }

        if (runStressTest)
        {
            if (!m_RunStressFlag)
            {
                TimeWheelStressTest();      // 此处测试添加定时器耗时，只执行一次
                m_RunStressFlag = true;
            }
        }
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
            m_TimeWheelInstance.AddScheduleTask(1, (id, _, _) => {});
        }
    }

    private void TimeWheelFuncTest()
    {
        #region 添加、删除定时器
        // 执行一个间隔2s，只执行5次的定时器
        m_TimeWheelInstance.AddScheduleTask(2, (id, _, _) => { UnityEngine.Debug.Log("定时器1：正在执行间隔2s，只执行5次的定时器"); }, 0, 5);
        
        // 延迟执行方法
        m_TimeWheelInstance.DelayInvoke(5, () => { UnityEngine.Debug.Log("定时器2：延迟5s执行的方法"); });

        // 执行间隔为1s的定时器
        int tick = 0;
        m_TimeWheelInstance.AddScheduleTask(1, (id, _, _) =>
        {
            tick++;     // 在定时回调中执行到一定条件后取消定时器
            UnityEngine.Debug.Log("定时器3：执行次数：" + tick);
            if (tick >= 5)
            {
                m_TimeWheelInstance.RemoveScheduleTask(id);
                UnityEngine.Debug.Log("定时器3：执行次数达到上限，删除定时器");
            }
        });
        #endregion

        # region 修改定时器
        int modifyTick = 0;
        var scheduleId = m_TimeWheelInstance.AddScheduleTask(5, (id, _, _) =>
        {
            UnityEngine.Debug.Log("定时器4：正在执行间隔5s");

            modifyTick++;
            if (modifyTick >= 5)        // 执行5次之后，修改时间间隔为2s
            {
                m_TimeWheelInstance.ModifyScheduleTaskInterval(id, 2);
                m_TimeWheelInstance.ModifyScheduleTaskAction(id, (id, _, _) => { UnityEngine.Debug.Log("定时器4：正在执行间隔2s"); });
            }
        });
        m_TimeWheelInstance.DelayInvoke(50, () =>
        {
            UnityEngine.Debug.Log("在定时器4添加之后，延迟50s将定时器4的执行次数修改为5次");
            m_TimeWheelInstance.ModifyScheduleTaskLoopTimes(scheduleId, 5);
        });
        #endregion

        #region 透传参数
        m_TimeWheelInstance.DelayInvokeWithParams(2, (args1, _) =>
        {
            UnityEngine.Debug.Log("回调中透传参数：" + args1);
        }, "param1");
        #endregion
    }
}
