using System.Collections.Generic;
using UnityEngine;

namespace Hybel.ObjectPooling
{
    [CreateAssetMenu(fileName = "New Object Pool", menuName = "Objects/Object Pool")]
    public class ObjectPoolAsset : ScriptableObject, IObjectPool<GameObject>
    {
#if HYBEL_VARIABLES
        [SerializeField] private PrefabRef prefab;
        [SerializeField] private OverflowMode overflowMode;
        [SerializeField, Min(0)] private IntRef overflowIncrement = 50;
        [SerializeField] private IntRef amountToStartWith;
        [SerializeField] private InstantiationType instantiationType;
        [SerializeField, Min(1)] private IntRef batchAmount = 1;
        [SerializeField] private BoolRef logHighestAmount = false;
        [SerializeField] private BoolRef logAverageAmount = false;
#else
        [SerializeField] private GameObject prefab;
        [SerializeField] private OverflowMode overflowMode;
        [SerializeField, Min(0)] private int overflowIncrement = 50;
        [SerializeField] private int amountToStartWith;
        [SerializeField] private InstantiationType instantiationType;
        [SerializeField, Min(1)] private int batchAmount = 1;
        [SerializeField] private bool logHighestAmount = false;
        [SerializeField] private bool logAverageAmount = false;
#endif   

        public GameObject Prefab => prefab;
        public int AmountToStartWith => amountToStartWith;
        public bool ShouldInstantiatePerFrame => instantiationType == InstantiationType.BatchesPerFrame;
        public int BatchAmount => batchAmount;
        public bool LogHighestAmount => logHighestAmount;
        public bool LogAverageAmount => logAverageAmount;
        
        public int CountAll => ObjectPooler.CountAll(this);

        public int CountActive => ObjectPooler.CountActive(this);

        public int CountInactive => ObjectPooler.CountInactive(this);

        public int MaxPoolSize => ObjectPooler.MaxPoolSize(this);

        public OverflowMode OverflowMode => overflowMode;
        public int OverflowIncrement => overflowIncrement;

        public GameObject Get() => ObjectPooler.Get(this, default, default);
        public IEnumerable<GameObject> Get(int amount) => ObjectPooler.Get(this, amount);
        public bool TryGet(out GameObject obj) => ObjectPooler.TryGet(this, out obj);
        public void Release(GameObject obj) => ObjectPooler.Release(this, obj);
        public void Clear() => ObjectPooler.Clear(this);

        public enum InstantiationType
        {
            Bulk,
            BatchesPerFrame,
        }
    }
}