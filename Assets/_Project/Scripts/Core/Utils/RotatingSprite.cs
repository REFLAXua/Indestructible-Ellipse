using UnityEngine;

namespace Core.Utils
{
    /// <summary>
    /// Rotates the sprite around its local Z-axis.
    /// Useful for spinning effects on billboarded sprites.
    /// </summary>
    public class RotatingSprite : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 200f;
        
        public float RotationSpeed { get; set; }
        public bool IsRotating { get; set; }

        private void Start()
        {
            RotationSpeed = _rotationSpeed;
        }

        private void LateUpdate()
        {
            if (IsRotating)
            {
                transform.Rotate(0, 0, RotationSpeed * Time.deltaTime, Space.Self);
            }
        }
    }
}
