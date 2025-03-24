using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 5; // Da�o que inflige la bala (actualizado a 5)
    public float speed = 10f; // Velocidad de la bala
    public float maxLifeTime = 10f; // Tiempo m�ximo de vida antes de autodestrucci�n

    private Vector2 direction; // Direcci�n en la que viajar� la bala
    private float lifeTime; // Tiempo transcurrido desde que la bala fue creada
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Bullet needs a Rigidbody2D to detect collisions properly.");
        }

        // Calcula la direcci�n inicial del movimiento
        rb.velocity = direction * speed;
    }

    private void Update()
    {
        // Incrementa el tiempo de vida
        lifeTime += Time.deltaTime;

        // Destruye la bala si supera el tiempo m�ximo de vida
        if (lifeTime >= maxLifeTime)
        {
            Destroy(gameObject);
        }
    }

    // Inicializa la bala con su direcci�n y da�o
    public void Initialize(Vector2 dir, int dmg)
    {
        direction = dir.normalized; // Aseg�rate de que la direcci�n sea un vector unitario
        damage = dmg;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si colisiona con el jugador, aplica da�o
        if (collision.collider.CompareTag("Player"))
        {
            PlayerController player = collision.collider.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Damage(damage); // Inflige 5 puntos de da�o al jugador
            }
        }

        // Destruye la bala despu�s de cualquier colisi�n
        Destroy(gameObject);
    }
}
