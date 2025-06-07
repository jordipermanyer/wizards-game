using UnityEngine;

public class GestorClones : MonoBehaviour
{
    [Header("Objetos a seguir (Clones)")]
    public GameObject clone1;
    public GameObject clone2;
    public GameObject clone3;

    [Header("Objeto a dropear")]
    public GameObject objetoADropear;

    private bool clone1Destroyed = false;
    private bool clone2Destroyed = false;
    private bool clone3Destroyed = false;

    private GameObject ultimoCloneVivo;

    private void Start()
    {
        // Al inicio, asignamos uno de los clones como el "último vivo"
        ActualizarUltimoCloneVivo();
    }

    private void Update()
    {
        // Verificar estado de cada clone
        if (clone1 == null && !clone1Destroyed)
        {
            clone1Destroyed = true;
            Debug.Log("Clone1 destruido");
            ActualizarUltimoCloneVivo();
        }

        if (clone2 == null && !clone2Destroyed)
        {
            clone2Destroyed = true;
            Debug.Log("Clone2 destruido");
            ActualizarUltimoCloneVivo();
        }

        if (clone3 == null && !clone3Destroyed)
        {
            clone3Destroyed = true;
            Debug.Log("Clone3 destruido");
            ActualizarUltimoCloneVivo();
        }

        // Si los tres clones están destruidos:
        if (clone1Destroyed && clone2Destroyed && clone3Destroyed)
        {
            DropearObjeto();
            Destroy(gameObject); // Destruye el GestorClones después del drop
        }
    }

    private void ActualizarUltimoCloneVivo()
    {
        // Esta función busca cuál es el último clone vivo
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
            ultimoCloneVivo = null; // Ya no queda ningún clone
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
                // Si por alguna razón no queda referencia, dropea en el centro del gestor
                posicionDrop = transform.position;
            }

            Instantiate(objetoADropear, posicionDrop, Quaternion.identity);
            Debug.Log("Objeto dropeado en: " + posicionDrop);
        }
        else
        {
            Debug.LogWarning("No se asignó un objeto a dropear en el GestorClones.");
        }
    }
}

