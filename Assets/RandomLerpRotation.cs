using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLerpRotation : MonoBehaviour
{
    public Vector3 minAngles; // Minimum rotation angles
    public Vector3 maxAngles; // Maximum rotation angles
    public float rotationSpeed = 180f; // Degrees per second
    public float changeInterval = 2f; // Time in seconds between target angle changes

    private Vector3 targetAngles;
    private float timer;

    void Start()
    {
        UpdateTargetRotation();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            UpdateTargetRotation();
            timer = 0; // Reset the timer
        }

        // Smoothly interpolate the rotation towards the target angles
        Vector3 currentAngles = transform.localEulerAngles;
        currentAngles.x = MoveTowardsAngle(currentAngles.x, targetAngles.x, rotationSpeed * Time.deltaTime);
        currentAngles.y = MoveTowardsAngle(currentAngles.y, targetAngles.y, rotationSpeed * Time.deltaTime);
        currentAngles.z = MoveTowardsAngle(currentAngles.z, targetAngles.z, rotationSpeed * Time.deltaTime);

        transform.localEulerAngles = currentAngles;
    }

    void UpdateTargetRotation()
    {
        float x = Random.Range(minAngles.x, maxAngles.x);
        float y = Random.Range(minAngles.y, maxAngles.y);
        float z = Random.Range(minAngles.z, maxAngles.z);
        targetAngles = new Vector3(x, y, z);
    }

    float MoveTowardsAngle(float current, float target, float maxDelta)
    {
        current = NormalizeAngle(current);
        target = NormalizeAngle(target);
        return Mathf.MoveTowardsAngle(current, target, maxDelta);
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        return angle;
    }
}