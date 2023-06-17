using UnityEngine;

namespace Hybel.ObjectPooling
{
    public abstract class PoolableObject : MonoBehaviour, IPoolableObject
    {
        private ObjectPoolAsset _objectPool;

        public ObjectPoolAsset ObjectPool
        {
            get => _objectPool;
            set
            {
                if (_objectPool is null)
                    _objectPool = value;
                else
                    throw new System.Exception("Bad Object Pool usage! It should only be assigned once.");
            }
        }

        public abstract void HandlePoolReturn();
    }
}
