using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPos; // an empty gameObject inside the player, that indicates where the camera should be

    private void Update()
    {
        // move the cameraHolder to the intendet position
        transform.position = cameraPos.position;
    }
}

