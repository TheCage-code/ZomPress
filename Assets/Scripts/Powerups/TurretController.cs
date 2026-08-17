using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("References")]
    public Transform turretPivot;
    public SpriteRenderer turretSprite;
    public Animator turretAnimator;

    [Header("Settings")]
    public float detectionRange = 8f;
    public float rotationSpeed = 360f;
    public float fireRate = 1f;

    private float nextFireTime;

    private void Update()
    {
        if (UpgradeManager.Instance == null || !UpgradeManager.Instance.hasTurret)
            return;

        if (turretPivot == null)
            return;

        Enemy target = FindClosestEnemy();
        if (target == null)
            return;

        Vector3 direction = target.transform.position - turretPivot.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        turretPivot.rotation = Quaternion.RotateTowards(
            turretPivot.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        if (Time.time >= nextFireTime)
        {
            FireAtNearestEnemy(target);
            nextFireTime = Time.time + fireRate;
        }
    }

    private Enemy FindClosestEnemy()
    {
        if (Enemies.Instance == null)
            return null;

        return Enemies.Instance.GetClosestEnemy(transform.position, detectionRange);
    }

    private void FireAtNearestEnemy(Enemy target)
    {
        if (target == null)
            return;

        TriggerFireAnimation();
        target.TakeDamage(target.maxHealth);
    }

    private void TriggerFireAnimation()
    {
        if (turretAnimator != null)
        {
            turretAnimator.SetTrigger("Fire");
        }
    }

    public void SetTurretVisible(bool visible)
    {
        if (turretPivot != null)
            turretPivot.gameObject.SetActive(visible);

        if (turretSprite != null)
            turretSprite.enabled = visible;
    }
}
