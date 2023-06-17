using UnityEngine;

namespace Hybel.ObjectPooling
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private ObjectPoolAsset pool;
        [SerializeField] private int amount;
        [SerializeField] private Vector3 positionOffset = Vector3.forward;
        [SerializeField] private Vector3 rotationOffset = Vector3.zero;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
                Spawn();
        }

        public void Spawn()
        {
            for (int i = 0; i < amount; i++)
            {
                Vector3 position = transform.position + positionOffset;
                Vector3 eulerAngles = transform.eulerAngles + rotationOffset;
                ObjectPooler.Get(pool, position, Quaternion.Euler(eulerAngles));
            }
        }
    }
}
