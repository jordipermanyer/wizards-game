using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestorClones : MonoBehaviour
{
    [Header("Boss Health Bar")]
    public Slider healthBar;

    [Header("Drop Object")]
    public GameObject objetoADropear;

    private List<ICloneBossUnit> clones = new List<ICloneBossUnit>();
    private int maxTotalHp = 0;
    private bool dropped = false;

    private void Start()
    {
        UpdateHealthBarMax();
        UpdateHealthBarValue();
    }

    private void Update()
    {
        CleanupNullClones();
        UpdateHealthBarValue();

        if (!dropped && AllClonesDead())
        {
            dropped = true;
            DropearObjeto();
            Destroy(gameObject);
        }
    }

    public void RegisterClone(ICloneBossUnit clone)
    {
        if (clone == null) return;
        if (clones.Contains(clone)) return;

        clones.Add(clone);
        maxTotalHp += Mathf.Max(0, clone.MaxHp);

        UpdateHealthBarMax();
        UpdateHealthBarValue();
    }

    private void UpdateHealthBarMax()
    {
        if (healthBar == null) return;

        int safeMax = Mathf.Max(1, maxTotalHp);
        healthBar.maxValue = safeMax;

        if (healthBar.value > safeMax)
            healthBar.value = safeMax;
    }

    private void UpdateHealthBarValue()
    {
        if (healthBar == null) return;

        int currentTotalHp = 0;

        for (int i = 0; i < clones.Count; i++)
        {
            ICloneBossUnit c = clones[i];
            if (c == null) continue;

            currentTotalHp += Mathf.Max(0, c.CurrentHp);
        }

        healthBar.value = Mathf.Clamp(currentTotalHp, 0, Mathf.Max(1, maxTotalHp));
    }

    private void CleanupNullClones()
    {
        for (int i = clones.Count - 1; i >= 0; i--)
        {
            if (clones[i] == null || clones[i].IsDestroyed)
                clones.RemoveAt(i);
        }
    }

    private bool AllClonesDead()
    {
        if (clones.Count == 0) return true;

        for (int i = 0; i < clones.Count; i++)
        {
            ICloneBossUnit c = clones[i];
            if (c == null) continue;

            if (c.CurrentHp > 0)
                return false;
        }

        return true;
    }

    private void DropearObjeto()
    {
        if (objetoADropear == null)
        {
            Debug.LogWarning("GestorClones: objetoADropear not assigned.");
            return;
        }

        Vector3 pos = transform.position;

        for (int i = 0; i < clones.Count; i++)
        {
            ICloneBossUnit c = clones[i];
            if (c != null && c.TransformRef != null)
            {
                pos = c.TransformRef.position;
                break;
            }
        }

        Instantiate(objetoADropear, pos, Quaternion.identity);
    }
}

public interface ICloneBossUnit
{
    int CurrentHp { get; }
    int MaxHp { get; }
    bool IsDestroyed { get; }
    Transform TransformRef { get; }
}
