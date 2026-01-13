using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour
{
    private EnemyShooterRadioControllerScript owner;
    private int damageToPlayer;

    private SpriteRenderer spriteRenderer;
    private Collider2D shieldCollider;

    // Nerf: boss controls this. Default is old value.
    public float disableDuration = 2.5f;

    public void Setup(EnemyShooterRadioControllerScript owner, int damage)
    {
        this.owner = owner;
        this.damageToPlayer = damage;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        shieldCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            PlayerController player = collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Damage(damageToPlayer);
            }
        }
        else if (collider.CompareTag("Spell"))
        {
            if (owner != null)
                owner.OnShieldHit();

            StartCoroutine(DeactivateShieldTemporarily());
        }
    }

    public bool IsActive()
    {
        if (spriteRenderer == null || shieldCollider == null) return false;
        return spriteRenderer.enabled && shieldCollider.enabled;
    }

    private IEnumerator DeactivateShieldTemporarily()
    {
        // Hide shield
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (shieldCollider != null) shieldCollider.enabled = false;

        // Wait
        yield return new WaitForSeconds(disableDuration);

        // Reactivate shield
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (shieldCollider != null) shieldCollider.enabled = true;
    }
}
