using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainSensation : MonoBehaviour
{
    // Start is called before the first frame update


    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enter");
    }

    void OnCollisionStay(Collision collision)
    {
        Debug.Log("Stay");
    }

    void OnCollisionExit(Collision collision)
    {
        Debug.Log("Exit");
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log("Particulita");
        Debug.DrawLine(this.transform.position, other.transform.position);
    }
}
