using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 5; // Daño que inflige la bala
    public float speed = 10f; // Velocidad de la bala
    public float maxLifeTime = 10f; // Tiempo máximo de vida antes de autodestrucción

    private Vector2 direction; // Dirección en la que viajará la bala
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Bullet needs a Rigidbody2D to function properly.");
            return;
        }

        rb.velocity = direction * speed;
    }

    public void Initialize(Vector2 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;

        if (rb != null)
        {
            rb.velocity = direction * speed; // Asegura que la bala tenga dirección desde el inicio
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") || collider.CompareTag("Pared"))
        {
            PlayerController player = collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Damage(damage); // Aplica daño correctamente
            }

            Destroy(gameObject); // Se destruye la bala después del impacto
        }
    }

    private void Update()
    {
        Destroy(gameObject, maxLifeTime);
    }
}
