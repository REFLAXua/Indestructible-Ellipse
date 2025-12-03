using UnityEngine;

namespace Core.Utils
{
    public class BillBoard : MonoBehaviour
    {
        private Camera _cam;
        
        /// <summary>
        /// Additional rotation around the Z axis (view axis).
        /// Useful for spinning effects on billboarded sprites.
        /// </summary>
        public float ZRotation { get; set; }

        private void LateUpdate()
        {
            if (_cam == null)
            {
                _cam = Camera.main;
                if (_cam == null) _cam = FindObjectOfType<Camera>();
            }

            if (_cam != null)
            {
                transform.rotation = _cam.transform.rotation * Quaternion.Euler(0, 0, ZRotation);
            }
        }
    }
}
