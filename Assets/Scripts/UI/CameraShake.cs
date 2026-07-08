using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private Transform camTransform;

    private Coroutine shakeRoutine;
    private Vector3 currentShakeOffset;

    public void Shake(float duration, float magnitude)
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            camTransform.localPosition -= currentShakeOffset;
            currentShakeOffset = Vector3.zero;
        }

        shakeRoutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            camTransform.localPosition -= currentShakeOffset;
            currentShakeOffset = new Vector3(
                Random.Range(-1f, 1f) * magnitude,
                Random.Range(-1f, 1f) * magnitude,
                0f);
            camTransform.localPosition += currentShakeOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition -= currentShakeOffset;
        currentShakeOffset = Vector3.zero;
        shakeRoutine = null;
    }
}
