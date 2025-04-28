using System.Collections;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Turret Settings")]
    public int maxHp = 70;
    public float detectionRange = 8f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shootInterval = 1.5f;
    public int bulletDamage = 10;

    private Transform playerTransform;
    private int currentHp;
    private bool isPlayerDetected = false;

    private void Start()
    {
        currentHp = maxHp;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        StartCoroutine(ShootAtPlayer());
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionRange;

        if (isPlayerDetected)
        {
            RotateTowardsPlayer();
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private IEnumerator ShootAtPlayer()
    {
        while (true)
        {
            if (isPlayerDetected && bulletPrefab != null && firePoint != null)
            {
                Vector2 direction = (playerTransform.position - firePoint.position).normalized;
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                bullet.GetComponent<Bullet>().Initialize(direction, bulletDamage);
            }
            yield return new WaitForSeconds(shootInterval);
        }
    }

    public void Damage(int damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Turret destroyed.");
        Destroy(gameObject);
    }
}
