using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectPlayerTrigger : MonoBehaviour
{
    [SerializeField] private List<WaterPriestess> playersOnRange = new List<WaterPriestess>(); 

    public Action<List<WaterPriestess>> playersOnRangeChanged;

    public List<WaterPriestess> GetWaterPriestesses() => playersOnRange;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent<WaterPriestess>(out WaterPriestess player))
        {
            playersOnRange.Add(player);
            playersOnRangeChanged?.Invoke(playersOnRange);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<WaterPriestess>(out WaterPriestess player))
        {
            playersOnRange.Remove(player);
            playersOnRangeChanged?.Invoke(playersOnRange);
        }
    }
}
