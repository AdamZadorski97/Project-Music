using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform shakePivot; // Assign this in the inspector to the ShakePivot GameObject
    public Transform movePivot; // Assign this in the inspector to the MovePivot GameObject

    private Vector3 originalMovePosition;

    void Start()
    {
        originalMovePosition = movePivot.localPosition;
    }

    public void ShakeCamera(float duration, float power, AnimationCurve curve)
    {
        StartCoroutine(Shake(duration, power, curve));
    }

    private IEnumerator Shake(float duration, float power, AnimationCurve curve)
    {
        Vector3 originalPos = shakePivot.localPosition;
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percentComplete = elapsed / duration;
            float damper = curve.Evaluate(percentComplete);
            float x = Random.Range(-1f, 1f) * power * damper;
            float y = Random.Range(-1f, 1f) * power * damper;
            shakePivot.localPosition = new Vector3(x, y, originalPos.z);
            yield return null;
        }
        shakePivot.localPosition = originalPos;
    }

    public void MoveCamera(Vector3 newPosition, float speed, AnimationCurve curve)
    {
        StartCoroutine(MoveToPosition(newPosition, speed, curve));
    }

    private IEnumerator MoveToPosition(Vector3 newPosition, float speed, AnimationCurve curve)
    {
        Vector3 startPosition = movePivot.localPosition;
        float time = 0.0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime * speed;
            float curveValue = curve.Evaluate(time);
            movePivot.localPosition = Vector3.Lerp(startPosition, newPosition, curveValue);
            yield return null;
        }
        // Optionally wait at the new position
        yield return new WaitForSeconds(1f); // wait time at new position

        // Return to original position
        time = 0.0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime * speed;
            float curveValue = curve.Evaluate(time);
            movePivot.localPosition = Vector3.Lerp(newPosition, originalMovePosition, curveValue);
            yield return null;
        }
    }
}