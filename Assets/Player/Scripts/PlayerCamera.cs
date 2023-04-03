using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DoTween for the camera effects, so this reference is needed


// Content:
/// - first person camera rotation
/// - camera effects such as fov changes, tilt or cam shake
/// - headBob effect while walking or sprinting
///
// Note:
/// This script is assigned to the player (like every other script).
/// It rotates the camera vertically and horizontally, while also rotating the orientation of the player, but only horizontally.
/// -> Most scripts then use this orientation to find out where "forward" is.


public class PlayerCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensX = 10f;
    public float sensY = 10f;

    [Header("Assignables")]
    public Transform cameraHolder; // the cameraHolder
    public Camera cameraObject; // the camera (inside the cameraHolder)
    public Transform orientation; // reference to the orientation of the player

    [Header("Effects")]
    public float baseFov = 90f;
    public float fovTransitionTime = 0.25f; // how fast the cameras fov changes
    public float tiltTransitionTime = 0.25f; // how fast the cameras tilt changes

    [Header("Effects - HeadBob")]
    [HideInInspector] public bool hbEnabled; // this bool is changed by the PlayerMovement script, depending on the MovementState
    public float hbAmplitude = 0.5f; // how large the headBob effect is
    public float hbFrequency = 12f; // how fast the headBob effect plays


    // the rest are just private variables to store information

    private float hbToggleSpeed = 3f; // if the players speed is smaller than the hbToggleSpeed, the headBob effect wont play
    private Vector3 hbStartPos;
    private Rigidbody rb;

    [HideInInspector] public float mouseX;
    [HideInInspector] public float mouseY;

    private float multiplier = 0.01f;

    private float xRotation;
    private float yRotation;

    private void Start()
    {
        // store the startPosition of the camera
        hbStartPos = cameraObject.transform.localPosition;

        // get the components
        cameraHolder = GameObject.Find("CameraHolder").transform;
        rb = GetComponent<Rigidbody>();

        // lock the mouse cursor in the middle of the screen
        // TODO Cursor.lockState = CursorLockMode.Locked;
        // make the mouse coursor invisible
        // TODO: Cursor.visible = false;
    }

    private void Update()
    {
        RotateCamera();

        // if headBob is enabled, start the CheckMotion() function
        /// which then starts -> PlayMotion() and ResetPosition()
        if (hbEnabled)
        {
            CheckMotion();
            cameraObject.transform.LookAt(FocusTarget());
        }
    }

    public void RotateCamera()
    {
        // first get the mouseX and Y Input
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;

        // then calculate the x and y rotation of your camera using this formula:
        /// yRotation plus mouseX
        /// xRotation minus mouseY
        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;

        // make sure that you can't look up or down more than 90* degrees
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        // rotate the camera holder along the x and y axis
        cameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        // rotate the players orientation, but only along the y (horizontal) axis
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }


    /// Note: For smooth transitions I use the free DoTween Asset!
    #region Fov, Tilt and CamShake

    /// function called when starting dash
    /// a simple function that just takes in an endValue, and then smoothly sets the cameras fov to this end value
    public void DoFov(float endValue, float transitionTime = -1)
    {
        if (transitionTime == -1)
            cameraObject.DOFieldOfView(endValue, fovTransitionTime);

        else
            cameraObject.DOFieldOfView(endValue, transitionTime);
    }

    public void ResetFov()
    {
        cameraObject.DOFieldOfView(baseFov, fovTransitionTime);
    }

    private Tweener shakeTween;
    public void DoShake(float amplitude, float frequency)
    {
        shakeTween = cameraObject.transform.DOShakePosition(1f, .4f, 1, 90).SetLoops(-1);
    }

    public void ResetShake()
    {
        StartCoroutine(ResetShakeRoutine());
    }
    public IEnumerator ResetShakeRoutine()
    {
        /// needs to be fixed!

        shakeTween.SetLoops(1);
        cameraObject.transform.DOKill(); // not optimal, sometimes kills the tilt or fov stuff too...

        yield return shakeTween.WaitForCompletion();

        cameraObject.transform.DOLocalMove(Vector3.zero, .2f);
    }

    #endregion

    #region HeadBob

    private void CheckMotion()
    {
        // get the current speed of the players rigidbody (y axis excluded)
        float speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

        ResetPosition();

        // check if the speed is high enough to activate the headBob effect
        if (speed < hbToggleSpeed) return;

        PlayMotion(FootStepMotion());
    }

    private void PlayMotion(Vector3 motion)
    {
        // take the calculated motion and apply it to the camera
        cameraObject.transform.localPosition += motion * Time.deltaTime;
    }

    private void ResetPosition()
    {
        if (cameraObject.transform.localPosition == hbStartPos) return;

        // smoothly reset the position of the camera back to normal
        cameraObject.transform.localPosition = Vector3.Lerp(cameraObject.transform.localPosition, hbStartPos, 1 * Time.deltaTime);
    }

    private Vector3 FootStepMotion()
    {
        // use Sine and Cosine to create a smooth looking motion that swings from left to right and from up to down 
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * hbFrequency) * hbAmplitude;
        pos.x += Mathf.Cos(Time.time * hbFrequency * 0.5f) * hbAmplitude * 2f;
        return pos;
    }


    private Vector3 FocusTarget()
    {
        // make sure the camera focuses (Looks at) a point 15 tiles away from the player
        // this stabilizes the camera
        Vector3 pos = new Vector3(transform.position.x, cameraHolder.position.y, transform.position.z);
        pos += cameraHolder.forward * 15f;
        return pos;
    }

    #endregion
}