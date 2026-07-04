using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFindingLogic
{
    Dictionary<Vector3Int, Node> gridNodes => GridManager.Instance.GetGrid();

    readonly Vector3Int[] neighborDirs = { new(1, 0, 0), new(-1, 0, 0), new(0, 1, 0), new(0, -1, 0) };

    bool lockPortal = false;
    bool isStartNode = false;

    public PathFindingLogic()
    {
        // foreach (var kvp in GridManager.Instance.GetGrid())
        // {
        //     gridNodes[kvp.Key] = new Node
        //     {
        //         position = kvp.Value.position,
        //         type = kvp.Value.type,
        //         specialTile = kvp.Value.specialTile
        //     };
        // }
    }


    public IEnumerable<Node> WalkableNeighbor(Node node)
    {
        if (node == null)
            yield break;
        if (node.type == TileType.Portal)
        {
            if (!lockPortal && !isStartNode)
            {
                lockPortal = true;
                var portal = node.specialTile as Portal;
                if (portal.linkedPortal != null)
                {
                    Vector3Int linkedPortalPos = GridManager.Instance.WorldToCell(portal.linkedPortal.transform.position);
                    yield return gridNodes[linkedPortalPos];
                }
            }
            else
            {
                lockPortal = false;
                foreach (var dir in neighborDirs)
                {
                    Vector3Int neighborPos = node.position + dir;
                    if (IsNodeWalkable(neighborPos))
                        yield return gridNodes[neighborPos];
                }
            }
        }
        else foreach (var dir in neighborDirs)
        {
            Vector3Int neighborPos = node.position + dir;
            if (IsNodeWalkable(neighborPos))
                yield return gridNodes[neighborPos];
        }
    }

    public List<Node> FindPath(Node startNode, Node targetNode)
    {
        Queue<Node> openList = new();
        Dictionary<Node, Node> cameFrom = new();
        openList.Enqueue(startNode);
        cameFrom[startNode] = null;
        isStartNode = true;

        while (openList.Count > 0)
        {
            var node = openList.Dequeue();
            if (node == targetNode)
            {
                List<Node> path = new();
                Node current = node;
                while (current != null)
                {
                    path.Add(current);
                    current = cameFrom.ContainsKey(current) ? cameFrom[current] : null;
                }
                path.Reverse();
                return path;
            }

            foreach (var neighbor in WalkableNeighbor(node))
            {
                if (cameFrom.ContainsKey(neighbor))
                    continue;

                cameFrom[neighbor] = node;
                openList.Enqueue(neighbor);
            }
            isStartNode = false;
        }

        return new List<Node>();
    }

    public List<Node> FindPathFromPlayer(Vector3 playerWorldPos, SpecialTile targetTile)
    {
        var startNode = gridNodes[GridManager.Instance.WorldToCell(playerWorldPos)];
        if (startNode == null)
        {
            Debug.LogError($"Player position {playerWorldPos} is not on the grid!");
            return null;
        }

        var targetNode = gridNodes[GridManager.Instance.WorldToCell(targetTile.transform.position)];
        if (targetNode == null)
        {
            Debug.LogError($"Target tile {targetTile.name} position {targetTile.transform.position} is not on the grid!");
            return null;
        }


        Debug.Log($"Finding path from {startNode.position} to {targetNode.position}");
        return FindPath(startNode, targetNode);
    }

    public List<Node> FindPathFromWorldPos(Vector3 startWorldPos, Vector3 targetWorldPos)
    {
        var startNode = gridNodes[GridManager.Instance.WorldToCell(startWorldPos)];
        if (startNode == null)
        {
            Debug.LogError($"Start position {startWorldPos} is not on the grid!");
            return null;
        }

        var targetNode = gridNodes[GridManager.Instance.WorldToCell(targetWorldPos)];
        if (targetNode == null)
        {
            Debug.LogError($"Target position {targetWorldPos} is not on the grid!");
            return null;
        }

        Debug.Log($"Finding path from {startNode.position} to {targetNode.position}");
        return FindPath(startNode, targetNode);
    }

    private bool IsNodeWalkable(Vector3Int pos)
    {
        return gridNodes.ContainsKey(pos) && gridNodes[pos].type != TileType.Wall;
        // return gridNodes.TryGetValue(pos, out Node node) && node.type == TileType.Walkable;
    }

    public void SetNodeType(Vector3Int pos, TileType type)
    {
        if (gridNodes.ContainsKey(pos))
            gridNodes[pos].type = type;
    }
}