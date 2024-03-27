using System.Collections.Generic;
using UnityEngine;

public class VRObjectNew : MonoBehaviour
{
    public GameObject toasterFront;
    public GameObject toasterTop;
    public GameObject fridgeRight;
    public GameObject fridgeTopRight;
    public GameObject ovenFront;
    public GameObject microwaveTop;
    public GameObject microwaveRight;
    public GameObject cupboardFrontRight;
    public GameObject croissantFrontRight;

    public enum TargetType
    {
        None,
        ToasterFront,
        ToasterTop,
        fridgeRight,
        fridgeTopRight,
        OvenFront,
        MicrowaveTop,
        MicrowaveRight,
        CupboardFrontRight,
        CroissantFrontRight
    }
    public enum Orientation
    {
        Front,
        Top,
        Bottom,
        Left,
        Right,
        FrontRight,
        TopRight
    }

    public enum TargetZone
    {
        None,
        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public List<TargetInfo> potentialTargets = new List<TargetInfo>();

    void Awake()
    {
        potentialTargets.Add(new TargetInfo(toasterFront, TargetType.ToasterFront, toasterFront.transform.position, Orientation.Front, 0.88f, TargetZone.TopLeft));
        potentialTargets.Add(new TargetInfo(ovenFront, TargetType.OvenFront, ovenFront.transform.position, Orientation.Front, 0.92f, TargetZone.TopLeft));
        potentialTargets.Add(new TargetInfo(fridgeRight, TargetType.fridgeRight, fridgeRight.transform.position, Orientation.Right, 0.88f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(cupboardFrontRight, TargetType.CupboardFrontRight, cupboardFrontRight.transform.position, Orientation.FrontRight, 1f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(croissantFrontRight, TargetType.CroissantFrontRight, croissantFrontRight.transform.position, Orientation.FrontRight, 1f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(fridgeTopRight, TargetType.fridgeTopRight, fridgeTopRight.transform.position, Orientation.TopRight, 1f, TargetZone.Center));


        potentialTargets.Add(new TargetInfo(toasterTop, TargetType.ToasterTop, toasterTop.transform.position, Orientation.Top, 1.25f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(microwaveTop, TargetType.MicrowaveTop, microwaveTop.transform.position, Orientation.Top, 1.4f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(microwaveRight, TargetType.MicrowaveRight, microwaveRight.transform.position, Orientation.Right, 0.45f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(null, TargetType.None, new Vector3(0.8f,1f,0), Orientation.Front, 0.8f, TargetZone.Center));


    }

    public class TargetInfo
    {
        public GameObject gameObject;
        public TargetType targetType;
        public Vector3 defaultPosition;
        public Vector3 rotation;
        public Orientation orientation;
        public TargetZone targetZone;
        public float depth;
        private static readonly Dictionary<Orientation, Vector3> orientationToRotation = new Dictionary<Orientation, Vector3>
        {
            { Orientation.Front, new Vector3(90f, 90f, 0) },
            { Orientation.Top, new Vector3(0f, 90f, 0) },
            { Orientation.Bottom, new Vector3(170f, 90f, 0) },
            { Orientation.Left, new Vector3(90f, 0f, 0) },
            { Orientation.Right, new Vector3(90f, 170f, 0) },
            { Orientation.FrontRight, new Vector3(90f, 135f, 0) },
            { Orientation.TopRight, new Vector3(55f, 170f, 0) }
        };

        public TargetInfo(GameObject obj, TargetType type, Vector3 pos, Orientation orient, float depth, TargetZone zone)
        {
            gameObject = obj;
            targetType = type;
            defaultPosition = pos;
            orientation = orient;
            this.depth = depth; 
            targetZone = zone;
        }
        public Vector3 GetRotation()
        {
            return orientationToRotation[orientation];
        }
        public Orientation GetOrientation()
        {
            return orientation;
        }
    }

    public GameObject GetTargetObject(TargetType type)
    {
        foreach (var targetInfo in potentialTargets)
        {
            if (targetInfo.targetType == type)
            {
                return targetInfo.gameObject;
            }
        }
        Debug.LogWarning("Target of type " + type.ToString() + " not found.");
        return null;
    }

    public GameObject GetGameObject(GameObject obj)
    {
        foreach (var targetInfo in potentialTargets)
        {
            if (targetInfo.gameObject == obj)
            {
                return obj;
            }
        }
        return null;
    }
    public float GetDepth(TargetType type)
    {
        foreach (var targetInfo in potentialTargets)
        {
            if (targetInfo.targetType == type)
            {
                return targetInfo.depth;
            }
        }
        Debug.LogWarning("Depth value for target type " + type.ToString() + " not found.");
        return 0f;
    }
    public Vector3 GetRotation(TargetType type)
    {
        foreach (var targetInfo in potentialTargets)
        {
            if (targetInfo.targetType == type)
            {
                return targetInfo.GetRotation();
            }
        }
        Debug.LogWarning($"Orientation for target type {type} not found.");
        return new Vector3(90f, 90f, 0f);
    }
    public Orientation GetOrientation(TargetType type)
    {
        foreach (var targetInfo in potentialTargets)
        {
            if (targetInfo.targetType == type)
            {
                return targetInfo.GetOrientation();
            }
        }
        Debug.LogWarning($"Orientation for target type {type} not found.");
        return Orientation.Front; // Default orientation if not found
    }

    public TargetType GetTargetType(GameObject obj)
    {
        foreach (var targetInfo in potentialTargets)
        {
            if (targetInfo.gameObject == obj)
            {
                return targetInfo.targetType;
            }
        }
        return TargetType.None;
    }

    public TargetInfo GetTargetInfo(TargetType type)
    {
        foreach (var targetInfo in potentialTargets)
        {
            if (targetInfo.targetType == type)
            {
                return targetInfo;
            }
        }

        Debug.LogWarning("Target of type " + type.ToString() + " not found.");
        return null;
    }
    
    public TargetZone GetTargetZone(TargetType targetType)
    {
        foreach (var targetInfo in potentialTargets)
        {
            if (targetInfo.targetType == targetType)
            {
                return targetInfo.targetZone; // Return the TargetZone of the found TargetType
            }
        }
        Debug.LogWarning($"TargetZone for target type {targetType} not found.");
        return TargetZone.None; // Default return value if no matching TargetType is found
    }

    public Vector3 GetTargetZonePosition(Orientation orientation, TargetZone targetZone, Vector3 hitPointPosition)
    {
        var offset = Vector3.zero;

        switch (orientation)
        {
            case Orientation.Front:
                switch (targetZone)
                {
                    case TargetZone.TopLeft:
                        offset = new Vector3(0, 0.1f, 0.1f);
                        break;
                    case TargetZone.TopRight:
                        offset = new Vector3(0, 0.1f, -0.1f);
                        break;
                    case TargetZone.BottomLeft:
                        offset = new Vector3(0, -0.1f, 0.1f);
                        break;
                    case TargetZone.BottomRight:
                        offset = new Vector3(0, -0.1f, -0.1f);
                        break;
                    case TargetZone.Center:
                        offset = Vector3.zero;
                        break;
                }
                break;
            case Orientation.Right:
                switch (targetZone)
                {
                    case TargetZone.Center:
                        offset = Vector3.zero;
                        break;
                    case TargetZone.TopLeft:
                        offset = new Vector3(0.1f, 0.1f, 0);
                        break;
                    case TargetZone.TopRight:
                        offset = new Vector3(-0.1f, 0.1f, 0);
                        break;
                    case TargetZone.BottomLeft:
                        offset = new Vector3(0.1f, -0.1f, 0);
                        break;
                    case TargetZone.BottomRight:
                        offset = new Vector3(-0.1f, -0.1f, 0);
                        break;
                }
                break;
            case Orientation.Top:
                switch (targetZone)
                {
                    case TargetZone.Center:
                        offset = Vector3.zero;
                        break;
                    case TargetZone.TopLeft:
                        offset = new Vector3(0.1f, 0, 0.1f);
                        break;
                    case TargetZone.TopRight:
                        offset = new Vector3(-0.1f, 0, 0.1f);
                        break;
                    case TargetZone.BottomLeft:
                        offset = new Vector3(0.1f, 0, -0.1f);
                        break;
                    case TargetZone.BottomRight:
                        offset = new Vector3(-0.1f, 0, -0.1f);
                        break;
                }
                break;
            // Implement cases for other orientations (Bottom, Left, FrontRight, TopRight)
        }
        return hitPointPosition + offset;
    }



    public TargetInfo FindClosestTarget(Vector3 position)
    {
        TargetInfo closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (var targetInfo in potentialTargets)
        {
            float distance = Vector3.Distance(position, targetInfo.defaultPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = targetInfo;
            }
        }

        return closestTarget;
    }
}
