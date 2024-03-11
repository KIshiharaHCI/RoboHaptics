using System.Collections.Generic;
using UnityEngine;

public class EHDObject : MonoBehaviour
{
    public GameObject toasterFront;
    public GameObject toasterTop;
    public GameObject microwaveLeft;
    public GameObject drawerLeft;
    public GameObject drawerRight;
    public GameObject microwaveFront;
    public GameObject microwaveTop;
    public GameObject microwaveRight;
    public GameObject tableEdge;

    public enum TargetType
    {
        None,
        ToasterFront,
        ToasterTop,
        MicrowaveLeft,
        DrawerLeft,
        DrawerRight,
        MicrowaveFront,
        MicrowaveTop,
        MicrowaveRight,
        TableEdge
    }
    public enum Orientation
    {
        Front,
        Top,
        Bottom,
        Left,
        Right
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
        potentialTargets.Add(new TargetInfo(toasterFront, TargetType.ToasterFront, toasterFront.transform.position, Orientation.Front, 0.88f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(toasterTop, TargetType.ToasterTop, toasterTop.transform.position, Orientation.Top, 1.25f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(microwaveLeft, TargetType.MicrowaveLeft, microwaveLeft.transform.position, Orientation.Left, -0.2f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(drawerLeft, TargetType.DrawerLeft, drawerLeft.transform.position, Orientation.Front,  0.88f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(drawerRight, TargetType.DrawerRight, drawerRight.transform.position, Orientation.Front, 0.88f, TargetZone.Center));
        potentialTargets.Add(new TargetInfo(microwaveFront, TargetType.MicrowaveFront, microwaveFront.transform.position, Orientation.Front, 0.88f, TargetZone.Center));
        //potentialTargets.Add(new TargetInfo(tableEdge, TargetType.TableEdge, tableEdge.transform.position, Orientation.Front, 0.9385f));
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
            { Orientation.Bottom, new Vector3(180f, 90f, 0) },
            { Orientation.Left, new Vector3(90f, 0f, 0) },
            { Orientation.Right, new Vector3(90f, 179.9f, 0) },
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
