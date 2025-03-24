using UnityEngine;

public class SpellPlayerScript : MonoBehaviour
{
    [Header("Spell Settings")]
    public float speed = 10f; // Velocidad de la bala
    public float lifeTime = 5f; // Duraci�n de la bala antes de destruirse
    public int damage = 10; // Da�o que inflige la bala

    private Vector3 direction; // Direcci�n en la que viaja la bala
    private Rigidbody2D rb; // Referencia al Rigidbody2D

    // Inicializa la bala con su direcci�n y da�o
    public void Initialize(Vector3 dir, int dmg)
    {
        direction = dir.normalized; // Normalizar la direcci�n
        damage = Mathf.Max(dmg, 0); // Asegurarse de que el da�o no sea negativo
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("SpellPlayerScript necesita un Rigidbody2D para funcionar correctamente.");
        }
    }

    private void Start()
    {
        if (rb != null)
        {
            // Configurar la velocidad inicial
            rb.velocity = direction * speed;
        }

        // Destruye la bala autom�ticamente despu�s de que expire su duraci�n
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Evita que la bala interact�e con el jugador
        if (collider.CompareTag("Player"))
        {
            return; // No hace nada si colisiona con el jugador
        }

        // Comprueba si la bala golpea a un enemigo o jefe
        if (collider.CompareTag("Enemy") || collider.CompareTag("Boss"))
        {
            // Intenta obtener un script que tenga el m�todo Damage
            var damageable = collider.GetComponent<EnemyShooterControllerScript>();
            if (damageable != null)
            {
                damageable.Damage(damage); // Aplica da�o al enemigo o jefe
            }

            // Destruye la bala tras impactar
            Destroy(gameObject);
        }
        else if (collider.CompareTag("Wall") || collider.CompareTag("Untagged"))
        {
            // Destruye la bala si impacta con una pared u objeto sin etiqueta relevante
            Destroy(gameObject);
        }
    }
}
