using UnityEngine;

public class SpellPlayerSecondary : MonoBehaviour
{
    [Header("Spell Settings")]
    public float speed = 5f; // Más lento que el primario
    public float lifeTime = 5f;
    public int damage = 50;

    private Vector3 direction;
    private Rigidbody2D rb;

    public void Initialize(Vector3 dir, int dmg)
    {
        direction = dir.normalized;
        damage = Mathf.Max(dmg, 0);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            return;
        }

        if (collider.CompareTag("Enemy") || collider.CompareTag("Boss") || collider.CompareTag("Pared"))
        {
            var enemy = collider.GetComponent<EnemyShooterControllerScript>();
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

            if (enemy != null) enemy.Damage(damage);
            if (enemyMele != null) enemyMele.Damage(damage);
            if (boss != null) boss.Damage(damage);
            if (enemyShooterDroop != null) enemyShooterDroop.Damage(damage);
            if (enemyShooterTeleport != null) enemyShooterTeleport.Damage(damage);
            if (enemyMeleIce != null) enemyMeleIce.Damage(damage);
            if (enemyMeleAcid != null) enemyMeleAcid.Damage(damage);
            if (enemyClone != null) enemyClone.Damage(damage);
            if (enemyOriginal != null) enemyOriginal.Damage(damage);
            if (enemyRadio != null) enemyRadio.Damage(damage);
            if (Torreta != null) Torreta.Damage(damage);
            if (Dragon != null) Dragon.Damage(damage);


            Destroy(gameObject);
        }
        else if (collider.CompareTag("Wall") || collider.CompareTag("Untagged"))
        {
            Destroy(gameObject);
        }
    }
}
