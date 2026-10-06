using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform playerObj;
    public Transform orientation;
    public PlayerCamera cam;

    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float groundDrag;

    [Header("Momentum")]
    public float speedIncreaseMultiplier = 1.5f;
    public float slopeIncreaseMultiplier = 2.5f;

    [Header("Sprinting")]
    public bool toggleSprint = false;
    private bool sprintToggled;
    private bool isSprinting;

    [Header("Camera FOV")]
    public float normalFov = 80f;
    public float sprintFov = 90f;
    public float speedBuffFovBonus = 10f;
    private float targetFov;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public Key jumpKey = Key.Space;
    public Key sprintKey = Key.LeftShift;
    public Key crouchKey = Key.LeftCtrl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool grounded;
    public bool Grounded => grounded;

    [Header("Gravity")]
    public float gravityMultiplier = 2.5f;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    private float horizontalInput;
    private float verticalInput;

    private PlayerStats stats;
    private float SpeedMultiplier => stats != null ? stats.SpeedMultiplier : 1f;
    private float EffectiveSpeed => moveSpeed * SpeedMultiplier;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    private MovementState lastState;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air,
        wallrunning,
    }

    public bool sliding;
    public bool crouching;
    public bool wallrunning;

    private void Start()
    {
        stats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = playerObj.localScale.y;
        targetFov = normalFov;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded && !sliding)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        ApplyGravityModifiers();
    }

    private void MyInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current.dKey.isPressed) horizontalInput += 1f;
        if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f;
        if (Keyboard.current.wKey.isPressed) verticalInput += 1f;
        if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;

        bool moving = horizontalInput != 0 || verticalInput != 0;

        if (Keyboard.current[jumpKey].isPressed && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // moving + crouch key = slide (handled by Sliding), standing still = crouch
        if (Keyboard.current[crouchKey].wasPressedThisFrame && !moving && !sliding)
            StartCrouch();

        if (Keyboard.current[crouchKey].wasReleasedThisFrame && crouching)
            StopCrouch();

        if (toggleSprint)
        {
            if (Keyboard.current[sprintKey].wasPressedThisFrame && !wallrunning)
                sprintToggled = !sprintToggled;

            if (!moving)
                sprintToggled = false;

            isSprinting = sprintToggled;
        }
        else
        {
            isSprinting = Keyboard.current[sprintKey].isPressed;
        }
    }

    public void StartCrouch()
    {
        if (crouching) return;

        crouching = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, crouchYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    public void StopCrouch()
    {
        if (!crouching) return;

        crouching = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }

    private void StateHandler()
    {
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                // real velocity already includes the speed multiplier, so convert it back to base speed
                float currentSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude / SpeedMultiplier;
                desiredMoveSpeed = Mathf.Clamp(currentSpeed, walkSpeed, slideSpeed);
            }
        }
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (grounded && isSprinting)
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }

        // only slide-related speed changes ease; everything else snaps
        bool keepMomentum = state == MovementState.sliding || lastState == MovementState.sliding;
        bool desiredChanged = !Mathf.Approximately(desiredMoveSpeed, lastDesiredMoveSpeed);

        if (keepMomentum && Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            if (desiredChanged)
                StopAllCoroutines();

            moveSpeed = desiredMoveSpeed;
        }

        UpdateFov();

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private void UpdateFov()
    {
        // WallRunning controls the FOV during a wall run; clearing targetFov forces a reapply afterwards
        if (state == MovementState.wallrunning)
        {
            targetFov = 0f;
            return;
        }

        // keep whatever FOV we had while airborne so jumping mid-sprint doesn't snap it back
        if (state == MovementState.air)
            return;

        float fov = state == MovementState.sprinting ? sprintFov : normalFov;

        if (SpeedMultiplier > 1f)
            fov += speedBuffFovBonus;

        if (!Mathf.Approximately(fov, targetFov))
        {
            targetFov = fov;
            cam.DoFov(fov);
        }
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        bool onSlope = OnSlope() && !exitingSlope;

        if (onSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * EffectiveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0f)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * EffectiveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * EffectiveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (!wallrunning) rb.useGravity = !onSlope;
    }

    private void SpeedControl()
    {
        float maxSpeed = EffectiveSpeed;

        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > maxSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > maxSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private void ApplyGravityModifiers()
    {
        // extra gravity only when normal gravity is on
        if (rb.useGravity)
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}