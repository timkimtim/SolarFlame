using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Helpers;


// Content:
/// - basic player movement (x, z axis)
/// - slope movement
/// - jumping & double jumping
/// - crouching & walking
/// - full state handler


public class PlayerMovement : MonoBehaviour
{
    public float playerHeight = 2f;

    /// this is an empty gameObject inside the player, it is rotated by the camera
    /// -> keeps track of where the player is looking -> orientation.forward is the direction you're looking in
    public Transform orientation;

    /// public Transform playerObj; // your player object with the collider on it

    [Header("Movement")]
    public float moveForce = 12f;
    /// how much air control you have
    /// for example: airMultiplier = 0.5f -> you can only move half as fast will being in the air
    public float airMultiplier = 0.4f;

    public float groundDrag = 5f;

    public float jumpForce = 13f;
    public float jumpCooldown = 0.25f;

    public float crouchYScale = 0.5f; // how tall your player is while crouching (0.5f -> half as tall as normal)
    private float startYScale;
    private bool crouchStarted;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;
    private bool readyToJump = true;

    [Header("Speed handling")]
    // these variables define how fast your player can move while being in the specific movemt mode
    public float sprintMaxSpeed = 10f;
    public float walkMaxSpeed = 7f;
    public float crouchMaxSpeed = 2f;
    public float slopeSlideMaxSpeed = 30f;
    public float dashMaxSpeed = 15f;
    public float airMaxSpeed = 7f;

    public float limitedMaxSpeed = 20f; // changes based on how fast the player needs to go

    public float dashSpeedChangeFactor;

    private float maxSpeed; // this variable changes depending on which movement mode you are in
    private float desiredMaxSpeed; // needed to smoothly change between speed limitations
    private float lastDesiredMaxSpeed; // the previous desired max speed

    public float speedIncreaseMultiplier = 1.5f; // how fast the maxSpeed changes
    public float slopeIncreaseMultiplier = 2.5f; // how fast the maxSpeed changes on a slope

    /// how fast your player can maximally move on the y axis
    /// if set to -1, y speed will not be limited
    [HideInInspector] public float maxYSpeed;

    [Header("Ground Detection")]
    public LayerMask GroundMask;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public float maxSlopeAngle = 40f; // how steep the slopes you walk on can be

    [Header("Jump Prediction")]
    // this is needed for precise jumping and walljumping
    public float maxJumpRange;
    public float maxJumpHeight;

    [Header("References")]
    // all script references are assigned in void Start
    private PlayerCamera cam;

    [Header("Movement Modes")]
    [HideInInspector] public MovementMode mm; // this variable stores the current movement mode of the player
    public enum MovementMode // here are all movement modes defined
    {
        unlimited, // players speed is not being limited at all
        limited, // limit speed to a specific value using EnableLimitedSpeed()
        freeze, // player can't move at all
        dashing,
        sliding,
        crouching,
        sprinting,
        walking,
        air
    };

    // these bools are activated from different scripts
    // if for example the wallrunning bool is set to true, the movement mode will change to MovementMode.wallrunning#

    [HideInInspector] public bool freeze;
    [HideInInspector] public bool unlimitedSpeed;
    [HideInInspector] public bool restricted;
    [HideInInspector] private bool tierTwoRestricted;
    [HideInInspector] public bool dashing;
    [HideInInspector] public bool sprinting;
    [HideInInspector] public bool crouching;
    [HideInInspector] public bool sliding;

    // these bools are changed using specific functions
    [HideInInspector] private bool limitedSpeed;

    // other variables
    [HideInInspector] public float horizontalInput;
    [HideInInspector] public float verticalInput;

    [HideInInspector] public bool grounded;

    private Vector3 moveDirection;

    private Rigidbody rb; // the players rigidbody

    RaycastHit slopeHit; // variable needed for slopeCheck


    // text variables needed to display the speed and movement state ingame

    public TextMeshProUGUI text_speed;
    public TextMeshProUGUI text_ySpeed;
    public TextMeshProUGUI text_moveState;

    private void Start()
    {
        // if the player has not yet assigned a groundMask, just set it to "Default"
        if (GroundMask.value == 0)
            GroundMask = LayerMask.GetMask("Default");

        // assign references
        cam = GetComponent<PlayerCamera>();
        rb = GetComponent<Rigidbody>();

        // freeze all rotation on the rigidbody, otherwise the player falls over
        /// (like you would expect from a capsule with round surface)
        rb.freezeRotation = true;

        // if maxYSpeed is set to -1, the y speed of the player will be unlimited
        maxYSpeed = -1;

        startYScale = transform.localScale.y;

        readyToJump = true;
    }

    private void Update()
    {
        print("slope" + OnSlope());

        // make sure to call all functions every frame
        PlayerInput();
        LimitVelocity();
        HandleDrag();
        StateHandler();

        // shooting a raycast down from the middle of the player and checking if it hits the ground
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, GroundMask);

        if (Input.GetKeyDown(KeyCode.J))
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 50f, GroundMask))
            {
                JumpToPosition(hit.point, 10f);
                print("trying to jump to " + hit.point);
            }
        }

        DebugText();
    }

    /// functions that directly move the player should be called in FixedUpdate()
    /// this way your movement is not dependent on how many FPS you have
    /// if you call it in void Update, a player with 120FPS could move twice as fast as someone with just 60FPS
    private void FixedUpdate()
    {
        // if you're walking, sprinting, crouching or in the air, the MovePlayer function, which takes care of all basic movement, should be active
        /// this also makes sure that you can't move left or right while dashing for example
        if (mm == MovementMode.walking || mm == MovementMode.sprinting || mm == MovementMode.crouching || mm == MovementMode.air)
            MovePlayer();

        else
            LimitVelocity();
    }

    #region Input, Movement & Velocity Limiting

    private void PlayerInput()
    {
        // get your W,A,S,D inputs from your keyboard
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // whenever you press the jump key, you're grounded and readyToJump (which means jumping is not in cooldown),
        // you want to call the Jump() function
        if (Input.GetKey(jumpKey) && grounded && readyToJump)
        {
            readyToJump = false;

            Jump();

            // This will set readyToJump to true again after the cooldown is over
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // if you press the crouch key while not pressing W,A,S or D -> start crouching
        /// Note: if you are pressing W,A,S or D, the sliding script will start a slide instead
        if (Input.GetKeyDown(crouchKey))
            StartCrouch();

        // uncrouch again when you release the crouch key
        if (Input.GetKeyUp(crouchKey) && crouching)
            StopCrouch();

        // whenever you press the sprint key, sprinting should be true
        sprinting = Input.GetKey(sprintKey);
    }

    /// entire function only called when mm == walking, sprinting crouching or air
    private void MovePlayer()
    {
        if (restricted || tierTwoRestricted) return;

        // calculate the direction you need to move in
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // To Add the movement force, just use Rigidbody.AddForce (with ForceMode.Force, because you are adding force continuously)

        // movement on a slope
        if (OnSlope())
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveForce * 7.5f, ForceMode.Force);

        // movement on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveForce * 10f, ForceMode.Force);

        // movement in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveForce * 10f * airMultiplier, ForceMode.Force);
    }

    /// this function is always called
    private void LimitVelocity()
    {
        // get the velocity of your rigidbody without the y axis
        Vector3 rbFlatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        float currYVel = rb.velocity.y;

        // if you move faster over the x/z axis than you are allowed...
        if (rbFlatVelocity.magnitude > maxSpeed)
        {
            // ...then first calculate what your maximal velocity would be
            Vector3 limitedFlatVelocity = rbFlatVelocity.normalized * maxSpeed;

            // and then apply this velocity to your rigidbody
            rb.velocity = new Vector3(limitedFlatVelocity.x, rb.velocity.y, limitedFlatVelocity.z);
        }

        // if you move faster over the y axis than you are allowed...
        if (maxYSpeed != -1 && currYVel > maxYSpeed)
        {
            // ...just set your rigidbodys y velocity to you maxYSpeed, while leaving the x and z axis untouched
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
        }
    }

    /// function called the entire time
    private void HandleDrag()
    {
        // if you're walking or sprinting, apply drag to your rigidbody in order to prevent slippery movement
        if (mm == MovementMode.walking || mm == MovementMode.sprinting)
            rb.drag = groundDrag;

        // in any other case you don't want any drag
        else
            rb.drag = 0;
    }

    #endregion

    #region Jump Abilities

    /// called when jumpKeyPressed, readyToJump and grounded
    public void Jump()
    {
        // while dashing you shouldn't be able to jump
        if (dashing) return;

        // reset the y velocity of your rigidbody, while leaving the x and z velocity untouched
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // add upward force to your rigidbody
        /// make sure to use ForceMode.Impulse, since you're only adding force once
        rb.AddForce(orientation.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    // Uses Vector Maths to make the player jump exactly to a desired position
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight, Vector3 startPosition = new Vector3(), float maxRestrictedTime = 1f)
    {
        tierTwoRestricted = true;

        if (startPosition == Vector3.zero) startPosition = transform.position;

        Vector3 velocity = PhysicsExtension.CalculateJumpVelocity(startPosition, targetPosition, trajectoryHeight);

        // enter limited state
        Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
        EnableLimitedState(flatVel.magnitude);

        velocityToSet = velocity;
        Invoke(nameof(SetVelocity), 0.05f);
        Invoke(nameof(EnableMovementNextTouchDelayed), 0.01f);

        Invoke(nameof(ResetRestrictions), maxRestrictedTime);
    }
    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        rb.velocity = velocityToSet;
        cam.DoFov(100f);
    }
    private void EnableMovementNextTouchDelayed()
    {
        enableMovementOnNextTouch = true;
    }

    public void ResetRestrictions()
    {
        if (tierTwoRestricted)
        {
            tierTwoRestricted = false;
            cam.ResetFov();
        }

        DisableLimitedState();
    }

    #endregion

    #region Crouching

    /// called when crouchKey is pressed down
    private void StartCrouch()
    {
        // shrink the player down
        transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

        // after shrinking, you'll be a bit in the air, so add downward force to hit the ground again
        /// you don't really notice this while playing
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        crouching = true;
    }

    /// called when crouchKey is released
    private void StopCrouch()
    {
        // make sure your players size is the same as before
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);

        crouching = false;
    }

    #endregion

    #region StateMachine

    // Basically it just decides in which movement mode the player is currently in and sets the maxSpeed accordingly
    MovementMode lastMovementMode;
    private void StateHandler()
    {
        bool gradualVelBoost = false; // -> "build up speed", will make maxVel changes gradually (only if higher than current one!)
        bool instantVelChange = false; // will change maxVel instantly, no matter what

        // Mode - Freeze
        if (freeze)
        {
            mm = MovementMode.freeze;
            desiredMaxSpeed = 0f;
            instantVelChange = true;

            // make sure the player can't move at all
            rb.velocity = Vector3.zero;
        }

        // Mode - Unlimited
        else if (unlimitedSpeed)
        {
            mm = MovementMode.unlimited;

            // this way the player can go as fast as he wants
            desiredMaxSpeed = 1234.5678f;
        }

        // Mode - Limited
        else if (limitedSpeed)
        {
            mm = MovementMode.limited;
            desiredMaxSpeed = limitedMaxSpeed;
        }

        // Mode - Dashing
        else if (dashing)
        {
            mm = MovementMode.dashing;
            instantVelChange = true;
            speedChangeFactor = dashSpeedChangeFactor;
            desiredMaxSpeed = dashMaxSpeed;
        }

        // Mode - Sliding
        else if (sliding)
        {
            mm = MovementMode.sliding;

            if (OnSlope() && rb.velocity.y < 0.2f)
            {
                desiredMaxSpeed = slopeSlideMaxSpeed;
                gradualVelBoost = true;
            }
            else
                desiredMaxSpeed = sprintMaxSpeed;
        }

        // Mode - Crouching
        else if (crouching && grounded)
        {
            mm = MovementMode.crouching;
            desiredMaxSpeed = crouchMaxSpeed;
        }

        // Mode - Sprinting
        else if (grounded && sprinting)
        {
            mm = MovementMode.sprinting;
            desiredMaxSpeed = sprintMaxSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            mm = MovementMode.walking;
            desiredMaxSpeed = walkMaxSpeed;
        }

        // Mode - Air
        else
        {
            mm = MovementMode.air;

            if (desiredMaxSpeed < walkMaxSpeed)
                desiredMaxSpeed = sprintMaxSpeed;

            else
                desiredMaxSpeed = walkMaxSpeed;
        }

        Debug.Log("desiredMaxSpeed " + desiredMaxSpeed);
        Debug.Log("lastDesiredMaxSpeed " + lastDesiredMaxSpeed);

        Debug.Log("MaxSpeed " + maxSpeed);
        bool desiredMaxSpeedHasChanged = desiredMaxSpeed != lastDesiredMaxSpeed;
        bool boostedModes = lastMovementMode == MovementMode.sliding || lastMovementMode == MovementMode.dashing;

        // Change maxSpeed gradually, always when slowing down, for speeding up only if specified above
        if (desiredMaxSpeedHasChanged)
        {
            StopAllCoroutines();
            maxSpeed = desiredMaxSpeed;
            if ((gradualVelBoost && desiredMaxSpeed > maxSpeed) || boostedModes && !instantVelChange)
            {
                // check if desiredMoveSpeed has changed drastically
                // Something not working here!! else statement get's called whenever the heck it wants
                StartCoroutine(SmoothlyLerpMaxSpeed());
                print("chaning speed to " + desiredMaxSpeed + " last speed was " + lastDesiredMaxSpeed);
            }

            // In any other case change vel instantly
            else
            {
                print("speed changed instantly to " + desiredMaxSpeed + " last speed was " + lastDesiredMaxSpeed);
            }
        }

        lastDesiredMaxSpeed = desiredMaxSpeed;
        lastMovementMode = lastMovementMode == mm ? lastMovementMode : mm;

        // if you're walking or sprinting, the head bob movement of the camer should be enabled
        cam.hbEnabled = mm == MovementMode.walking || mm == MovementMode.sprinting ? true : false;
    }

    float speedChangeFactor;
    private IEnumerator SmoothlyLerpMaxSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMaxSpeed - maxSpeed);
        float startValue = maxSpeed;

        float boostFactor = 1f;
        boostFactor = speedChangeFactor;

        while (time < difference)
        {
            maxSpeed = Mathf.Lerp(startValue, desiredMaxSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f) * 2f;

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease * boostFactor;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier * boostFactor;

            yield return null;
        }

        maxSpeed = desiredMaxSpeed;

        // reset boost
        speedChangeFactor = 1;
    }

    public void EnableLimitedState(float speedLimit)
    {
        limitedMaxSpeed = speedLimit;
        limitedSpeed = true;
    }
    public void DisableLimitedState()
    {
        limitedSpeed = false;
    }

    #endregion

    #region Variables

    public bool OnSlope()
    {
        // shoot a raycast down to check if you hit something
        /// the "out slopeHit" bit makes sure that you store the information of the object you hit
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.5f))
        {
            // calculate the angle of the ground you're standing on (how steep it is)
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            // check if the angle is smaller than your maxSlopeAngle
            /// -> that means you're standing on a slope -> return true
            return angle < maxSlopeAngle && angle != 0;
        }

        // if the raycast doesn't hit anything, just return false
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        // calcualte the direction you need to move relative to the slope you're standing on
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    #endregion

    #region Moving Platforms

    private Rigidbody movingPlatform;
    public void AssignPlatform(Rigidbody platform)
    {
        movingPlatform = platform;
    }
    public void UnassignPlatform()
    {
        movingPlatform = null;
    }

    #endregion

    #region Collision Detection

    private bool enableMovementOnNextTouch;
    private void OnCollisionEnter(Collision collision)
    {
        bool touch = false;
        print("Contact count" + collision.contactCount);
        // Note: What is ground layer means Layer 7!
        print("Contact Layer " + collision.collider.gameObject.layer + " / " + GroundMask.value);
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.collider.gameObject.layer == 9 || collision.collider.gameObject.layer == 10)
                touch = true;
        }

        if (touch) print("GroundObjectTouched");

        print("event sucessfully called");

        if (enableMovementOnNextTouch && touch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();
        }
    }

    #endregion

    #region Text Displaying

    private void DebugText()
    {
        if (text_speed != null)
        {
            Vector3 rbFlatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            text_speed.SetText("Speed: " + Round(rbFlatVelocity.magnitude, 1) + "/" + Round(maxSpeed, 0));
        }

        if (text_ySpeed != null)
            text_ySpeed.SetText("Y Speed: " + Round(rb.velocity.y, 1));

        if (text_moveState != null)
            text_moveState.SetText(mm.ToString());
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    #endregion
}