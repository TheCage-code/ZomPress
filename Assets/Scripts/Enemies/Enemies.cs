using System.Collections.Generic;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public EnemyPool enemyPool;

    [Header("Spawn Settings")]
    public float spawnDistanceMin = 8f;
    public float spawnDistanceMax = 22f;
    public int maxEnemies = 60;
    public float spawnInterval = 0.7f;
    public float destroyDistance = 50f;

    private float spawnTimer;
    private float gameTimer = 0f;
    private readonly List<Enemy> spawnedEnemies = new List<Enemy>();

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

        EnemyPool.EnemyType zombieType;

        // İlk 30 saniye: sadece Normal
        if (gameTimer < 30f)
        {
            zombieType = EnemyPool.EnemyType.Normal;
        }
        // 30-60 saniye: Normal + Big karışık (%70 Normal, %30 Big)
        else if (gameTimer < 60f)
        {
            zombieType = Random.value < 0.7f ? EnemyPool.EnemyType.Normal : EnemyPool.EnemyType.Big;
        }
        // 60 saniye sonrası: Normal + Big + Boss karışık
        else
        {
            float rand = Random.value;
            if (rand < 0.55f)
                zombieType = EnemyPool.EnemyType.Normal;
            else if (rand < 0.85f)
                zombieType = EnemyPool.EnemyType.Big;
            else
                zombieType = EnemyPool.EnemyType.Boss;
        }
        
        Enemy enemy = enemyPool.GetEnemy(zombieType, player, spawnPosition);
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
}
