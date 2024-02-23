using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Header("Sensivity Parameters")]
    public float sensX;
    public float sensY;
    [Range(0f, 2f)]
    public float mouseSensivity = 1f;

    public Transform orientation;

    float xRotation;
    float yRotation;

    private void Update()
    {
        // Get Mouse Input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX * mouseSensivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY * mouseSensivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate Cam and Orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
