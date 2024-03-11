using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject controller;

    // Update is called once per frame
    void Update()
    {
        if(controller != null)
        {
            transform.position = controller.transform.position;
            transform.rotation = controller.transform.rotation;
        }
    }
}
