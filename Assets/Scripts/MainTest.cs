using System;
using TimeWheelDemo;
using UnityEngine;
using UnityEngine.Profiling;

public class MainTest : MonoBehaviour
{
    private const float k_TimeSpan = 1f;
    private TimeWheelMgr m_TimeWheelInstance;
    private DateTime m_LastStepTime;

    public bool checkFunc = false;
    private bool m_CheckFuncFlag = false;

    public bool checkStress = false;
    private bool m_CheckStrFlag = false;

    private void Awake()
    {
        m_TimeWheelInstance = new TimeWheelMgr(k_TimeSpan);
        m_LastStepTime = DateTime.Now;
        Debug.Log("初始化时间轮，间隔为" + k_TimeSpan + "秒");
    }

    private void Update()
    {
        if ((DateTime.Now - m_LastStepTime).TotalSeconds >= k_TimeSpan)
        {
            Profiler.BeginSample("ExcuteTimeTask");
            m_TimeWheelInstance.Step();
            Profiler.EndSample();

            m_LastStepTime =  DateTime.Now;
        }

        if (checkFunc && !m_CheckFuncFlag)
        {
            // CheckFunc();
            m_CheckFuncFlag = true;
        }

        if (checkStress && !m_CheckStrFlag)
        {
            CheckStress();
            m_CheckStrFlag = true;
        }
    }

    private void CheckStress()
    {
        Profiler.BeginSample("AddTimeTask");
        Debug.Log("time flag");
        for (int i = 0; i < 1; i++)
        {
            m_TimeWheelInstance.SetInterval(2, (_, _, _) => { Debug.Log("执行一个2s定时器"); }, 0, -1);     // 创建100万个循环定时器
        }
        Profiler.EndSample();
    }

    private void CheckFunc()
    {
        #region 添加、删除定时器
        // 执行一个间隔2s，只执行5次的定时器
        m_TimeWheelInstance.SetInterval(2, (id, _, _) => { Debug.Log("定时器1：正在执行间隔2s，只执行5次的定时器"); }, 0, 5);
        
        // 延迟执行方法
        m_TimeWheelInstance.SetDelay(5, () => { Debug.Log("定时器2：延迟5s执行的方法"); });

        // 执行间隔为1s的定时器
        int tick = 0;
        m_TimeWheelInstance.SetInterval(1, (id, _, _) =>
        {
            tick++;     // 在定时回调中执行到一定条件后取消定时器
            Debug.Log("定时器3：执行次数：" + tick);
            if (tick >= 5)
            {
                m_TimeWheelInstance.RemoveInterval(id);
                Debug.Log("定时器3：执行次数达到上限，删除定时器");
            }
        });
        #endregion

        # region 修改定时器
        int modifyTick = 0;
        var scheduleId = m_TimeWheelInstance.SetInterval(5, (id, _, _) =>
        {
            Debug.Log("定时器4：正在执行间隔5s");

            modifyTick++;
            if (modifyTick >= 5)        // 执行5次之后，修改时间间隔为2s
            {
                m_TimeWheelInstance.ModifyTaskInterval(id, 2);
                m_TimeWheelInstance.ModifyTaskAction(id, (id, _, _) => { Debug.Log("定时器4：正在执行间隔2s"); });
            }
        });
        m_TimeWheelInstance.SetDelay(50, () =>
        {
            Debug.Log("在定时器4添加之后，延迟50s将定时器4的执行次数修改为5次");
            m_TimeWheelInstance.ModifyTaskLoopTimes(scheduleId, 5);
        });
        #endregion

        #region 透传参数
        m_TimeWheelInstance.SetDelayWithParams(2, (args1, _) =>
        {
            Debug.Log("回调中透传参数：" + args1);
        }, "param1");
        #endregion
    }
}
