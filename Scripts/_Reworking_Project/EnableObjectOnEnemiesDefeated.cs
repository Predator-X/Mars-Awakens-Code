using UnityEngine;

public class EnableObjectOnEnemiesDefeated : MonoBehaviour
{
    public GameObject objectToEnable;

    [SerializeField] private int totalEnemies;
    private int defeatedEnemies;

    private void Start()
    {
        totalEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // Subscribe to the event
        EnemyManager.OnEnemyDefeated += HandleEnemyDefeated;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        EnemyManager.OnEnemyDefeated -= HandleEnemyDefeated;
    }

    private void HandleEnemyDefeated()
    {
        defeatedEnemies++;

        if (defeatedEnemies == totalEnemies)
        {
            objectToEnable.SetActive(true);
        }
    }
}
