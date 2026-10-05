using UnityEngine;
using UnityEngine.InputSystem;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    public Key jumpKey = Key.Space;
    public Key upwardsRunKey = Key.LeftShift;
    public Key downwardsRunKey = Key.LeftCtrl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    public float exitWallTime;
    private bool exitingWall;
    private float exitWallTimer;

    [Header("Wall Reuse")]
    public float resetWallTime = 2f;   // 0 = only reset on landing
    private float resetWallTimer;
    private Transform lastWall;
    private Transform currentWall;
    private bool usingRightWall;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    public PlayerCamera cam;
    private PlayerMovement pm;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    // picks a wall that isn't the one we just ran on (handles corridors with walls on both sides)
    private bool FindUsableWall()
    {
        if (wallRight && rightWallHit.transform != lastWall)
        {
            currentWall = rightWallHit.transform;
            usingRightWall = true;
            return true;
        }

        if (wallLeft && leftWallHit.transform != lastWall)
        {
            currentWall = leftWallHit.transform;
            usingRightWall = false;
            return true;
        }

        return false;
    }

    private void StateMachine()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        upwardsRunning = Keyboard.current[upwardsRunKey].isPressed;
        downwardsRunning = Keyboard.current[downwardsRunKey].isPressed;

        if (Keyboard.current.dKey.isPressed) horizontalInput += 1f;
        if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f;
        if (Keyboard.current.wKey.isPressed) verticalInput += 1f;
        if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;

        UpdateWallReset();

        // keep the wall we're already on; only look for a new one when not wall running
        bool hasWall = pm.wallrunning
            ? (usingRightWall ? wallRight : wallLeft)
            : FindUsableWall();

        if (hasWall && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!pm.wallrunning)
                StartWallRun();

            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if (wallRunTimer <= 0 && pm.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            if (Keyboard.current[jumpKey].wasPressedThisFrame)
                WallJump();
        }
        else if (exitingWall)
        {
            if (pm.wallrunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }
        else
        {
            if (pm.wallrunning)
                StopWallRun();
        }
    }

    private void UpdateWallReset()
    {
        if (pm.Grounded)
        {
            lastWall = null;
            return;
        }

        if (!pm.wallrunning && resetWallTime > 0 && resetWallTimer > 0)
        {
            resetWallTimer -= Time.deltaTime;

            if (resetWallTimer <= 0)
                lastWall = null;
        }
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        wallRunTimer = maxWallRunTime;

        cam.DoFov(90f);
        cam.DoTilt(usingRightWall ? 3f : -3f);
    }

    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = usingRightWall ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (upwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimbSpeed, rb.linearVelocity.z);
        if (downwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimbSpeed, rb.linearVelocity.z);

        // push into the wall unless the player is steering away from it
        if (!(!usingRightWall && horizontalInput > 0) && !(usingRightWall && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.wallrunning = false;

        lastWall = currentWall;
        resetWallTimer = resetWallTime;

        cam.DoFov(pm.normalFov);
        cam.DoTilt(0f);
    }

    private void WallJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = usingRightWall ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}