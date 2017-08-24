using System;
using System.Collections.Generic;

namespace System.Collections.Concurrent
{
    public class ConcurrentQueue<T>
    {
        Queue<T> innerQueue = null;
        public ConcurrentQueue()
        {
            innerQueue = new Queue<T>();
        }
        public bool TryDequeue(out T item)
        {
            lock (innerQueue)
            {
                if (innerQueue.Count > 0)
                {
                    item = innerQueue.Dequeue();
                    return true;
                }
                else
                {
                    item = default(T);
                    return false;
                }
            }
        }

        public void Enqueue(T item)
        {
            lock (innerQueue)
            {
                innerQueue.Enqueue(item);
            }
        }
        public int Count
        {
            get
            {

                lock (innerQueue)
                {
                    return innerQueue.Count;
                };
            }
        }
    }
}