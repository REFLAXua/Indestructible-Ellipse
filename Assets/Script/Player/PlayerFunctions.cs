using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFunctions : MonoBehaviour
{
    [Tooltip("Put the CinemachineCameraController script")]
    [SerializeField] private CinemachineCameraController cameraFunc;

    public void SensitivitySlow()
    {
        cameraFunc.SensitivitySlow();
    }

    public void SensitivityNormilized()
    {
        cameraFunc.SensitivityNormilized();
    }
}
