using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    [Header("Doors")]
    public Door doorUp;
    public Door doorDown;
    public Door doorLeft;
    public Door doorRight;

    [Header("Tilemap Settings")]
    public Tilemap tilemap; // Tilemap asociado a la sala
    public TileBase wallTile; // Tile para reemplazar las puertas con paredes

    [Header("Door Positions")]
    public Vector2Int doorUpPosition = new Vector2Int(0, 4);    // Centro superior
    public Vector2Int doorDownPosition = new Vector2Int(0, -4); // Centro inferior
    public Vector2Int doorLeftPosition = new Vector2Int(-7, 0); // Centro izquierdo
    public Vector2Int doorRightPosition = new Vector2Int(7, 0); // Centro derecho

    private Dictionary<string, (Door door, Vector2Int position)> doorData;

    private void Awake()
    {
        InitializeDoorData();
    }

    private void InitializeDoorData()
    {
        // Mapear las direcciones a las puertas y sus posiciones
        doorData = new Dictionary<string, (Door, Vector2Int)>
        {
            { "up", (doorUp, doorUpPosition) },
            { "down", (doorDown, doorDownPosition) },
            { "left", (doorLeft, doorLeftPosition) },
            { "right", (doorRight, doorRightPosition) }
        };

        // Validar configuración de puertas
        ValidateDoors();
    }

    private void ValidateDoors()
    {
        foreach (var entry in doorData)
        {
            if (entry.Value.door == null)
            {
                Debug.LogWarning($"The door in direction '{entry.Key}' is not assigned in room '{gameObject.name}'");
            }
        }
    }

    public void RemoveDoor(string direction)
    {
        direction = direction.ToLower();

        if (!doorData.ContainsKey(direction))
        {
            Debug.LogWarning($"Invalid direction '{direction}' specified. Use 'up', 'down', 'left', or 'right'.");
            return;
        }

        var (door, position) = doorData[direction];

        if (door != null)
        {
            // Desactivar el objeto de la puerta
            door.gameObject.SetActive(false);

            // Actualizar el tilemap para reemplazar la puerta con un muro
            UpdateTile(position);
        }
    }

    private void UpdateTile(Vector2Int relativePosition)
    {
        if (tilemap == null)
        {
            Debug.LogWarning("Tilemap is not assigned!");
            return;
        }

        // Calcular la posición absoluta de la puerta en el tilemap
        Vector3Int tilePosition = tilemap.WorldToCell(transform.position + new Vector3(relativePosition.x, relativePosition.y, 0));

        // Reemplazar el tile de la puerta con un muro
        tilemap.SetTile(tilePosition, wallTile);
    }

    public void ConfigureTilemapPhysics()
    {
        if (tilemap == null)
        {
            Debug.LogWarning("Tilemap is not assigned!");
            return;
        }

        // Asegurarse de que el Tilemap Collider esté configurado correctamente
        var tilemapCollider = tilemap.GetComponent<TilemapCollider2D>();
        if (tilemapCollider == null)
        {
            tilemapCollider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
        }

        // Opcional: Configurar el Composite Collider si es necesario
        var rigidbody = tilemap.GetComponent<Rigidbody2D>();
        if (rigidbody == null)
        {
            rigidbody = tilemap.gameObject.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Static;
        }
    }
}
