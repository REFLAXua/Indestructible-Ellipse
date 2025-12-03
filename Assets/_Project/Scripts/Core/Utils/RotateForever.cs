using UnityEngine;

namespace Core.Utils
{
    public class RotateForever : MonoBehaviour
    {
        public float rotationSpeed = 90f;

        void Update()
        {
            transform.rotation *= Quaternion.Euler(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }
}
