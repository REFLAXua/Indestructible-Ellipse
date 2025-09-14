using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFunctions : MonoBehaviour
{
    [SerializeField] private PlayerCamera cameraFunc;

    public void SensitivitySlow()
    {
        cameraFunc.SensitivitySlow();
    }

    public void SensitivityNormilized()
    {
        cameraFunc.SensitivityNormilized();
    }
}
