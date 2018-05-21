namespace System.Data.SQLite
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal static class SQLiteConnectionPool
    {
        private static SortedList<string, Pool> _connections = new SortedList<string, Pool>(StringComparer.OrdinalIgnoreCase);
        private static int _poolVersion = 1;

        internal static void Add(string fileName, SQLiteConnectionHandle hdl, int version)
        {
            lock (_connections)
            {
                Pool pool;
                if (_connections.TryGetValue(fileName, out pool) && (version == pool.PoolVersion))
                {
                    ResizePool(pool, true);
                    pool.Queue.Enqueue(new WeakReference(hdl, false));
                    GC.KeepAlive(hdl);
                }
                else
                {
                    hdl.Close();
                }
            }
        }

        internal static void ClearAllPools()
        {
            lock (_connections)
            {
                foreach (KeyValuePair<string, Pool> pair in _connections)
                {
                    while (pair.Value.Queue.Count > 0)
                    {
                        SQLiteConnectionHandle target = pair.Value.Queue.Dequeue().Target as SQLiteConnectionHandle;
                        if (target != null)
                        {
                            target.Dispose();
                        }
                    }
                    if (_poolVersion <= pair.Value.PoolVersion)
                    {
                        _poolVersion = pair.Value.PoolVersion + 1;
                    }
                }
                _connections.Clear();
            }
        }

        internal static void ClearPool(string fileName)
        {
            lock (_connections)
            {
                Pool pool;
                if (_connections.TryGetValue(fileName, out pool))
                {
                    pool.PoolVersion++;
                    while (pool.Queue.Count > 0)
                    {
                        SQLiteConnectionHandle target = pool.Queue.Dequeue().Target as SQLiteConnectionHandle;
                        if (target != null)
                        {
                            target.Dispose();
                        }
                    }
                }
            }
        }

        internal static SQLiteConnectionHandle Remove(string fileName, int maxPoolSize, out int version)
        {
            lock (_connections)
            {
                Pool pool;
                version = _poolVersion;
                if (!_connections.TryGetValue(fileName, out pool))
                {
                    pool = new Pool(_poolVersion, maxPoolSize);
                    _connections.Add(fileName, pool);
                    return null;
                }
                version = pool.PoolVersion;
                pool.MaxPoolSize = maxPoolSize;
                ResizePool(pool, false);
                while (pool.Queue.Count > 0)
                {
                    SQLiteConnectionHandle target = pool.Queue.Dequeue().Target as SQLiteConnectionHandle;
                    if (target != null)
                    {
                        return target;
                    }
                }
                return null;
            }
        }

        private static void ResizePool(Pool queue, bool forAdding)
        {
            int maxPoolSize = queue.MaxPoolSize;
            if (forAdding && (maxPoolSize > 0))
            {
                maxPoolSize--;
            }
            while (queue.Queue.Count > maxPoolSize)
            {
                SQLiteConnectionHandle target = queue.Queue.Dequeue().Target as SQLiteConnectionHandle;
                if (target != null)
                {
                    target.Dispose();
                }
            }
        }

        internal class Pool
        {
            internal int MaxPoolSize;
            internal int PoolVersion;
            internal readonly Queue<WeakReference> Queue = new Queue<WeakReference>();

            internal Pool(int version, int maxSize)
            {
                this.PoolVersion = version;
                this.MaxPoolSize = maxSize;
            }
        }
    }
}

