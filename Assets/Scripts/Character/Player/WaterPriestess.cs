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
    [SerializeField] private Health health;

    [Header("Movement Config")]
    [SerializeField] private float moveSpeedXAxis = 10;
    [SerializeField] private float airModifierXAxis = 0.8f;
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float jumpTimeMax = 0.5f;
    [SerializeField] private float slideSpeed = 2f;
    [SerializeField] private float dodgeVelocity = 30f;
    [SerializeField] private float dodgeDuration = 0.8f;
    [SerializeField] private float dodgeCooldown = 0.5f;
    [SerializeField] private float surfVelocity = 10f;
    [SerializeField] private float surfDuration = 2f;
    [SerializeField] private float maxFallVelocity = -40f;
    private float xMovementInput = 0;
    private bool isGoingLeft = false;
    private bool isJumping = false;
    private bool isJumpButtonDown = false;
    private bool isTryingToSprint = false;
    private float jumpTimeCounter = 0f;
    private bool isTouchingWall = false;
    private bool isDodging = false;
    private bool isDodgingCooldown = false;
    private bool hasSurfed = false;
    private bool isSurfing = false;
    public Action OnDodgeStatusChanged;

    [Header("Attack Config")]
    [SerializeField] private float attackCooldown = 0.04f;
    [SerializeField] private float attackReset = 1.2f;
    private int attackIndex = 0;
    private bool isAttacking = false;
    private bool isAttackOnCooldown = false;
    private Coroutine resetAttackPattern;
    [SerializeField] private float airAttackCooldown = 2f;
    [SerializeField] private CharacterAnimationTriggerManager triggerManager;
    [SerializeField] private EnemyDetectionTrigger enemyDetectionTrigger;
    private bool isAirAttacking = false;
    private bool isAirAttackOnCooldown = false;
    public Action BasicAttackStatusChanged;
    public Action SpecialAttackStatusChanged;

    [SerializeField] private float spAttackCooldown = 5f;
    private float spAttackCooldownTimer = 0f;
    private bool isSpAttacking = false;
    private bool isBlocking = false;
    [SerializeField] private List<float> attack1TickDmg;
    [SerializeField] private List<float> attack2TickDmg;
    [SerializeField] private List<float> attack3TickDmg;
    [SerializeField] private List<float> spAttackTickDmg;
    [SerializeField] private List<float> airAttackTickDmg;

    [Header("Collision Config")]
    [SerializeField] private List<Transform> leftWallRayCasters = new List<Transform>();
    [SerializeField] private List<Transform> rightWallRayCasters = new List<Transform>();
    [SerializeField] private List<Transform> upRayCasters = new List<Transform>();

    private PlayerInput input;
    


    //Animations
    private const string idleAnim="idle";
    private const string walkingAnim="walk";
    private const string jumpingAnim="jump";
    private const string fallingAnim="falling";
    private const string dodgeAnim="dodge";
    private const string surfAnim="surf";
    private const string attack1Anim = "attack1";
    private const string attack2Anim = "attack2";
    private const string attack3Anim = "attack3";
    private const string airAttackAnim = "airattack";
    private const string spAttackAnim = "spattack";
    private const string blockAnim = "defend";
    private const string unblockAnim = "lowerDefend";

    private Coroutine surfTimer;

    private bool CanJump() => IsGrounded() && !isDodging && !isAttacking && !isAirAttacking && !isSpAttacking && !isBlocking;
    private bool CanSurf() => !IsGrounded() && !hasSurfed && !isAttacking && !isAirAttacking && !isSpAttacking;
    public bool CanGroundAttack() => IsGrounded() && !isAttacking && !isAttackOnCooldown && !isAirAttacking && !isSpAttacking && !isBlocking;
    public bool CanAirAttack() => !IsGrounded() && !isAirAttacking && !isAirAttackOnCooldown && !isSpAttacking && !isBlocking;

    private bool CanMove() => !isAttacking && !isAirAttacking && !isSpAttacking && !isBlocking;

    private bool CanFlipSprite() => !isAttacking && !isDodging && !isAirAttacking && !isSpAttacking && !isBlocking;

    public bool CanSpAttack() => !isAttacking && !isAirAttacking && IsGrounded() && !isSpAttacking && spAttackCooldownTimer <= 0f && !isBlocking;

    private bool IsAnimationLocked() => isDodging || isAttacking || isAirAttacking || isSpAttacking || isBlocking;

    private bool CanBlock() => IsGrounded() && !isAttacking && !isAirAttacking && IsGrounded() && !isSpAttacking && !isBlocking;

    public float SpAttackCooldownPercentage() => spAttackCooldownTimer/spAttackCooldown;

    public bool CanDodge() => !isDodging && !isDodgingCooldown;

    private void Awake()
    {
        input = new PlayerInput();
        input.Movement.Move.performed += OnMovementStarted;
        input.Movement.Move.canceled += OnMovementEnded;
        input.Movement.Sprint.performed += OnSprintStarted;
        input.Movement.Sprint.canceled += OnSprintCanceled;

        input.Movement.Jump.performed += OnJumpStarted;
        input.Movement.Jump.canceled += OnJumpCanceled;

        input.Movement.Dodge.performed += OnDodgePerformed;
        input.Combat.BasicAttack.performed += OnBasicAttackPerformed;
        input.Combat.SpAttack.performed += OnSpAttackPerformed;
        input.Combat.Defend.performed += OnDefendPerformed;
        input.Combat.Defend.canceled+= OnDefendCanceled;

        triggerManager.OnFirstAttackTringger += ProcessAttack1;
        triggerManager.OnSecondAttackTringger += ProcessAttack2;
        triggerManager.OnThirdAttackTringger += ProcessAttack3;
        triggerManager.OnSpecialAttackTrigger += ProcessSpAttack;
        triggerManager.OnAirAttackTrigger += ProcessAirAttack;

        health.OnDamaged += OnDamageTaken;
        health.OnHealed += OnHealed;
        health.OnDeath += OnDeath;

        health.Initialize();
    }

    private void Start()
    {
       PlayerHealthUI.Instance.Setup(health);
    }

    private void OnDeath()
    {
        
    }

    private void OnHealed(float currentHP, float maxHP, float amount)
    {
        
    }

    private void OnDamageTaken(float currentHP, float maxHP, float damage)
    {
        
    }

    public void Damage(float damage)
    {
        health.Damage(damage);
    }

    public void Heal(float damage)
    {
        health.Heal(damage);
    }

    private void OnSprintCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        isTryingToSprint = false;

        if (isSurfing && !isJumpButtonDown)
        {
            isSurfing = false;

            if (surfTimer != null)
            {
                StopCoroutine(surfTimer);
            }
        }
    }

    private void OnSprintStarted(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        isTryingToSprint = true;
    }

    private void OnDefendCanceled(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        StartCoroutine(Unblock());
    }

    private IEnumerator Unblock()
    {
        animator.Play(unblockAnim);

        yield return null;

        yield return CoroutineManager.Instance.WaitAnimationCoroutine(unblockAnim, animator);

        isBlocking = false;
    }

    private void OnDefendPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (CanBlock())
        {
            Block();
        }
    }

    private void Block()
    {
        isBlocking = true;
        animator.Play(blockAnim);
    }

    private void OnSpAttackPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(CanSpAttack())
        {
            SpAttack();
        }
    }

    private void OnBasicAttackPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(CanGroundAttack())
        {
            GroundAttack();
            return;
        }

        if(CanAirAttack())
        {
            AirAttack();
            return;
        }
    }

    private void OnDodgePerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (!CanDodge())
        {
            return;
        }

        StartCoroutine(DodgeRoutine());
    }

    private void GroundAttack()
    {
        if(resetAttackPattern != null)
        {
            StopCoroutine(resetAttackPattern);
        }

        BasicAttackStatusChanged?.Invoke();

        switch (attackIndex)
        {
            case 0:
                StartCoroutine(Attack(attack1Anim));
                attackIndex = 1;
                break;
            case 1:
                StartCoroutine(Attack(attack2Anim));
                attackIndex = 2;
                break;
            case 2:
                StartCoroutine(Attack(attack3Anim));
                attackIndex = 0;
                break;
        }
    }

    public void ProcessAttack1(int tickNum)
    {
        foreach (var enemy in enemyDetectionTrigger.GetEnemies())
        {
            enemy.Damage(attack1TickDmg[tickNum - 1]);
        }
    }
    
    public void ProcessAttack2(int tickNum)
    {
        foreach (var enemy in enemyDetectionTrigger.GetEnemies())
        {
            enemy.Damage(attack2TickDmg[tickNum - 1]);
        }
    }

    public void ProcessAttack3(int tickNum)
    {
        foreach (var enemy in enemyDetectionTrigger.GetEnemies())
        {
            enemy.Damage(attack3TickDmg[tickNum - 1]);
        }
    }

    public void ProcessSpAttack(int tickNum)
    {
        foreach (var enemy in enemyDetectionTrigger.GetEnemies())
        {
            enemy.Damage(spAttackTickDmg[tickNum - 1]);
        }
    }

    public void ProcessAirAttack(int tickNum)
    {
        foreach (var enemy in enemyDetectionTrigger.GetEnemies())
        {
            enemy.Damage(airAttackTickDmg[tickNum - 1]);
        }
    }

    private void SpAttack()
    {
        StartCoroutine(SpAttackCoroutine());
    }

    private IEnumerator SpAttackCoroutine()
    {
        isSpAttacking = true;
        SpecialAttackStatusChanged?.Invoke();

        spAttackCooldownTimer = spAttackCooldown;
        rb.velocity = Vector3.zero;

        animator.Play(spAttackAnim);

        yield return null;

        yield return CoroutineManager.Instance.WaitAnimationCoroutine(spAttackAnim, animator);

        isSpAttacking = false;
        SpecialAttackStatusChanged?.Invoke();
    }

    private IEnumerator ResetAttackCombo()
    {
        yield return new WaitForSeconds(attackReset);
        attackIndex = 0;
    }

    private void AirAttack()
    {
        BasicAttackStatusChanged?.Invoke();
        StartCoroutine(AirAttackRoutine());
    }

    private IEnumerator AirAttackRoutine()
    {
        Debug.Log("air attack");
        isAirAttacking = true;
        BasicAttackStatusChanged?.Invoke();

        animator.Play(airAttackAnim);

        rb.velocity = Vector3.zero;
        rb.gravityScale = 0;

        yield return null;

        yield return CoroutineManager.Instance.WaitAnimationCoroutine(airAttackAnim, animator);

        isAirAttacking = false;
        rb.gravityScale = 1;

        isAirAttackOnCooldown = true;
        yield return new WaitForSeconds(airAttackCooldown);
        isAirAttackOnCooldown = false;

        BasicAttackStatusChanged?.Invoke();
    }

    private IEnumerator Attack(string attackAnim)
    {
        isAttacking = true;
        BasicAttackStatusChanged?.Invoke();

        animator.Play(attackAnim);

        rb.velocity = Vector3.zero;

        yield return null;

        yield return CoroutineManager.Instance.WaitAnimationCoroutine(attackAnim, animator);

        isAttackOnCooldown = true;
        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);
        isAttackOnCooldown = false;

        resetAttackPattern = StartCoroutine(ResetAttackCombo());

        BasicAttackStatusChanged?.Invoke();
    }

    private IEnumerator DodgeRoutine()
    {
        isDodging=true;
        OnDodgeStatusChanged?.Invoke();
        animator.Play(dodgeAnim);
        yield return new WaitForSeconds(dodgeDuration);
        isDodging = false;

        isDodgingCooldown = true;
        yield return new WaitForSeconds(dodgeCooldown);
        isDodgingCooldown = false;

        OnDodgeStatusChanged?.Invoke();
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
        isJumpButtonDown = false;

        if (surfTimer != null)
        {
            StopCoroutine(surfTimer);
        }
    }

    private void OnJumpStarted(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        isJumpButtonDown = true;

        BasicAttackStatusChanged?.Invoke();

        if (CanJump())
        {
            isJumping = true;
            jumpTimeCounter = jumpTimeMax;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

            return;
        }

        if(CanSurf())
        {
            Surf();
            return;
        }
    }

    private void Surf()
    {
        isSurfing = true;
        hasSurfed = true;

        if (surfTimer != null)
        {
            StopCoroutine(surfTimer);
        }

        surfTimer = StartCoroutine(SurfTimer());
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
        if(spAttackCooldownTimer > 0)
        {
            spAttackCooldownTimer = Mathf.Clamp(spAttackCooldownTimer - Time.deltaTime, 0, float.MaxValue);
            SpecialAttackStatusChanged?.Invoke();
        }

        if(isTryingToSprint && !IsGrounded() && !hasSurfed)
        {
            Surf();
        }

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
        if(!CanFlipSprite())
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

       

        if (IsTouchingCelling())
        {
            Debug.Log("Touching ceiling");
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
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
        if (IsAnimationLocked())
        {
            return;
        }

        if (isSurfing)
        {
            animator.Play(surfAnim);
            return;
        }

        if (isTryingToSprint && IsGrounded())
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
        if(!CanFlipSprite())
        {
            return;
        }

        if(isGoingLeft)
        {
            characterSprite.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            characterSprite.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public bool IsGrounded()
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

    private bool IsTouchingCelling()
    {
        bool isTouchingCelling = false;

        foreach (Transform t in upRayCasters)
        {
            isTouchingCelling |= Physics2D.Raycast(t.position, Vector2.up, 0.1f, LayerMask.GetMask("Ground"));
        }

        return isTouchingCelling;
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
        if (!CanMove())
        {
            return;
        }

        Vector2 currentVelocity = rb.velocity;
        float gravityScale = 1f;

        if (isDodging)
        {
            currentVelocity.x = (isGoingLeft ? Vector2.left * dodgeVelocity : Vector2.right * dodgeVelocity).x;
        }
        else if (isTryingToSprint && IsGrounded())
        {
            currentVelocity.x = xMovementInput * surfVelocity;
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

        if (currentVelocity.y < maxFallVelocity)
        {
            currentVelocity.y = maxFallVelocity;
        }

        rb.gravityScale = gravityScale;
        rb.velocity = currentVelocity;
    }
}
