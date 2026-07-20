using UnityEngine;

public class BlockerObjects : MonoBehaviour
{
    [SerializeField] private string blockerObjectTag = "BlockerObject";
    [SerializeField] private string carTag = "Car";

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!gameObject.CompareTag(blockerObjectTag))
            return;

        if (!other.gameObject.CompareTag(carTag))
            return;

        var car = other.gameObject.GetComponent<CarMovement>();
        if (car != null)
        {
            car.HitTree();
        }
    }
}
