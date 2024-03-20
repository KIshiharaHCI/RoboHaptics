using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackController : MonoBehaviour
{
    public GameObject controller;
    private Vector3 position;
    private bool redirection = false;
    private bool hold = false;

    public void SetRedirectionPosition(Vector3 redirectedPosition, bool redirect, bool hold)
    {
        position = redirectedPosition;
        redirection = redirect;
    }

    void Update()
    {
        if(controller != null)
        {
            if (redirection || hold)
            {
                //Debug.Log("Redirection Position: " + position);
                //Debug.Log("Controller Position: " + controller.transform.position);
                transform.position = position;
            }
            else
            {
                transform.position = controller.transform.position;
            }
            transform.rotation = controller.transform.rotation;
        }
    }
}
