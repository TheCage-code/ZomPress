using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RareHealPickup : MonoBehaviour
{
    [Header("Heal Effect")]
    [SerializeField] private float healAmount = 100f;

    private MapGenerator owner;
    private Vector2Int ownerChunk;
    private bool collected;

    public void BindOwner(MapGenerator mapGenerator, Vector2Int chunkCoord)
    {
        owner = mapGenerator;
        ownerChunk = chunkCoord;
    }

    private void Awake()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        Transform carRoot = other.transform.root;
        bool isCar = other.CompareTag("Car") || (carRoot != null && carRoot.CompareTag("Car"));

        if (!isCar)
            return;

        collected = true;

        Transform target = carRoot != null && carRoot.CompareTag("Car") ? carRoot : other.transform;
        CarHealth carHealth = target.GetComponentInChildren<CarHealth>();

        if (carHealth == null)
        {
            carHealth = target.GetComponent<CarHealth>();
        }

        if (carHealth != null)
        {
            if (healAmount >= 100f)
            {
                carHealth.RestoreFullHealth();
            }
            else
            {
                carHealth.RestoreHealth(healAmount);
            }
        }

        if (owner != null)
        {
            owner.NotifyRarePickupCollected(gameObject, ownerChunk);
        }

        Destroy(gameObject);
    }
}
