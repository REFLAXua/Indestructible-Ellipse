using UnityEngine;

namespace Core.Utils
{
    public class BillBoard : MonoBehaviour
    {
        private Transform _cameraTransform;
        private bool _initialized;

        public float ZRotation { get; set; }

        private void Start()
        {
            CacheCamera();
        }

        private void CacheCamera()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
                _initialized = true;
            }
        }

        private void LateUpdate()
        {
            if (!_initialized || _cameraTransform == null)
            {
                CacheCamera();
                if (!_initialized) return;
            }

            transform.rotation = _cameraTransform.rotation * Quaternion.Euler(0, 0, ZRotation);
        }

        public void ForceRefreshCamera()
        {
            _initialized = false;
            CacheCamera();
        }
    }
}
