using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaterPriestess : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer characterSprite;

    [Header("Movement Config")]
    [SerializeField] private float moveSpeedXAxis = 10;
    [SerializeField] private float airModifierXAxis = 0.8f;
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float jumpTimeMax = 0.5f;
    [SerializeField] private float slideSpeed = 2f;
    [SerializeField] private float dodgeVelocity = 30f;
    [SerializeField] private float dodgeDuration = 0.8f;
    [SerializeField] private float surfVelocity = 10f;
    [SerializeField] private float surfDuration = 2f;

    [Header("Collision Config")]
    [SerializeField] private List<Transform> leftWallRayCasters = new List<Transform>();
    [SerializeField] private List<Transform> rightWallRayCasters = new List<Transform>();

    private PlayerInput input;
    private float xMovementInput = 0;
    private bool isGoingLeft= false;
    private bool isJumping = false;
    private float jumpTimeCounter = 0f;
    private bool isTouchingWall = false;
    private bool isDodging = false;
    private bool hasSurfed = false;
    private bool isSurfing = false;

    private const string idleAnim="idle";
    private const string walkingAnim="walk";
    private const string jumpingAnim="jump";
    private const string fallingAnim="falling";
    private const string dodgeAnim="dodge";
    private const string surfAnim="surf";

    private Coroutine surfTimer;

    private bool CanJump() => IsGrounded() && !isDodging;
    private bool CanSurf() => !IsGrounded() && !hasSurfed;

    private void Awake()
    {
        input = new PlayerInput();
        input.Movement.Move.performed += OnMovementStarted;
        input.Movement.Move.canceled += OnMovementEnded;

        input.Movement.Jump.performed += OnJumpStarted;
        input.Movement.Jump.canceled += OnJumpCanceled;

        input.Movement.Dodge.performed += OnDodgePerformed;
    }

    private void OnDodgePerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        StartCoroutine(DodgeRoutine());
    }

    private IEnumerator DodgeRoutine()
    {
        isDodging=true;
        animator.Play(dodgeAnim);
        yield return new WaitForSeconds(dodgeDuration);
        isDodging = false;
    }

    private IEnumerator SurfTimer()
    {
        yield return new WaitForSeconds(surfDuration);

        isSurfing = false;
    }

    private void OnJumpCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        isJumping = false;
        isSurfing = false;

        if (surfTimer != null)
        {
            StopCoroutine(surfTimer);
        }
    }

    private void OnJumpStarted(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (CanJump())
        {
            isJumping = true;
            jumpTimeCounter = jumpTimeMax;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            return;
        }

        if(CanSurf())
        {
            isSurfing = true;

            if(surfTimer != null)
            {
                StopCoroutine(surfTimer);
            }

            surfTimer = StartCoroutine(SurfTimer());
        }
    }

    private void OnMovementEnded(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        xMovementInput = 0;
    }

    private void OnMovementStarted(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        xMovementInput = context.ReadValue<float>();
        if(xMovementInput > 0)
        {
            isGoingLeft = false;
        }
        else
        {
            isGoingLeft = true;
        }
    }

    private void OnEnable()
    {
        if(input != null)
        {
            input.Enable();
        }
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.Disable();
        }
    }

    private void Update()
    {
        if (IsGrounded())
        {
            hasSurfed = false;
        }

        HandleSpriteDirection();
        HandleSpriteAnimation();

        HandleJumpDuration();
    }

    private void HandleJumpDuration()
    {
        if(isDodging)
        {
            return;
        }
        
        if (isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        isTouchingWall = IsTouchingAWall();
        Vector2 currentVelocity = rb.velocity;

        if (isTouchingWall && !IsGrounded() && rb.velocity.y <= 0)
        {
            currentVelocity.y = -slideSpeed;
        }
    }

    private void HandleSpriteAnimation()
    {
        if (isDodging)
        {
            return;
        }

        if (isSurfing)
        {
            animator.Play(surfAnim);
            return;
        }


        if(rb.velocity.y > 0 && !IsGrounded())
        {
            animator.Play(jumpingAnim);
            return;
        }

        if (rb.velocity.y < 0 && !IsGrounded())
        {
            animator.Play(fallingAnim);
            return;
        }

        if (Math.Abs(xMovementInput) > 0)
        {
            animator.Play(walkingAnim);
        }
        else
        {
            animator.Play(idleAnim);
        }
    }

    private void HandleSpriteDirection()
    {
        if(isDodging)
        {
            return;
        }

        if(isGoingLeft)
        {
            characterSprite.flipX = true;
        }
        else
        {
            characterSprite.flipX = false;
        }
    }

    private bool IsGrounded()
    {
        // Implementação simples de detecção de chão, pode ser melhorada
        return Physics2D.Raycast(transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
    }

    private bool IsTouchingAWall()
    {
        bool isTouchingWall = false;

        if(xMovementInput < 0)
        {
            foreach(Transform t in leftWallRayCasters)
            {
                isTouchingWall |= Physics2D.Raycast(t.position, Vector2.left, 0.1f, LayerMask.GetMask("Ground"));
            }
        }

        if(xMovementInput > 0)
        {
            foreach (Transform t in rightWallRayCasters)
            {
                isTouchingWall |= Physics2D.Raycast(t.position, Vector2.right, 0.1f, LayerMask.GetMask("Ground"));
            }
        }

        return isTouchingWall;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (Transform t in leftWallRayCasters)
        {
            Gizmos.DrawLine(t.position, t.position + Vector3.left * 0.1f);
        }

        foreach (Transform t in rightWallRayCasters)
        {
            Gizmos.DrawLine(t.position, t.position + Vector3.right * (0.1f));
        }
    }

    private void FixedUpdate()
    {
        Vector2 currentVelocity = rb.velocity;
        float gravityScale = 1f;

        if (isDodging)
        {
            currentVelocity.x = (isGoingLeft ? Vector2.left * dodgeVelocity : Vector2.right * dodgeVelocity).x;
        }
        else if (isSurfing)
        {
            currentVelocity.x = xMovementInput * surfVelocity;
            gravityScale = 0;
            currentVelocity.y = 0;
        }
        else
        {
            currentVelocity.x = xMovementInput * moveSpeedXAxis * (IsGrounded() ? 1 : airModifierXAxis);
        }

        rb.gravityScale = gravityScale;
        rb.velocity = currentVelocity;
    }
}
