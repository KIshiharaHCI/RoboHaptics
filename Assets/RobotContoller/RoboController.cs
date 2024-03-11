using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Collections;

public class RoboController : MonoBehaviour
{



    [DllImport("kinovaController.dll")]
    private static extern void example_move_to_home_position();

    [DllImport("kinovaController.dll")]
    private static extern void sessionStart(string ipaddress, uint port);

    [DllImport("kinovaController.dll")]
    private static extern void createServices();

    [DllImport("kinovaController.dll")]
    private static extern void closeSession();

    [DllImport("kinovaController.dll")]
    private static extern void setPose(float x, float y, float z);

    [DllImport("kinovaController.dll")]
    private static extern void setAngle(float pitch, float yaw, float roll);

    [DllImport("kinovaController.dll")]
    private static extern void setProportional(float prop);

    [DllImport("kinovaController.dll")]
    private static extern void setProportionalRotation(float prop);
    [DllImport("kinovaController.dll")]
    private static extern void setDerivative(float der);

    [DllImport("kinovaController.dll")]
    private static extern void setDerivativeRotation(float der);

    [DllImport("kinovaController.dll")]
    private static extern float getActuatorFeedback(int actuator);

    [DllImport("kinovaController.dll")]
    private static extern float getxpos();

    [DllImport("kinovaController.dll")]
    private static extern float getypos();

    [DllImport("kinovaController.dll")]
    private static extern float getzpos();

    [DllImport("kinovaController.dll")]
    private static extern void wakeUpRobot();


    [Header("Comunication Parameters")]
    public string address;
    public uint port;
    public string user;
    public string password;

    [Header("Robot Joints Model")]
    [Space(10)]
    public int JointNumber;

    public GameObject q1, q2, q3, q4, q5, q6;

    [Header("Target inputs")]
    public Vector3 targetPosition = new Vector3(0.457665f, 0.421093f, 0.0f);

    public float pitch = 90.0f;

    public float yaw = 90.0f;

    [ReadOnly, SerializeField]
    private float roll = 0.0f;
    [Header("Control Parameters")]
    public float proportional = 0.35f;
    public float proportionalRotation = 0.35f;

    public float derivative = 0.0f;
    public float derivativeRotation = 0.0f;
    [Header("Feedback Variables")]
    public Vector3 measuredPos = new Vector3(0.0f, 0.0f, 0.0f);
    [Header("Gripper Parameters")]
    public bool HasGripper = false;
    private bool GripperStarted = false;
    [Header("Additional Parameters")]
    public float speedD = 1;
    public Vector2 gripper = new Vector2(0.0f, 1.0f);
    // Start is called before the first frame update
    void Start()
    {

        targetPosition += this.transform.position;
        // Create API objects

        sessionStart(address, port);

        // Create services

        createServices();


    }
    void Update()
    {
        
    }
    // Update is called once per frame
    void FixedUpdate()
    {

        setPosition(targetPosition);
        setAngle(pitch, yaw, roll);
        setProportional(proportional);
        setProportionalRotation(proportionalRotation);
        setDerivative(derivative);
        setDerivativeRotation(derivativeRotation);


        ///*
        try
        {

            var step = speedD * Time.fixedDeltaTime;
            q1.transform.localRotation = Quaternion.Lerp(q1.transform.localRotation, Quaternion.Euler(0, getActuatorFeedback(0) - 180, 0),     step);
            q2.transform.localRotation = Quaternion.Lerp(q2.transform.localRotation, Quaternion.Euler(0, 0, (getActuatorFeedback(1))),         step);
            q3.transform.localRotation = Quaternion.Lerp(q3.transform.localRotation, Quaternion.Euler(0, 0, -1 * (getActuatorFeedback(2) - 0)),step);
            q4.transform.localRotation = Quaternion.Lerp(q4.transform.localRotation, Quaternion.Euler(0, getActuatorFeedback(3), 0),           step);
            q5.transform.localRotation = Quaternion.Lerp(q5.transform.localRotation, Quaternion.Euler(0, 0, (getActuatorFeedback(4) * -1)),    step);
            q6.transform.localRotation = Quaternion.Lerp(q6.transform.localRotation, Quaternion.Euler(0, getActuatorFeedback(5), 0),           step);
            /*
            var step = speedD * Time.fixedDeltaTime;
            q1.transform.localRotation = Quaternion.RotateTowards(q1.transform.localRotation, Quaternion.Euler(0, getActuatorFeedback(0) - 180, 0),     step);
            q2.transform.localRotation = Quaternion.RotateTowards(q2.transform.localRotation, Quaternion.Euler(0, 0, (getActuatorFeedback(1))),         step);
            q3.transform.localRotation = Quaternion.RotateTowards(q3.transform.localRotation, Quaternion.Euler(0, 0, -1 * (getActuatorFeedback(2) - 0)),step);
            q4.transform.localRotation = Quaternion.RotateTowards(q4.transform.localRotation, Quaternion.Euler(0, getActuatorFeedback(3), 0),           step);
            q5.transform.localRotation = Quaternion.RotateTowards(q5.transform.localRotation, Quaternion.Euler(0, 0, (getActuatorFeedback(4) * -1)),    step);
            q6.transform.localRotation = Quaternion.RotateTowards(q6.transform.localRotation, Quaternion.Euler(0, getActuatorFeedback(5), 0),           step);*/

            /*
                        q1.transform.localRotation = Quaternion.Euler(0, getActuatorFeedback(0) - 180, 0);
                        q2.transform.localRotation = Quaternion.Euler(0, 0, (getActuatorFeedback(1)));
                        q3.transform.localRotation = Quaternion.Euler(0, 0, -1 * (getActuatorFeedback(2) - 0));
                        q4.transform.localRotation = Quaternion.Euler(0, getActuatorFeedback(3), 0);
                        q5.transform.localRotation = Quaternion.Euler(0, 0, (getActuatorFeedback(4) * -1));
                        q6.transform.localRotation = Quaternion.Euler(0, getActuatorFeedback(5), 0);*/

        }
        catch { }//
        updateMeasuredPos();
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Vector3 targetDraw = measuredPos + this.transform.position;
        Gizmos.DrawSphere(targetDraw, 0.03f);
    }

    void updateMeasuredPos()
    {
        measuredPos = new Vector3(getxpos(), getzpos(), getypos());
    }


    void setPosition(Vector3 pos)
    {
        pos = pos - this.transform.position;
        setPose(pos[0], pos[2], pos[1]);
    }


    void OnDestroy()
    {
        Debug.Log("OnDestroy1");
        closeSession();
    }

}
