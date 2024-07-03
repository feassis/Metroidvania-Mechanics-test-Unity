using UnityEngine;
using UnityEngine.UI;

public class DodgeUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private WaterPriestess character;

    private void Awake()
    {
        character.OnDodgeStatusChanged += UpdateFillImage;
    }

    private void UpdateFillImage()
    {
        if (character.CanDodge())
        {
            fillImage.fillAmount = 1;
        }
        else
        {
            fillImage.fillAmount = 0;
        }
    }
}
