using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthChangePopUp : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI textAmount;

    private const string spawnAnim = "spawnAnim";

    public void Setup(float amount)
    {
        
        if(amount > 0)
        {
            textAmount.color = Color.green;
            textAmount.text = $"+ {amount}";
        }
        else if(amount < 0)
        {
            textAmount.color = Color.red;
            textAmount.text = $"- {amount}";
        }
    }

    private void Awake()
    {
        StartCoroutine(PopUpAnim());
    }

    private IEnumerator PopUpAnim()
    {
        animator.Play(spawnAnim);

        yield return null;

        yield return CoroutineManager.Instance.WaitAnimationCoroutine(spawnAnim, animator);

        Destroy(gameObject);
    }
}
