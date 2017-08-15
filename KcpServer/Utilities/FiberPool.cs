using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
namespace Utilities
{
    public class FiberPool
    {
        Thread[] threads = null;
        ConcurrentQueue<Action>[] works;
        bool[] threadCloseState;
        public int GetWorkerCount => threads.Length;
        /// <summary>
        /// 执行中
        /// </summary>
        bool Running = true;
        /// <summary>
        /// 不再接受更多任务
        /// </summary>
        bool Closing = false;
        void debug(string s)
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        }

        public FiberPool(int cnt)
        {
            works = new ConcurrentQueue<Action>[cnt];
            threadCloseState = new bool[cnt];
            for (int i = 0; i < works.Length; i++) works[i] = new ConcurrentQueue<Action>();
            threads = new Thread[cnt];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(DoWork);
                var t = threads[i];
                t.IsBackground = true;
                t.Name = $"fp_worker_{i}";
                t.Start(i);
                threadCloseState[i] = false;
            }
            debug($"{nameof(FiberPool)} started");
        }
        private void DoWork(object arg)
        {
            var i = (int)arg;
            var w = works[i];
            SpinWait sw = new SpinWait();
            while (Running)
            {
                while (Running && w.TryDequeue(out var a))
                {
                    a();//todo 出错以后是忽略还是崩溃应当有配置:RESTART IGNORE NONE
                }
                if (Closing)
                {
                    threadCloseState[i] = true;
                    debug($"{nameof(FiberPool)} thread {i} exit");
                    break;
                }
                sw.SpinOnce();
            }
        }
        public void SyncClose(TimeSpan timeout)
        {
            Closing = true;
            debug($"{nameof(FiberPool)} Closing");
            SpinWait sw = new SpinWait();
            DateTime starttime = DateTime.Now;
            while (true)
            {
                var closed = threadCloseState.Count(a => a);
                if (closed == threadCloseState.Length)
                {
                    //all closed
                    debug($"{nameof(FiberPool)} All thread closed");
                    Running = false;
                    break;
                }
                if (DateTime.Now.Subtract(starttime) > timeout)
                {
                    Running = false;
                    debug($"{nameof(FiberPool)} Close time out");
                    break;
                }
                else
                {
                    sw.SpinOnce();
                }

            }
        }
        /// <summary>
        /// 把任务堆到线程的工作队列
        /// </summary>
        /// <param name="id"></param>
        /// <param name="a"></param>
        public void Enqueue(int id, Action a)
        {
            if (Closing)
            {
                throw new InvalidOperationException("不要在关闭的时候继续堆任务");
            }
            var tid = id % threads.Length;
            works[tid].Enqueue(a);
        }
    }
}
