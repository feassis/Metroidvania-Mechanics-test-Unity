using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer characterSprite;

    [Header("Movement Config")]
    [SerializeField] private float moveSpeedXAxis = 10;
    [SerializeField] private float airModifierXAxis = 0.8f;
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float jumpTimeMax = 0.5f;


    private PlayerInput input;
    private float xMovementInput = 0;
    private bool isGoingLeft= false;
    private bool isJumping = false;
    private float jumpTimeCounter = 0f;

    private const string idleAnim="idle";
    private const string walkingAnim="walk";
    private const string jumpingAnim="jump";
    private const string fallingAnim="falling";

    private void Awake()
    {
        input = new PlayerInput();
        input.Movement.Move.performed += OnMovementStarted;
        input.Movement.Move.canceled += OnMovementEnded;

        input.Movement.Jump.performed += OnJumpStarted;
        input.Movement.Jump.canceled += OnJumpCanceled;
    }

    private void OnJumpCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        isJumping = false;
    }

    private void OnJumpStarted(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            isJumping = true;
            jumpTimeCounter = jumpTimeMax;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
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
        HandleSpriteDirection();
        HandleSpriteAnimation();

        HandleJumpDuration();
    }

    private void HandleJumpDuration()
    {
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
    }

    private void HandleSpriteAnimation()
    {
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

    private void FixedUpdate()
    {
        Vector2 currentVelocity = rb.velocity;
        currentVelocity.x = xMovementInput * moveSpeedXAxis * (IsGrounded() ? 1 : airModifierXAxis);
        rb.velocity = currentVelocity;
    }
}
