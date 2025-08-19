using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Foundation.Utils
{
    public static class AsyncJobQueue
    {
        /// <summary>
        /// thread-safe한 비동기 큐. SLM이 Producer(작업 delegate 보냄), WorkerLoop 하나가 Consumer(받아서 처리함)
        /// </summary>
        private static readonly Channel<Func<CancellationToken, Task>> Queue =
            Channel.CreateUnbounded<Func<CancellationToken, Task>>();
        private static CancellationTokenSource globalCts = new CancellationTokenSource();
        private static readonly Dictionary<string, CancellationTokenSource> KeyCts = new();
        private static readonly object Gate = new();
        private static readonly Task _worker = Task.Run(WorkerLoop);

        private static int pendingCount;
        private static TaskCompletionSource<bool> idle = NewIdle();
        private static TaskCompletionSource<bool> NewIdle() => new(TaskCreationOptions.RunContinuationsAsynchronously);
        
        /// <summary>
        /// 대길이같은 역할. 비동기 작업수행을 의미한다.
        /// </summary>
        private static async Task WorkerLoop()
        {
            try
            {
                while (await Queue.Reader.WaitToReadAsync(globalCts.Token))
                {
                    while (Queue.Reader.TryRead(out Func<CancellationToken, Task> job))
                    {
                        try { await job(globalCts.Token); }
                        catch (OperationCanceledException)
                        {/*꺼@지는거지 뭐*/}
                        catch (Exception e) { Debug.LogError($"[AsyncJobQueue] job error: {e}"); }
                        finally
                        {
                            if (Interlocked.Decrement(ref pendingCount) == 0)
                                idle.TrySetResult(true);
                        }
                    }
                }
            }
            catch(OperationCanceledException){ /*꺼@지는거지 뭐*/}
        }

        public static void Enqueue(Func<CancellationToken, Task> job)
        {
            if (Interlocked.Increment(ref pendingCount) == 1)
                idle = NewIdle();
            if (!Queue.Writer.TryWrite(job))
            {
                if (Interlocked.Decrement(ref pendingCount) == 0)
                    idle.TrySetResult(true);
                Debug.LogWarning("[AsyncJobQueue] job failed. Writer Closed");
            }
        }

        public static void EnqueueKeyed(string key, Func<CancellationToken, Task> job)
        {
            CancellationToken t;
            lock (Gate)
            {
                if (KeyCts.TryGetValue(key, out var prev))
                {
                    prev.Cancel();
                    prev.Dispose();
                }

                var cts = CancellationTokenSource.CreateLinkedTokenSource(globalCts.Token);
                KeyCts[key] = cts;
                t = cts.Token;
            }

            if (Interlocked.Increment(ref pendingCount) == 1)
                idle = NewIdle();
            Queue.Writer.TryWrite(async _ =>
            {
                try { await job(t); }
                finally
                {
                    lock (Gate)
                    {
                        if (KeyCts.TryGetValue(key, out var prev) && prev.Token == t)
                        {
                            prev.Dispose();
                            KeyCts.Remove(key);
                        }
                    }

                    if (Interlocked.Decrement(ref pendingCount) == 0)
                        idle.TrySetResult(true);
                }
            });
        }

        public static Task EnqueueKeyedWithCompletion(string key, Func<CancellationToken, Task> job)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            EnqueueKeyed(key, async ct =>
            {
                try{ await job(ct); tcs.TrySetResult(true); }
                catch(OperationCanceledException) {tcs.TrySetCanceled(ct);}
                catch(Exception e) { tcs.TrySetException(e); }
            });
            return tcs.Task;

        }
        
        // key 작업 때려치는 함수
        public static void CancelKey(string key)
        {
            lock (Gate)
            {
                if (KeyCts.TryGetValue(key, out var cts))
                {
                    cts.Cancel(); cts.Dispose();
                    KeyCts.Remove(key);
                }
            }
        }
        
        // 작업 다 때려치는 함수
        public static void CancelAll()
        {
            lock (Gate)
            {
                foreach (var kv in KeyCts.Values) kv.Cancel();
                foreach (var kv in KeyCts.Values) kv.Dispose();
                KeyCts.Clear();
                
                globalCts.Cancel();
                globalCts.Dispose();
                globalCts = new();
            }
        }

        public static Task WaitIdleAsync() => idle.Task;
    }
    
}
