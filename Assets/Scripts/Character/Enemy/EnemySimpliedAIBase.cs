using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySimpliedAIBase : MonoBehaviour, IDamageble
{
    [SerializeField] private DetectPlayerTrigger detection;
    [SerializeField] private float speed;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyAnimationEvents events;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private HealthChangePopUp healthChangePopUpPrebab;
    [SerializeField] private Transform popupSpawnPoint;
    [SerializeField] private Health health;

    [Header("Movement Setup")]
    [SerializeField] private float timeToLoseTrackOfPlayer = 3f;
    [SerializeField] private float attackDistance = 1f;
    [SerializeField] private float stopMovingDistanceInX = 0.5f;

    [Header("Attack Setup")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float damage = 10f;
    
    private bool isFacingRight = true;
    private const string walkAnim = "walk";
    private const string idleAnim = "idle";
    private const string attackAnim = "attack";
    private bool isAttackOnCooldown = false;
    private bool isAttacking = false;

    private bool canFollowPlayer = false;
    private bool hasPlayerOnRange = false;
    private WaterPriestess target;
    private Coroutine loseTrackOfPlayerRoutine;

    private void Start()
    {
        detection.playersOnRangeChanged += DetectPlayerTrigger_PlayerOnRangeChanged;
        health.OnDamaged += OnDamageTaken;
        health.OnHealed += OnHealed;
        events.OnAttackHit += OnAttackHit;
    }

    private void OnAttackHit()
    {
        AttackHit();
    }

    private void Update()
    {
        HandleFacingAnimation();
        HandleMovementAnimation();
    }

    private void HandleFacingAnimation()
    {
        if(rb.velocity.x > 0.05f)
        {
            isFacingRight = true;
        }
        else if(rb.velocity.x < -0.05f)
        {
            isFacingRight = false;
        }

        sprite.flipX = isFacingRight;
    }

    private void HandleMovementAnimation()
    {
        if (isAttacking)
        {
            return;
        }

        if(rb.velocity.x == 0)
        {
            animator.Play(idleAnim);
        }
        else
        {
            animator.Play(walkAnim);
        }
    }
    private IEnumerator AttackRoutine()
    {
        animator.Play(attackAnim);
        isAttackOnCooldown = true;
        isAttacking = true;

        yield return null;

        yield return CoroutineManager.Instance.WaitAnimationCoroutine(attackAnim,animator);

        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);

        isAttackOnCooldown = false;
    }

    private void FixedUpdate()
    {
        
        if(canFollowPlayer)
        {
            Vector3 playerDirection = target.transform.position - transform.position;

            if (isAttacking)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            if(Vector3.Distance(target.transform.position, transform.position) < attackDistance && !isAttackOnCooldown)
            {
                StartCoroutine(AttackRoutine());
                rb.velocity = Vector2.zero;
                return;
            }

            if (Math.Abs(playerDirection.x) < stopMovingDistanceInX)
            {
                rb.velocity = Vector2.zero;
                return;
            }


            GoToPlayerTarget();

            return;
        }

        if (hasPlayerOnRange)
        {
            foreach (WaterPriestess player in detection.GetWaterPriestesses())
            {
                if (IsFacingPlayer(player))
                {
                    canFollowPlayer = true;
                    target = player;
                    break;
                }
            }
        }

        rb.velocity = Vector2.zero;
    }

    private void GoToPlayerTarget()
    {
        var playerDirection = target.transform.position - transform.position;

        float xDir = playerDirection.x > 0 ? 1 : -1;

        var currentVel = rb.velocity;

        rb.velocity = new Vector2(xDir * speed, currentVel.y);
    }

    public void AttackHit()
    {
        foreach(WaterPriestess player in detection.GetWaterPriestesses())
        {
            player.Damage(damage);
        }
    }

    private void DetectPlayerTrigger_PlayerOnRangeChanged(List<WaterPriestess> list)
    {
        if(list.Count <= 0)
        {
            hasPlayerOnRange = false;

            if (canFollowPlayer)
            {
                loseTrackOfPlayerRoutine = StartCoroutine(LoseTrackOfPlayerTimer());
            }
            return;
        }

        hasPlayerOnRange = true;
        
        if(loseTrackOfPlayerRoutine != null)
        {
            StopCoroutine(loseTrackOfPlayerRoutine); 
        }

        foreach(WaterPriestess player in list) { 
            if (!canFollowPlayer)
            {
                if(IsFacingPlayer(player)) 
                {
                    canFollowPlayer = true;
                    target = player;
                    break;
                }
            }
        }
    }

    private bool IsFacingPlayer(WaterPriestess player)
    {
        float playerDirection = player.transform.position.x - transform.position.x;
        return (playerDirection >= 0 && isFacingRight) || (playerDirection <= 0 && !isFacingRight);
    }

    private IEnumerator LoseTrackOfPlayerTimer()
    {
        yield return new WaitForSeconds(timeToLoseTrackOfPlayer);

        canFollowPlayer = false;
        target = null;
    }

    public void Damage(float damage)
    {
        health.Damage(damage);
    }

    private void OnDamageTaken(float currentHP, float maxHP, float damage)
    {
        HealthChangePopUp popup = Instantiate<HealthChangePopUp>(healthChangePopUpPrebab, popupSpawnPoint);
        popup.Setup(-damage);
    }

    public void Heal(float amount)
    {
        health.Heal(amount);
    }

    private void OnHealed(float currentHP, float maxHP, float amount)
    {

    }
}
