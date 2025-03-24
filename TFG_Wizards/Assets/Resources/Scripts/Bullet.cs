using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 5; // Daño que inflige la bala (actualizado a 5)
    public float speed = 10f; // Velocidad de la bala
    public float maxLifeTime = 10f; // Tiempo máximo de vida antes de autodestrucción

    private Vector2 direction; // Dirección en la que viajará la bala
    private float lifeTime; // Tiempo transcurrido desde que la bala fue creada
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Bullet needs a Rigidbody2D to detect collisions properly.");
        }

        // Calcula la dirección inicial del movimiento
        rb.velocity = direction * speed;
    }

    private void Update()
    {
        // Incrementa el tiempo de vida
        lifeTime += Time.deltaTime;

        // Destruye la bala si supera el tiempo máximo de vida
        if (lifeTime >= maxLifeTime)
        {
            Destroy(gameObject);
        }
    }

    // Inicializa la bala con su dirección y daño
    public void Initialize(Vector2 dir, int dmg)
    {
        direction = dir.normalized; // Asegúrate de que la dirección sea un vector unitario
        damage = dmg;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si colisiona con el jugador, aplica daño
        if (collision.collider.CompareTag("Player"))
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Damage(damage); // Inflige 5 puntos de daño al jugador
            }
        }

        // Destruye la bala después de cualquier colisión
        Destroy(gameObject);
    }
}
