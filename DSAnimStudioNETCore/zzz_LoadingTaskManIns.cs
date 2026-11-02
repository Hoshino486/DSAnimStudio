using DSAnimStudio.ImguiOSD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class zzz_LoadingTaskManIns
    {
        public zzz_DocumentIns ParentDocument;
        public zzz_LoadingTaskManIns(zzz_DocumentIns parentDocument)
        {
            ParentDocument = parentDocument;
        }

        [Flags]
        public enum TaskFlags : int
        {
            None = 0,
            Critical = 1 << 0,
            AllowsInteraction = 1 << 2,
            //Hide = 1 << 3,
        }




        public class LoadingTask
        {
            public readonly string TaskKey;
            public string DisplayString;
            public double ProgressRatio { get; private set; }
            public bool IsComplete { get; private set; }
            //public bool IsBeingKilledManually = false;
            //public System.Diagnostics.Stopwatch Timer { get; private set; }
            //public double ElapsedSeconds => Timer.Elapsed.TotalSeconds;
            Thread taskThread;


            public float MinElapsedTimeForVisible = 0.25f;
            public float ElapsedTime = 0;
            public DrawStates DrawState = DrawStates.InvisibleWait;
            public enum DrawStates
            {
                InvisibleWait = 0,
                FirstFrameAfterWait_Invisible = 1,
                Drawing = 2,
            }

            public void UpdateElapsedTime(float elapsedTime)
            {
                ElapsedTime += elapsedTime;
                if (ElapsedTime >= MinElapsedTimeForVisible && DrawState < DrawStates.FirstFrameAfterWait_Invisible)
                {
                    DrawState = DrawStates.FirstFrameAfterWait_Invisible;
                }
            }

            public void KILL_IMMEDIATELY()
            {
                taskThread.Interrupt();
                taskThread.Join();
            }

            public TaskFlags Flags = TaskFlags.None;

            //public void UpdateKillCheck()
            //{
            //    if (IsBeingKilledManually)
            //    {
            //        Kill();
            //        IsComplete = true;
            //    }
            //}

            public LoadingTask(zzz_LoadingTaskManIns loadingTaskMan, string taskKey, string displayString, Action<IProgress<double>> doLoad,
                double startingProgressRatio = 0)
            {
                TaskKey = taskKey;
                //Timer = System.Diagnostics.Stopwatch.StartNew();

                DisplayString = displayString;
                IProgress<double> prog = new Progress<double>(x =>
                {
                    ProgressRatio = x;
                    loadingTaskMan.CheckOnTask(TaskKey);
                });

                ProgressRatio = startingProgressRatio;

                taskThread = new Thread(() =>
                {

                    try
                    {
                        doLoad.Invoke(prog);
                        prog.Report(1.0);
                    }
                    catch (Exception handled_ex) when (Main.EnableErrorHandler.LoadingTask)
                    {
                        Main.HandleError(nameof(Main.EnableErrorHandler.LoadingTask), handled_ex);
                    }


                    // 我们不看 ProgressRatio 来判断做完没，因为
                    // 线程一完成就被 INSTANTLY KILLED，那样
                    // 进度取整的一丁点误差就能把
                    // 整个宇宙给毁了。所以，只有在整个 doLoad 
                    // 跑完之后才算它做完。
                    IsComplete = true;

                    loadingTaskMan.CheckOnTask(TaskKey);
                });

                taskThread.SetApartmentState(ApartmentState.STA);

                taskThread.IsBackground = true;
                taskThread.Start();
            }

            //public void Kill()
            //{
            //    if (taskThread != null && taskThread.IsAlive)
            //        AbortThread(taskThread);
            //}
        }

        internal object _lock_TaskDictEdit = new object();

        public Dictionary<string, LoadingTask> TaskDict = new Dictionary<string, LoadingTask>();

        public void CheckOnTask(string taskKey)
        {
            lock (_lock_TaskDictEdit)
            {
                if (TaskDict.ContainsKey(taskKey))
                {
                    //TaskDict[taskKey].UpdateKillCheck();

                    if (TaskDict[taskKey].IsComplete)
                        TaskDict.Remove(taskKey);
                }
            }

        }

        public bool AnyTasks()
        {
            bool result = false;
            lock (_lock_TaskDictEdit)
            {
                result = TaskDict.Any();
            }
            return result;
        }

        public bool AnyCriticalTasks()
        {
            bool result = false;
            lock (_lock_TaskDictEdit)
            {
                result = TaskDict.Any(t => t.Value.Flags.HasFlag(TaskFlags.Critical));
            }
            return result;
        }

        public bool AnyInteractionBlockingTasks()
        {
            bool result = false;
            lock (_lock_TaskDictEdit)
            {
                result = TaskDict.Any(t => !t.Value.Flags.HasFlag(TaskFlags.AllowsInteraction));
            }
            return result;
        }

        /// <summary>
        ///     那个任务还没在跑的话，就启一个加载任务。
        /// </summary>
        /// <param name="taskKey">
        ///     用来引用任务的字符串 key。 
        ///     就是靠这个判断它是不是已经在跑了。
        /// </param>
        /// <param name="displayString">
        ///     实际显示在屏幕上、贴在进度条旁边 
        ///     用的那个字符串。
        /// </param>
        /// <param name="taskDelegate">
        ///     The actual task to perform. An IProgress&lt;double&gt; is given
        ///     给你自己控制加载条用的
        ///     (0.0 = bar empty, 1.0 = bar full, -1.0 = continuous bar).
        ///     做完记得把进度条填满。
        /// </param>
        /// <param name="addFluffMilliseconds">
        ///     任务结尾等待的毫秒数， 
        ///     好让进度条在屏幕上停够，能看清。
        /// </param>
        /// <param name="waitForTaskToComplete">
        ///     是否等任务跑完， 
        ///     而不是丢另一个线程里启动完立刻返回。
        /// </param>
        /// <param name="synchronousWaitThreadSpinMilliseconds">
        ///     每个线程自旋循环等待的毫秒数， 
        ///     在等任务跑完的这段时间里。
        /// </param>
        /// <param name="disableProgressBarByDefault">
        ///     是否以禁用进度条的状态启动。这项为 false 时， 
        ///     你仍然能在任务进行中把进度条关掉，做法是上报
        ///     一个 -1.0 的进度值
        /// </param>
        /// <returns>
        ///     任务刚被启动就返回 true，已经在跑则返回 false。
        /// </returns>
        public bool DoLoadingTask(string taskKey,
            string displayString,
            Action<IProgress<double>> taskDelegate,
            int addFluffMilliseconds = 0,//100, 
            bool waitForTaskToComplete = false,
            int synchronousWaitThreadSpinMilliseconds = 16,//250,
            bool disableProgressBarByDefault = false,
            TaskFlags flags = TaskFlags.None)
        {
            taskKey = taskKey ?? (System.Guid.NewGuid().ToString());

            lock (_lock_TaskDictEdit)
            {
                if (TaskDict.ContainsKey(taskKey))
                    return false;
                // LoadingTask 一被创建就开始跑。
                TaskDict.Add(taskKey, new LoadingTask(this, taskKey, displayString, progress =>
                {
                    taskDelegate.Invoke(progress);
                    if (addFluffMilliseconds > 0)
                        Thread.Sleep(addFluffMilliseconds);
                }, disableProgressBarByDefault ? -1 : 0)
                {
                    Flags = flags,
                });
                if (!flags.HasFlag(TaskFlags.AllowsInteraction))
                    OSD.RequestCollapse = true;
            }

            if (waitForTaskToComplete)
            {
                bool isTaskDone = false;
                while (!isTaskDone)
                {
                    // 检查时在这儿加锁，免得它去枚举的时候
                    // 字典正在被改，直接当场发疯 :fatcat:
                    lock (_lock_TaskDictEdit)
                    {
                        if (TaskDict.ContainsKey(taskKey))
                        {
                            isTaskDone = TaskDict[taskKey].IsComplete;
                            if (TaskDict[taskKey].IsComplete)
                                TaskDict.Remove(taskKey);
                        }
                        else
                        {
                            isTaskDone = true;
                        }
                    }

                    Thread.Sleep(synchronousWaitThreadSpinMilliseconds);
                }
            }

            return true;
        }

        /// <summary>
        ///     那个任务还没在跑的话就启一个加载任务，然后
        ///     等它跑完再返回。
        /// </summary>
        /// <param name="taskKey">
        ///     用来引用任务的字符串 key。 
        ///     就是靠这个判断它是不是已经在跑了。
        /// </param>
        /// <param name="displayString">
        ///     实际显示在屏幕上、贴在进度条旁边 
        ///     用的那个字符串。
        /// </param>
        /// <param name="taskDelegate">
        ///     The actual task to perform. An IProgress&lt;double&gt; is given
        ///     for you to report the loading bar percentage (1.0 = bar full).
        ///     做完记得把进度条填满。
        /// </param>
        /// <param name="addFluffMilliseconds">
        ///     任务结尾等待的毫秒数， 
        ///     好让进度条在屏幕上停够，能看清。
        /// </param>
        /// <param name="synchronousWaitThreadSpinMilliseconds">
        ///     每个线程自旋循环等待的毫秒数， 
        ///     在等任务跑完的这段时间里。
        /// </param>
        /// <returns>
        ///     任务刚被启动就返回 true，已经在跑则返回 false。
        /// </returns>
        public bool DoLoadingTaskSynchronous(
            string taskKey,
            string displayString,
            Action<IProgress<double>> taskDelegate,
            int addFluffMilliseconds = 20,
            int synchronousWaitThreadSpinMilliseconds = 16)
        {
            return DoLoadingTask(taskKey, displayString, taskDelegate,
                addFluffMilliseconds, waitForTaskToComplete: true,
                synchronousWaitThreadSpinMilliseconds);
        }

        public void KILL_ALL_TASKS()
        {
            lock (_lock_TaskDictEdit)
            {
                foreach (var kvp in TaskDict)
                {
                    kvp.Value?.KILL_IMMEDIATELY();
                }
            }
        }

        public bool KillTask(string taskKey)
        {
            throw new NotImplementedException();
            //if (IsTaskRunning(taskKey))
            //{
            //    lock (_lock_TaskDictEdit)
            //    {
            //        TaskDict[taskKey].IsBeingKilledManually = true;
            //    }
            //    return true;
            //}

            //return false;
        }

        public bool IsTaskRunning(string taskKey)
        {
            lock (_lock_TaskDictEdit)
            {
                if (TaskDict.ContainsKey(taskKey))
                {
                    // 既然都到这儿了，顺手再确认一下那任务是不是做完了。
                    if (TaskDict[taskKey].IsComplete)
                    {
                        //TaskDict[taskKey].Kill();
                        TaskDict.Remove(taskKey);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public void Update(float elapsedTime)
        {
            lock (_lock_TaskDictEdit)
            {
                // 清理做完的任务
                var keyList = TaskDict.Keys;
                var keysOfTasksThatAreFinished = new List<string>();
                foreach (var key in keyList)
                {
                    if (TaskDict[key].IsComplete)
                    {
                        //TaskDict[key].Kill();
                        keysOfTasksThatAreFinished.Add(key);
                    }
                    else
                    {
                        TaskDict[key].UpdateElapsedTime(elapsedTime);
                    }
                }
                foreach (var key in keysOfTasksThatAreFinished)
                {
                    TaskDict.Remove(key);
                }
            }
        }

        public const int GuiDistFromEdgesOfScreenX = 8;
        public const int GuiDistFromEdgesOfScreenY = 40;
        public const int GuiDistBetweenProgressRects = 8;
        public const int GuiTaskRectWidth = 360;
        public const int GuiTaskRectHeightWithBar = 64;
        public const int GuiTaskRectHeightNoBar = 40;
        public const int GuiProgBarHeight = 20;
        public const int GuiProgBarDistFromRectEdge = 8;
        public const int GuiProgBarEdgeThickness = 2;
        public const float GuiProgNameDistFromEdge = 8;

        public void DrawAllTasks()
        {
            lock (_lock_TaskDictEdit)
            {
                if (TaskDict.Count > 0)
                {
                    GFX.SpriteBatchBegin();

                    //int i = 0;
                    int currentVerticalOffset = 0;
                    foreach (var kvp in TaskDict)
                    {
                        if (string.IsNullOrWhiteSpace(kvp.Value.DisplayString))
                            continue;

                        if (kvp.Value.DrawState == LoadingTask.DrawStates.InvisibleWait)
                            continue;

                        if (kvp.Value.DrawState == LoadingTask.DrawStates.FirstFrameAfterWait_Invisible)
                        {
                            kvp.Value.DrawState = LoadingTask.DrawStates.Drawing;
                            continue;
                        }

                        int taskRectHeight = GuiTaskRectHeightWithBar;

                        if (kvp.Value.ProgressRatio < 0)
                        {
                            taskRectHeight = GuiTaskRectHeightNoBar;
                        }

                        // 绘制任务矩形
                        Rectangle thisTaskRect = new Rectangle(
                            (int)Math.Round((GFX.Device.Viewport.Width / Main.DPI) - GuiTaskRectWidth - GuiDistFromEdgesOfScreenX),
                            GuiDistFromEdgesOfScreenY + currentVerticalOffset,
                            GuiTaskRectWidth, taskRectHeight);

                        GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, thisTaskRect, Color.Black * 0.85f);

                        if (kvp.Value.ProgressRatio >= 0)
                        {
                            // 绘制进度背景矩形

                            Rectangle progBackgroundRect = new Rectangle(thisTaskRect.X + GuiProgBarDistFromRectEdge,
                                thisTaskRect.Y + taskRectHeight - GuiProgBarDistFromRectEdge - GuiProgBarHeight,
                                thisTaskRect.Width - (GuiProgBarDistFromRectEdge * 2), GuiProgBarHeight);

                            GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, progBackgroundRect, new Color(0.25f, 0.25f, 0.25f) * 0.95f);

                            // 绘制进度前景矩形

                            Rectangle progForegroundRect = new Rectangle(
                                progBackgroundRect.X + GuiProgBarEdgeThickness,
                                progBackgroundRect.Y + GuiProgBarEdgeThickness,
                                (int)((progBackgroundRect.Width - (GuiProgBarEdgeThickness * 2)) * kvp.Value.ProgressRatio),
                                progBackgroundRect.Height - (GuiProgBarEdgeThickness * 2));

                            //GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, progForegroundRect,
                            //    kvp.Value.IsBeingKilledManually ? Color.Red : Color.White);
                            GFX.SpriteBatch.Draw(Main.WHITE_TEXTURE, progForegroundRect, Color.White);
                        }

                        // 绘制任务名

                        Vector2 taskNamePos = new Vector2(thisTaskRect.X + GuiProgNameDistFromEdge, thisTaskRect.Y + GuiProgNameDistFromEdge);

                        DBG.DrawOutlinedText(kvp.Value.DisplayString, taskNamePos,
                            Color.White, DBG.DEBUG_FONT_SMALL, startAndEndSpriteBatchForMe: false);

                        currentVerticalOffset += (GuiDistBetweenProgressRects + taskRectHeight);
                    }

                    GFX.SpriteBatchEnd();
                }
            }
        }
    }
}
