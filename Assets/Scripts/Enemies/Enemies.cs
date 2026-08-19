using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    public static Enemies Instance { get; private set; }

    [Header("References")]
    public Transform player;
    public EnemyPool enemyPool;

    [Header("Spawn Settings")]
    public float spawnDistanceMin = 8f;
    public float spawnDistanceMax = 22f;
    public int maxEnemies = 400;
    public float spawnInterval = 0.5f;
    public float destroyDistance = 50f;

    private float spawnTimer;
    private float gameTimer = 0f;
    private readonly List<Enemy> spawnedEnemies = new List<Enemy>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Enemies: Player reference is missing.");
            enabled = false;
            return;
        }

        if (enemyPool == null)
        {
            enemyPool = GetComponent<EnemyPool>();
            if (enemyPool == null)
            {
                Debug.LogError("Enemies: EnemyPool reference is missing.");
                enabled = false;
                return;
            }
        }

        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (player == null)
            return;

        gameTimer += Time.deltaTime;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = spawnInterval;
            SpawnEnemy();
        }

        CleanUpFarEnemies();
    }

    void SpawnEnemy()
    {
        if (spawnedEnemies.Count >= maxEnemies)
            return;

        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float distance = Random.Range(spawnDistanceMin, spawnDistanceMax);
        Vector3 spawnPosition = player.position + (Vector3)(randomDirection * distance);

        Enemy enemy;

        // 0-20 sn: sadece Little (default) zombi.
        if (gameTimer < 20f)
        {
            enemy = enemyPool.GetDefaultZombie(player, spawnPosition);
        }
        // 20-35 sn: Little (default) + enemyPrefab normal karışık.
        else if (gameTimer < 35f)
        {
            enemy = Random.value < 0.55f
                ? enemyPool.GetDefaultZombie(player, spawnPosition)
                : enemyPool.GetEnemy(EnemyPool.EnemyType.Normal, player, spawnPosition);
        }
        // 35-50 sn: Little (default) + normal + big karışık.
        else if (gameTimer < 50f)
        {
            float rand = Random.value;
            if (rand < 0.45f)
                enemy = enemyPool.GetDefaultZombie(player, spawnPosition);
            else if (rand < 0.8f)
                enemy = enemyPool.GetEnemy(EnemyPool.EnemyType.Normal, player, spawnPosition);
            else
                enemy = enemyPool.GetEnemy(EnemyPool.EnemyType.Big, player, spawnPosition);
        }
        // 50 sn sonrası: Little (default) + normal + big + boss karışık.
        else
        {
            float rand = Random.value;
            if (rand < 0.35f)
                enemy = enemyPool.GetDefaultZombie(player, spawnPosition);
            else if (rand < 0.65f)
                enemy = enemyPool.GetEnemy(EnemyPool.EnemyType.Normal, player, spawnPosition);
            else if (rand < 0.88f)
                enemy = enemyPool.GetEnemy(EnemyPool.EnemyType.Big, player, spawnPosition);
            else
                enemy = enemyPool.GetEnemy(EnemyPool.EnemyType.Boss, player, spawnPosition);
        }

        if (enemy != null)
        {
            enemy.manager = this;
            spawnedEnemies.Add(enemy);
        }
    }

    void CleanUpFarEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = spawnedEnemies[i];
            if (enemy == null)
            {
                spawnedEnemies.RemoveAt(i);
                continue;
            }

            if (Vector3.Distance(enemy.transform.position, player.position) > destroyDistance)
            {
                enemyPool.ReturnEnemy(enemy);
                spawnedEnemies.RemoveAt(i);
            }
        }
    }

    public void ReturnEnemyToPool(Enemy enemy)
    {
        if (enemy == null)
            return;

        spawnedEnemies.Remove(enemy);
        enemyPool.ReturnEnemy(enemy);
    }

    public void EnemyDefeated()
    {
        // Enemy öldüğünde çağrılır
        // İstatistik takibi vs. için
    }

    public Enemy GetClosestEnemy(Vector3 origin, float maxDistance)
    {
        Enemy closest = null;
        float closestDistance = maxDistance;

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            Enemy enemy = spawnedEnemies[i];
            if (enemy == null)
                continue;

            float distance = Vector3.Distance(origin, enemy.transform.position);
            if (distance <= maxDistance && distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    public void InstantKillEnemiesAround(Transform center, float radius)
    {
        if (center == null)
            return;

        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            Enemy enemy = spawnedEnemies[i];
            if (enemy == null)
                continue;

            if (Vector3.Distance(enemy.transform.position, center.position) <= radius)
            {
                enemy.TakeDamage(enemy.maxHealth);
            }
        }
    }
}
