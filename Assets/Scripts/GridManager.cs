using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    #region Singleton
    private static GridManager _instance;
    public static GridManager Instance { get { return _instance; } }

    [SerializeField] private GameObject raisableWallPrefab;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        foreach (var pos in walkableTilemap.cellBounds.allPositionsWithin)
        {
            // Do something with each position
            gridMap[pos] = new Node { position = pos, type = TileType.Walkable };
            if (wallTilemap.HasTile(pos))
            {
                gridMap[pos] = new Node { position = pos, type = TileType.Wall };
            }
        }
    }
    #endregion

    [SerializeField] Tilemap walkableTilemap;
    [SerializeField] Tilemap wallTilemap;
    [SerializeField] Grid grid;

    Dictionary<Vector3Int, Node> gridMap = new(); //true for walkable, false for wall
    Dictionary<Vector3Int, bool> hasItems = new();

    public Grid GetMainGrid() => grid;
    public Dictionary<Vector3Int, Node> GetGrid()
    {
        return gridMap;
    }

    void OnEnable()
    {
        SpecialTile.OnSpecialTileInstantiated += HandleSpecialTileInstantiated;
        SpecialTile.OnSpecialTileInteracted += HandleSpecialTileInteracted;
        Item.OnItemInstantiated += HandleItemInstantiated;
        Item.OnItemInteracted += HandleItemInteracted;
    }

    void OnDisable()
    {
        SpecialTile.OnSpecialTileInstantiated -= HandleSpecialTileInstantiated;
        SpecialTile.OnSpecialTileInteracted -= HandleSpecialTileInteracted;
        Item.OnItemInstantiated -= HandleItemInstantiated;
        Item.OnItemInteracted -= HandleItemInteracted;
    }

    public Path FindPathFromWorld(Vector3 startWorldPos, Vector2Int direction)
    {
        Vector3Int startCellPos = grid.WorldToCell(startWorldPos);
        return FindPathFromCell(startCellPos, direction);
    }

    public Path FindPathFromCell(Vector3Int startCellPos, Vector2Int direction)
    {
        var result = new Path
        {
            stepLength = grid.transform.localScale.x
        };
        Vector3Int toCellPos;
        float stopTime = 0;
        var cachedStartCellPos = startCellPos;

        while (true)
        {
            stopTime = 0; // reset stopTime before checking each cell
            toCellPos = startCellPos + (Vector3Int)direction;
            if (toCellPos == cachedStartCellPos)
                break;
            if (!IsWalkable(startCellPos, toCellPos, direction, ref stopTime))
                break;

            result.directions.Add(new NodeData(direction, stopTime));

            if (gridMap[toCellPos].type == TileType.Slime) // slime stops movement
                break;

            var prevDirection = -direction;
            var fourDirections = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var countPossibleDirections = 0;
            foreach (var dir in fourDirections)
            {
                if (dir == prevDirection)
                    continue;

                var nextCellPos = toCellPos + (Vector3Int)dir;
                if (IsWalkable(toCellPos, nextCellPos, dir, ref stopTime))
                {
                    countPossibleDirections++;
                    direction = dir;
                }
            }

            if (countPossibleDirections != 1)
                break;
            startCellPos = toCellPos;
        }
        return result;
    }

    public bool IsWalkable(Vector3Int fromCellPos, Vector3Int toCellPos, Vector2 direction, ref float stopTime)
    {
        if (!gridMap.ContainsKey(toCellPos))
            return false;

        if (!gridMap.ContainsKey(fromCellPos))
            return false;

        // Check if the toCellPos is a ladder
        if (gridMap[toCellPos].specialTile != null && gridMap[toCellPos].specialTile.Type == TileType.Ladder)
        {
            var ladder = gridMap[toCellPos].specialTile as Ladder;
            Debug.Log("Ladder: " + ladder.CanGoIn(direction).ToString() + " direction: " + direction.ToString());
            if (ladder.CanGoIn(direction))
                return true;
            else
                return false;
        }

        // Check if the fromCellPos is a ladder
        if (gridMap[fromCellPos].specialTile != null && gridMap[fromCellPos].specialTile.Type == TileType.Ladder)
        {
            var ladder = gridMap[fromCellPos].specialTile as Ladder;
            if (ladder.CanGoIn(direction))
                return true;
            else
                return false;
        }

        // Check if the toCellPos and fromCellPos are walkable or wall,
        // if one of them is walkable and the other is wall, return false
        TileType toTileType = gridMap[toCellPos].type;
        TileType fromTileType = gridMap[fromCellPos].type;

        if ((toTileType == TileType.Walkable && fromTileType == TileType.Wall) ||
            (toTileType == TileType.Wall && fromTileType == TileType.Walkable) ||
            (toTileType == TileType.Wall && fromTileType == TileType.Slime))
            return false;

        // Check if the toCellPos is a one way door
        if (gridMap[toCellPos].specialTile != null && gridMap[toCellPos].specialTile.Type == TileType.OneWayDoor)
        {
            var oneWayDoor = gridMap[toCellPos].specialTile as OneWayDoor;
            if (oneWayDoor.CanGoThrough(direction))
            {
                stopTime = oneWayDoor.StopTime;
                return true;
            }
            else
                return false;
        }

        return true;
    }

    public bool IsWalkable(Vector3Int cellPos)
    {
        return gridMap.ContainsKey(cellPos) && gridMap[cellPos].type != TileType.Wall;
    }

    public bool IsItem(Vector3Int cellPos)
    {
        return gridMap.ContainsKey(cellPos) && hasItems.ContainsKey(cellPos);
    }

    public bool HasNode(Vector3Int cellPos)
    {
        return gridMap.ContainsKey(cellPos);
    }

    public bool TrySetNodeType(Vector3Int cellPos, TileType type, SpecialTile specialTile = null)
    {
        if (!gridMap.TryGetValue(cellPos, out var node))
            return false;

        node.type = type;
        node.specialTile = specialTile;
        return true;
    }

    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        return grid.WorldToCell(worldPos);
    }

    public Vector3 CellToWorld(Vector3Int cellPos)
    {
        return grid.CellToWorld(cellPos);
    }

    public Vector3 GetCellCenteredWorldPosition(Vector3Int cellPos)
    {
        return grid.GetCellCenterWorld(cellPos);
    }

    public Vector3 GetCellCenterWorld(Vector3Int cellPos)
    {
        return grid.GetCellCenterWorld(cellPos);
    }

    private void HandleSpecialTileInstantiated(SpecialTile tile)
    {
        //gridMap[WorldToCell(tile.transform.position)].type = tile.Type;
        gridMap[WorldToCell(tile.transform.position)].specialTile = tile;
    }

    private void HandleItemInstantiated(Item item)
    {
        Vector3Int cellPos = WorldToCell(item.transform.position);
        hasItems[cellPos] = true;
    }

    private void HandleItemInteracted(Item item)
    {
        Vector3Int cellPos = WorldToCell(item.transform.position);
        if (hasItems.ContainsKey(cellPos))
        {
            hasItems.Remove(cellPos);
        }
    }

    public bool IsNodeInteractable(Vector3Int cellPos)
    {
        return gridMap.ContainsKey(cellPos);
    }

    private void HandleSpecialTileInteracted(SpecialTile tile)
    {
        if (tile.Type == TileType.Rock)
        {
            Vector3Int cellPos = wallTilemap.WorldToCell(tile.transform.position);
            gridMap[cellPos].type = TileType.Wall;
        }
    }

    // Hàm này dùng để debug loại tile và special tile khi click chu?t, có thể bỏ qua nếu không cần
    //private void Update()
    //{
    //    // 1. Ki?m tra click chu?t theo New Input System (t??ng ???ng GetMouseButtonDown)
    //    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
    //    {
    //        // 2. L?y v? trí chu?t trên màn hình theo New Input System (t??ng ???ng Input.mousePosition)
    //        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

    //        // 3. Chuy?n ??i sang t?a ?? th? gi?i thông qua Camera
    //        // T?o m?t Vector3 t?m th?i v?i Z phù h?p ?? Camera.ScreenToWorldPoint tính toán ?úng
    //        Vector3 screenPosWithZ = new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(Camera.main.transform.position.z));
    //        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPosWithZ);

    //        // 4. D?ch t?a ?? th? gi?i sang t?a ?? ô l??i (Cell Position)
    //        Vector3Int cellPos = WorldToCell(mouseWorldPos);

    //        // 5. Ki?m tra d? li?u trong gridMap c?a b?n
    //        if (gridMap != null && gridMap.TryGetValue(cellPos, out var cell))
    //        {
    //            string specialType = cell.specialTile != null ? cell.specialTile.Type.ToString() : "None";
    //            Debug.Log($"[CLICK] Ô vuông: {cell.position} | Lo?i ??t: {cell.type} | ??c bi?t: {specialType}");
    //        }
    //        else
    //        {
    //            Debug.LogWarning($"B?n v?a click vào ô {cellPos}, nh?ng ô này không n?m trong d? li?u gridMap!");
    //        }
    //    }
    //}
    public void SetNodeType(Vector3Int cellPos, TileType type)
    {
        TrySetNodeType(cellPos, type);
    }

    public void SetNodeTile(Vector3Int cellPos, SpecialTile tile)
    {
        if (gridMap.ContainsKey(cellPos))
        {
            gridMap[cellPos].specialTile = tile;
        }
    }

    public void RaiseWall(Vector3Int cellPos)
    {
        if (gridMap.ContainsKey(cellPos) && gridMap[cellPos].type != TileType.Wall)
        {
            // if currently slime, remove slime first
            if (gridMap[cellPos].type == TileType.Slime)
            {
                var slimeTile = gridMap[cellPos].specialTile as Slime;
                if (slimeTile != null)
                {
                    Destroy(slimeTile.gameObject);
                }
            }
            // Instantiate a raisable wall at the given cell position

            var worldPos = GetCellCenteredWorldPosition(cellPos);
            var wallObj = Instantiate(raisableWallPrefab, worldPos, Quaternion.identity, grid.transform);
            var wallTile = wallObj.GetComponent<RaisableWall>();
            if (wallTile != null)
            {
                wallTile.Raise();
                gridMap[cellPos].type = TileType.Wall;
                gridMap[cellPos].specialTile = wallTile;
            }
        }
    }

    public void LowerWall(Vector3Int cellPos)
    {
        if (gridMap.ContainsKey(cellPos) && gridMap[cellPos].type == TileType.Wall && gridMap[cellPos].specialTile != null)
        {
            var wallTile = gridMap[cellPos].specialTile.GetComponent<RaisableWall>();
            if (wallTile != null)
            {
                gridMap[cellPos].type = TileType.Walkable;
                wallTile.Lower();
                // grid map will set to walkable after wall finishes lowering
            }
        }
    }

    List<Vector3Int> walkableCells = new List<Vector3Int>();
    public Vector3 GetRandomWalkableCellPosition()
    {
        if (walkableCells.Count == 0)
            foreach (var kvp in gridMap)
            {
                if (kvp.Value.type == TileType.Walkable)
                {
                    walkableCells.Add(kvp.Key);
                }
            }

        if (walkableCells.Count == 0)
        {
            Debug.LogWarning("No walkable cells found!");
            return Vector3.zero;
        }

        int randomIndex = Random.Range(0, walkableCells.Count);
        return CellToWorld(walkableCells[randomIndex]);
    }
    public GameObject CreateSpecialTile(Vector3Int cellPos, GameObject tilePrefab)
    {
        var worldPos = GetCellCenteredWorldPosition(cellPos);
        var tileObj = Instantiate(tilePrefab, worldPos, Quaternion.identity, grid.transform);
        var specialTile = tileObj.GetComponent<SpecialTile>();
        var warningTile = tileObj.GetComponent<WarningTile>();
        if (specialTile != null && !specialTile.isEffect)
        {
            gridMap[cellPos].type = specialTile.Type;
            gridMap[cellPos].specialTile = specialTile;
        }
        return tileObj;
    }

    public void RemoveSpecialTile(Vector3Int cellPos)
    {
        if (gridMap.ContainsKey(cellPos) && gridMap[cellPos].specialTile != null)
        {
            Destroy(gridMap[cellPos].specialTile.gameObject);
            gridMap[cellPos].type = TileType.Walkable;
            gridMap[cellPos].specialTile = null;
        }
    }

    public void MakeEffectAtCell(Vector3Int cellPos)
    {
        var node = gridMap.TryGetValue(cellPos, out var n) ? n : null;
        if (n == null)
            return;

        if (node.specialTile != null && node.specialTile.Type == TileType.OneWayDoor)
        {
            var oneWayDoor = node.specialTile as OneWayDoor;
            if (oneWayDoor != null)
            {
                oneWayDoor.Open();
            }
        }    
    }    
}
