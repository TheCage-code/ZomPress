using UnityEngine;

public class GoldDrop : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float lifeTime = 1f;
    public Vector3 floatOffset = new Vector3(0f, 1f, 0f);

    private float timer;
    private Vector3 startPosition;

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
            Destroy(gameObject);
        }
    }
}
