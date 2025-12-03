using UnityEngine;
using UnityEngine.UI;

namespace Features.UI
{
    [RequireComponent(typeof(RawImage))]
    public class LiquidAnimator : MonoBehaviour
    {
        [SerializeField] private float _speedX = 0.5f;
        [SerializeField] private float _speedY = 0.0f;

        private RawImage _rawImage;
        private Rect _uvRect;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
            _uvRect = _rawImage.uvRect;
        }

        private void Update()
        {
            _uvRect.x += _speedX * Time.deltaTime;
            _uvRect.y += _speedY * Time.deltaTime;
            _rawImage.uvRect = _uvRect;
        }
    }
}
