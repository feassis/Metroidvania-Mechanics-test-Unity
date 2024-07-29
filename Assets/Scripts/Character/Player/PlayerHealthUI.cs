using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private float healthPerHeart = 20f;
    [SerializeField] private HealthHeart heartPrefab;

    private Health health;

    private List<HealthHeart> hearts = new List<HealthHeart>();

    public static PlayerHealthUI Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    public void Setup(Health health)
    {
        foreach (HealthHeart heart in hearts)
        {
            Destroy(heart.gameObject);
        }
        hearts.Clear();

        this.health = health;

        int fullHearts = Mathf.FloorToInt( health.GetMaxHealth() / healthPerHeart);

        for(int i = 0; i < fullHearts; i++)
        {
            var heart = Instantiate<HealthHeart>(heartPrefab, transform);
            heart.Setup(healthPerHeart, healthPerHeart);
            hearts.Add(heart);
        }

        if(fullHearts*healthPerHeart > health.GetMaxHealth())
        {
            var heart = Instantiate<HealthHeart>(heartPrefab, transform);
            heart.Setup(health.GetMaxHealth() - fullHearts * healthPerHeart, health.GetMaxHealth() - fullHearts * healthPerHeart);
            hearts.Add(heart);
        }

        UpdateHearts();

        health.OnDamaged += OnHpChanged;
        health.OnHealed += OnHpChanged;
    }

    private void OnHpChanged(float arg1, float arg2, float arg3)
    {
        UpdateHearts();
    }

    private void UpdateHearts()
    {
        float currentHP = health.GetCurrentHP();

        foreach (var heart in hearts)
        {
            float lifeToAdd = 0;

            if(currentHP < healthPerHeart)
            {
                lifeToAdd = currentHP;
            }
            else
            {
                lifeToAdd = healthPerHeart;
            }

            heart.UpdateHeart(lifeToAdd);

            currentHP -= healthPerHeart;
        }
    }

    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;

        }
    }
}
