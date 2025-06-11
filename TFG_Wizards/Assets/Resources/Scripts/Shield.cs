using UnityEngine;
using System.Collections; // Necesario para usar IEnumerator

public class Shield : MonoBehaviour
{
    private EnemyShooterRadioControllerScript owner;
    private int damageToPlayer;

    private SpriteRenderer spriteRenderer;
    private Collider2D shieldCollider;

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
            StartCoroutine(DeactivateShieldTemporarily());
        }
    }

    private IEnumerator DeactivateShieldTemporarily()
    {
        // Ocultar escudo
        spriteRenderer.enabled = false;
        shieldCollider.enabled = false;

        // Esperar 2 segundos
        yield return new WaitForSeconds(2.5f);

        // Volver a activar escudo
        spriteRenderer.enabled = true;
        shieldCollider.enabled = true;
    }
}
