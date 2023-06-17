namespace Hybel.ObjectPooling.Tests
{
    public partial class object_pool_tests
    {
        private class PooledObject
        {
            public bool Enabled { get; set; }
            public bool Destroyed { get; set; }
        }

        private const int MAX_POOL_SIZE = 5;

        private static IObjectPool<PooledObject> EmptyObjectPoolWithDestroyFuncAndMaxSizeOfFive => new ObjectPool<PooledObject>(
            CreatePooledObject,
            OnTake,
            OnReturn,
            OnDestroy,
            0,
            MAX_POOL_SIZE,
            OverflowMode.HardLimit);

        private static IObjectPool<PooledObject> FullObjectPoolWithDestroyFuncAndMaxSizeOfFive => new ObjectPool<PooledObject>(
            CreatePooledObject,
            OnTake,
            OnReturn,
            OnDestroy,
            MAX_POOL_SIZE,
            MAX_POOL_SIZE,
            OverflowMode.HardLimit);

        private static PooledObject CreatePooledObject() => new PooledObject { Destroyed = false };
        private static void OnTake(PooledObject pooledObject) => pooledObject.Enabled = true;
        private static void OnReturn(PooledObject pooledObject) => pooledObject.Enabled = false;
        private static void OnDestroy(PooledObject pooledObject) => pooledObject.Destroyed = true;
    }
}
