using UnityEngine;

namespace Hybel.ObjectPooling
{
    public interface IPoolableObject
    {
        public ObjectPoolAsset ObjectPool { get; set; }
        public void HandlePoolReturn();
    }
}