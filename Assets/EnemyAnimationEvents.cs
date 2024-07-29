using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    public Action OnAttackHit;

    public void AttackHit()
    {
        OnAttackHit?.Invoke();
    }
}
