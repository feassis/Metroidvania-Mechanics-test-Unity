using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHP = 100f;

    private float currentHP;

    public Action<float, float, float> OnDamaged;
    public Action<float, float, float> OnHealed;
    public Action OnDeath;

    public float GetMaxHealth() { return maxHP; }
    public float GetCurrentHP() {  return currentHP; }

    public void Initialize()
    {
        currentHP = maxHP;
    }

    public void Damage(float damage)
    {
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        OnDamaged?.Invoke(currentHP, maxHP, damage);

        if(currentHP == 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float heal)
    {
        currentHP = Mathf.Clamp(currentHP + heal, 0, maxHP);
        OnHealed?.Invoke(currentHP, maxHP, heal);
    }
}
