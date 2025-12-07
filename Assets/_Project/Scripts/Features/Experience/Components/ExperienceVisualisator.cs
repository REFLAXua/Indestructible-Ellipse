using UnityEngine;
using System.Collections;

namespace Features.Experience.Components
{
    public class ExperienceVisualisator : MonoBehaviour
    {
        [SerializeField] private float _floatDuration = 2f;
        [SerializeField] private float _floatHeight = 2f;
        
        [SerializeField] private Sprite[] _xpNumberSprites; // 0-9 for XP (yellow)
        [SerializeField] private Sprite[] _goldNumberSprites; // 0-9 for Gold (gold/orange)
        
        [SerializeField] private float _spriteScale = 1f;
        [SerializeField] private float _digitSpacing = 0.4f;
        [SerializeField] private float _numberGroupSpacing = 1.5f;

        public enum NumberType { XP, Gold }

        public void ShowFloatingXP(Vector3 position, int xpAmount, int groupIndex = 0)
        {
            ShowFloatingNumber(position, xpAmount, NumberType.XP, groupIndex);
        }

        public void ShowFloatingGold(Vector3 position, int goldAmount, int groupIndex = 0)
        {
            ShowFloatingNumber(position, goldAmount, NumberType.Gold, groupIndex);
        }

        public void ShowFloatingNumber(Vector3 position, int numberValue, NumberType type, int groupIndex = 0)
        {
            StartCoroutine(DisplayNumber(position, numberValue, type, groupIndex));
        }

        private IEnumerator DisplayNumber(Vector3 position, int numberValue, NumberType type, int groupIndex)
        {
            string numberText = numberValue.ToString();
            GameObject[] digitObjects = new GameObject[numberText.Length];

            // Get player position for rotation
            Transform player = GameObject.FindGameObjectWithTag("MainCamera")?.transform;

            // Select sprite array and color based on type
            Sprite[] spriteArray = type == NumberType.XP ? _xpNumberSprites : _goldNumberSprites;

            // Create container for this number group
            GameObject container = new GameObject($"Number_Container_{type}_{groupIndex}");
            container.transform.position = position;

            // Calculate total width for centering this number
            float totalWidth = (numberText.Length - 1) * _digitSpacing;
            float startX = -totalWidth / 2f;
            
            // Offset by group index for multiple numbers on one line
            startX += groupIndex * _numberGroupSpacing;

            // Create sprite for each digit
            for (int i = 0; i < numberText.Length; i++)
            {
                int digit = int.Parse(numberText[i].ToString());
                digitObjects[i] = new GameObject($"Digit_{i}");
                digitObjects[i].transform.parent = container.transform;
                digitObjects[i].transform.localPosition = Vector3.right * (startX + i * _digitSpacing);
                digitObjects[i].transform.localScale = Vector3.one * _spriteScale;

                SpriteRenderer spriteRenderer = digitObjects[i].AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = spriteArray[digit];
                spriteRenderer.sortingOrder = 100 + i;

                digitObjects[i].name = $"Digit_{digit}";
            }

            float elapsedTime = 0f;
            Vector3 startPos = position;

            while (elapsedTime < _floatDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _floatDuration;

                // Move container up
                Vector3 newPos = startPos + Vector3.up * (progress * _floatHeight);
                container.transform.position = newPos;

                // Rotate entire container to face player
                if (player != null)
                {
                    Vector3 dirToPlayer = (player.position - container.transform.position).normalized;
                    float angle = Mathf.Atan2(dirToPlayer.x, dirToPlayer.z) * Mathf.Rad2Deg;
                    container.transform.rotation = Quaternion.Euler(0, angle + 180f, 0);
                }

                // Fade out all digits
                foreach (GameObject digit in digitObjects)
                {
                    SpriteRenderer sr = digit.GetComponent<SpriteRenderer>();
                    Color color = sr.color;
                    color.a = Mathf.Lerp(1f, 0f, progress);
                    sr.color = color;
                }

                yield return null;
            }

            Destroy(container);
        }
    }
}