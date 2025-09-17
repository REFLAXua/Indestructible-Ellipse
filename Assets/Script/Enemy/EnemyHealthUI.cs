using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{

    private EnemyHealth enemyHealth;
    private Slider slider;

    void Start()
    {
        slider = GetComponentInChildren<Slider>();
        if (slider == null)
            Debug.LogWarning("Slider component not found on EnemyHealthUI GameObject.", this);

        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
            Debug.LogWarning("EnemyHealth component not found in parent hierarchy.", this);
    }

    void Update()
    {
        if (slider != null && enemyHealth != null)
            slider.value = enemyHealth.enemyHealth;
    }
}
