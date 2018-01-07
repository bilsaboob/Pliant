using System;
using System.Collections.Generic;

namespace Pliant.Utilities
{
    internal class ObjectPool<T> where T : class
    {
        private object _sync = new object();

        private readonly Queue<T> _queue;
        private readonly ObjectPoolFactory _factory;

        internal delegate T ObjectPoolFactory();
        
        internal ObjectPool(int size, ObjectPoolFactory factory)
        {
            _factory = factory;
            _queue = new Queue<T>(size);
        }

        internal ObjectPool(ObjectPoolFactory factory)
            : this(20, factory)
        {
        }
        
        internal T Allocate()
        {
            lock (_sync)
            {
                if (_queue.Count == 0)
                    return CreateInstance();
                return _queue.Dequeue();
            }
        }

        private T CreateInstance()
        {
            return _factory();
        }

        internal void Free(T value)
        {
            lock (_sync)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _queue.Enqueue(value);
            }
        }        
    }
}
