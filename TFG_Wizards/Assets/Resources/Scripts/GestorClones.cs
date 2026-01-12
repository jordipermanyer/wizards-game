using UnityEngine;
using UnityEngine.UI;

public class GestorClones : MonoBehaviour
{
    [Header("Clones")]
    public GameObject clone1;
    public GameObject clone2;
    public GameObject clone3;

    [Header("Boss Health Bar (Total)")]
    public Slider healthBar;

    [Header("Drop Object")]
    public GameObject objetoADropear;

    private bool clone1Destroyed = false;
    private bool clone2Destroyed = false;
    private bool clone3Destroyed = false;

    private GameObject ultimoCloneVivo;

    private int maxTotalHp = 0;

    private void Start()
    {
        maxTotalHp = GetCloneMaxHp(clone1) + GetCloneMaxHp(clone2) + GetCloneMaxHp(clone3);

        if (healthBar != null)
        {
            healthBar.maxValue = maxTotalHp;
            healthBar.value = maxTotalHp;
        }

        ActualizarUltimoCloneVivo();
    }

    private void Update()
    {
        // Track destroyed clones
        if (clone1 == null && !clone1Destroyed)
        {
            clone1Destroyed = true;
            ActualizarUltimoCloneVivo();
        }

        if (clone2 == null && !clone2Destroyed)
        {
            clone2Destroyed = true;
            ActualizarUltimoCloneVivo();
        }

        if (clone3 == null && !clone3Destroyed)
        {
            clone3Destroyed = true;
            ActualizarUltimoCloneVivo();
        }

        // Update total HP bar
        if (healthBar != null)
        {
            int currentTotalHp = GetCloneCurrentHp(clone1) + GetCloneCurrentHp(clone2) + GetCloneCurrentHp(clone3);
            healthBar.value = Mathf.Clamp(currentTotalHp, 0, maxTotalHp);
        }

        // If all clones are destroyed
        if (clone1Destroyed && clone2Destroyed && clone3Destroyed)
        {
            DropearObjeto();
            Destroy(gameObject);
        }
    }

    private int GetCloneMaxHp(GameObject cloneObj)
    {
        if (cloneObj == null) return 0;

        EnemyShooterOriginalControllerScript c = cloneObj.GetComponent<EnemyShooterOriginalControllerScript>();
        if (c == null) return 0;

        return c.MaxHp;
    }

    private int GetCloneCurrentHp(GameObject cloneObj)
    {
        if (cloneObj == null) return 0;

        EnemyShooterOriginalControllerScript c = cloneObj.GetComponent<EnemyShooterOriginalControllerScript>();
        if (c == null) return 0;

        return Mathf.Max(0, c.CurrentHp);
    }

    private void ActualizarUltimoCloneVivo()
    {
        if (!clone1Destroyed && clone1 != null)
        {
            ultimoCloneVivo = clone1;
        }
        else if (!clone2Destroyed && clone2 != null)
        {
            ultimoCloneVivo = clone2;
        }
        else if (!clone3Destroyed && clone3 != null)
        {
            ultimoCloneVivo = clone3;
        }
        else
        {
            ultimoCloneVivo = null;
        }
    }

    private void DropearObjeto()
    {
        if (objetoADropear != null)
        {
            Vector3 posicionDrop = Vector3.zero;

            if (ultimoCloneVivo != null)
            {
                posicionDrop = ultimoCloneVivo.transform.position;
            }
            else
            {
                posicionDrop = transform.position;
            }

            Instantiate(objetoADropear, posicionDrop, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("GestorClones: objetoADropear not assigned.");
        }
    }
}
