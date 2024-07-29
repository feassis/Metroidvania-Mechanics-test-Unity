using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionTrigger : MonoBehaviour
{
    [SerializeField] private List<EnemySimpliedAIBase> enemySimpliedAIBases = new List<EnemySimpliedAIBase>();

    public List<EnemySimpliedAIBase> GetEnemies() => enemySimpliedAIBases;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.TryGetComponent<EnemySimpliedAIBase>(out EnemySimpliedAIBase enemy))
        {
            enemySimpliedAIBases.Add(enemy);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<EnemySimpliedAIBase>(out EnemySimpliedAIBase enemy))
        {
            enemySimpliedAIBases.Remove(enemy);
        }
    }
}
