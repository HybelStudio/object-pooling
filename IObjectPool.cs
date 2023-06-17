using System.Collections.Generic;

namespace Hybel.ObjectPooling
{
    public interface IObjectPool<T>
    {
        /// <summary>
        /// The total number of active and inactive objects.
        /// </summary>
        public int CountAll { get; }

        /// <summary>
        /// Number of objects that have been created by the pool but are currently in use and have not yet been returned.
        /// </summary>
        public int CountActive { get; }

        /// <summary>
        /// Number of objects that are currently available in the pool.
        /// </summary>
        public int CountInactive { get; }

        /// <summary>
        /// Maximum amount of object active or inactive.
        /// </summary>
        public int MaxPoolSize { get; }

        /// <summary>
        /// Get an instance from the pool. If the pool is empty, a new instance will be created if the pool has not reached its <see cref="MaxPoolSize"/>.
        /// </summary>
        public T Get();

        /// <summary>
        /// Get several instances from the pool. If the pool is empty, a new instance will be created if the pool has not reached its <see cref="MaxPoolSize"/>.
        /// </summary>
        /// <param name="amount">Number of instances to take from the pool.</param>
        public IEnumerable<T> Get(int amount);

        /// <summary>
        /// Get an instance from the pool. If the pool is empty, a new instance will be created if the pool has not reached its <see cref="MaxPoolSize"/>.
        /// </summary>
        /// <param name="obj">An instance taken from the pool or null.</param>
        /// <returns>True if <paramref name="obj"/> isn't null.</returns>
        public bool TryGet(out T obj);

        /// <summary>
        /// Removes all pooled items. If the pool contains a destroy callback then it will be called for each item that is in the pool.
        /// </summary>
        public void Clear();

        /// <summary>
        /// Returns the instance back to the pool.
        /// </summary>
        /// <param name="obj">Instance to return to the pool.</param>
        public void Release(T obj);
    }
}
