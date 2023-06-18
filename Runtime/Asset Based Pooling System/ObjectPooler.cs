#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using Hybel.Monads;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hybel.ObjectPooling
{
    public class ObjectPooler : MonoBehaviour
    {
        #region Singleton

        private static ObjectPooler _instance;

        public static ObjectPooler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameObject("Object Pooler").AddComponent<ObjectPooler>();

                return _instance;
            }
        }

        private void CreateSingleton()
        {
            if (_instance != null && _instance != this)
                Destroy(gameObject);

            _instance = this;
        }

        #endregion

#if UNITY_EDITOR
        #region EditorLogging

        [RuntimeInitializeOnLoadMethod]
        private static void SubscribeOnPlayModeChanged() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

        private static void OnPlayModeChanged(PlayModeStateChange stateChange)
        {
            if (stateChange is PlayModeStateChange.EnteredPlayMode)
            {
                foreach (KeyValuePair<ObjectPoolAsset,ObjectPool> pair in Instance._poolDictionary)
                {
                    ObjectPoolAsset poolAsset = pair.Key;
                    ObjectPool pool = pair.Value;

                    if (poolAsset.LogAverageAmount)
                        Instance.StartCoroutine(pool.TrackAverageRoutine());
                }
                
                return;
            }
            
            if (stateChange is not PlayModeStateChange.ExitingPlayMode)
                return;
            
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

            foreach (KeyValuePair<ObjectPoolAsset, ObjectPool> pair in Instance._poolDictionary)
            {
                ObjectPoolAsset poolAsset = pair.Key;
                ObjectPool pool = pair.Value;

                switch (poolAsset.LogHighestAmount, poolAsset.LogAverageAmount)
                {
#if HYBEL_CLOGGER
                    case (true, false):
                        poolAsset.Log($"Highest amount of objects in '{poolAsset.name}' was {pool.HighestAmount}.");
                        break;
                    
                    case (false, true):
                        poolAsset.Log($"Average amount of objects in '{poolAsset.name}' was {pool.HighestAmount}.");
                        break;
                    
                    case (true, true):
                        poolAsset.Log($"{poolAsset.name}' has an average amount of objects at {pool.AverageAmount} with a peak of {pool.HighestAmount}.");
                        break;
#else
                    case (true, false):
                        Debug.Log($"[ObjectPooler] Highest amount of objects in '{poolAsset.name}' was {pool.HighestActiveAmount}.", poolAsset);
                        break;
                    
                    case (false, true):
                        Debug.Log($"[ObjectPooler] Average amount of objects in '{poolAsset.name}' was {pool.AverageActiveAmount}.", poolAsset);
                        break;
                    
                    case (true, true):
                        Debug.Log($"[ObjectPooler] '{poolAsset.name}' has an average amount of objects at {pool.AverageActiveAmount} with a peak of {pool.HighestActiveAmount}.", poolAsset);
                        break;
#endif
                }
            }
        }

        #endregion
#endif
        
#if ODIN_INSPECTOR
        [InlineEditor]
#endif
        [SerializeField] private List<ObjectPoolAsset> pools = new List<ObjectPoolAsset>();

        private Dictionary<ObjectPoolAsset, ObjectPool> _poolDictionary = new Dictionary<ObjectPoolAsset, ObjectPool>();
        private Dictionary<ObjectPoolAsset, Transform> _parentDictionary = new Dictionary<ObjectPoolAsset, Transform>();

        private void Awake()
        {
            CreateSingleton();

            foreach (var pool in pools)
                CreatePool(pool);
        }

        public static int CountAll(ObjectPoolAsset pool)
        {
            if (Instance._poolDictionary.ContainsKey(pool))
                return Instance._poolDictionary[pool].CountAll;

            return -1;
        }

        public static int CountActive(ObjectPoolAsset pool)
        {
            if (Instance._poolDictionary.ContainsKey(pool))
                return Instance._poolDictionary[pool].CountActive;

            return -1;
        }

        public static int CountInactive(ObjectPoolAsset pool)
        {
            if (Instance._poolDictionary.ContainsKey(pool))
                return Instance._poolDictionary[pool].CountInactive;

            return -1;
        }

        public static int MaxPoolSize(ObjectPoolAsset pool)
        {
            if (Instance._poolDictionary.ContainsKey(pool))
                return Instance._poolDictionary[pool].MaxPoolSize;

            return -1;
        }

        public static GameObject Get(ObjectPoolAsset pool, Vector3 position, Quaternion rotation)
        {
            if (Instance._poolDictionary[pool].TryGet(position, rotation, out GameObject objToSpawn))
                objToSpawn.transform.SetPositionAndRotation(position, rotation);

            return objToSpawn;
        }

        public static GameObject Get(ObjectPoolAsset pool)
        {
            if (!Instance._poolDictionary.ContainsKey(pool))
                Instance.CreatePool(pool);

            return Instance._poolDictionary[pool].Get(Option<Vector3>.None, Option<Quaternion>.None);
        }

        public static bool TryGet(ObjectPoolAsset pool, out GameObject obj)
        {
            if (!Instance._poolDictionary.ContainsKey(pool))
            {
                obj = null;
                return false;
            }

            return Instance._poolDictionary[pool].TryGet(Option<Vector3>.None, Option<Quaternion>.None, out obj);
        }

        public static IEnumerable<GameObject> Get(ObjectPoolAsset pool, int amount)
        {
            for (int i = 0; i < amount; i++)
                if (TryGet(pool, out GameObject poolableObject))
                    yield return poolableObject;
        }

        public static void Release(ObjectPoolAsset pool, GameObject poolableObject)
        {
            if (!Instance._poolDictionary.ContainsKey(pool))
                return;

            Instance._poolDictionary[pool].Release(poolableObject);
        }

        public static void Clear(ObjectPoolAsset pool)
        {
            if (!Instance._poolDictionary.ContainsKey(pool))
                return;

            Instance._poolDictionary[pool].Clear();
        }

        private void CreatePool(ObjectPoolAsset pool)
        {
            if (!_parentDictionary.ContainsKey(pool))
                CreateParent(pool);

            ObjectPool objectPool = new ObjectPool(
                Create,
                Take,
                Release,
                Destroy,
                pool.AmountToStartWith,
                pool.OverflowIncrement,
                pool.OverflowMode);

            _poolDictionary.Add(pool, objectPool);

            if (pool.AmountToStartWith > 0)
            {
                if (pool.ShouldInstantiatePerFrame)
                    StartCoroutine(PopulatePerFrame(pool));
                else
                    Populate(pool);
            }

            GameObject Create()
            {
                var obj = Instantiate(pool.Prefab, _parentDictionary[pool]);

                if (obj.TryGetComponent(out IPoolableObject poolableObject))
                {
                    poolableObject.ObjectPool = pool;
                    obj.transform.parent = _parentDictionary[pool];
                    return obj;
                }

                throw new InvalidOperationException($"Assigned prefab in {name} does not implement {nameof(IPoolableObject)}.");
            }

            void Take(GameObject poolableObject, Option<Vector3> positionOption, Option<Quaternion> rotationOption)
            {
                if (positionOption.TryUnwrap(out Vector3 position))
                    poolableObject.transform.position = position;

                if (rotationOption.TryUnwrap(out Quaternion rotation))
                    poolableObject.transform.rotation = rotation;

                poolableObject.SetActive(true);
            }

            void Release(GameObject poolableObject) => poolableObject.SetActive(false);
            void Destroy(GameObject poolableObject) => GameObject.Destroy(poolableObject);
        }

        private void CreateParent(ObjectPoolAsset pool)
        {
            GameObject newObject = new GameObject(pool.name);
            Transform newParent = newObject.transform;
            newParent.parent = transform;
            _parentDictionary.Add(pool, newParent);
        }

        private void Populate(ObjectPoolAsset pool) // Cannot be null here and it is contained in _poolDictionary.
        {
            _poolDictionary[pool].PopulateInactive(pool.AmountToStartWith);
        }

        private IEnumerator PopulatePerFrame(ObjectPoolAsset pool)
        {
            int totalToStartWith = pool.AmountToStartWith;

            if (totalToStartWith <= 0)
                yield break;

            int batch = pool.BatchAmount;
            int current = 0;

            while (current < totalToStartWith)
            {
                int currentBatch = Mathf.Min(batch, totalToStartWith - current);
                int count = _poolDictionary[pool].PopulateInactive(currentBatch);

                if (count <= 0)
                    yield break;

                current += count;
                yield return null;
            }
        }

        private class ObjectPool
        {
            private readonly Queue<GameObject> _inactive;
            private readonly List<GameObject> _active;

            private readonly Func<GameObject> _onCreateObject;
            private readonly Action<GameObject, Option<Vector3>, Option<Quaternion>> _onTakeObject;
            private readonly Action<GameObject> _onReleaseObject;
            private readonly Action<GameObject> _onDestroyObject;

            private int _maxPoolSize;
            private readonly int _overflowIncrement;
            private readonly OverflowMode _overflowMode;
            private readonly List<int> _activeAmounts = new();

            public int CountAll => CountActive + CountInactive;

            public int CountActive => _active.Count;

            public int CountInactive => _inactive.Count;

            public int MaxPoolSize => _maxPoolSize;

            public int HighestActiveAmount { get; private set; }

            public int AverageActiveAmount => Mathf.RoundToInt((float)_activeAmounts.Average());

            public ObjectPool(
                Func<GameObject> onCreateObject,
                Action<GameObject, Option<Vector3>, Option<Quaternion>> onTakeObject,
                Action<GameObject> onReleaseObject,
                Action<GameObject> onDestroyObject,
                int maxPoolSize,
                int overflowIncrement,
                OverflowMode overflowMode)
            {
                _inactive = new Queue<GameObject>(maxPoolSize);
                _active = new List<GameObject>(maxPoolSize);

                _onCreateObject = onCreateObject;
                _onTakeObject = onTakeObject;
                _onReleaseObject = onReleaseObject;
                _onDestroyObject = onDestroyObject;

                _maxPoolSize = maxPoolSize;
                _overflowIncrement = overflowIncrement;
                _overflowMode = overflowMode;
            }

            public GameObject Get(Option<Vector3> position, Option<Quaternion> rotation)
            {
                if (_inactive.Count > 0)
                {
                    GameObject dequeuedObject = _inactive.Dequeue();
                    _onTakeObject(dequeuedObject, position, rotation);
                    _active.Add(dequeuedObject);
                    HighestActiveAmount = Mathf.Max(HighestActiveAmount, CountActive);
                    return dequeuedObject;
                }

                if (PopulateInactive())
                {
                    HighestActiveAmount = Mathf.Max(HighestActiveAmount, CountActive);
                    return Get(position, rotation);
                }
                
                return null;
            }

            public IEnumerable<GameObject> Get(int amount)
            {
                for (int i = 0; i < amount; i++)
                    if (TryGet(Option<Vector3>.None, Option<Quaternion>.None, out GameObject obj))
                        yield return obj;
            }

            public bool TryGet(Option<Vector3> position, Option<Quaternion> rotation, out GameObject obj)
            {
                obj = Get(position, rotation);
                return obj != null;
            }

            public void Release(GameObject obj)
            {
                if (!_active.Contains(obj))
                    return;

                _active.Remove(obj);
                _onReleaseObject(obj);

                if (CountAll >= _maxPoolSize)
                {
                    _onDestroyObject?.Invoke(obj);
                    return;
                }

                _inactive.Enqueue(obj);
            }

            public void Clear()
            {
                if (_onDestroyObject != null)
                {
                    foreach (GameObject inactiveObject in _inactive)
                        _onDestroyObject(inactiveObject);

                    foreach (GameObject activeObject in _active)
                        _onDestroyObject(activeObject);
                }

                _inactive.Clear();
                _active.Clear();
            }

            public bool PopulateInactive()
            {
                if (CountAll >= _maxPoolSize)
                {
                    switch (_overflowMode)
                    {
                        case OverflowMode.HardLimit:
                            return false;

                        case OverflowMode.StealFromActive:
                            if (_maxPoolSize <= 0)
                                return false;

                            GameObject stolenObj = _active[0];
                            _active.Remove(stolenObj);
                            _onReleaseObject(stolenObj);
                            _inactive.Enqueue(stolenObj);
                            return true;

                        case OverflowMode.IncreaseSize:
                            _maxPoolSize += _overflowIncrement;
                            break;
                    }
                }

                GameObject createdObject = _onCreateObject();
                _onReleaseObject(createdObject);
                _inactive.Enqueue(createdObject);
                return true;
            }

            public int PopulateInactive(int amount)
            {
                int count = 0;
                for (int i = 0; i < amount; i++)
                {
                    if (!PopulateInactive())
                        return count;

                    count++;
                }

                return count;
            }

            public IEnumerator TrackAverageRoutine()
            {
                const float UPDATE_INTERVAL = 1f;
                const int MAX_AMOUNTS = 1024;

                var interval = new WaitForSeconds(UPDATE_INTERVAL);
                
                while (true)
                {
                    while (_activeAmounts.Count >= MAX_AMOUNTS)
                        _activeAmounts.RemoveAt(0);
                    
                    _activeAmounts.Add(CountActive);
                    yield return interval;
                }
            }
        }
    }
}
