using UnityEngine;
using UnityEngine.UI;

public class SpAttackUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private WaterPriestess character;

    private void Awake()
    {
        character.SpecialAttackStatusChanged += UpdateFillImage;
    }

    private void UpdateFillImage()
    {
        if (character.CanSpAttack())
        {
            fillImage.fillAmount = 1;
        }
        else
        {
            fillImage.fillAmount =  1 - character.SpAttackCooldownPercentage();
        }
    }
}
