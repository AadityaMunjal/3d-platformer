using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airMultiplier = 0.4f;
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float airDrag = 2f;
    [SerializeField] private bool allowDoubleJump = true;
    [SerializeField] private float doubleJumpForce = 10f;
    
    [Header("Crouching")]
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float crouchYScale = 0.5f;
    
    [Header("Wall Running")]
    [SerializeField] private float wallRunSpeed = 8f;
    [SerializeField] private float wallRunForce = 200f;
    [SerializeField] private float wallJumpUpForce = 7f;
    [SerializeField] private float wallJumpSideForce = 12f;
    [SerializeField] private float wallRunGravity = 1f;
    [SerializeField] private float wallCheckDistance = 0.7f;
    [SerializeField] private float minJumpHeight = 1.5f;
    [SerializeField] private LayerMask wallMask = -1;
    
    [Header("Ledge Climbing")]
    [SerializeField] private float ledgeCheckDistance = 1f;
    [SerializeField] private float ledgeGrabSpeed = 8f;
    [SerializeField] private float climbSpeed = 5f;
    
    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private LayerMask groundMask = -1;
    
    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 40f;
    
    [Header("References")]
    public Transform orientation;
    
    // Input variables
    private Vector2 moveInput;
    private bool isJumping;
    private bool isSprinting;
    private bool isCrouching;
    
    // Movement state
    private float moveSpeed;
    private Vector3 moveDirection;
    private bool grounded;
    private bool readyToJump = true;
    private float startYScale;
    
    // Parkour variables
    private bool hasDoubleJumped = false;
    private bool wallLeft, wallRight;
    private bool wallRunning;
    private bool isClimbing;
    private bool onLedge;
    private RaycastHit leftWallHit, rightWallHit;
    private RaycastHit ledgeHit;
    
    // Slope handling
    private RaycastHit slopeHit;
    private bool exitingSlope;
    
    // Components
    private Rigidbody rb;
    
    public enum MovementState
    {
        Walking,
        Sprinting,
        Crouching,
        Air,
        WallRunning,
        Climbing
    }
    
    public MovementState state;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        startYScale = transform.localScale.y;
    }
    
    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, groundMask);
        
        // legacy input
        HandleDirectInput();
        
        CheckForWalls();
        CheckForLedge();
        
        HandleDrag();
        
        StateHandler();
        
        if (wallRunning)
            WallRunningMovement();
        
        SpeedControl();
        
        // Reset double jump when grounded
        if (grounded && rb.linearVelocity.y <= 0)
            hasDoubleJumped = false;
    }
    
    private void FixedUpdate()
    {
        MovePlayer();
        ClimbingMovement();
    }
    
    private void HandleDirectInput()
    {
        // keyboard input
        float horizontal = 0f;
        float vertical = 0f;
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            vertical = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            vertical = -1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;
        
        moveInput = new Vector2(horizontal, vertical);
        
        // jumping
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            Jump();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }
        
        // sprinting
        isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        // crouching
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            
            if (isCrouching)
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }
    }
    
    private void StateHandler()
    {
        // wall running
        if ((wallLeft || wallRight) && !grounded && rb.linearVelocity.y < 2f)
        {
            state = MovementState.WallRunning;
            wallRunning = true;
            moveSpeed = wallRunSpeed;
        }
        // climbing
        else if (isClimbing)
        {
            state = MovementState.Climbing;
            moveSpeed = climbSpeed;
        }
        // crouching
        else if (isCrouching)
        {
            state = MovementState.Crouching;
            moveSpeed = crouchSpeed;
            wallRunning = false;
        }
        // sprinting
        else if (grounded && isSprinting)
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
            wallRunning = false;
        }
        // walking
        else if (grounded)
        {
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
            wallRunning = false;
        }
        // air
        else
        {
            state = MovementState.Air;
            wallRunning = false;
        }
    }
    
    private void MovePlayer()
    {
        // movement direction
        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        
        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // on ground
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // in air
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
        
        // gravity off while on slope or wall running
        rb.useGravity = !OnSlope() && !wallRunning;
    }
    
    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }
    
    private void Jump()
    {
        if (readyToJump && grounded)
        {
            readyToJump = false;
            exitingSlope = true;
            
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        else if (wallRunning)
        {
            WallJump();
        }
        else if (!grounded && allowDoubleJump && !hasDoubleJumped && readyToJump)
        {
            hasDoubleJumped = true;
            readyToJump = false;
            
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            
            rb.AddForce(transform.up * doubleJumpForce, ForceMode.Impulse);
            
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }
    
    private void HandleDrag()
    {
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = airDrag;
    }
    
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        
        return false;
    }
    
    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
    
    private void CheckForWalls()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, wallMask);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, wallMask);
    }
    
    private void CheckForLedge()
    {
        Vector3 ledgeCheckPos = transform.position + Vector3.up * (playerHeight * 0.4f);
        onLedge = Physics.Raycast(ledgeCheckPos, orientation.forward, out ledgeHit, ledgeCheckDistance, wallMask);
        
        if (onLedge && rb.linearVelocity.y < 0 && !grounded)
        {
            isClimbing = true;
        }
        else if (grounded || rb.linearVelocity.y > 0)
        {
            isClimbing = false;
        }
    }
    
    private void WallRunningMovement()
    {
        rb.useGravity = false;
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        
        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;
        
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
        
        if (!(wallLeft && moveInput.x > 0) && !(wallRight && moveInput.x < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        
        rb.AddForce(transform.up * wallRunGravity, ForceMode.Force);
    }
    
    private void WallJump()
    {
        exitingSlope = true;
        wallRunning = false;
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
        
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
        
        readyToJump = false;
        Invoke(nameof(ResetJump), jumpCooldown);
    }
    
    private void ClimbingMovement()
    {
        if (isClimbing && onLedge)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbSpeed, rb.linearVelocity.z);
        }
    }
}