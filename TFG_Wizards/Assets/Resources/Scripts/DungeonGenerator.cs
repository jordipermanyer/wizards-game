using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    // Clase para representar una habitaci�n con su prefab y las direcciones de sus puertas
    [System.Serializable]
    public class Room
    {
        public GameObject prefab;
        public List<Vector2> doors; // Direcciones de las puertas (ej: (0,1) arriba, (0,-1) abajo, (1,0) derecha, (-1,0) izquierda)
    }

    // Listas de prefabs por tipo de habitaci�n
    public List<Room> roomPrefabs;         // Habitaciones normales
    public List<Room> coinRoomPrefabs;     // Habitaciones con monedas
    public List<Room> itemRoomPrefabs;     // Habitaciones con objetos
    public List<Room> enemyRoomPrefabs;    // Habitaciones de enemigo (solo una entrada)
    public Room placeholderRoom;           // Habitaci�n temporal mientras se genera el mapa
    public Room startingRoom;              // Habitaci�n inicial con 4 puertas
    public Room fallbackRoom;              // Habitaci�n de respaldo si no se encuentra otra adecuada
    public int numberOfRooms = 5;          // Total de habitaciones a generar (incluyendo inicial)
    public Vector2 roomSize = new Vector2(18, 10); // Tama�o de cada habitaci�n (ancho, alto)
    public GameObject player;              // Referencia al jugador

    // Diccionarios para almacenar habitaciones colocadas y sus GameObjects
    private Dictionary<Vector2, GameObject> placedObjects = new Dictionary<Vector2, GameObject>();
    private Dictionary<Vector2, Room> placedRooms = new Dictionary<Vector2, Room>();

    // Lista de posiciones que a�n pueden expandirse (conectar nuevas habitaciones)
    private List<Vector2> openPositions = new List<Vector2>();

    // Generador de n�meros aleatorios
    private System.Random rng = new System.Random();

    private Vector2 startingRoomPosition = Vector2.zero;

    void Start()
    {
        GenerateDungeon(); // Inicia la generaci�n al empezar el juego
    }

    void GenerateDungeon()
    {
        // Limpia habitaciones anteriores
        foreach (var obj in placedObjects.Values)
        {
            if (obj != null) Destroy(obj);
        }

        placedRooms.Clear();
        placedObjects.Clear();
        openPositions.Clear();

        // Generaci�n inicial
        GenerateBaseLayout();
        OptimizeRooms();
        PlaceEnemyRoom(); // Agrega sala de enemigo lejos de la inicial
        TeleportPlayerToStartingRoom();
    }

    void GenerateBaseLayout()
    {
        // Coloca la sala inicial en el centro
        startingRoomPosition = Vector2.zero;
        PlaceRoom(startingRoomPosition, startingRoom);

        // Coloca placeholder en cada direcci�n de puerta de la sala inicial
        foreach (Vector2 direction in startingRoom.doors)
        {
            Vector2 newPos = startingRoomPosition + new Vector2(direction.x * roomSize.x, direction.y * roomSize.y);
            PlaceRoom(newPos, placeholderRoom);
        }

        // Expande el mapa hasta alcanzar el n�mero deseado de habitaciones
        while (placedRooms.Count < numberOfRooms)
        {
            if (!ExpandDungeon()) break;
        }
    }

    void PlaceRoom(Vector2 position, Room room)
    {
        if (placedRooms.ContainsKey(position)) return;

        // Instancia el prefab y lo guarda
        GameObject roomObj = Instantiate(room.prefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        placedRooms[position] = room;
        placedObjects[position] = roomObj;

        // Calcula las posiciones vecinas que pueden ser puertas abiertas
        foreach (Vector2 door in room.doors)
        {
            Vector2 neighborPos = position + new Vector2(door.x * roomSize.x, door.y * roomSize.y);
            if (!placedRooms.ContainsKey(neighborPos) && !openPositions.Contains(neighborPos))
            {
                openPositions.Add(neighborPos);
            }
        }
    }

    bool ExpandDungeon()
    {
        if (openPositions.Count == 0) return false;

        // Selecciona una posici�n abierta al azar y la convierte en placeholder
        Vector2 position = openPositions[rng.Next(openPositions.Count)];
        openPositions.Remove(position);

        if (placedRooms.ContainsKey(position)) return true;

        PlaceRoom(position, placeholderRoom);
        return true;
    }

    void OptimizeRooms()
    {
        // Baraja las posiciones para que el orden sea aleatorio
        List<Vector2> positions = new List<Vector2>(placedRooms.Keys);
        positions = ShuffleList(positions);

        bool coinRoomPlaced = false;
        bool itemRoomPlaced = false;

        foreach (var position in positions)
        {
            if (position == startingRoomPosition) continue;

            List<Vector2> requiredDoors = GetRequiredDoors(position);
            Room selectedRoom = null;

            // Intenta poner una sala de monedas
            if (!coinRoomPlaced)
            {
                selectedRoom = GetBestRoomFromList(coinRoomPrefabs, requiredDoors);
                if (selectedRoom != null) coinRoomPlaced = true;
            }

            // Si no se pudo, intenta con una de �tem
            if (selectedRoom == null && !itemRoomPlaced)
            {
                selectedRoom = GetBestRoomFromList(itemRoomPrefabs, requiredDoors);
                if (selectedRoom != null) itemRoomPlaced = true;
            }

            // Si no se pudo, usa una sala normal
            if (selectedRoom == null)
            {
                selectedRoom = GetBestRoomFromList(roomPrefabs, requiredDoors);
            }

            // Si tampoco hay sala adecuada, usa la de respaldo
            if (selectedRoom == null && fallbackRoom != null)
            {
                if (requiredDoors.TrueForAll(dir => fallbackRoom.doors.Contains(dir)))
                {
                    selectedRoom = fallbackRoom;
                }
            }

            // Reemplaza la sala placeholder por la seleccionada
            if (selectedRoom != null)
            {
                ReplaceRoom(position, selectedRoom);
            }
        }
    }

    // Calcula qu� puertas se necesitan en una habitaci�n basadas en las salas vecinas
    List<Vector2> GetRequiredDoors(Vector2 position)
    {
        List<Vector2> requiredDoors = new List<Vector2>();

        foreach (var neighbor in placedRooms)
        {
            Vector2 neighborPos = neighbor.Key;
            Room neighborRoom = neighbor.Value;

            Vector2 direction = (neighborPos - position) / roomSize;
            if (neighborRoom.doors.Contains(-direction))
            {
                requiredDoors.Add(direction);
            }
        }

        return requiredDoors;
    }

    // Devuelve la mejor habitaci�n que cumpla con los requisitos de puertas (la que tenga menos puertas extra)
    Room GetBestRoomFromList(List<Room> roomList, List<Vector2> requiredDoors)
    {
        Room bestRoom = null;
        int fewestExtraDoors = int.MaxValue;

        foreach (Room room in roomList)
        {
            if (requiredDoors.TrueForAll(dir => room.doors.Contains(dir)))
            {
                int extra = room.doors.Count - requiredDoors.Count;
                if (extra < fewestExtraDoors)
                {
                    bestRoom = room;
                    fewestExtraDoors = extra;
                }
            }
        }

        return bestRoom;
    }

    // Reemplaza una sala ya colocada por una nueva
    void ReplaceRoom(Vector2 position, Room newRoom)
    {
        if (placedObjects.TryGetValue(position, out GameObject oldObj))
        {
            Destroy(oldObj);
        }

        GameObject newObj = Instantiate(newRoom.prefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        placedRooms[position] = newRoom;
        placedObjects[position] = newObj;
    }

    // Lleva al jugador al punto inicial de la sala de inicio (tag "Punto")
    void TeleportPlayerToStartingRoom()
    {
        if (player != null)
        {
            GameObject[] allRooms = GameObject.FindGameObjectsWithTag("Room");
            foreach (GameObject room in allRooms)
            {
                if (room.transform.position == new Vector3(startingRoomPosition.x, startingRoomPosition.y, 0))
                {
                    Transform[] children = room.GetComponentsInChildren<Transform>(true);
                    foreach (Transform child in children)
                    {
                        if (child.CompareTag("Punto"))
                        {
                            player.transform.position = child.position;
                            return;
                        }
                    }
                }
            }

            // Si no hay punto "Punto", usa el centro de la sala como fallback
            Vector3 fallbackCenter = new Vector3(
                startingRoomPosition.x + roomSize.x / 2,
                startingRoomPosition.y + roomSize.y / 2,
                0
            );
            fallbackCenter.y -= 1.5f;
            player.transform.position = fallbackCenter;
        }
    }

    // Coloca una sala de enemigo lo m�s lejos posible de la sala inicial
    void PlaceEnemyRoom()
    {
        Vector2 farthestRoomPos = Vector2.zero;
        float maxDistance = -1;

        foreach (var pos in placedRooms.Keys)
        {
            if (pos == startingRoomPosition) continue;

            List<Vector2> requiredDoors = GetRequiredDoors(pos);
            if (requiredDoors.Count == 1) // Solo una entrada
            {
                float distance = Vector2.Distance(startingRoomPosition, pos);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthestRoomPos = pos;
                }
            }
        }

        if (maxDistance != -1)
        {
            List<Vector2> requiredDoors = GetRequiredDoors(farthestRoomPos);
            if (requiredDoors.Count == 1)
            {
                Vector2 dir = requiredDoors[0];
                foreach (Room enemyRoom in enemyRoomPrefabs)
                {
                    if (enemyRoom.doors.Count == 1 && enemyRoom.doors[0] == dir)
                    {
                        ReplaceRoom(farthestRoomPos, enemyRoom);
                        return;
                    }
                }
            }
        }
    }

    // Mezcla una lista de posiciones para hacer la distribuci�n aleatoria
    List<Vector2> ShuffleList(List<Vector2> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            Vector2 temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
        return list;
    }
}
