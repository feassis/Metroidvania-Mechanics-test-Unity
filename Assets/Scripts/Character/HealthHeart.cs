using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthHeart : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Image bgImage;

    private float maxHP;
    private float currentHP;

    public void Setup(float maxHP, float currentHP)
    {
        this.maxHP = maxHP;
        UpdateHeart(currentHP);
    }

    public void UpdateHeart(float currentHP)
    {
        this.currentHP = currentHP;


        fillImage.color = new Color(fillImage.color.r, fillImage.color.g, fillImage.color.b, this.currentHP / maxHP);

        if(currentHP <= 0)
        {
            fillImage.gameObject.SetActive(false);
        }
        else
        {
            fillImage.gameObject.SetActive(true);
        }
    }
}
