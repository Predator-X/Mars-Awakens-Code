using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;          // The prefab of the enemy to spawn
    public Transform spawnPoint;            // The position where the enemy will be spawned
    public float spawnInterval = 3f;        // Time interval between enemy spawns
    public int maxEnemies = 10;             // Maximum number of enemies to spawn
    public bool spawnEnabled = true;        // Whether enemy spawning is enabled

    private int spawnedEnemiesCount = 0;    // Counter for the spawned enemies
    private float nextSpawnTime = 0f;       // Time when the next enemy will be spawned

    private void Start()
    {
        EnableSpawning();
    }

    private void Update()
    {
        // Check if enemy spawning is enabled and if it's time to spawn a new enemy
        if (spawnEnabled && Time.time >= nextSpawnTime && spawnedEnemiesCount < maxEnemies)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        // Instantiate the enemy prefab at the specified spawn point
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedEnemiesCount++;
    }

    public void EnableSpawning()
    {
        spawnEnabled = true;
    }

    public void DisableSpawning()
    {
        spawnEnabled = false;
    }
}