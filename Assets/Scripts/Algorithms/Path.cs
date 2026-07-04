using System.Collections.Generic;
using UnityEngine;

public class Path
{
    public List<NodeData> directions = new();
    public float stepLength;
}

public class NodeData
{
    public NodeData(Vector2Int direction, float stopTime = 0)
    {
        this.direction = direction;
        this.stopTime = stopTime;
    }

    public Vector2Int direction;
    public float stopTime = 0;
}