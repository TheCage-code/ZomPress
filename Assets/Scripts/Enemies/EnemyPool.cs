using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject goldDropPrefab;
    public Transform enemyParent;
    public int initialPoolSize = 20;

    private readonly Queue<Enemy> pool = new Queue<Enemy>();

    void Awake()
    {
        if (enemyParent == null)
        {
            enemyParent = transform;
        }

        PrewarmPool();
    }

    void PrewarmPool()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyPool: enemyPrefab is missing.");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledEnemy();
        }
    }

    private Enemy CreatePooledEnemy()
    {
        GameObject instance = Instantiate(enemyPrefab, Vector3.zero, Quaternion.identity, enemyParent);
        Enemy enemyScript = instance.GetComponent<Enemy>();

        if (enemyScript == null)
        {
            Debug.LogError("EnemyPool: enemyPrefab does not contain an Enemy component.");
            Destroy(instance);
            return null;
        }

        enemyScript.pool = this;
        enemyScript.goldDropPrefab = goldDropPrefab;
        instance.SetActive(false);
        pool.Enqueue(enemyScript);
        return enemyScript;
    }

    public Enemy GetEnemy(Transform target, Vector3 position)
    {
        if (pool.Count == 0)
        {
            CreatePooledEnemy();
        }

        if (pool.Count == 0)
            return null;

        Enemy enemy = pool.Dequeue();
        enemy.ResetState();
        enemy.transform.position = position;
        enemy.transform.rotation = Quaternion.identity;
        enemy.target = target;
        enemy.goldDropPrefab = goldDropPrefab;
        enemy.gameObject.SetActive(true);

        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        enemy.ResetState();
        enemy.gameObject.SetActive(false);

        if (!pool.Contains(enemy))
        {
            pool.Enqueue(enemy);
        }
    }
}
