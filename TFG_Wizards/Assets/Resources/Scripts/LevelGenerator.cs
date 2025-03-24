using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public Transform roomGroup;

    public GameObject cellPrefab;
    public GameObject startPrefab;
    public GameObject bossPrefab;
    public GameObject rewardPrefab;
    public GameObject coinPrefab;

    public Vector2 cellSize = new Vector2(17, 9);
    public int gridWidth = 19;
    public int gridHeight = 19;
    public int maxRooms = 10;
    public int minRooms = 7;

    private bool placedSpecial = false;
    private int[,] floorPlan; // 0 empty 1 room
    private int floorPlanCount = 0;
    private Queue<Vector2Int> cellQueue;
    private List<Vector2Int> endRooms;
    private Vector2Int startRoom;
    private Vector2Int bossRoom;
    private Vector2 offset;

    public Room[,] rooms;

    void Start()
    {
        offset.x = gridWidth / 2 * cellSize.x + 0.5f;
        offset.y = gridHeight / 2 * cellSize.y + 0.5f;
        GenerateLevel();
    }
    public void GenerateLevel()
    {
        floorPlan = new int[gridWidth, gridHeight];
        cellQueue = new Queue<Vector2Int>();
        endRooms = new List<Vector2Int>();
        rooms = new Room[gridWidth, gridHeight];
        placedSpecial = false;

        // Start by spawning the initial room
        SpawnInitialRoom(new Vector2Int(gridWidth / 2, gridHeight / 2));

        // Generate the main floor plan
        while (cellQueue.Count > 0)
        {
            Vector2Int cell = cellQueue.Dequeue();
            bool created = false;

            // Attempt to visit adjacent cells
            if (cell.x > 0) created |= Visit(cell + Vector2Int.left);
            if (cell.x < gridWidth - 1) created |= Visit(cell + Vector2Int.right);
            if (cell.y > 0) created |= Visit(cell + Vector2Int.down);
            if (cell.y < gridHeight - 1) created |= Visit(cell + Vector2Int.up);

            if (!created)
            {
                endRooms.Add(cell);
            }
        }

        // Ensure minimum room count
        if (floorPlanCount < minRooms)
        {
            Restart(); // Retry if generation failed to meet the minimum criteria
            return;
        }

        // Place special rooms after main floor plan is complete
        PlaceSpecialRooms();

        CloseDoorsWithoutAdjacentRooms();
    }

    private void Restart()
    {
        foreach (Transform child in roomGroup)
        {
            Destroy(child.gameObject);
        }
        GenerateLevel();
    }


    private void SpawnInitialRoom(Vector2Int position)
    {
        startRoom = position;
        Visit(position, true);
    }

    private bool Visit(Vector2Int position, bool start = false)
    {
        if (floorPlan[position.x, position.y] != 0) return false; // Already visited
        if (NeighborCount(position) > 1) return false; // Avoid overcrowding/loops
        if (floorPlanCount >= maxRooms) return false;

        if (floorPlanCount > 0 && Random.value < 0.5f) return false; // Randomization

        floorPlan[position.x, position.y] = 1;
        floorPlanCount++;
        cellQueue.Enqueue(position);

        // Spawn a room and get its Room script
        Vector3 worldPosition = new Vector3(
            position.x * cellSize.x - offset.x,
            position.y * cellSize.y - offset.y,
            0
        );

        GameObject room;
        if (!start)
        {
            room = Instantiate(cellPrefab, worldPosition, Quaternion.identity, roomGroup);
        }
        else
        {
            room = Instantiate(startPrefab, worldPosition, Quaternion.identity, roomGroup);
        }

        // Assign the Room component to the array
        Room roomScript = room.GetComponent<Room>();
        if (roomScript != null)
        {
            rooms[position.x, position.y] = roomScript;
        }
        else
        {
            Debug.LogWarning($"No Room script found on the instantiated prefab at {position}.");
        }

        return true;
    }

    private int NeighborCount(Vector2Int position)
    {
        int count = 0;
        if (position.x > 0 && floorPlan[position.x - 1, position.y] == 1) count++;
        if (position.x < gridWidth - 1 && floorPlan[position.x + 1, position.y] == 1) count++;
        if (position.y > 0 && floorPlan[position.x, position.y - 1] == 1) count++;
        if (position.y < gridHeight - 1 && floorPlan[position.x, position.y + 1] == 1) count++;
        return count;
    }

    private void PlaceSpecialRooms()
    {
        if (endRooms.Count == 0) return;

        // Place Boss Room
        bossRoom = endRooms[endRooms.Count - 1];
        endRooms.RemoveAt(endRooms.Count - 1);
        SpawnRoom(bossRoom, bossPrefab);

        // Place Reward Room
        if (endRooms.Count > 0)
        {
            var rewardRoom = endRooms[endRooms.Count - 1];
            endRooms.RemoveAt(endRooms.Count - 1);
            SpawnRoom(rewardRoom, rewardPrefab);
        }

        // Place Coin Room
        if (endRooms.Count > 0)
        {
            var coinRoom = endRooms[endRooms.Count - 1];
            endRooms.RemoveAt(endRooms.Count - 1);
            SpawnRoom(coinRoom, coinPrefab);
        }
    }

    private void SpawnRoom(Vector2Int position, GameObject prefab)
    {
        Vector3 worldPosition = new Vector3(
            position.x * cellSize.x - offset.x,
            position.y * cellSize.y - offset.y,
            0
        );
        GameObject room = Instantiate(prefab, worldPosition, Quaternion.identity, roomGroup);

        Room roomScript = room.GetComponent<Room>();
        if (roomScript != null) rooms[position.x, position.y] = roomScript;
    }

    private void CloseDoorsWithoutAdjacentRooms()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Skip if there's no room at this position
                Room room = rooms[x, y];
                if (room == null) continue;

                // Check adjacent cells and close doors if there's no adjacent room
                if (x == 0 || rooms[x - 1, y] == null) room.RemoveDoor("left"); // Left
                if (x == gridWidth - 1 || rooms[x + 1, y] == null) room.RemoveDoor("right"); // Right
                if (y == 0 || rooms[x, y - 1] == null) room.RemoveDoor("down"); // Down
                if (y == gridHeight - 1 || rooms[x, y + 1] == null) room.RemoveDoor("up"); // Up
            }
        }
    }

}