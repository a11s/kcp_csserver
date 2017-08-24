using System;

namespace System.Collections.Concurrent
{
    public class ConcurrentQueue<T>
    {
        public bool TryDequeue(out T ibuff)
        {
            throw new NotImplementedException();
        }

        public void Enqueue(T data)
        {
            throw new NotImplementedException();
        }
        public int Count { get => 0; }
    }
}