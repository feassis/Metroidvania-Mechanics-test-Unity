using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class BasicAttackUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private WaterPriestess character;

    private void Awake()
    {
        character.BasicAttackStatusChanged += UpdateFillImage;        
    }

    private void UpdateFillImage()
    {
        if (character.IsGrounded())
        {
            if (character.CanGroundAttack())
            {
                fillImage.fillAmount = 1;
            }
            else
            {
                fillImage.fillAmount = 0;
            }
        }
        else
        {
            if (character.CanAirAttack())
            {
                fillImage.fillAmount = 1;
            }
            else
            {
                fillImage.fillAmount = 0;
            }
        }
    }
}
