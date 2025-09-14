using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float sensitivity = 1;
    private float baseSensitivity;
    private float rotationX;
    private float rotationY;

    [SerializeField] private float cameraPivotX;
    [SerializeField] private float cameraPivotY;
    [SerializeField] private float cameraPivotZ;

    private Transform playerTransform;
    private Transform cameraTransform;
    private Transform cameraPivot;
    void Start()
    {
        baseSensitivity = sensitivity;

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        cameraTransform = GameObject.Find("Main Camera").transform;
        cameraPivot = GameObject.Find("CameraPivot").transform;

        cameraPivotX = playerTransform.position.x + 1f;
        cameraPivotY = playerTransform.position.y + 2f;
        cameraPivotZ = playerTransform.position.z - 3f;

        cameraPivot.position = new Vector3(cameraPivotX, cameraPivotY, cameraPivotZ);
    }


    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        rotationX += mouseX;
        rotationY -= mouseY;

        rotationY = Mathf.Clamp(rotationY, -20f, 20f);

        cameraTransform.rotation = Quaternion.Euler(rotationY, rotationX, 0);
        cameraTransform.position = cameraPivot.position;


        playerTransform.rotation = Quaternion.Euler(0, rotationX, 0);
    }

    public void SensitivitySlow()
    {
        sensitivity = 0.15f;
    }

    public void SensitivityNormilized()
    {
        sensitivity = baseSensitivity;
    }
}
