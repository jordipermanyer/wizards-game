using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    [System.Serializable]
    public class Room
    {
        public GameObject prefab;
        public List<Vector2> doors;
    }

    public List<Room> roomPrefabs;
    public List<Room> coinRoomPrefabs;
    public List<Room> itemRoomPrefabs;
    public List<Room> enemyRoomPrefabs;
    public Room placeholderRoom;
    public Room startingRoom;
    public Room fallbackRoom;

    public int numberOfRooms = 5;
    public Vector2 roomSize = new Vector2(18, 10);
    public GameObject player;

    public int coinRoomCount = 1;
    public int itemRoomCount = 1;

    private Dictionary<Vector2, GameObject> placedObjects = new Dictionary<Vector2, GameObject>();
    private Dictionary<Vector2, Room> placedRooms = new Dictionary<Vector2, Room>();
    private List<Vector2> openPositions = new List<Vector2>();
    private System.Random rng = new System.Random();
    private Vector2 startingRoomPosition = Vector2.zero;

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        foreach (var obj in placedObjects.Values)
        {
            if (obj != null) Destroy(obj);
        }

        placedRooms.Clear();
        placedObjects.Clear();
        openPositions.Clear();

        GenerateBaseLayout();
        OptimizeRooms();
        PlaceEnemyRoom();
        TeleportPlayerToStartingRoom();
    }

    void GenerateBaseLayout()
    {
        startingRoomPosition = Vector2.zero;
        PlaceRoom(startingRoomPosition, startingRoom);

        foreach (Vector2 direction in startingRoom.doors)
        {
            Vector2 newPos = startingRoomPosition + new Vector2(direction.x * roomSize.x, direction.y * roomSize.y);
            PlaceRoom(newPos, placeholderRoom);
        }

        while (placedRooms.Count < numberOfRooms)
        {
            if (!ExpandDungeon()) break;
        }
    }

    void PlaceRoom(Vector2 position, Room room)
    {
        if (placedRooms.ContainsKey(position)) return;

        GameObject roomObj = Instantiate(room.prefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        placedRooms[position] = room;
        placedObjects[position] = roomObj;

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

        Vector2 position = openPositions[rng.Next(openPositions.Count)];
        openPositions.Remove(position);

        if (placedRooms.ContainsKey(position)) return true;

        PlaceRoom(position, placeholderRoom);
        return true;
    }

    void OptimizeRooms()
    {
        List<Vector2> allPositions = new List<Vector2>(placedRooms.Keys);

        // Excluir la sala inicial
        allPositions.Remove(startingRoomPosition);

        // Detectar la posición de la sala del enemigo
        Vector2 enemyRoomPos = Vector2.zero;
        float maxDistance = -1f;

        foreach (var pos in allPositions)
        {
            List<Vector2> requiredDoors = GetRequiredDoors(pos);
            if (requiredDoors.Count == 1)
            {
                float distance = Vector2.Distance(startingRoomPosition, pos);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    enemyRoomPos = pos;
                }
            }
        }

        allPositions.Remove(enemyRoomPos);

        // Mezclar posiciones disponibles
        allPositions = ShuffleList(allPositions);

        // Limitar las cantidades si exceden las disponibles
        int availableCount = allPositions.Count;
        int totalSpecialRooms = Mathf.Min(coinRoomCount + itemRoomCount, availableCount);
        int coinsToPlace = Mathf.Min(coinRoomCount, totalSpecialRooms);
        int itemsToPlace = Mathf.Min(itemRoomCount, totalSpecialRooms - coinsToPlace);

        int index = 0;

        // Colocar salas de monedas
        for (int i = 0; i < coinsToPlace; i++, index++)
        {
            Vector2 pos = allPositions[index];
            List<Vector2> requiredDoors = GetRequiredDoors(pos);
            Room selectedRoom = GetBestRoomFromList(coinRoomPrefabs, requiredDoors);
            if (selectedRoom != null) ReplaceRoom(pos, selectedRoom);
        }

        // Colocar salas de ítems
        for (int i = 0; i < itemsToPlace; i++, index++)
        {
            Vector2 pos = allPositions[index];
            List<Vector2> requiredDoors = GetRequiredDoors(pos);
            Room selectedRoom = GetBestRoomFromList(itemRoomPrefabs, requiredDoors);
            if (selectedRoom != null) ReplaceRoom(pos, selectedRoom);
        }

        // Resto de salas serán normales
        for (; index < allPositions.Count; index++)
        {
            Vector2 pos = allPositions[index];
            List<Vector2> requiredDoors = GetRequiredDoors(pos);
            Room selectedRoom = GetBestRoomFromList(roomPrefabs, requiredDoors);

            if (selectedRoom == null && fallbackRoom != null)
            {
                if (requiredDoors.TrueForAll(dir => fallbackRoom.doors.Contains(dir)))
                {
                    selectedRoom = fallbackRoom;
                }
            }

            if (selectedRoom != null) ReplaceRoom(pos, selectedRoom);
        }
    }

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

            Vector3 fallbackCenter = new Vector3(
                startingRoomPosition.x + roomSize.x / 2,
                startingRoomPosition.y + roomSize.y / 2,
                0
            );
            fallbackCenter.y -= 1.5f;
            player.transform.position = fallbackCenter;
        }
    }

    void PlaceEnemyRoom()
    {
        Vector2 farthestRoomPos = Vector2.zero;
        float maxDistance = -1;

        foreach (var pos in placedRooms.Keys)
        {
            if (pos == startingRoomPosition) continue;

            List<Vector2> requiredDoors = GetRequiredDoors(pos);
            if (requiredDoors.Count == 1)
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
