using System;

namespace Utilities
{
    public interface IFiber
    {
        WorkingState State { get; }

        void Enqueue(Action t);
    }
}