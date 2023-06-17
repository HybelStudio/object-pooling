using UnityEngine;

namespace Hybel.ObjectPooling
{
    public sealed class PoolableObjectLifeTime : PoolableObject
    {
        #region Editable Fields

        [SerializeField] private float _maxLifeTime;

        #endregion

        #region Fields

        private Rigidbody _rb;
        private bool _hasRb = false;
        private float _lifeTime;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (TryGetComponent(out _rb))
            {
                _hasRb = true;
                return;
            }
        }

        private void Update()
        {
            _lifeTime += Time.deltaTime;
            if (_lifeTime > _maxLifeTime)
                HandlePoolReturn();
        }

        #endregion

        public override void HandlePoolReturn()
        {
            if (_hasRb)
            {
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
            _lifeTime = 0f;
            ObjectPooler.Release(ObjectPool, gameObject);
        }
    }
}