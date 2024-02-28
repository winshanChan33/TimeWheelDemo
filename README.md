# 基于时间轮的定时器实现

## 使用

在初始化时需要实例化 `TimeWheelMgr`。

提供的外部接口包括：

1. `DelayInvoke(int delay, Action callback)`: 延迟执行回调。参数 `delay` 表示延迟的秒数，`callback` 是回调执行的逻辑。
2. `AddScheduleTask(int interval, Action<string> callback)`: 增加一个定时器。参数 `interval` 表示时间间隔，`callback` 是执行回调的方法。返回值为定时器的唯一ID，可用于后续对该定时器进行修改。
3. `RemoveScheduleTask(string id)`: 移除指定ID的定时器。
4. 修改定时器相关接口：
   - `ModifyScheduleTaskInterval(string id, int interval)`: 修改定时器的时间间隔。
   - `ModifyScheduleTaskLoopTimes(string id, int loopTimes)`: 修改定时器的执行次数。
   - `ModifyScheduleTaskAction(string id, Action<string> callback)`: 修改定时器的执行回调。

## 结构

主要由 `TimeWheelMgr` 驱动，调用其中的 `Run` 方法开始驱动时间轮，另外开启线程在 `while` 循环中执行时间轮的转动。

主要依赖两个存储结构：

- 任务映射列表 `ConcurrentDictionary<string, IJob> m_scheduleTasks`
- 时间调度列表 `ConcurrentDictionary<long, HashSet<string>> m_timeTasks`

转动一次称之为一个 tick，每个 tick 检测任务列表中是否存在合法定时器任务，如果存在，则将其添加到时间调度列表中，在下一个 tick 中执行。

其中定时器任务设计了接口 `IJob`，创建 `Job` 类实现 `IJob` 接口，包含了定时器任务的唯一ID、执行回调、定时器实例等信息。

定时器类设计了 `IScheduleTask` 接口，创建普通定时器类 `TimeTask` 实现 `IScheduleTask` 接口。其中包含了时间间隔、执行次数、延迟执行描述的定义和定时器任务合法性检测方法。

根据以上内容整理，可生成结构化的 Git 项目 README.md 文件，以提高可读性。