using UnityEngine;

public class GoldDrop : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float lifeTime = 1f;
    public Vector3 floatOffset = new Vector3(0f, 1f, 0f);

    private float timer;
    private Vector3 startPosition;
    private int goldValue = 1;

    public void SetGoldValue(int value)
    {
        goldValue = value;
    }

    void OnEnable()
    {
        timer = 0f;
        startPosition = transform.position;
    }

    void Update()
    {
        timer += Time.deltaTime;
        transform.position = startPosition + floatOffset * (timer / lifeTime);

        if (timer >= lifeTime)
        {
            CollectGold();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Car"))
        {
            CollectGold();
        }
    }

    private void CollectGold()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.AddGold(goldValue);
        }
        Destroy(gameObject);
    }
}
