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
    public VRObjectNew vrObjects;
    private VRObjectNew.TargetType targetObject = VRObjectNew.TargetType.None;
    public GameObject virtualController;
    private Vector3 reachOriginPosition = Vector3.zero;
    private bool updateReachOriginPosition = true;
    private bool hasReachStarted = false;

    private InputDevice xrController;
    private Vector3 controllerVelocity;
    private Vector3 controllerPosition;
    private Vector3 robotTargetPosition = Vector3.zero;
    private Vector3 robotTargetRotation = new Vector3(90f, 90f, 0f);
    private Vector3 virtualTargetPosition;
    private Vector3 defaultRobotPosition = new Vector3(0.8f, 1f, 0);
    private bool stateChangedSinceLastCheck = false;
    private bool lastCheckMovingTowardsTarget = false;
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
        UpdateVirtualControllerPosition();

    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1f);
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

    private bool gazeHit = false;
    private Vector3 previousHitPosition = Vector3.zero;
    private Vector3 prevTargetPosition = Vector3.zero;
    private void TrackGaze()
    {
        RaycastHit hit;
        Ray forward = new Ray(virtualCamera.transform.position, virtualCamera.transform.forward);

        if (Physics.Raycast(forward, out hit, 2.0f))
        {
            gazeHit = true;
            targetObject = vrObjects.GetTargetType(hit.collider.gameObject);
            Vector3 hitPointPosition = Vector3.zero;
            
            switch (vrObjects.GetOrientation(targetObject))
            {
                case VRObjectNew.Orientation.Front:
                    hitPointPosition = new Vector3(vrObjects.GetDepth(targetObject), hit.point.y, hit.point.z);
                    break;
                case VRObjectNew.Orientation.Top:
                    hitPointPosition = new Vector3(hit.point.x + 0.1f, vrObjects.GetDepth(targetObject), hit.point.z);
                    break;
                case VRObjectNew.Orientation.Bottom:
                    // Code for handling bottom orientation
                    break;
                case VRObjectNew.Orientation.Left:
                    hitPointPosition = new Vector3(hit.point.x, hit.point.y, vrObjects.GetDepth(targetObject));
                    break;
                case VRObjectNew.Orientation.Right:
                    hitPointPosition = new Vector3(hit.point.x, hit.point.y, vrObjects.GetDepth(targetObject));
                    break;
                case VRObjectNew.Orientation.FrontRight:
                    hitPointPosition = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                    break;
                case VRObjectNew.Orientation.TopRight:
                    hitPointPosition = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                    break;
                default:
                    break;
            }
            virtualTargetPosition = hitPointPosition;
            robotTargetPosition = IsWithinSphere(hitPointPosition) ? hitPointPosition : FindNearestPointOnSphere(hitPointPosition);

            robotTargetRotation = vrObjects.GetRotation(targetObject);

            prevTargetPosition = robotTargetPosition;
            previousHitPosition = hitPointPosition;
            
        }
        else
        {
            gazeHit = false;
            
            if (previousHitPosition != Vector3.zero)
            {
                virtualTargetPosition = previousHitPosition;
                robotTargetPosition = prevTargetPosition;
            }
            else
            {
                robotTargetPosition = defaultRobotPosition;
                virtualTargetPosition = defaultRobotPosition;
            }

        }
    }

    private bool StateSwitchIfDisengaging()
    {
        bool isMovingTowards = CheckMovementTowardsTarget();
        stateChangedSinceLastCheck = !isMovingTowards;
        return stateChangedSinceLastCheck; 
    }

    private Vector3 previousTargetPosition;
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

        bool isCurrentlyMovingTowardsTarget = totalDistanceChange >= 0.015;
        Collider[] hitColliders = Physics.OverlapSphere(virtualController.transform.position, 0.3f);
        isControllerInProximity = hitColliders.Length > 0;

        if (updateReachOriginPosition && isCurrentlyMovingTowardsTarget && gazeHit && robotTargetPosition != previousTargetPosition && !controllerCollisionDetected)
        {
            previousTargetPosition = robotTargetPosition;
            reachOriginPosition = controllerPosition;
            hasReachStarted = true;
            updateReachOriginPosition = false;
            Debug.Log("Reach origin position set to: " + reachOriginPosition);
        }
        else if (!isCurrentlyMovingTowardsTarget && Physics.OverlapSphere(virtualController.transform.position, 0.15f).Length == 0)
        {
            Debug.Log("position updated");
            updateReachOriginPosition = true;
            hasReachStarted = false;

        }
        

    // Update the last state
    lastCheckMovingTowardsTarget = isCurrentlyMovingTowardsTarget;

    return isCurrentlyMovingTowardsTarget;
    }

    private bool controllerCollisionDetected = false;
    private void HandleRobotMovement()
    {
        if (targetObject == VRObjectNew.TargetType.None) return;

        Collider[] hitColliders = Physics.OverlapSphere(controllerPosition, 0.15f);
        isControllerInProximity = hitColliders.Length > 0;
        

        if (Vector3.Distance(controllerPosition, new Vector3(0, 0.78f, 0)) < 1f)
        {
            controllerCollisionDetected = true;
            return; // Early return if the condition is met
        }

        if (isControllerInProximity)
        {
            controllerCollisionDetected = true;
            return;
        } 
        
        Collider[] virtualHitColliders = Physics.OverlapSphere(virtualController.transform.position, 0.15f);
        isControllerInProximity = virtualHitColliders.Length > 0;
        
        if (isControllerInProximity)
        {
            controllerCollisionDetected = true;
            return;
        } 
        else
        {
            controllerCollisionDetected = false;
        }
        
        robot.targetPosition = robotTargetPosition;
        robot.pitch = robotTargetRotation.x;
        robot.yaw = robotTargetRotation.y; 


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

    private bool holdPosition = false;

    private void UpdateVirtualControllerPosition()
    {
        Vector3 Hv = new Vector3(controllerPosition.x + 0.000000001f, controllerPosition.y, controllerPosition.z);
        bool withinSphere =  controllerPosition != Vector3.zero && Vector3.Distance(controllerPosition, new Vector3(0, 0.78f, 0)) < 1.2f;
        if (lastCheckMovingTowardsTarget && hasReachStarted || withinSphere)
        {
            Vector3 d = (robotTargetPosition - reachOriginPosition).normalized; // Direction vector
            Vector3 Hp = controllerPosition; // Physical hand position (H_p)
            Vector3 pv = virtualTargetPosition; 
            Vector3 pp = new Vector3(robotTargetPosition.x, robotTargetPosition.y, robotTargetPosition.z);

            // Calculate Ds and Dp based on your environment and the definitions provided
            float Ds = Vector3.Dot(Hp - reachOriginPosition, d); // Distance travelled towards the target
            float Dp = Vector3.Dot(pp - Hp, d); // Distance remaining to the target

            // Using Mathf.SmoothStep for smoother interpolation
            float totalDistance = Ds + Dp;
            float smoothStepFactor = Mathf.SmoothStep(0.0f, 1.0f, Ds / totalDistance);

            // Adjusted calculation of W using the smooth step factor
            Vector3 W = smoothStepFactor * (pv - pp);

            // Apply the computed offset W to the virtual hand's position (H_v)
            Hv = Hp + W; // Virtual hand position after applying the offset
        } 
        
        bool enableRedirection = hasReachStarted;

         if (!hasReachStarted) 
        {
            enableRedirection = false;
            holdPosition = false;

            if (controllerPosition != Vector3.zero && Vector3.Distance(controllerPosition, new Vector3(0, 0.78f, 0)) < 1.2f || virtualController.transform.position != Vector3.zero && Physics.OverlapSphere(virtualController.transform.position, 0.15f).Length > 0)
            {
                enableRedirection = true;
                holdPosition = true;

            }
        }
        else
        {
            enableRedirection = true;
            holdPosition = true;
        }

        TrackController virtualControllerScript = virtualController.GetComponent<TrackController>();
        virtualControllerScript.SetRedirectionPosition(Hv, enableRedirection, holdPosition);

    }

    public bool IsWithinSphere(Vector3 point)
    {
        float radius = 0.78f;
        Vector3 center = new Vector3(0f, 0.78f, 0.00f);
        return Vector3.Distance(point, center) <= radius;
    }
    public Vector3 FindNearestPointOnSphere(Vector3 point)
    {
        float radius = 0.78f; 
        Vector3 robotCenter = new Vector3(0f, 0.78f, 0.00f);

        var direction = (point - robotCenter).normalized;
        var nearestPoint = robotCenter + direction * radius;
        return new Vector3(Mathf.Max(nearestPoint.x, -0.2f), nearestPoint.y, nearestPoint.z);
    }

}