using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 5; // Da�o que inflige la bala
    public float speed = 10f; // Velocidad de la bala
    public float maxLifeTime = 10f; // Tiempo m�ximo de vida antes de autodestrucci�n

    private Vector2 direction; // Direcci�n en la que viajar� la bala
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
            rb.velocity = direction * speed; // Asegura que la bala tenga direcci�n desde el inicio
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player") || collider.CompareTag("Pared"))
        {
            PlayerController player = collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Damage(damage); // Aplica da�o correctamente
            }

            Destroy(gameObject); // Se destruye la bala despu�s del impacto
        }
    }

    private void Update()
    {
        Destroy(gameObject, maxLifeTime);
    }
}
