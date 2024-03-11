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
    public MeshRenderer controllerMeshRenderer;
    public Camera virtualCamera;
    public VRObject vrObjects;
    private VRObject.TargetType targetObject = VRObject.TargetType.None;

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
                case VRObject.Orientation.Front:
                    float targetX = vrObjects.GetDepth(targetObject);
                    robotTargetPosition = new Vector3(targetX, hit.point.y, hit.point.z);
                    break;
                case VRObject.Orientation.Top:
                    robotTargetPosition = new Vector3(hit.point.x + 0.1f, vrObjects.GetDepth(targetObject), hit.point.z);
                    break;
                case VRObject.Orientation.Bottom:
                    // Code for handling bottom orientation
                    break;
                case VRObject.Orientation.Left:
                    robotTargetPosition = new Vector3(hit.point.x, hit.point.y, vrObjects.GetDepth(targetObject));
                    break;
                case VRObject.Orientation.Right:
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
        if (targetObject == VRObject.TargetType.None) return;

        Collider[] hitColliders = Physics.OverlapSphere(rightController.transform.position, 0.15f);
        isControllerInProximity = hitColliders.Length > 0;

        if (isControllerInProximity) return;
        
        robot.targetPosition = robotTargetPosition;
        robot.pitch = robotTargetRotation.x;
        robot.yaw = robotTargetRotation.y; 


    }

    private VRObject.Orientation previousOrientation = VRObject.Orientation.Front;

    private bool CheckOrientationChange(VRObject.Orientation currentOrientation)
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
