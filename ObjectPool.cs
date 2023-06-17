using System;
using System.Collections.Generic;

namespace Hybel.ObjectPooling
{
    /// <summary>
    /// A Queue based Object pooling solution
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> : IObjectPool<T>
        where T : class
    {
        public delegate T CreateObjectFunc();
        public delegate void OnTakeObjectFunc(T item);
        public delegate void OnReleaseObjectFunc(T item);
        public delegate void OnDestroyObjectFunc(T item);

        private const int DEFAULT_STARTING_POOL_SIZE = 50;
        /// <summary>
        /// 50
        /// </summary>
        // Remember to update the summary value here. This is a slight hack for the docs to use this value in a different summary.
        private const int DEFAULT_POOL_SIZE_INCREMENT = 50;
        private const OverflowMode DEFAULT_OVERFLOW_MODE = OverflowMode.StealFromActive;

        private readonly Queue<T> _inactiveObjects;
        private readonly List<T> _activeObjects;

        private readonly CreateObjectFunc _createObject;
        private readonly OnTakeObjectFunc _onTakeObject;
        private readonly OnReleaseObjectFunc _onReleaseObject;
        private readonly OnDestroyObjectFunc _onDestroyObject;

        private int _maxPoolSize;
        private readonly OverflowMode _overflowMode;
        private readonly int _poolSizeIncrement;

        public ObjectPool(
            CreateObjectFunc createObject,
            OnTakeObjectFunc onTakeObject,
            OnReleaseObjectFunc onReleaseObject,
            int maxPoolSize,
            OverflowMode overflowMode = DEFAULT_OVERFLOW_MODE)
            : this(createObject, onTakeObject, onReleaseObject, null, DEFAULT_STARTING_POOL_SIZE, maxPoolSize, overflowMode)
        { }

        public ObjectPool(
            CreateObjectFunc createObject,
            OnTakeObjectFunc onTakeObject,
            OnReleaseObjectFunc onReleaseObject,
            int startingAmount,
            int maxPoolSize,
            OverflowMode overflowMode = DEFAULT_OVERFLOW_MODE)
            : this(createObject, onTakeObject, onReleaseObject, null, startingAmount, maxPoolSize, overflowMode)
        { }

        public ObjectPool(
            CreateObjectFunc createObject,
            OnTakeObjectFunc onTakeObject,
            OnReleaseObjectFunc onReleaseObject,
            OnDestroyObjectFunc onDestroyObject,
            int maxPoolSize,
            OverflowMode overflowMode = DEFAULT_OVERFLOW_MODE)
            : this(createObject, onTakeObject, onReleaseObject, onDestroyObject, DEFAULT_STARTING_POOL_SIZE, maxPoolSize, overflowMode)
        { }

        public ObjectPool(
            CreateObjectFunc createObject,
            OnTakeObjectFunc onTakeObject,
            OnReleaseObjectFunc onReleaseObject,
            OnDestroyObjectFunc onDestroyObject,
            int startingAmount,
            int maxPoolSize,
            OverflowMode overflowMode = DEFAULT_OVERFLOW_MODE)
            : this(createObject, onTakeObject, onReleaseObject, onDestroyObject, startingAmount, maxPoolSize, DEFAULT_POOL_SIZE_INCREMENT, overflowMode)
        { }

        /// <param name="poolSizeIncrement">If this option is used the OverflowMode is always <see cref="OverflowMode.IncreaseSize"/> and it uses this increment instead of the default.</param>
        public ObjectPool(
            CreateObjectFunc createObject,
            OnTakeObjectFunc onTakeObject,
            OnReleaseObjectFunc onReleaseObject,
            int startingAmount,
            int maxPoolSize,
            int poolSizeIncrement)
            : this(createObject, onTakeObject, onReleaseObject, null, startingAmount, maxPoolSize, poolSizeIncrement, OverflowMode.IncreaseSize)
        { }

        /// <param name="poolSizeIncrement">If this option is used the OverflowMode is always <see cref="OverflowMode.IncreaseSize"/> and it uses this increment instead of the default.</param>
        public ObjectPool(
            CreateObjectFunc createObject,
            OnTakeObjectFunc onTakeObject,
            OnReleaseObjectFunc onReleaseObject,
            OnDestroyObjectFunc onDestroyObject,
            int startingAmount,
            int maxPoolSize,
            int poolSizeIncrement)
            : this(createObject, onTakeObject, onReleaseObject, onDestroyObject, startingAmount, maxPoolSize, poolSizeIncrement, OverflowMode.IncreaseSize)
        { }

        private ObjectPool(
            CreateObjectFunc createObject,
            OnTakeObjectFunc onTakeObject,
            OnReleaseObjectFunc onReleaseObject,
            OnDestroyObjectFunc onDestroyObject,
            int startingAmount,
            int maxPoolSize,
            int poolSizeIncrement,
            OverflowMode overflowMode)
        {
            _inactiveObjects = new Queue<T>(maxPoolSize);
            _activeObjects = new List<T>(maxPoolSize);

            if (createObject is null)
                throw new ArgumentNullException(nameof(createObject));

            if (onTakeObject is null)
                throw new ArgumentNullException(nameof(onTakeObject));

            if (onReleaseObject is null)
                throw new ArgumentNullException(nameof(onReleaseObject));

            _createObject = createObject;
            _onTakeObject = onTakeObject;
            _onReleaseObject = onReleaseObject;
            _onDestroyObject = onDestroyObject;

            _maxPoolSize = maxPoolSize;
            _overflowMode = overflowMode;
            _poolSizeIncrement = poolSizeIncrement;
            PopulateInactive(startingAmount);
        }

        public int CountAll => CountActive + CountInactive;
        public int CountActive => _activeObjects.Count;
        public int CountInactive => _inactiveObjects.Count;
        public int MaxPoolSize => _maxPoolSize;

        /// <summary>
        /// The overflow mode of the pool.
        /// </summary>
        public OverflowMode OverflowMode => _overflowMode;

        public T Get()
        {
            if (_inactiveObjects.Count > 0)
            {
                var dequeuedObject = _inactiveObjects.Dequeue();
                _onTakeObject(dequeuedObject);
                _activeObjects.Add(dequeuedObject);
                return dequeuedObject;
            }

            if (PopulateInactive())
                return Get();

            return null;
        }

        public IEnumerable<T> Get(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (TryGet(out T obj))
                    yield return obj;
                else
                    yield break;
            }
        }

        public void Release(T obj)
        {
            if (!_activeObjects.Contains(obj))
                return;

            _activeObjects.Remove(obj);
            _onReleaseObject(obj);

            if (CountAll >= _maxPoolSize)
            {
                _onDestroyObject?.Invoke(obj);
                return;
            }

            _inactiveObjects.Enqueue(obj);
        }

        public bool TryGet(out T obj)
        {
            obj = Get();
            return !(obj is null);
        }

        public void Clear()
        {
            if (_onDestroyObject != null)
            {
                foreach (var inactiveObject in _inactiveObjects)
                    _onDestroyObject(inactiveObject);

                foreach (var activeObject in _activeObjects)
                    _onDestroyObject(activeObject);
            }

            _inactiveObjects.Clear();
            _activeObjects.Clear();
        }

        private int PopulateInactive(int amount)
        {
            int numberOfPopulations = 0;

            for (int i = 0; i < amount; i++)
            {
                if (!PopulateInactive())
                    break;

                numberOfPopulations++;
            }

            return numberOfPopulations;
        }

        private bool PopulateInactive()
        {
            if (CountAll >= _maxPoolSize)
            {
                switch (_overflowMode)
                {
                    case OverflowMode.HardLimit:
                        return false;

                    case OverflowMode.StealFromActive:
                        var stolenObj = _activeObjects[0];
                        _activeObjects.Remove(stolenObj);
                        _onReleaseObject(stolenObj);
                        _inactiveObjects.Enqueue(stolenObj);
                        return true;

                    case OverflowMode.IncreaseSize:
                        _maxPoolSize += _poolSizeIncrement;
                        break;
                }
            }

            T createdObject = _createObject();
            _onReleaseObject(createdObject);
            _inactiveObjects.Enqueue(createdObject);
            return true;
        }
    }

    /// <summary>
    /// What to do when the pool is full, but the user is requesting more items.
    /// </summary>
    public enum OverflowMode
    {
        /// <summary>
        /// This option will prevent any further creation of objects and will stop the lifetime of active objects.
        /// </summary>
        StealFromActive,

        /// <summary>
        /// This option will prevent any further creation of objects and will not stop the lifetime of active objects.
        /// </summary>
        HardLimit,

        /// <summary>
        /// This option will not prevent creation of objects and will saturate the pool with more objects than its capacity.
        /// <para>Extra objects will be destroyed when they end their lifetime instead of being marked as inactive.</para>
        /// </summary>
        AllowOverflow,

        /// <summary>
        /// This option will increase the max size of the pool when it overflows by <inheritdoc cref="ObjectPool{T}.DEFAULT_POOL_SIZE_INCREMENT"/>.
        /// </summary>
        IncreaseSize,
    }
}
