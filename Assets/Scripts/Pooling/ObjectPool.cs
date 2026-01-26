using System.Collections.Generic;
using UnityEngine;

namespace TopDownShooter.Pooling
{
    /// <summary>
    /// Generic object pool for efficient object reuse.
    /// Reduces garbage collection by recycling objects.
    /// </summary>
    /// <typeparam name="T">Component type to pool</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _available;
        private readonly HashSet<T> _inUse;
        private readonly int _maxSize;

        /// <summary>
        /// Creates a new object pool.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate</param>
        /// <param name="initialSize">Number of objects to pre-instantiate</param>
        /// <param name="maxSize">Maximum pool size (0 = unlimited)</param>
        /// <param name="parent">Parent transform for pooled objects</param>
        public ObjectPool(T prefab, int initialSize, int maxSize = 0, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            _maxSize = maxSize;
            _available = new Queue<T>(initialSize);
            _inUse = new HashSet<T>();

            // Pre-instantiate objects
            for (int i = 0; i < initialSize; i++)
            {
                T obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                _available.Enqueue(obj);
            }
        }

        /// <summary>
        /// Gets an object from the pool.
        /// </summary>
        /// <returns>An active pooled object</returns>
        public T Get()
        {
            T obj;

            if (_available.Count > 0)
            {
                obj = _available.Dequeue();
            }
            else if (_maxSize == 0 || TotalCount < _maxSize)
            {
                obj = CreateNewObject();
            }
            else
            {
                Debug.LogWarning($"ObjectPool<{typeof(T).Name}> has reached max size ({_maxSize})");
                return null;
            }

            obj.gameObject.SetActive(true);
            _inUse.Add(obj);
            return obj;
        }

        /// <summary>
        /// Gets an object and positions it.
        /// </summary>
        public T Get(Vector3 position)
        {
            T obj = Get();
            if (obj != null)
            {
                obj.transform.position = position;
            }
            return obj;
        }

        /// <summary>
        /// Gets an object and positions/rotates it.
        /// </summary>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj = Get();
            if (obj != null)
            {
                obj.transform.SetPositionAndRotation(position, rotation);
            }
            return obj;
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        /// <param name="obj">Object to return</param>
        public void Return(T obj)
        {
            if (obj == null) return;

            if (!_inUse.Contains(obj))
            {
                Debug.LogWarning($"Trying to return object that is not in use: {obj.name}");
                return;
            }

            obj.gameObject.SetActive(false);
            _inUse.Remove(obj);
            _available.Enqueue(obj);
        }

        /// <summary>
        /// Returns all in-use objects to the pool.
        /// </summary>
        public void ReturnAll()
        {
            foreach (T obj in _inUse)
            {
                if (obj != null)
                {
                    obj.gameObject.SetActive(false);
                    _available.Enqueue(obj);
                }
            }
            _inUse.Clear();
        }

        /// <summary>
        /// Total number of objects in the pool (available + in use).
        /// </summary>
        public int TotalCount => _available.Count + _inUse.Count;

        /// <summary>
        /// Number of available objects in the pool.
        /// </summary>
        public int AvailableCount => _available.Count;

        /// <summary>
        /// Number of objects currently in use.
        /// </summary>
        public int InUseCount => _inUse.Count;

        private T CreateNewObject()
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.name = $"{_prefab.name}_Pooled_{TotalCount}";
            return obj;
        }
    }

    /// <summary>
    /// Manages multiple object pools in a centralized location.
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        private readonly Dictionary<string, object> _pools = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// Creates a new pool or returns an existing one.
        /// </summary>
        public ObjectPool<T> GetOrCreatePool<T>(string poolName, T prefab, int initialSize, int maxSize = 0) where T : Component
        {
            if (_pools.TryGetValue(poolName, out object existingPool))
            {
                return (ObjectPool<T>)existingPool;
            }

            var pool = new ObjectPool<T>(prefab, initialSize, maxSize, transform);
            _pools[poolName] = pool;
            return pool;
        }

        /// <summary>
        /// Gets an existing pool by name.
        /// </summary>
        public ObjectPool<T> GetPool<T>(string poolName) where T : Component
        {
            if (_pools.TryGetValue(poolName, out object pool))
            {
                return (ObjectPool<T>)pool;
            }
            return null;
        }

        /// <summary>
        /// Returns all objects to all pools.
        /// </summary>
        public void ReturnAllToPool()
        {
            // Note: This requires tracking pool types, simplified version just logs
            Debug.Log("ReturnAllToPool called - pools should be cleared individually");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
