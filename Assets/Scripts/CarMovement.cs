using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CarMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float slowSpeed = 2f;
    public float recoveryRate = 1.5f;
    public float turnSpeed = 180f;
    public float extraSpeed = 0f;
    float currentSpeed;


    Rigidbody2D rb2D;
    float moveInput;
    float steerInput;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        currentSpeed = moveSpeed;
    }

    void FixedUpdate()
    {
        float targetSpeed = moveSpeed + extraSpeed;

        if (UpgradeManager.Instance != null)
        {
            targetSpeed += UpgradeManager.Instance.totalSpeedBonus;
        }

        rb2D.linearVelocity = transform.up * moveInput * currentSpeed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, recoveryRate * Time.fixedDeltaTime);

        if (Mathf.Abs(steerInput) > 0.01f)
        {
            float rotationAmount = -steerInput * turnSpeed * Time.fixedDeltaTime;
            rb2D.MoveRotation(rb2D.rotation + rotationAmount);
        }
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Vertical");
        steerInput = Input.GetAxisRaw("Horizontal");
    }

    
    public void HitTree()
{
    currentSpeed = Mathf.Min(currentSpeed, slowSpeed);
}
}
