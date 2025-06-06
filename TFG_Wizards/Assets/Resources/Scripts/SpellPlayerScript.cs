using UnityEngine;

public class SpellPlayerScript : MonoBehaviour
{
    [Header("Spell Settings")]
    public float speed = 10f; // Velocidad de la bala
    public float lifeTime = 5f; // Duración de la bala antes de destruirse
    public int damage = 10; // Daño que inflige la bala

    private Vector3 direction; // Dirección en la que viaja la bala
    private Rigidbody2D rb; // Referencia al Rigidbody2D

    // Inicializa la bala con su dirección y daño
    public void Initialize(Vector3 dir, int dmg)
    {
        direction = dir.normalized; // Normalizar la dirección
        damage = Mathf.Max(dmg, 0); // Asegurarse de que el daño no sea negativo
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

        // Destruye la bala automáticamente después de que expire su duración
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        // Evita que la bala interactúe con el jugador
        if (collider.CompareTag("Player"))
        {
            return; // No hace nada si colisiona con el jugador
        }

        // Comprueba si la bala golpea a un enemigo o jefe
        if (collider.CompareTag("Enemy") || collider.CompareTag("Boss") || collider.CompareTag("Pared")) 
        {
            // Verifica si el objeto tiene uno de los scripts que manejan daño
            var enemyShooter = collider.GetComponent<EnemyShooterControllerScript>();
            var enemyMele = collider.GetComponent<EnemyMeleControllerScript>();
            var boss = collider.GetComponent<Boss>();
            var enemyShooterDroop = collider.GetComponent<EnemyShooterControllerDroopScript>();
            var enemyShooterTeleport = collider.GetComponent<EnemyShooterControllerTeleportScript>();
            var enemyMeleIce = collider.GetComponent<EnemyMeleControllerIceScript>();
            var enemyMeleAcid = collider.GetComponent<EnemyMeleControllerAcidScript>();
            var enemyClone = collider.GetComponent<EnemyShooterCloneControllerScript>();
            var enemyOriginal = collider.GetComponent<EnemyShooterOriginalControllerScript>();
            var enemyRadio = collider.GetComponent<EnemyShooterRadioControllerScript>();
            var Torreta = collider.GetComponent<Turret>();
            var Dragon = collider.GetComponent<EnemyDragonScript>();

            if (enemyShooter != null)
            {
                enemyShooter.Damage(damage);
            }
            else if (enemyMele != null)
            {
                enemyMele.Damage(damage);
            }
            else if (boss != null)
            {
                boss.Damage(damage);
            }
            else if (enemyShooterDroop != null)
            {
                enemyShooterDroop.Damage(damage);
            }
            else if (enemyShooterTeleport != null)
            {
                enemyShooterTeleport.Damage(damage);
            }
            else if (enemyMeleIce != null)
            {
                enemyMeleIce.Damage(damage);
            }
            else if (enemyMeleAcid != null)
            {
                enemyMeleAcid.Damage(damage);
            }
            else if (enemyClone != null)
            {
                enemyClone.Damage(damage);
            }
            else if (enemyOriginal != null)
            {
                enemyOriginal.Damage(damage);
            }
            else if (enemyRadio != null)
            {
                enemyRadio.Damage(damage);
            }
            else if (Torreta != null)
            {
                Torreta.Damage(damage);
            }
            else if (Dragon != null)
            {
                Dragon.Damage(damage);
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
