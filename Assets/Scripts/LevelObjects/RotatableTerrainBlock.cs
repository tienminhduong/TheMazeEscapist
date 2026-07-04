using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class RotatableTerrainBlock : MonoBehaviour
{
    [SerializeField] private Transform rotatingRoot;
    [SerializeField] private Transform pivot;
    [SerializeField] private Tilemap rotatableTilemap;
    [SerializeField] private Tilemap carriedAreaTilemap;
    [SerializeField] private float rotationDuration = 0.35f;
    [SerializeField] private bool updateGridBeforeAnimation = true;
    [SerializeField] private bool registerInitialWallsOnStart = true;
    [SerializeField] private bool useTilemapBoundsCenterAsPivot = true;
    [SerializeField] private bool requirePlayerOnPivotWhenInsideTerrain = false;
    [SerializeField] private bool carryPlayerStandingOnTerrain = true;
    [SerializeField] private bool rotateCarriedAreaTilemap = true;
    [SerializeField] private PlayerController playerRequiredForRotation;

    private readonly HashSet<Vector3Int> currentBlockingCells = new();
    private bool hasCachedTilemapPivot;
    private Vector3 cachedTilemapPivotCell;
    private Vector3 cachedTilemapPivotWorld;

    public event UnityAction<RotatableTerrainBlock> RotationStarted;
    public event UnityAction<RotatableTerrainBlock> RotationFinished;

    public bool IsRotating { get; private set; }

    private void Reset()
    {
        rotatableTilemap = GetComponent<Tilemap>();
        carriedAreaTilemap = FindChildTilemapByName("Walkable");
    }

    private void Awake()
    {
        if (rotatableTilemap == null)
            rotatableTilemap = GetComponent<Tilemap>() ?? FindChildTilemapByName("Wall");

        if (carriedAreaTilemap == null)
            carriedAreaTilemap = FindChildTilemapByName("Walkable");

        if (rotatingRoot == null)
            rotatingRoot = transform;

        if (pivot == null)
            pivot = transform;
    }

    private void Start()
    {
        CacheCurrentBlockingCells();

        if (registerInitialWallsOnStart)
            SetCellsType(currentBlockingCells, TileType.Wall);
    }

    public bool TryRotateClockwise()
    {
        return TryRotateClockwiseTurns(1);
    }

    public bool TryRotateCounterClockwise()
    {
        return TryRotateClockwiseTurns(-1);
    }

    public bool TryRotateClockwiseTurns(int quarterTurns)
    {
        var normalizedTurns = NormalizeQuarterTurns(quarterTurns);
        if (IsRotating || normalizedTurns == 0)
            return false;

        if (rotatableTilemap == null)
        {
            Debug.LogWarning($"{nameof(RotatableTerrainBlock)} needs a Tilemap assigned.", this);
            return false;
        }

        var nextBlockingCells = GetRotatedBlockingCells(normalizedTurns);
        if (!CanRotateWithPlayer(normalizedTurns, nextBlockingCells, out var playerRotationData))
            return false;

        StartCoroutine(RotateRoutine(normalizedTurns, nextBlockingCells, playerRotationData));
        return true;
    }

    private bool CanRotateWithPlayer(int clockwiseQuarterTurns, HashSet<Vector3Int> nextBlockingCells, out PlayerRotationData playerRotationData)
    {
        playerRotationData = default;

        if (!TryGetPlayerCell(out var player, out var playerCell))
            return true;

        if (carryPlayerStandingOnTerrain && IsCellOnCarriedTerrain(playerCell))
        {
            var targetCell = RotateCellClockwise(playerCell, GetPivotCell(), clockwiseQuarterTurns);
            if (GridManager.Instance == null || !GridManager.Instance.HasNode(targetCell))
                return false;

            if (nextBlockingCells.Contains(targetCell))
                return false;

            var targetWorldPosition = GridManager.Instance.GetCellCenterWorld(targetCell);
            targetWorldPosition.z = player.transform.position.z;

            playerRotationData = new PlayerRotationData
            {
                ShouldCarry = true,
                Player = player,
                StartRotation = player.transform.rotation,
                TargetWorldPosition = targetWorldPosition
            };

            return true;
        }

        if (requirePlayerOnPivotWhenInsideTerrain && IsCellInsideCurrentTerrainBounds(playerCell) && playerCell != GetPivotGridCell())
            return false;

        return !nextBlockingCells.Contains(playerCell);
    }

    private bool IsCellInsideCurrentTerrainBounds(Vector3Int cellPos)
    {
        if (currentBlockingCells.Count == 0)
            return false;

        var minX = int.MaxValue;
        var maxX = int.MinValue;
        var minY = int.MaxValue;
        var maxY = int.MinValue;
        var minZ = int.MaxValue;
        var maxZ = int.MinValue;

        foreach (var blockingCell in currentBlockingCells)
        {
            minX = Mathf.Min(minX, blockingCell.x);
            maxX = Mathf.Max(maxX, blockingCell.x);
            minY = Mathf.Min(minY, blockingCell.y);
            maxY = Mathf.Max(maxY, blockingCell.y);
            minZ = Mathf.Min(minZ, blockingCell.z);
            maxZ = Mathf.Max(maxZ, blockingCell.z);
        }

        return cellPos.x >= minX && cellPos.x <= maxX
            && cellPos.y >= minY && cellPos.y <= maxY
            && cellPos.z >= minZ && cellPos.z <= maxZ;
    }

    private bool TryGetPlayerCell(out PlayerController player, out Vector3Int playerCell)
    {
        player = null;
        playerCell = default;

        if (GridManager.Instance == null)
            return false;

        if (playerRequiredForRotation == null)
            playerRequiredForRotation = FindFirstObjectByType<PlayerController>();

        if (playerRequiredForRotation == null)
            return false;

        player = playerRequiredForRotation;
        playerCell = GridManager.Instance.WorldToCell(playerRequiredForRotation.transform.position);
        return true;
    }

    private Vector3Int GetPivotGridCell()
    {
        var pivotCell = GetPivotCell();
        return new Vector3Int(
            Mathf.RoundToInt(pivotCell.x),
            Mathf.RoundToInt(pivotCell.y),
            Mathf.RoundToInt(pivotCell.z));
    }

    private IEnumerator RotateRoutine(int clockwiseQuarterTurns, HashSet<Vector3Int> nextBlockingCells, PlayerRotationData playerRotationData)
    {
        IsRotating = true;
        RotationStarted?.Invoke(this);

        var tilemapRotations = CollectTilemapRotations();
        if (updateGridBeforeAnimation)
            ApplyGridTransition(nextBlockingCells);

        yield return AnimateTilemapRotationAndBake(tilemapRotations, clockwiseQuarterTurns, playerRotationData);

        if (!updateGridBeforeAnimation)
            ApplyGridTransition(nextBlockingCells);

        CacheCurrentBlockingCells();
        IsRotating = false;
        RotationFinished?.Invoke(this);
    }

    private void CacheCurrentBlockingCells()
    {
        currentBlockingCells.Clear();

        if (GridManager.Instance == null)
        {
            Debug.LogWarning($"{nameof(RotatableTerrainBlock)} needs a GridManager in the scene.", this);
            return;
        }

        if (rotatableTilemap != null)
        {
            var tileSnapshots = CollectTilemapSnapshots(rotatableTilemap);
            CacheTilemapPivotIfNeeded();

            foreach (var tileSnapshot in tileSnapshots)
                currentBlockingCells.Add(tileSnapshot.GridCell);

            return;
        }
    }

    private HashSet<Vector3Int> GetRotatedBlockingCells(int clockwiseQuarterTurns)
    {
        var result = new HashSet<Vector3Int>();

        if (GridManager.Instance == null)
            return result;

        var pivotCell = GetPivotCell();
        foreach (var cellPos in currentBlockingCells)
            result.Add(RotateCellClockwise(cellPos, pivotCell, clockwiseQuarterTurns));

        return result;
    }

    private void ApplyGridTransition(HashSet<Vector3Int> nextBlockingCells)
    {
        foreach (var cellPos in currentBlockingCells)
        {
            if (!nextBlockingCells.Contains(cellPos))
                TrySetGridCell(cellPos, TileType.Walkable);
        }

        SetCellsType(nextBlockingCells, TileType.Wall);
        currentBlockingCells.Clear();

        foreach (var cellPos in nextBlockingCells)
            currentBlockingCells.Add(cellPos);
    }

    private void SetCellsType(IEnumerable<Vector3Int> cellPositions, TileType type)
    {
        foreach (var cellPos in cellPositions)
            TrySetGridCell(cellPos, type);
    }

    private bool TrySetGridCell(Vector3Int cellPos, TileType type)
    {
        if (GridManager.Instance == null)
            return false;

        var didSet = GridManager.Instance.TrySetNodeType(cellPos, type);
        if (!didSet)
            Debug.LogWarning($"{nameof(RotatableTerrainBlock)} tried to set {cellPos} to {type}, but the cell is not in GridManager.", this);

        return didSet;
    }

    private IEnumerator AnimateTilemapRotationAndBake(List<TilemapRotation> tilemapRotations, int clockwiseQuarterTurns, PlayerRotationData playerRotationData)
    {
        if ((tilemapRotations == null || tilemapRotations.Count == 0) && !playerRotationData.ShouldCarry)
            yield break;

        var root = rotatingRoot != null ? rotatingRoot : rotatableTilemap.transform;
        var startPosition = root.position;
        var startRotation = root.rotation;
        var pivotWorld = GetPivotWorldPosition();
        var targetAngle = -90f * clockwiseQuarterTurns;

        if (rotationDuration > 0f)
        {
            var elapsed = 0f;
            var previousAngle = 0f;

            while (elapsed < rotationDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / rotationDuration);
                t = t * t * (3f - 2f * t);
                var angle = Mathf.Lerp(0f, targetAngle, t);
                RotateVisualsAndPlayer(root, pivotWorld, angle - previousAngle, playerRotationData);
                previousAngle = angle;

                yield return null;
            }

            RotateVisualsAndPlayer(root, pivotWorld, targetAngle - previousAngle, playerRotationData);
        }

        root.SetPositionAndRotation(startPosition, startRotation);

        if (playerRotationData.ShouldCarry && playerRotationData.Player != null)
            playerRotationData.Player.transform.SetPositionAndRotation(playerRotationData.TargetWorldPosition, playerRotationData.StartRotation);

        foreach (var tilemapRotation in tilemapRotations)
            ApplyTilemapRotation(tilemapRotation.Tilemap, tilemapRotation.TileSnapshots, clockwiseQuarterTurns);
    }

    private void RotateVisualsAndPlayer(Transform root, Vector3 pivotWorld, float deltaAngle, PlayerRotationData playerRotationData)
    {
        root.RotateAround(pivotWorld, Vector3.forward, deltaAngle);

        if (!playerRotationData.ShouldCarry || playerRotationData.Player == null)
            return;

        var playerTransform = playerRotationData.Player.transform;
        playerTransform.RotateAround(pivotWorld, Vector3.forward, deltaAngle);
        playerTransform.rotation = playerRotationData.StartRotation;
    }

    private void ApplyTilemapRotation(Tilemap tilemap, List<TileSnapshot> tileSnapshots, int clockwiseQuarterTurns)
    {
        if (tilemap == null || tileSnapshots == null || tileSnapshots.Count == 0)
            return;

        var pivotCell = GetPivotCell();

        foreach (var tileSnapshot in tileSnapshots)
            tilemap.SetTile(tileSnapshot.TilemapCell, null);

        foreach (var tileSnapshot in tileSnapshots)
        {
            var rotatedGridCell = RotateCellClockwise(tileSnapshot.GridCell, pivotCell, clockwiseQuarterTurns);
            var targetTilemapCell = GridCellToTilemapCell(tilemap, rotatedGridCell);

            tilemap.SetTile(targetTilemapCell, tileSnapshot.Tile);
            tilemap.SetTileFlags(targetTilemapCell, TileFlags.None);
            tilemap.SetColor(targetTilemapCell, tileSnapshot.Color);
            tilemap.SetTransformMatrix(targetTilemapCell, tileSnapshot.TransformMatrix);
            tilemap.SetTileFlags(targetTilemapCell, tileSnapshot.Flags);
        }

        tilemap.CompressBounds();
    }

    private List<TilemapRotation> CollectTilemapRotations()
    {
        var tilemapRotations = new List<TilemapRotation>();
        AddTilemapRotation(tilemapRotations, rotatableTilemap);

        if (rotateCarriedAreaTilemap && carriedAreaTilemap != null && carriedAreaTilemap != rotatableTilemap)
            AddTilemapRotation(tilemapRotations, carriedAreaTilemap);

        return tilemapRotations;
    }

    private void AddTilemapRotation(List<TilemapRotation> tilemapRotations, Tilemap tilemap)
    {
        if (tilemap == null)
            return;

        tilemapRotations.Add(new TilemapRotation
        {
            Tilemap = tilemap,
            TileSnapshots = CollectTilemapSnapshots(tilemap)
        });
    }

    private List<TileSnapshot> CollectTilemapSnapshots(Tilemap tilemap)
    {
        var tileSnapshots = new List<TileSnapshot>();
        if (tilemap == null || GridManager.Instance == null)
            return tileSnapshots;

        foreach (var tilemapCell in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(tilemapCell);
            if (tile == null)
                continue;

            tileSnapshots.Add(new TileSnapshot
            {
                GridCell = GridManager.Instance.WorldToCell(tilemap.GetCellCenterWorld(tilemapCell)),
                TilemapCell = tilemapCell,
                Tile = tile,
                Color = tilemap.GetColor(tilemapCell),
                TransformMatrix = tilemap.GetTransformMatrix(tilemapCell),
                Flags = tilemap.GetTileFlags(tilemapCell)
            });
        }

        return tileSnapshots;
    }

    private Vector3Int GridCellToTilemapCell(Tilemap tilemap, Vector3Int gridCell)
    {
        return GridManager.Instance != null
            ? tilemap.WorldToCell(GridManager.Instance.GetCellCenterWorld(gridCell))
            : gridCell;
    }

    private Vector3 GetPivotCell()
    {
        if (useTilemapBoundsCenterAsPivot)
        {
            CacheTilemapPivotIfNeeded();
            if (hasCachedTilemapPivot)
                return cachedTilemapPivotCell;
        }

        return GridManager.Instance != null && pivot != null
            ? GridManager.Instance.WorldToCell(pivot.position)
            : Vector3.zero;
    }

    private Vector3 GetPivotWorldPosition()
    {
        if (useTilemapBoundsCenterAsPivot)
        {
            CacheTilemapPivotIfNeeded();
            if (hasCachedTilemapPivot)
                return cachedTilemapPivotWorld;
        }

        return pivot != null ? pivot.position : transform.position;
    }

    private void CacheTilemapPivotIfNeeded()
    {
        if (hasCachedTilemapPivot || !useTilemapBoundsCenterAsPivot || GridManager.Instance == null)
            return;

        var terrainCells = CollectTerrainCells();
        if (terrainCells.Count == 0)
            return;

        var minX = terrainCells[0].x;
        var maxX = terrainCells[0].x;
        var minY = terrainCells[0].y;
        var maxY = terrainCells[0].y;
        var z = terrainCells[0].z;

        foreach (var terrainCell in terrainCells)
        {
            minX = Mathf.Min(minX, terrainCell.x);
            maxX = Mathf.Max(maxX, terrainCell.x);
            minY = Mathf.Min(minY, terrainCell.y);
            maxY = Mathf.Max(maxY, terrainCell.y);
        }

        cachedTilemapPivotCell = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, z);

        var minWorld = GridManager.Instance.GetCellCenterWorld(new Vector3Int(minX, minY, z));
        var maxWorld = GridManager.Instance.GetCellCenterWorld(new Vector3Int(maxX, maxY, z));
        cachedTilemapPivotWorld = (minWorld + maxWorld) * 0.5f;
        hasCachedTilemapPivot = true;
    }

    private bool IsCellOnCarriedTerrain(Vector3Int gridCell)
    {
        if (carriedAreaTilemap == null || GridManager.Instance == null)
            return false;

        var tilemapCell = carriedAreaTilemap.WorldToCell(GridManager.Instance.GetCellCenterWorld(gridCell));
        return carriedAreaTilemap.HasTile(tilemapCell);
    }

    private List<Vector3Int> CollectTerrainCells()
    {
        var terrainCells = new List<Vector3Int>();
        AddTilemapGridCells(terrainCells, carriedAreaTilemap);

        if (rotatableTilemap != carriedAreaTilemap)
            AddTilemapGridCells(terrainCells, rotatableTilemap);

        return terrainCells;
    }

    private void AddTilemapGridCells(List<Vector3Int> gridCells, Tilemap tilemap)
    {
        if (gridCells == null || tilemap == null || GridManager.Instance == null)
            return;

        foreach (var tilemapCell in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(tilemapCell))
                continue;

            gridCells.Add(GridManager.Instance.WorldToCell(tilemap.GetCellCenterWorld(tilemapCell)));
        }
    }

    private Tilemap FindChildTilemapByName(string tilemapName)
    {
        var childTilemaps = GetComponentsInChildren<Tilemap>(true);
        foreach (var childTilemap in childTilemaps)
        {
            if (childTilemap != null && childTilemap.name == tilemapName)
                return childTilemap;
        }

        return null;
    }

    private static Vector3Int RotateCellClockwise(Vector3Int cellPos, Vector3 pivotCell, int clockwiseQuarterTurns)
    {
        var offset = new Vector3(cellPos.x - pivotCell.x, cellPos.y - pivotCell.y, cellPos.z - pivotCell.z);
        for (var i = 0; i < clockwiseQuarterTurns; i++)
            offset = new Vector3(offset.y, -offset.x, offset.z);

        return new Vector3Int(
            Mathf.RoundToInt(pivotCell.x + offset.x),
            Mathf.RoundToInt(pivotCell.y + offset.y),
            Mathf.RoundToInt(pivotCell.z + offset.z));
    }

    private static int NormalizeQuarterTurns(int quarterTurns)
    {
        quarterTurns %= 4;
        if (quarterTurns < 0)
            quarterTurns += 4;

        return quarterTurns;
    }

    private struct TileSnapshot
    {
        public Vector3Int GridCell { get; set; }
        public Vector3Int TilemapCell { get; set; }
        public TileBase Tile { get; set; }
        public Color Color { get; set; }
        public Matrix4x4 TransformMatrix { get; set; }
        public TileFlags Flags { get; set; }
    }

    private struct TilemapRotation
    {
        public Tilemap Tilemap { get; set; }
        public List<TileSnapshot> TileSnapshots { get; set; }
    }

    private struct PlayerRotationData
    {
        public bool ShouldCarry { get; set; }
        public PlayerController Player { get; set; }
        public Quaternion StartRotation { get; set; }
        public Vector3 TargetWorldPosition { get; set; }
    }
}
