using UnityEngine;

public class EnergyPickup : MonoBehaviour
{
    public int energyAmount = 35;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerShooting playerShooting = other.GetComponent<PlayerShooting>();
            if (playerShooting != null)
            {
                playerShooting.AddEnergy(energyAmount);
                Destroy(gameObject);
            }
        }
    }
}
