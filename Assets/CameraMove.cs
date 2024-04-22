using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float smoothTime = 2.0f; // Time taken to move from the current to the target position.
    public float minZ = -10.0f; // Minimum Z value for the random target position.
    public float maxZ = 10.0f; // Maximum Z value for the random target position.
    public float minY = -5.0f; // Minimum Y value for the random target position.
    public float maxY = 5.0f; // Maximum Y value for the random target position.

    private Vector3 targetPosition; // Target position for the camera.
    private float timeToChange = 0; // Timer to track when to change the target position.
    private float timer = 0; // Current timer value.

    void Start()
    {
        // Initialize the target position as the current local position of the camera.
        targetPosition = transform.localPosition;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Check if it's time to change the target position.
        if (timer >= timeToChange)
        {
            SetNewTargetPosition();
            timer = 0; // Reset timer.
        }

        // Smoothly interpolate the camera's position towards the target position.
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime / smoothTime);
    }

    void SetNewTargetPosition()
    {
        // Generate a random Z value within specified limits.
        float randomZ = Random.Range(minZ, maxZ);
        // Generate a random Y value within specified limits.
        float randomY = Random.Range(minY, maxY);

        // Update the target position with the new Y and Z values, maintaining the current X value.
        targetPosition = new Vector3(transform.localPosition.x, randomY, randomZ);

        // Optionally, adjust timeToChange if you want the changes to occur at variable times.
        timeToChange = Random.Range(2, 5); // Change target every 2 to 5 seconds.
    }
}