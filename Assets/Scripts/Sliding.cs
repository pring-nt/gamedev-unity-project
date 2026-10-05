using UnityEngine;
using UnityEngine.InputSystem;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public Key slideKey = Key.LeftCtrl;
    private float horizontalInput;
    private float verticalInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current.dKey.isPressed) horizontalInput += 1f;
        if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f;
        if (Keyboard.current.wKey.isPressed) verticalInput += 1f;
        if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;

        if (Keyboard.current[slideKey].wasPressedThisFrame && (horizontalInput != 0 || verticalInput != 0))
        {
            StartSlide();
        }

        if (Keyboard.current[slideKey].wasReleasedThisFrame && pm.sliding)
        {
            StopSlide();
        }
    }

    private void FixedUpdate()
    {
        if(pm.sliding)
        {
            SlidingMovement();
        }
    }

    private void StartSlide()
    {
        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(!pm.OnSlope() || rb.linearVelocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }


        if (slideTimer <= 0)
        {
            StopSlide();
        }
        
    }

    private void StopSlide()
    {
        pm.sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);

        if (Keyboard.current[slideKey].isPressed)
            pm.StartCrouch();
    }
}
