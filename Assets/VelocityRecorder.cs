using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityRecorder : MonoBehaviour
{
    public GameObject rightController;
    private List<(float time, float velocity)> recordedReaches = new List<(float, float)>();
    private float startTime;
    private float startVelocity;

    void Start()
    {
        StartCoroutine(RecordReaches());
    }

    IEnumerator RecordReaches()
    {
        while (true)
        {
            // Check if right controller exists and is active
            if (rightController != null && rightController.activeInHierarchy)
            {
                Vector3 currentPosition = rightController.transform.position;
                float currentVelocity = CalculateControllerVelocity(currentPosition);

                // If velocity exceeds threshold, start recording
                if (currentVelocity > 3f && startVelocity == 0f)
                {
                    startTime = Time.time;
                    startVelocity = currentVelocity;
                }
                // If velocity drops below threshold and recording has started, finish recording
                else if (startVelocity > 0f && currentVelocity < 3f)
                {
                    float reachTime = Time.time - startTime;
                    recordedReaches.Add((reachTime, startVelocity));
                    startVelocity = 0f;
                }
            }
            else
            {
                // Handle case where right controller GameObject is not assigned or inactive
                Debug.LogWarning("Right controller not assigned or inactive.");
            }

            yield return null;
        }
    }

    private float CalculateControllerVelocity(Vector3 currentPosition)
    {
        // Calculate velocity based on change in position
        float distance = Vector3.Distance(currentPosition, transform.position);
        float velocity = distance / Time.deltaTime;
        return velocity;
    }

    void OutputRecordedReaches()
    {
        Debug.Log("Recorded Reaches:");
        foreach (var reach in recordedReaches)
        {
            Debug.Log("Time: " + reach.time + " Velocity: " + reach.velocity);
        }
    }

    IEnumerator OutputAfterDelay()
    {
        yield return new WaitForSeconds(8f);
        OutputRecordedReaches();
    }

    void OnDisable()
    {
        StopCoroutine(RecordReaches());
        StartCoroutine(OutputAfterDelay());
    }
}
