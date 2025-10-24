using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    public enum MovementState
    {
        IDLE,
        WALK,
        SPRINT,
        CROUCH,
        AIR,
    }

    [Header("Movement")]
    public float sprintSpeed;
    public float walkSpeed;
    public float groundDrag;
    public MovementState movementState;
    private float moveSpeed;

    [Header("Jump")]
    public float jumpForce;
    public float airMultiplier;
    public float coyoteTime;
    public float jumpBuffer;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    [Header("Crouch")]
    public float crouchSpeed;
    public float crouchScale;
    public float upDetectionHeight;
    private float downDetectionHeight = 0.2f;
    public float defaultVignette,
        crouchVignette;
    private float defaultScale;

    [Header("Slope Check")]
    public float maxSlopeAngle;
    public bool Grounded { get; private set; }
    RaycastHit slopeHit;
    public float playerHeight { get; private set; }
    bool exitingSlope;

    [Header("Step Check")]
    public bool stepClimbEnabled;
    public GameObject rayLower;
    public GameObject rayUpper;
    public float stepHeight;
    public float stepSmoothing;

    Rigidbody rb;
    Vector3 moveDirection;
    float horizontalInput,
        verticalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerHeight =
            GetComponentInChildren<CapsuleCollider>().height * gameObject.transform.localScale.y;

        rayUpper.transform.position = rayLower.transform.position + stepHeight * Vector3.up;
        defaultScale = transform.localScale.y;
    }

    void Update()
    {
        // Check if is grounded
        Grounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            playerHeight * 0.5f + downDetectionHeight,
            ~0,
            QueryTriggerInteraction.Ignore
        );
        exitingSlope = !Grounded;

        GetInput();
        SpeedControl();
        SetDrag();
        HandleMovementState();
    }

    void FixedUpdate()
    {
        Move();

        if (stepClimbEnabled)
        {
            StepClimb();
        }
    }

    void GetInput()
    {
        Vector2 movement = InputController.Instance.GetWalkDirection();
        horizontalInput = movement.x;
        verticalInput = movement.y;
        PreJumpCheck();

        if (Grounded)
        {
            if (
                coyoteTimeCounter > 0f
                && jumpBufferCounter > 0f
                && movementState != MovementState.CROUCH
            )
            {
                Jump();

                // Reset jump buffer to prevent jumping again
                jumpBufferCounter = 0f;
                return;
            }

            if (InputController.Instance.GetCrouchDown())
            {
                Crouch();
                return;
            }
            else if (
                InputController.Instance.GetCrouchHold()
                || Physics.Raycast(
                    transform.position,
                    Vector3.up,
                    playerHeight * 0.5f + upDetectionHeight
                )
            )
            {
                transform.localScale = new Vector3(
                    transform.localScale.x,
                    crouchScale,
                    transform.localScale.z
                );
                return;
            }
            else
            {
                transform.localScale = new Vector3(
                    transform.localScale.x,
                    defaultScale,
                    transform.localScale.z
                );
                return;
            }
        }
    }

    void PreJumpCheck()
    {
        // Coyote time check
        if (Grounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump buffer check
        if (InputController.Instance.GetJumpDown())
        {
            jumpBufferCounter = jumpBuffer;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    void HandleMovementState()
    {
        if (!Grounded)
        {
            movementState = MovementState.AIR;
        }
        else
        {
            // Something above and below
            if (
                Physics.Raycast(
                    transform.position,
                    Vector3.up,
                    playerHeight * 0.5f + upDetectionHeight
                )
                && Physics.Raycast(
                    transform.position,
                    Vector3.down,
                    playerHeight * 0.5f + downDetectionHeight
                )
            )
            {
                movementState = MovementState.CROUCH;
                moveSpeed = crouchSpeed;

                transform.localScale = new Vector3(
                    transform.localScale.x,
                    crouchScale,
                    transform.localScale.z
                );
            }
            else if (
                (InputController.Instance.GetCrouchHold() && Grounded)
                // Crouching and something above
                || movementState == MovementState.CROUCH
                    && !InputController.Instance.GetCrouchHold()
                    && Physics.Raycast(
                        transform.position,
                        Vector3.up,
                        playerHeight * 0.5f + upDetectionHeight
                    )
            )
            {
                movementState = MovementState.CROUCH;
                moveSpeed = crouchSpeed;
            }
            else if (InputController.Instance.GetSprint())
            {
                movementState = MovementState.SPRINT;
                moveSpeed = sprintSpeed;
            }
            else if (InputController.Instance.GetWalkDirection().magnitude > 0)
            {
                movementState = MovementState.WALK;
                moveSpeed = walkSpeed;
            }
            else
            {
                movementState = MovementState.IDLE;
                moveSpeed = 0;
            }
        }
    }

    void Move()
    {
        moveDirection = (
            transform.right * horizontalInput + transform.forward * verticalInput
        ).normalized;

        // Apply force perpendicular to slope's normal if on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(20 * moveSpeed * GetSlopeMoveDirection(), ForceMode.Force);

            // Apply downward force to keep player on slope
            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(
                    Vector3.down * (movementState == MovementState.CROUCH ? 40f : 80f),
                    ForceMode.Force
                );
            }
        }
        // Move in direction
        else if (Grounded)
        {
            rb.AddForce(10 * moveSpeed * moveDirection, ForceMode.Force);
        }
        // Move in direction but slower in air
        else if (!Grounded)
        {
            rb.AddForce(10 * moveSpeed * moveDirection * airMultiplier, ForceMode.Force);
        }
    }

    void SetDrag()
    {
        if (movementState == MovementState.AIR)
        {
            rb.linearDamping = 0;
        }
        else
        {
            rb.linearDamping = groundDrag;
        }
    }

    void SpeedControl()
    {
        // Prevents player from exceeding move speed on slopes
        if (OnSlope() && !exitingSlope && rb.linearVelocity.magnitude > moveSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 rawVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

            // Clamp x and z axis velocity
            if (rawVelocity.magnitude > moveSpeed)
            {
                Vector3 clampedVelocity = rawVelocity.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(
                    clampedVelocity.x,
                    rb.linearVelocity.y,
                    clampedVelocity.z
                );
            }
        }
    }

    public void Crouch()
    {
        // Shrink to crouch size
        transform.localScale = new Vector3(
            transform.localScale.x,
            crouchScale,
            transform.localScale.z
        );

        // Apply downward force so doesn't float
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }

    void Jump()
    {
        exitingSlope = true;

        // Resets y-velocity to have consistent jump height
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void StepClimb()
    {
        Debug.DrawRay(rayLower.transform.position, rayLower.transform.forward * 0.1f, Color.red);
        Debug.DrawRay(rayUpper.transform.position, rayUpper.transform.forward * 0.2f, Color.red);

        if (
            Physics.Raycast(rayLower.transform.position, rayLower.transform.forward, out _, 0.15f)
            && !Physics.Raycast(
                rayUpper.transform.position,
                rayUpper.transform.forward,
                out _,
                0.3f
            )
        )
        {
            rb.position += new Vector3(0f, stepSmoothing, 0f);
        }
    }

    bool OnSlope()
    {
        if (
            Physics.Raycast(
                transform.position,
                Vector3.down,
                out slopeHit,
                playerHeight * 0.5f + 0.3f
            )
        )
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    public Vector3 GetMoveVelocity()
    {
        if (rb == null)
        {
            return Vector3.zero;
        }

        return new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    }

    public MovementState GetMovementState()
    {
        return movementState;
    }
}
