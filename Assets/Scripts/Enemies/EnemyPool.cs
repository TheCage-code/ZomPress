using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject enemyBigZombiePrefab;
    public GameObject enemyBossZombiePrefab;
    public GameObject goldDropPrefab;
    public Transform enemyParent;
    public enum EnemyType { Normal, Big, Boss }
    public int initialPoolSize = 20;
    private readonly Dictionary<EnemyType, Queue<Enemy>> pools = new Dictionary<EnemyType, Queue<Enemy>>();

    void Awake()
    {
        if (enemyParent == null)
        {
            enemyParent = transform;
        }

        pools[EnemyType.Normal] = new Queue<Enemy>();
        pools[EnemyType.Big] = new Queue<Enemy>();
        pools[EnemyType.Boss] = new Queue<Enemy>();

        PrewarmPool();
    }

    void PrewarmPool()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPool: enemyPrefab is missing.");
            return;
        }

        // Normal zombi pooluna başlangıç yükle
        for (int i = 0; i < initialPoolSize / 3; i++)
        {
            CreatePooledEnemy(enemyPrefab, EnemyType.Normal);
        }

        // Big zombi pooluna başlangıç yükle
        if (enemyBigZombiePrefab != null)
        {
            for (int i = 0; i < initialPoolSize / 3; i++)
            {
                CreatePooledEnemy(enemyBigZombiePrefab, EnemyType.Big);
            }
        }

        // Boss zombi pooluna başlangıç yükle
        if (enemyBossZombiePrefab != null)
        {
            for (int i = 0; i < initialPoolSize / 3; i++)
            {
                CreatePooledEnemy(enemyBossZombiePrefab, EnemyType.Boss);
            }
        }
    }

    private Enemy CreatePooledEnemy()
    {
        return CreatePooledEnemy(enemyPrefab, EnemyType.Normal);
    }

    private Enemy CreatePooledEnemy(GameObject prefab, EnemyType type = EnemyType.Normal)
    {
        if (prefab == null)
        {
            Debug.LogError("EnemyPool: prefab is null.");
            return null;
        }

        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity, enemyParent);
        Enemy enemyScript = instance.GetComponent<Enemy>();

        if (enemyScript == null)
        {
            Debug.LogError("EnemyPool: prefab does not contain an Enemy component.");
            Destroy(instance);
            return null;
        }

        // Zombi tipini ayarla
        enemyScript.zombieType = (Enemy.ZombieType)(int)type;
        
        // Gold değerini türe göre set et
        if (type == EnemyType.Boss)
            enemyScript.goldValue = 15;
        else if (type == EnemyType.Big)
            enemyScript.goldValue = 5;
        else
            enemyScript.goldValue = 1;
        
        enemyScript.pool = this;
        enemyScript.goldDropPrefab = goldDropPrefab;
        instance.SetActive(false);
        pools[type].Enqueue(enemyScript);
        return enemyScript;
    }

    public Enemy GetEnemy(Transform target, Vector3 position)
    {
        return GetEnemy(EnemyType.Normal, target, position);
    }

    public Enemy GetEnemy(EnemyType type, Transform target, Vector3 position)
    {
        Enemy enemy;
        Queue<Enemy> targetPool = pools[type];

        if (targetPool.Count > 0)
        {
            enemy = targetPool.Dequeue();
        }
        else
        {
            // Type'a uygun prefab seç
            GameObject prefabToUse = enemyPrefab;
            if (type == EnemyType.Big && enemyBigZombiePrefab != null)
                prefabToUse = enemyBigZombiePrefab;
            else if (type == EnemyType.Boss && enemyBossZombiePrefab != null)
                prefabToUse = enemyBossZombiePrefab;
            
            enemy = CreatePooledEnemy(prefabToUse, type);
            if (enemy == null) return null;
        }

        enemy.zombieType = (Enemy.ZombieType)(int)type;
        
        // Gold değerini türe göre set et
        if (type == EnemyType.Boss)
            enemy.goldValue = 15;
        else if (type == EnemyType.Big)
            enemy.goldValue = 5;
        else
            enemy.goldValue = 1;

        enemy.target = target;
        enemy.transform.position = position;
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        enemy.gameObject.SetActive(false);
        EnemyType poolType = (EnemyType)enemy.zombieType;
        pools[poolType].Enqueue(enemy);
    }
}
