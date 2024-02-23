using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingCameraShake : MonoBehaviour
{
    [Range(0, 2f)]
    public float jumpDuration = 0.5f;
    [Range(0, 2f)]
    public float jumpHeight = 0.5f;

    public Transform cameraTransform;
    private Vector3 originalPosition;

    private void Start()
    {
        originalPosition = cameraTransform.localPosition;
    }
    public IEnumerator PerformJumpEffect()
    {
        float elapsedTime = 0f;

        while (elapsedTime < jumpDuration)
        {
            float newY = Mathf.Sin(elapsedTime / jumpDuration * Mathf.PI) * jumpHeight;
            cameraTransform.localPosition = originalPosition - new Vector3(0f, newY, 0f);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
