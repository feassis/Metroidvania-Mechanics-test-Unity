using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationTriggerManager : MonoBehaviour
{
    public Action<int> OnFirstAttackTringger;
    public Action<int> OnSecondAttackTringger;
    public Action<int> OnThirdAttackTringger;
    public Action<int> OnSpecialAttackTrigger;
    public Action<int> OnAirAttackTrigger;

    public void Attack1Trigger(int i)
    {
        OnFirstAttackTringger?.Invoke(i);
    }

    public void Attack2Trigger(int i)
    {
        OnSecondAttackTringger?.Invoke(i);
    }

    public void Attack3Trigger(int i)
    {
        OnThirdAttackTringger?.Invoke(i);
    }

    public void SpecialAttackTrigger(int i)
    {
        OnSpecialAttackTrigger?.Invoke(i);
    }

    public void AirAttackTrigger(int i)
    {
        OnAirAttackTrigger?.Invoke(i);
    }
}
