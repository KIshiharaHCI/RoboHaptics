using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Vector3 = UnityEngine.Vector3;

public class RoboGuidance : MonoBehaviour
{
    public RoboController robot;
    public GameObject rightController;
    public Camera virtualCamera;
    public EHDObject vrObjects;
    private EHDObject.TargetType targetObject = EHDObject.TargetType.None;

    private InputDevice xrController;
    private Vector3 controllerVelocity;
    private Vector3 controllerPosition;
    private Vector3 robotTargetPosition;
    private Vector3 robotTargetRotation = new Vector3(90f, 90f, 0f);
    private Vector3 defaultRobotPosition = new Vector3(0.8f, 1f, 0);
    private bool stateChangedSinceLastCheck = false;
    private bool orientationChanged = false;
    private bool isControllerInProximity = false;

    private Queue<Vector3> positionHistory = new Queue<Vector3>();
    private Queue<Vector3> velocityHistory = new Queue<Vector3>();
    private float historyTimer = 0f;


    private enum RobotState
    {
        Tracking,
        Engaging,
        Encounter
    }
    private RobotState currentState = RobotState.Tracking;
    private List<InputDevice> foundControllers = new List<InputDevice>();


    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    void Update()
    {

        UpdateControllerState();
        UpdateControllerHistory();
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2f);
        InitializeController();
        StartCoroutine(UpdateRobotPosition());
    }

    private void InitializeController()
    {
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, foundControllers);
        if (foundControllers.Count > 0)
        {
            xrController = foundControllers[0]; // Assume first found is the one we want
            Debug.Log("Right controller initialized.");
        }
        else
        {
            Debug.LogWarning("Right controller not found.");
        }
    }

    IEnumerator UpdateRobotPosition()
    {
        while (true)
        {
            
            if (stateChangedSinceLastCheck)
            {
                ProcessState();
                HandleRobotMovement();
            } 
            StateSwitchIfDisengaging();

            yield return new WaitForFixedUpdate();
        }
    }

    private void ProcessState()
    {

        switch (currentState)
        {

            case RobotState.Tracking:
                TrackGaze();
                break;
            case RobotState.Engaging:
                //MoveToPredictedTarget();
                break;
            case RobotState.Encounter:
                //HandleEncounter();
                break;
        }
        //Debug.Log("Current Robot State: " + currentState.ToString());
    }

    private void TrackGaze()
    {
        RaycastHit hit;
        Ray forward = new Ray(virtualCamera.transform.position, virtualCamera.transform.forward);

        if (Physics.Raycast(forward, out hit, 3.0f))
        {
            targetObject = vrObjects.GetTargetType(hit.collider.gameObject);
            orientationChanged = CheckOrientationChange(vrObjects.GetOrientation(targetObject));

            switch (vrObjects.GetOrientation(targetObject))
            {
                case EHDObject.Orientation.Front:
                    float targetX = vrObjects.GetDepth(targetObject);
                    robotTargetPosition = new Vector3(targetX, hit.point.y, hit.point.z);
                    break;
                case EHDObject.Orientation.Top:
                    robotTargetPosition = new Vector3(hit.point.x + 0.1f, vrObjects.GetDepth(targetObject), hit.point.z);
                    break;
                case EHDObject.Orientation.Bottom:
                    // Code for handling bottom orientation
                    break;
                case EHDObject.Orientation.Left:
                    robotTargetPosition = new Vector3(hit.point.x, hit.point.y, vrObjects.GetDepth(targetObject));
                    break;
                case EHDObject.Orientation.Right:
                    robotTargetPosition = new Vector3(hit.point.x, hit.point.y, vrObjects.GetDepth(targetObject));
                    break;
                default:
                    // Code for handling other orientations
                    break;
            }
            robotTargetRotation = vrObjects.GetRotation(targetObject);
            
        }
        else
        {
            robotTargetPosition = defaultRobotPosition;
            //robotTargetRotation = new Vector3(90f, 90f, 0f);
        }
    }

    private bool StateSwitchIfDisengaging()
    {
        bool isMovingTowards = CheckMovementTowardsTarget();
        stateChangedSinceLastCheck = !isMovingTowards;
        return stateChangedSinceLastCheck; 
    }

    private bool CheckMovementTowardsTarget()
    {
        if (velocityHistory.Count == 0) return false;

        float totalDistanceChange = 0;
        Vector3 previousPosition = Vector3.zero;
        float previousDistanceToTarget = 0;

        foreach (var position in positionHistory)
        {
            if (previousPosition == Vector3.zero)
            {
                previousPosition = position;
                previousDistanceToTarget = Vector3.Distance(position, robotTargetPosition);
                continue;
            }

            float currentDistanceToTarget = Vector3.Distance(position, robotTargetPosition);
            float distanceChange = previousDistanceToTarget - currentDistanceToTarget;
            totalDistanceChange += distanceChange;

            previousDistanceToTarget = currentDistanceToTarget;
        }

        // If total distance change is greater than or equal to 0, it means the controller is getting closer or staying neutral relative to the target
        return totalDistanceChange >= 0;
    }






    private void HandleRobotMovement()
    {
        if (targetObject == EHDObject.TargetType.None) return;

        Collider[] hitColliders = Physics.OverlapSphere(rightController.transform.position, 0.15f);
        isControllerInProximity = hitColliders.Length > 0;

        if (isControllerInProximity) return;
        
        robot.targetPosition = robotTargetPosition;
        robot.pitch = robotTargetRotation.x;
        robot.yaw = robotTargetRotation.y; 


    }

    private void CheckControllerColliderProximity()
    {
        Collider[] hitColliders = Physics.OverlapSphere(rightController.transform.position, 0.05f); // 5cm radius
        isControllerInProximity = hitColliders.Length > 0; // Set true if any collider is within proximity
    }

    private void HandleRotateMovement()
    {
        if (targetObject == EHDObject.TargetType.None) return;

        // Start the coroutine to smoothly transition to the target position and orientation
        StartCoroutine(RotateGradually(robotTargetPosition, robotTargetRotation, 1.0f));
    }

    IEnumerator RotateGradually(Vector3 targetPosition, Vector3 targetRotation, float duration)
    {
        float time = 0;
        Vector3 startPosition = robot.targetPosition;
        float startPitch = robot.pitch;
        float startYaw = robot.yaw;

        while (time < duration)
        {
            // Interpolate the position
            robot.targetPosition = Vector3.Lerp(startPosition, targetPosition, time / duration);
            
            // Interpolate the pitch and yaw
            robot.pitch = Mathf.Lerp(startPitch, targetRotation.x, time / duration);
            robot.yaw = Mathf.Lerp(startYaw, targetRotation.y, time / duration);

            // Increment the time
            time += Time.deltaTime;
            yield return null;
        }

        // Ensure the final values are set
        robot.targetPosition = targetPosition;
        robot.pitch = targetRotation.x;
        robot.yaw = targetRotation.y;
    }

    private EHDObject.Orientation previousOrientation = EHDObject.Orientation.Front;

    private bool CheckOrientationChange(EHDObject.Orientation currentOrientation)
    {
        bool orientationUnchanged = currentOrientation == previousOrientation;
        previousOrientation = currentOrientation;

        return orientationUnchanged;
    }


    private void UpdateControllerState()
    {
        if (!xrController.isValid) return;

        Vector3 tempVelocity;
        if (xrController.TryGetFeatureValue(CommonUsages.deviceVelocity, out tempVelocity))
        {
            // Swap XZ to offset XR rig rotation
            controllerVelocity = new Vector3(tempVelocity.z, tempVelocity.y, tempVelocity.x);
        }

        if (rightController != null) 
        {
            controllerPosition = rightController.transform.position;
        }    
    }

    private void UpdateControllerHistory()
    {
        historyTimer += Time.deltaTime;
        float historyDuration = 0.1f;

        if (historyTimer >= historyDuration)
        {
            if (positionHistory.Count >= 10)
            {
                positionHistory.Dequeue();
                velocityHistory.Dequeue();
            }

            positionHistory.Enqueue(controllerPosition);
            velocityHistory.Enqueue(controllerVelocity);

            historyTimer = 0;
        }    
    }
}
